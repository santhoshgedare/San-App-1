namespace IdentityHub.Application.Common.Interfaces;

public sealed record OrderChatMessageDto(Guid Id, string SenderRole, string SenderName, string Body, DateTimeOffset CreatedAt, bool IsMine);

public sealed record OrderChatFaqDto(string Id, string Question);

public sealed class OrderChatDto
{
    /// <summary>Buyer, Seller or Observer (read-only).</summary>
    public string MyRole { get; init; } = "Observer";
    public bool CanPost { get; init; }
    /// <summary>True once the order is delivered or cancelled; the history stays readable.</summary>
    public bool IsClosed { get; init; }
    public string SellerName { get; init; } = "SRIVIDIKA";
    public IReadOnlyList<OrderChatMessageDto> Messages { get; init; } = [];
    public IReadOnlyList<OrderChatFaqDto> Faqs { get; init; } = [];
}

/// <summary>Per-order chat between the buyer and the seller, with canned FAQ answers for buyers.</summary>
public interface IOrderChatService
{
    /// <summary>Null when the order doesn't exist or the current user is not a participant.</summary>
    Task<OrderChatDto?> GetAsync(Guid orderId, CancellationToken ct);

    /// <summary>Posts a message, or when faqId is given the FAQ question plus its automatic answer. Returns an error when not allowed.</summary>
    Task<string?> SendAsync(Guid orderId, string? body, string? faqId, CancellationToken ct);
}