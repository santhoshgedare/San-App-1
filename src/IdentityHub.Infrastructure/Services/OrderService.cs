using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class OrderService(
    AppDbContext db,
    IActivityLogService activityLogs,
    IApprovalService approvals,
    IOrderNotifier notifier) : IOrderService
{
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Seller)
            .Include(o => o.ProgressUpdates)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null) return null;

        var dto = MapToDto(order);
        dto.ApprovalStatus = (await GetApprovalStatusAsync(order.Id, ct))?.ToString();
        return dto;
    }

    private async Task<ApprovalStatus?> GetApprovalStatusAsync(Guid orderId, CancellationToken ct)
    {
        var entityId = orderId.ToString();
        return await db.Approvals
            .Where(a => a.EntityType == EntityTypes.Order && a.EntityId == entityId && a.IsCurrent)
            .Select(a => (ApprovalStatus?)a.Status)
            .FirstOrDefaultAsync(ct);
    }

    public Task<Guid?> GetSellerIdAsync(Guid id, CancellationToken ct = default)
        => db.Orders.Where(o => o.Id == id).Select(o => o.SellerId).FirstOrDefaultAsync(ct);

    public async Task<PagedResult<OrderDto>> GetPagedAsync(
        string? search,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        Guid? customerId,
        int page,
        int pageSize,
        Guid? sellerId = null,
        CancellationToken ct = default)
    {
        var query = db.Orders.AsQueryable();

        if (sellerId.HasValue && customerId.HasValue)
        {
            // A seller sees orders for their company plus the orders they placed as a buyer.
            query = query.Where(o => o.SellerId == sellerId.Value || o.CustomerId == customerId.Value);
        }
        else if (sellerId.HasValue)
        {
            query = query.Where(o => o.SellerId == sellerId.Value);
        }
        else if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (paymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                o.CustomerName.ToLower().Contains(term) ||
                o.CustomerEmail.ToLower().Contains(term) ||
                o.CustomerPhone.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .Include(o => o.Seller)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<OrderDto>
        {
            Items = orders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(
        DateTimeOffset startInclusive,
        DateTimeOffset endExclusive,
        DateOnly startDate,
        DateOnly endDate,
        Guid? sellerId = null,
        CancellationToken ct = default)
    {
        var settledOrders = db.Orders.AsNoTracking()
            .Where(order => order.CreatedAt >= startInclusive &&
                order.CreatedAt < endExclusive &&
                (sellerId == null || order.SellerId == sellerId) &&
                (order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.Refunded));
        var refundedOrders = settledOrders.Where(order => order.PaymentStatus == PaymentStatus.Refunded);

        var paidOrderCount = await settledOrders.CountAsync(order => order.PaymentStatus == PaymentStatus.Paid, ct);
        var refundedOrderCount = await refundedOrders.CountAsync(ct);
        var legacyRefundCount = await refundedOrders.CountAsync(
            order => order.RefundAmount == null || order.RefundReferenceNumber == null,
            ct);
        var grossSales = await settledOrders.SumAsync(
            order => (decimal?)(order.PaymentStatus == PaymentStatus.Refunded ? (order.RefundAmount ?? order.TotalAmount) : order.TotalAmount),
            ct) ?? 0m;
        var refunds = await refundedOrders.SumAsync(order => (decimal?)(order.RefundAmount ?? order.TotalAmount), ct) ?? 0m;

        var settledItems = db.OrderItems.AsNoTracking()
            .Where(item => item.Order != null &&
                item.Order.CreatedAt >= startInclusive &&
                item.Order.CreatedAt < endExclusive &&
                (sellerId == null || item.Order.SellerId == sellerId) &&
                (item.Order.PaymentStatus == PaymentStatus.Paid || item.Order.PaymentStatus == PaymentStatus.Refunded));
        var missingCostLineCount = await settledItems.CountAsync(item => item.UnitCostPrice == null, ct);
        var costOfGoodsSold = await settledItems.SumAsync(
            item => (decimal?)((item.UnitCostPrice ?? 0m) * item.Quantity),
            ct) ?? 0m;
        var isComplete = missingCostLineCount == 0 && legacyRefundCount == 0;
        var netSales = grossSales - refunds;

        return new ProfitLossReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            PaidOrderCount = paidOrderCount,
            RefundedOrderCount = refundedOrderCount,
            LegacyRefundCount = legacyRefundCount,
            MissingCostLineCount = missingCostLineCount,
            GrossSales = grossSales,
            Refunds = refunds,
            NetSales = netSales,
            CostOfGoodsSold = costOfGoodsSold,
            GrossProfitOrLoss = isComplete ? netSales - costOfGoodsSold : null,
            IsComplete = isComplete
        };
    }

    public async Task<Result<OrderDto>> CreateAsync(
        Guid? customerId,
        string customerName,
        string customerEmail,
        string customerPhone,
        string shippingAddress,
        string? billingAddress,
        string? orderNotes,
        PaymentMethod paymentMethod,
        string? paymentReferenceNumber,
        string? offlinePaymentNotes,
        IReadOnlyList<OrderItemInput> items,
        CancellationToken ct = default)
    {
        if (items.Count == 0)
        {
            return Result<OrderDto>.Failure("An order must contain at least one item.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var itemIds = items.Select(i => i.ItemId).Distinct().OrderBy(id => id).ToArray();
        var products = await db.Items
            .Include(i => i.Seller)
            .Include(i => i.Variants)
            .Include(i => i.Images)
            .Where(i => itemIds.Contains(i.Id) && i.IsActive && i.ApprovalStatus == ItemApprovalStatus.Approved)
            .OrderBy(i => i.Id)
            .ToListAsync(ct);
        var productsById = products.ToDictionary(i => i.Id);
        var sellerIds = products.Select(p => p.SellerId).Distinct().ToList();
        if (sellerIds.Count > 1)
        {
            return Result<OrderDto>.Failure("Items from different sellers must be ordered separately.");
        }
        var orderSeller = products.Select(p => p.Seller).FirstOrDefault(s => s is not null);
        if (orderSeller is { CanSell: false })
        {
            return Result<OrderDto>.Failure($"{orderSeller.CompanyName} is not currently accepting orders. Please remove their items from your cart.");
        }
        if (orderSeller is not null && (orderSeller.UserId == customerId
            || await db.SellerEmployees.AnyAsync(e => e.SellerId == orderSeller.Id && e.UserId == customerId, ct)))
        {
            return Result<OrderDto>.Failure("You cannot order products from your own company.");
        }
        var requestedQuantities = new Dictionary<(Guid ItemId, Guid? VariantId), long>();

        foreach (var itemInput in items)
        {
            if (itemInput.Quantity <= 0)
            {
                return Result<OrderDto>.Failure("Order quantities must be greater than zero.");
            }

            if (!productsById.TryGetValue(itemInput.ItemId, out var product))
            {
                return Result<OrderDto>.Failure("An item in this order is no longer available.");
            }

            ItemVariant? variant = null;
            if (product.Variants.Count > 0)
            {
                if (!itemInput.ItemVariantId.HasValue)
                {
                    return Result<OrderDto>.Failure($"Choose an available option for {product.Name}.");
                }

                variant = product.Variants.FirstOrDefault(v => v.Id == itemInput.ItemVariantId.Value && v.IsActive);
                if (variant is null)
                {
                    return Result<OrderDto>.Failure($"The selected option for {product.Name} is no longer available.");
                }
            }
            else if (itemInput.ItemVariantId.HasValue)
            {
                return Result<OrderDto>.Failure($"The selected option for {product.Name} is no longer available.");
            }

            var key = (product.Id, variant?.Id);
            requestedQuantities[key] = requestedQuantities.GetValueOrDefault(key) + itemInput.Quantity;
        }

        foreach (var (key, requestedQuantity) in requestedQuantities)
        {
            var product = productsById[key.ItemId];
            var variant = key.VariantId.HasValue
                ? product.Variants.First(v => v.Id == key.VariantId.Value)
                : null;
            var availableQuantity = variant?.StockQuantity ?? product.StockQuantity;
            var itemLabel = variant is null ? product.Name : $"{product.Name} ({variant.Name})";

            if (availableQuantity < requestedQuantity)
            {
                return Result<OrderDto>.Failure(
                    $"{itemLabel} has only {availableQuantity} in stock; {requestedQuantity} requested.");
            }
        }

        foreach (var (key, requestedQuantity) in requestedQuantities)
        {
            var product = productsById[key.ItemId];
            if (key.VariantId.HasValue)
            {
                product.Variants.First(v => v.Id == key.VariantId.Value).StockQuantity -= (int)requestedQuantity;
            }
            else
            {
                product.StockQuantity -= (int)requestedQuantity;
            }
        }

        var orderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(10000, 99999)}";

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemInput in items)
        {
            var product = productsById[itemInput.ItemId];
            var variant = itemInput.ItemVariantId.HasValue
                ? product.Variants.First(v => v.Id == itemInput.ItemVariantId.Value)
                : null;
            var unitPrice = variant?.Price ?? product.Price;
            var lineTotal = unitPrice * itemInput.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ItemId = product.Id,
                ItemVariantId = variant?.Id,
                ItemCode = product.Code,
                ItemName = product.Name,
                VariantSku = variant?.Sku,
                VariantName = variant?.Name,
                AttributesJson = variant?.AttributesJson,
                ImageUrl = product.Images.OrderBy(image => image.SortOrder).FirstOrDefault(image => image.IsPrimary)?.Url
                    ?? product.Images.OrderBy(image => image.SortOrder).FirstOrDefault()?.Url,
                UnitPrice = unitPrice,
                UnitCostPrice = variant?.CostPrice ?? product.CostPrice,
                Quantity = itemInput.Quantity,
                TotalPrice = lineTotal
            });
        }

        var shippingFee = 0.00m; // Set by staff after the order is reviewed
        var taxAmount = 0.00m;
        var totalAmount = subtotal + shippingFee + taxAmount;

        var order = new Order
        {
            OrderNumber = orderNumber,
            SellerId = sellerIds.FirstOrDefault(),
            Seller = orderSeller,
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            CustomerEmail = customerEmail.Trim(),
            CustomerPhone = customerPhone.Trim(),
            ShippingAddress = shippingAddress.Trim(),
            BillingAddress = billingAddress?.Trim(),
            OrderNotes = orderNotes?.Trim(),
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            PaymentReferenceNumber = paymentReferenceNumber?.Trim(),
            OfflinePaymentNotes = offlinePaymentNotes?.Trim(),
            Status = OrderStatus.Pending,
            SubtotalAmount = subtotal,
            ShippingFee = shippingFee,
            ShippingFeeConfirmed = false,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "Created",
            $"Order {order.OrderNumber} placed by {order.CustomerName} for {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} ({order.PaymentMethod})",
            ct);
        await transaction.CommitAsync(ct);

        // Raise approval if approval workflow configured for Orders
        await approvals.RequestAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            $"Order Approval: {order.OrderNumber}",
            $"Customer: {order.CustomerName}, Payment Method: {order.PaymentMethod}, Total: {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}",
            ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.Placed, $"Total {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}.", ct);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result> UpdateStatusAsync(
        Guid id,
        OrderStatus status,
        string? trackingNumber,
        string? shippingCarrier,
        decimal? advanceAmount = null,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        var oldStatus = order.Status;
        if (oldStatus == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
        {
            return Result.Failure("A cancelled order cannot be reopened.");
        }

        if (status == OrderStatus.Cancelled && oldStatus is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            return Result.Failure("A shipped or delivered order cannot be cancelled.");
        }

        if (status != oldStatus && !IsValidTransition(oldStatus, status))
        {
            return Result.Failure($"An order cannot move from {oldStatus} to {status}.");
        }

        if (oldStatus == OrderStatus.Pending && status != oldStatus && status != OrderStatus.Cancelled)
        {
            // Orders with no approval record (placed before approvals were enforced) are not blocked.
            var approvalStatus = await GetApprovalStatusAsync(order.Id, ct);
            if (approvalStatus == ApprovalStatus.Rejected)
            {
                return Result.Failure("This order's approval was rejected. It can only be cancelled.");
            }
            if (approvalStatus is not null && approvalStatus != ApprovalStatus.Approved)
            {
                return Result.Failure("This order must be approved before it can be processed.");
            }
        }

        if (status != oldStatus && status is OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered && !order.ShippingFeeConfirmed)
        {
            return Result.Failure("Confirm the delivery charge before processing the order.");
        }

        if (oldStatus == OrderStatus.Pending && status == OrderStatus.Confirmed)
        {
            if (!order.ShippingFeeConfirmed)
            {
                return Result.Failure("Set the delivery charge before confirming the order.");
            }
            if (order.ChargesAcceptedAt is null)
            {
                return Result.Failure("The buyer must accept the delivery charge before the order can be confirmed.");
            }

            // Only custom orders may take an advance; in-stock orders are paid in full up front.
            var advance = order.CustomRequestId is null ? 0m : Math.Round(advanceAmount ?? 0m, 2);
            if (advance < 0 || advance > order.TotalAmount)
            {
                return Result.Failure("The advance amount must be between 0 and the order total.");
            }
            order.AdvanceAmount = advance;
            order.AdvanceStatus = PaymentStatus.Pending;
        }

        if (oldStatus == OrderStatus.Confirmed && status == OrderStatus.Processing && !IsAdvanceSettled(order))
        {
            return Result.Failure("Collect and verify the advance payment before starting fulfilment.");
                }

                if (oldStatus == OrderStatus.Confirmed && status == OrderStatus.Processing && order.AdvanceAmount <= 0 && order.PaymentStatus != PaymentStatus.Paid)
                {
                    return Result.Failure("Collect and verify the full payment before starting fulfilment.");
        }

        if (status != oldStatus && status is OrderStatus.Shipped or OrderStatus.Delivered && order.PaymentStatus != PaymentStatus.Paid)
        {
            return Result.Failure("Collect and verify the final payment before shipping the order.");
        }

        if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled && AmountHeld(order) > 0)
        {
            return Result.Failure("Payment has been received for this order. Record a refund before cancelling it.");
        }

        if (oldStatus != OrderStatus.Cancelled && status == OrderStatus.Cancelled)
        {
            var itemIds = order.Items.Select(i => i.ItemId).Distinct().ToArray();
            var items = await db.Items
                .IgnoreQueryFilters()
                .Include(i => i.Variants)
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, ct);

            foreach (var orderItem in order.Items)
            {
                if (!items.TryGetValue(orderItem.ItemId, out var item)) continue;

                if (orderItem.ItemVariantId.HasValue)
                {
                    var variant = item.Variants.FirstOrDefault(v => v.Id == orderItem.ItemVariantId.Value);
                    if (variant is not null) variant.StockQuantity += orderItem.Quantity;
                }
                else
                {
                    item.StockQuantity += orderItem.Quantity;
                }
            }
        }

        order.Status = status;
        if (!string.IsNullOrWhiteSpace(trackingNumber)) order.TrackingNumber = trackingNumber.Trim();
        if (!string.IsNullOrWhiteSpace(shippingCarrier)) order.ShippingCarrier = shippingCarrier.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "StatusUpdated",
            $"Order status changed from {oldStatus} to {status}. Tracking: {order.TrackingNumber ?? "N/A"}",
            ct);
        await transaction.CommitAsync(ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.StatusChanged, $"Status: {oldStatus} → {status}." + (order.TrackingNumber is null ? "" : $" Tracking: {order.ShippingCarrier} {order.TrackingNumber}"), ct);

        return Result.Success();
    }

    private static bool IsValidTransition(OrderStatus from, OrderStatus to) => from switch
    {
        OrderStatus.Pending => to is OrderStatus.Confirmed or OrderStatus.Cancelled,
        OrderStatus.Confirmed => to is OrderStatus.Processing or OrderStatus.Cancelled,
        OrderStatus.Processing => to is OrderStatus.Shipped or OrderStatus.Cancelled,
        OrderStatus.Shipped => to is OrderStatus.Delivered,
        _ => false,
    };

    public async Task<Result> CancelOwnOrderAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }
        if (order.Status != OrderStatus.Pending)
        {
            return Result.Failure("Only orders that are not yet confirmed can be cancelled. Please contact us for help.");
        }

        return await UpdateStatusAsync(id, OrderStatus.Cancelled, null, null, ct: ct);
    }

    public async Task<Result> AcceptChargesAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }
        if (order.Status != OrderStatus.Pending)
        {
            return Result.Failure("The delivery charge can only be accepted before the order is confirmed.");
        }
        if (!order.ShippingFeeConfirmed)
        {
            return Result.Failure("The delivery charge has not been set yet. Please wait for the seller to confirm it.");
        }
        if (order.ChargesAcceptedAt is not null)
        {
            return Result.Failure("You have already accepted the delivery charge.");
        }

        order.ChargesAcceptedAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = order.ChargesAcceptedAt;
        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "ChargesAccepted",
            $"Buyer accepted delivery charge {order.ShippingFee.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} and order total {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}.",
            ct);
        await notifier.NotifyAsync(order.Id, OrderNotice.ChargesAccepted, $"Delivery charge and total {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} accepted.", ct);

        return Result.Success();
    }

    private static bool IsAdvanceSettled(Order o) => o.AdvanceAmount <= 0 || o.AdvanceStatus == PaymentStatus.Paid;

    /// <summary>Money received and not refunded: the full total, or just the advance.</summary>
    private static decimal AmountHeld(Order o) =>
        o.PaymentStatus == PaymentStatus.Refunded ? 0m
        : o.PaymentStatus == PaymentStatus.Paid ? o.TotalAmount
        : o.AdvanceStatus == PaymentStatus.Paid ? o.AdvanceAmount
        : 0m;

    /// <summary>Which payment is being collected right now: the advance after confirmation, the balance during fulfilment.</summary>
    private static string PaymentStageOf(Order o)
    {
        if (o.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded) return "None";
        return o.Status switch
        {
            OrderStatus.Confirmed => o.AdvanceAmount <= 0 ? "Final" : o.AdvanceStatus != PaymentStatus.Paid ? "Advance" : "None",
            OrderStatus.Processing => IsAdvanceSettled(o) ? "Final" : "Advance",
            _ => "None",
        };
    }

    private static decimal AmountDueOf(Order o) => PaymentStageOf(o) switch
    {
        "Advance" => o.AdvanceAmount,
        "Final" => o.TotalAmount - (o.AdvanceStatus == PaymentStatus.Paid ? o.AdvanceAmount : 0m),
        _ => 0m,
    };

    public async Task<Result> SetShippingFeeAsync(Guid id, decimal shippingFee, string? note, CancellationToken ct = default)
    {
        if (shippingFee < 0 || shippingFee > 100000)
        {
            return Result.Failure("Delivery charge must be between 0 and 100000.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }
        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return Result.Failure("The delivery charge can no longer be changed for this order.");
        }
        if (order.Status != OrderStatus.Pending && order.ShippingFeeConfirmed)
        {
            return Result.Failure("The delivery charge cannot be changed after the buyer has accepted it and the order is confirmed.");
        }
        if (order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded || order.AdvanceStatus == PaymentStatus.Paid)
        {
            return Result.Failure("The delivery charge cannot change after payment is received.");
        }
        if (order.Status == OrderStatus.Pending)
        {
            var approvalStatus = await GetApprovalStatusAsync(order.Id, ct);
            if (approvalStatus is not null && approvalStatus != ApprovalStatus.Approved)
            {
                return Result.Failure("The order must be approved before the delivery charge is set.");
            }
        }

        var oldFee = order.ShippingFee;
        order.ShippingFee = Math.Round(shippingFee, 2);
        order.ShippingFeeNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        order.ShippingFeeConfirmed = true;
        order.ChargesAcceptedAt = null;
        order.TotalAmount = order.SubtotalAmount + order.ShippingFee + order.TaxAmount;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "ShippingFeeUpdated",
            $"Delivery charge set from {oldFee.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} to {order.ShippingFee.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}. New total {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}.",
            ct);
        await transaction.CommitAsync(ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.DeliveryCharge, $"Delivery charge {order.ShippingFee.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}. Order total {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}.", ct);

        return Result.Success();
    }

    public async Task<Result> UpdatePaymentStatusAsync(
        Guid id,
        PaymentStatus paymentStatus,
        string? paymentReferenceNumber,
        string? offlinePaymentNotes,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        if (paymentStatus == PaymentStatus.Refunded)
        {
            return Result.Failure("Use the refund workflow to record a refund.");
        }
        if (order.Status is OrderStatus.Pending or OrderStatus.Cancelled)
        {
            return Result.Failure("Payment can be updated only after the order is confirmed.");
        }

        var oldPaymentStatus = order.PaymentStatus;
        if (oldPaymentStatus == PaymentStatus.Refunded)
        {
            return Result.Failure("A refunded payment cannot be changed.");
        }

        if (oldPaymentStatus == PaymentStatus.Paid)
        {
            return Result.Failure("A paid payment cannot be changed; use the refund workflow if necessary.");
        }

        var stage = PaymentStageOf(order);
        if (stage == "None")
        {
            return Result.Failure("No payment is due at this stage of the order.");
        }

        if (paymentStatus == PaymentStatus.Paid && !order.ShippingFeeConfirmed)
        {
            return Result.Failure("Confirm the delivery charge before marking payment as paid.");
        }

        var isAdvance = stage == "Advance";
        var storedReference = isAdvance ? order.AdvanceReferenceNumber : order.PaymentReferenceNumber;
        if (paymentStatus == PaymentStatus.Paid &&
            string.IsNullOrWhiteSpace(paymentReferenceNumber) &&
            string.IsNullOrWhiteSpace(storedReference))
        {
            return Result.Failure("A payment reference is required before confirming payment.");
        }

        var oldStageStatus = isAdvance ? order.AdvanceStatus : oldPaymentStatus;
        if (isAdvance)
        {
            order.AdvanceStatus = paymentStatus;
            if (!string.IsNullOrWhiteSpace(paymentReferenceNumber)) order.AdvanceReferenceNumber = paymentReferenceNumber.Trim();
        }
        else
        {
            order.PaymentStatus = paymentStatus;
            if (!string.IsNullOrWhiteSpace(paymentReferenceNumber)) order.PaymentReferenceNumber = paymentReferenceNumber.Trim();
        }
        if (!string.IsNullOrWhiteSpace(offlinePaymentNotes)) order.OfflinePaymentNotes = offlinePaymentNotes.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "PaymentUpdated",
            $"{stage} payment status changed from {oldStageStatus} to {paymentStatus}. Ref: {(isAdvance ? order.AdvanceReferenceNumber : order.PaymentReferenceNumber) ?? "N/A"}",
            ct);
        await transaction.CommitAsync(ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.PaymentUpdated, $"{stage} payment status: {paymentStatus}.", ct);

        return Result.Success();
    }

    public async Task<Result> SubmitPaymentDetailsAsync(
        Guid id,
        string paymentReferenceNumber,
        string? offlinePaymentNotes,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(paymentReferenceNumber))
        {
            return Result.Failure("A payment reference is required.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }
        if (order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded)
        {
            return Result.Failure("Payment has already been confirmed.");
        }
        if (order.Status is OrderStatus.Pending or OrderStatus.Cancelled)
        {
            return Result.Failure("Payment details can be submitted only after the order is confirmed.");
        }
        if (!order.ShippingFeeConfirmed)
        {
            return Result.Failure("The delivery charge has not been confirmed yet. Please wait for it before paying.");
        }

        var stage = PaymentStageOf(order);
        if (stage == "None")
        {
            return Result.Failure("No payment is due at this stage of the order.");
        }

        if (stage == "Advance")
        {
            order.AdvanceReferenceNumber = paymentReferenceNumber.Trim();
        }
        else
        {
            order.PaymentReferenceNumber = paymentReferenceNumber.Trim();
        }
        order.OfflinePaymentNotes = offlinePaymentNotes?.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "PaymentDetailsSubmitted",
            $"{stage} payment details submitted for verification.",
            ct);
        await transaction.CommitAsync(ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.PaymentSubmitted, $"{stage} payment reference: {paymentReferenceNumber.Trim()}", ct);

        return Result.Success();
    }

    public async Task<Result> RecordRefundAsync(
        Guid id,
        decimal refundAmount,
        string refundReferenceNumber,
        string? refundNotes,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refundReferenceNumber))
        {
            return Result.Failure("A refund transaction reference is required.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }
        var held = AmountHeld(order);
        if (held <= 0)
        {
            return Result.Failure("Only an order with a received payment can be refunded.");
        }
        if (refundAmount != held)
        {
            return Result.Failure($"Only a full refund of the amount received ({held.ToString("C", CultureInfo.GetCultureInfo("en-IN"))}) is supported.");
        }

        order.PaymentStatus = PaymentStatus.Refunded;
        order.RefundAmount = refundAmount;
        order.RefundReferenceNumber = refundReferenceNumber.Trim();
        order.RefundNotes = refundNotes?.Trim();
        order.RefundedAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = order.RefundedAt;
        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "RefundRecorded",
            $"Full refund of {refundAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} recorded. Refund reference: {order.RefundReferenceNumber}.",
            ct);
        await transaction.CommitAsync(ct);

        await notifier.NotifyAsync(order.Id, OrderNotice.Refunded, $"Refund of {refundAmount.ToString("C", CultureInfo.GetCultureInfo("en-IN"))} recorded. Reference: {order.RefundReferenceNumber}.", ct);

        return Result.Success();
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        SellerId = o.SellerId,
        SellerName = o.Seller?.CompanyName ?? "SRIVIDIKA",
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerId = o.CustomerId,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        ShippingAddress = o.ShippingAddress,
        BillingAddress = o.BillingAddress,
        OrderNotes = o.OrderNotes,
        PaymentMethod = o.PaymentMethod.ToString(),
        PaymentStatus = o.PaymentStatus.ToString(),
        PaymentReferenceNumber = o.PaymentReferenceNumber,
        OfflinePaymentNotes = o.OfflinePaymentNotes,
        RefundAmount = o.RefundAmount,
        RefundReferenceNumber = o.RefundReferenceNumber,
        RefundNotes = o.RefundNotes,
        RefundedAt = o.RefundedAt,
        Status = o.Status.ToString(),
        TrackingNumber = o.TrackingNumber,
        ShippingCarrier = o.ShippingCarrier,
        SubtotalAmount = o.SubtotalAmount,
        ShippingFee = o.ShippingFee,
        ShippingFeeConfirmed = o.ShippingFeeConfirmed,
        ShippingFeeNote = o.ShippingFeeNote,
        ChargesAcceptedAt = o.ChargesAcceptedAt,
        AdvanceAmount = o.AdvanceAmount,
        AdvanceStatus = o.AdvanceStatus.ToString(),
        AdvanceReferenceNumber = o.AdvanceReferenceNumber,
        AmountPaid = AmountHeld(o),
        PaymentStage = PaymentStageOf(o),
        AmountDue = AmountDueOf(o),
        TotalAmount = o.TotalAmount,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        CustomRequestId = o.CustomRequestId,
        ProgressUpdates = o.ProgressUpdates.OrderBy(u => u.CreatedAt).Select(u => new OrderProgressUpdateDto
        {
            Id = u.Id,
            Stage = u.Stage.ToString(),
            Note = u.Note,
            ImageUrl = u.ImageUrl,
            CreatedAt = u.CreatedAt
        }).ToList(),
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            OrderId = i.OrderId,
            ItemId = i.ItemId,
            ItemVariantId = i.ItemVariantId,
            ItemCode = i.ItemCode,
            ItemName = i.ItemName,
            VariantSku = i.VariantSku,
            VariantName = i.VariantName,
            AttributesJson = i.AttributesJson,
            ImageUrl = i.ImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}
