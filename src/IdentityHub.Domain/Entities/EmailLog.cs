namespace IdentityHub.Domain.Entities;

/// <summary>Every outbound email attempt (sent or failed), kept for tracking and troubleshooting.</summary>
public sealed class EmailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ToAddresses { get; set; } = string.Empty;
    public string? CcAddresses { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? TextBody { get; set; }
    /// <summary>Attachment file names with sizes (content is not stored).</summary>
    public string? Attachments { get; set; }
    /// <summary>Sent or Failed.</summary>
    public string Status { get; set; } = "Failed";
    public string? Error { get; set; }
    public string Category { get; set; } = "General";
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
}
