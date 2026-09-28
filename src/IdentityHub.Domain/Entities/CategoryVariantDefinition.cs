using IdentityHub.Domain.Enums;

namespace IdentityHub.Domain.Entities;

/// <summary>
/// Definition of an allowable variant type and options for a category (e.g. Color with ["Red", "Blue"]).
/// </summary>
public sealed class CategoryVariantDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public CategoryVariantType Type { get; set; } = CategoryVariantType.Color;

    /// <summary>JSON serialized list of string values available for this variant.</summary>
    public string ValuesJson { get; set; } = "[]";

    public bool IsRequired { get; set; }
}
