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

        var orderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(10000, 99999)}";

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemInput in items)
        {
            var lineTotal = itemInput.UnitPrice * itemInput.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ItemId = itemInput.ItemId,
                ItemVariantId = itemInput.ItemVariantId,
                ItemCode = itemInput.ItemCode,
                ItemName = itemInput.ItemName,
                VariantSku = itemInput.VariantSku,
                VariantName = itemInput.VariantName,
                AttributesJson = itemInput.AttributesJson,
                ImageUrl = itemInput.ImageUrl,
                UnitPrice = itemInput.UnitPrice,
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

        // Record Activity Log
        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "Created",
            $"Order {order.OrderNumber} placed by {order.CustomerName} for {order.TotalAmount:C} ({order.PaymentMethod})",
            ct);

        // Raise approval if approval workflow configured for Orders
        await approvals.RequestAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            $"Order Approval: {order.OrderNumber}",
            $"Customer: {order.CustomerName}, Payment Method: {order.PaymentMethod}, Total: {order.TotalAmount:C}",
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
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        var oldStatus = order.Status;
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

        return Result.Success();
    }

    public async Task<Result> UpdatePaymentStatusAsync(
        Guid id,
        PaymentStatus paymentStatus,
        string? paymentReferenceNumber,
        string? offlinePaymentNotes,
        CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return Result.Failure("Order not found.");
        }

        var oldPaymentStatus = order.PaymentStatus;
        order.PaymentStatus = paymentStatus;
        if (!string.IsNullOrWhiteSpace(paymentReferenceNumber)) order.PaymentReferenceNumber = paymentReferenceNumber.Trim();
        if (!string.IsNullOrWhiteSpace(offlinePaymentNotes)) order.OfflinePaymentNotes = offlinePaymentNotes.Trim();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        // Auto-confirm order if payment is marked Paid and order is still Pending
        if (paymentStatus == PaymentStatus.Paid && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Confirmed;
        }

        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            "PaymentUpdated",
            $"Payment status changed from {oldPaymentStatus} to {paymentStatus}. Ref: {order.PaymentReferenceNumber ?? "N/A"}",
            ct);

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
