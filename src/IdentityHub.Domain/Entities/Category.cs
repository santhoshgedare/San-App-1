using IdentityHub.Domain.Enums;

namespace IdentityHub.Domain.Entities;

/// <summary>
/// Category master entity with configured Unit of Measurement and Variant Definitions (e.g. Color, Size).
/// </summary>
public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public UnitOfMeasurement UnitOfMeasurement { get; set; } = UnitOfMeasurement.Piece;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<CategoryVariantDefinition> VariantDefinitions { get; set; } = [];
}
