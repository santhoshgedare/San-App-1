using IdentityHub.Application.Common.Interfaces;

namespace IdentityHub.Application.Tests.Features;

internal sealed class NoOpOrderNotifier : IOrderNotifier
{
    public Task NotifyAsync(Guid orderId, OrderNotice notice, string? detail = null, CancellationToken ct = default) => Task.CompletedTask;
}
