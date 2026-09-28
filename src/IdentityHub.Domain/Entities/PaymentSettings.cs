namespace IdentityHub.Domain.Entities;

/// <summary>
/// Singleton configuration row holding the store's UPI QR payment details shown to customers at checkout.
/// </summary>
public sealed class PaymentSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The UPI ID / VPA (e.g. merchant@okhdfcbank) customers pay to.</summary>
    public string UpiId { get; set; } = string.Empty;

    /// <summary>Display name of the payee shown alongside the UPI ID.</summary>
    public string? PayeeName { get; set; }

    /// <summary>Base64 data URI (or URL) of the UPI QR code image to display at checkout.</summary>
    public string? QrCodeImageUrl { get; set; }

    /// <summary>Optional extra instructions shown to customers below the QR code.</summary>
    public string? Instructions { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedByEmail { get; set; }
}
