namespace IdentityHub.Domain.Entities;

/// <summary>
/// 1:N Image attachment for an Item.
/// </summary>
public sealed class ItemImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }

    /// <summary>Image URL or Base64 data URI.</summary>
    public string Url { get; set; } = string.Empty;

    public string? FileName { get; set; }
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
