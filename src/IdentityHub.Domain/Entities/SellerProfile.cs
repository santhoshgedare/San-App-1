namespace IdentityHub.Domain.Entities;

/// <summary>
/// Company profile of a seller (a user in the Manager role). Items and orders are linked to it
/// so each seller manages only their own catalogue and orders.
/// </summary>
public sealed class SellerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The manager account that operates this company. Null until a manager is linked or invited.</summary>
    public Guid? UserId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }

    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Payment details the seller collects customer payments with.
    public string? UpiId { get; set; }
    public string? PayeeName { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? BankDetails { get; set; }

    // Invitation workflow: the admin invites by email, the seller completes registration from the link.
    public string? InviteEmail { get; set; }
    public string? InviteTokenHash { get; set; }
    public DateTimeOffset? InviteExpiresAt { get; set; }
    public DateTimeOffset? InviteSentAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}