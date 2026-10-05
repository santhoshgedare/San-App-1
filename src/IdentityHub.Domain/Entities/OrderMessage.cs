namespace IdentityHub.Domain.Entities;

/// <summary>A chat message on an order between the buyer and the fulfilling seller (or an automatic FAQ answer).</summary>
public sealed class OrderMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? SenderUserId { get; set; }
    /// <summary>Buyer, Seller or Auto.</summary>
    public string SenderRole { get; set; } = "Buyer";
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}