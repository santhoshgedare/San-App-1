using IdentityHub.Domain.Enums;

namespace IdentityHub.Domain.Entities;

/// <summary>
/// Master entity representing an inventory item/product.
/// Belongs to a Category and supports 1:N Images, Documents, and Variants.
/// </summary>
public sealed class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public UnitOfMeasurement UnitOfMeasurement { get; set; } = UnitOfMeasurement.Piece;

    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Seller fulfilling this item. Null means the platform (SRIVIDIKA) itself.</summary>
    public Guid? SellerId { get; set; }
    public SellerProfile? Seller { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<ItemImage> Images { get; set; } = [];
    public List<ItemDocument> Documents { get; set; } = [];
    public List<ItemVariant> Variants { get; set; } = [];
}
