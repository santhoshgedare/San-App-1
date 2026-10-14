namespace IdentityHub.Domain.Entities;

/// <summary>A staff account that works on behalf of a seller company with the same access as the owner.</summary>
public sealed class SellerEmployee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SellerId { get; set; }
    /// <summary>Null while the email invite is still pending registration.</summary>
    public Guid? UserId { get; set; }
    public string InviteEmail { get; set; } = string.Empty;
    public string? InviteTokenHash { get; set; }
    public DateTimeOffset? InviteExpiresAt { get; set; }
    public DateTimeOffset? InviteSentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
