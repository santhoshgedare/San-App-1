namespace IdentityHub.Domain.Entities;

/// <summary>
/// 1:N Variant instance for an Item with dedicated variant pricing, SKU, barcode, and inventory.
/// Generated or customized based on Category Variant Definitions (e.g. Color=Red, Size=Large).
/// </summary>
public sealed class ItemVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>JSON serialized dictionary/list of variant attributes (e.g. {"Color": "Red", "Size": "XL"}).</summary>
    public string AttributesJson { get; set; } = "{}";

    /// <summary>Variant-specific selling price.</summary>
    public decimal Price { get; set; }

    /// <summary>Variant-specific cost price.</summary>
    public decimal CostPrice { get; set; }

    /// <summary>Variant-specific inventory stock quantity.</summary>
    public int StockQuantity { get; set; }

    public string? Barcode { get; set; }

    public bool IsActive { get; set; } = true;
}
