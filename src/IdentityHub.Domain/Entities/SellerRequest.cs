namespace IdentityHub.Domain.Entities;

/// <summary>A maker's public request to sell on the marketplace; an admin contacts them and invites them later.</summary>
public sealed class SellerRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? City { get; set; }
    public string? CraftType { get; set; }
    public string? Website { get; set; }
    public string? Message { get; set; }
    /// <summary>New, Contacted, Invited or Declined.</summary>
    public string Status { get; set; } = "New";
    public string? AdminNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
