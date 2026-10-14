using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using IdentityHub.Application.Common.Interfaces;

namespace IdentityHub.Api.Hubs;

[Authorize]
public sealed class OrderChatHub : Hub
{
}

public sealed class SignalROrderLiveNotifier(
    IHubContext<OrderChatHub> hub,
    ILogger<SignalROrderLiveNotifier> logger) : IOrderLiveNotifier
{
    public async Task NotifyAsync(IEnumerable<Guid> recipientUserIds, Guid orderId, string orderNumber, string notice, string message, CancellationToken ct)
    {
        var users = recipientUserIds.Distinct().Select(id => id.ToString()).ToList();
        if (users.Count == 0) return;
        try
        {
            await hub.Clients.Users(users)
                .SendAsync("OrderUpdated", new { orderId, orderNumber, notice, message }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not send live order update for order {OrderId}.", orderId);
        }
    }
}

public sealed class SignalROrderChatNotifier(
    IHubContext<OrderChatHub> hub,
    ILogger<SignalROrderChatNotifier> logger) : IOrderChatLiveNotifier
{
    public async Task NotifyAsync(Guid recipientUserId, Guid orderId, CancellationToken ct)
    {
        try
        {
            await hub.Clients.User(recipientUserId.ToString())
                .SendAsync("OrderChatMessage", new { orderId }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not send live chat notification for order {OrderId} to user {UserId}.", orderId, recipientUserId);
        }
    }
}
