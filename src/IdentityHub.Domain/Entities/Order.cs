using IdentityHub.Domain.Enums;

namespace IdentityHub.Domain.Entities;

/// <summary>
/// Master order entity representing customer orders with offline payment workflow and status tracking.
/// </summary>
public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OrderNumber { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? OrderNotes { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.UpiQr;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentReferenceNumber { get; set; }
    public string? OfflinePaymentNotes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
