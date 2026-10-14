namespace IdentityHub.Application.Common.Interfaces;

public interface IOrderChatLiveNotifier
{
    Task NotifyAsync(Guid recipientUserId, Guid orderId, CancellationToken ct);
}

/// <summary>Pushes live order-process updates to connected users. Never throws.</summary>
public interface IOrderLiveNotifier
{
    Task NotifyAsync(IEnumerable<Guid> recipientUserIds, Guid orderId, string orderNumber, string notice, string message, CancellationToken ct);
}
