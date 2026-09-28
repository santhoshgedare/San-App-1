namespace IdentityHub.Domain.Entities;

/// <summary>
/// 1:N Document/attachment for an Item (e.g. specification sheet, user manual, warranty document).
/// </summary>
public sealed class ItemDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }

    /// <summary>Document file URL or Data URI.</summary>
    public string Url { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
