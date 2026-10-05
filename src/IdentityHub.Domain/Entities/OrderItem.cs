namespace IdentityHub.Domain.Entities;

/// <summary>
/// 1:N line item associated with an Order.
/// </summary>
public sealed class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid ItemId { get; set; }
    public Item? Item { get; set; }

    public Guid? ItemVariantId { get; set; }
    public ItemVariant? ItemVariant { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? VariantSku { get; set; }
    public string? VariantName { get; set; }
    public string? AttributesJson { get; set; }
    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal? UnitCostPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
