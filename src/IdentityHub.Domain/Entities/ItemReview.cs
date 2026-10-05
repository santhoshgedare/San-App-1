namespace IdentityHub.Domain.Entities;

/// <summary>
/// A customer review of a purchased item, only allowed once the order has been delivered.
/// </summary>
public sealed class ItemReview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }

    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }

    public Guid UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }

    /// <summary>JSON array of image data URLs uploaded by the reviewer.</summary>
    public string? ImagesJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}