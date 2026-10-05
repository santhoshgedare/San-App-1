namespace IdentityHub.Application.Common.Interfaces;

public enum OrderNotice
{
    Placed,
    StatusChanged,
    DeliveryCharge,
    PaymentSubmitted,
    PaymentUpdated,
    Refunded,
    ChatMessage
}

/// <summary>Emails the people affected by an order event (buyer, fulfilling seller or admin). Never throws.</summary>
public interface IOrderNotifier
{
    Task NotifyAsync(Guid orderId, OrderNotice notice, string? detail = null, CancellationToken ct = default);
}