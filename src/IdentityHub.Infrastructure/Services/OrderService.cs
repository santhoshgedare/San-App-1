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
    IApprovalService approvals) : IOrderService
{
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return order is null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(
        string? search,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Orders.AsQueryable();

        if (customerId.HasValue)
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
            .Include(i => i.Variants)
            .Include(i => i.Images)
            .Where(i => itemIds.Contains(i.Id) && i.IsActive)
            .OrderBy(i => i.Id)
            .ToListAsync(ct);
        var productsById = products.ToDictionary(i => i.Id);
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
                Quantity = itemInput.Quantity,
                TotalPrice = lineTotal
            });
        }

        var shippingFee = 0.00m; // Free shipping
        var taxAmount = 0.00m;
        var totalAmount = subtotal + shippingFee + taxAmount;

        var order = new Order
        {
            OrderNumber = orderNumber,
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

        return Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result> UpdateStatusAsync(
        Guid id,
        OrderStatus status,
        string? trackingNumber,
        string? shippingCarrier,
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

        if (paymentStatus == PaymentStatus.Paid &&
            string.IsNullOrWhiteSpace(paymentReferenceNumber) &&
            string.IsNullOrWhiteSpace(order.PaymentReferenceNumber))
        {
            return Result.Failure("A payment reference is required before confirming payment.");
        }

        order.PaymentStatus = paymentStatus;
        if (!string.IsNullOrWhiteSpace(paymentReferenceNumber)) order.PaymentReferenceNumber = paymentReferenceNumber.Trim();
        if (!string.IsNullOrWhiteSpace(offlinePaymentNotes)) order.OfflinePaymentNotes = offlinePaymentNotes.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "PaymentUpdated",
            $"Payment status changed from {oldPaymentStatus} to {paymentStatus}. Ref: {order.PaymentReferenceNumber ?? "N/A"}",
            ct);
        await transaction.CommitAsync(ct);

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

        order.PaymentReferenceNumber = paymentReferenceNumber.Trim();
        order.OfflinePaymentNotes = offlinePaymentNotes?.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "PaymentDetailsSubmitted",
            "Payment details submitted for verification.",
            ct);
        await transaction.CommitAsync(ct);

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
        if (order.PaymentStatus != PaymentStatus.Paid)
        {
            return Result.Failure("Only a paid order can be refunded.");
        }
        if (refundAmount != order.TotalAmount)
        {
            return Result.Failure("Only a full refund for the order total is supported.");
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

        return Result.Success();
    }

    private static OrderDto MapToDto(Order o) => new()
    {
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
        TaxAmount = o.TaxAmount,
        TotalAmount = o.TotalAmount,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
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
