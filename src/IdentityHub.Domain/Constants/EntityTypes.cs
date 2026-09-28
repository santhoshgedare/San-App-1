namespace IdentityHub.Domain.Constants;

/// <summary>
/// Well-known entity-type discriminators used to tag <c>ActivityLog</c> rows so a single
/// audit table can reference any entity's primary key generically.
/// </summary>
public static class EntityTypes
{
    public const string User = "User";
    public const string Role = "Role";
    public const string Category = "Category";
    public const string Item = "Item";
    public const string Order = "Order";
    public const string PaymentSettings = "PaymentSettings";
    public const string Address = "Address";
}
