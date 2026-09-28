namespace IdentityHub.Domain.Entities;

/// <summary>
/// The finest-grained permission unit — a specific area/action within a Page
/// (e.g. "Users - Edit Roles", "Roles - Delete"). This is the key that
/// <c>canRenderDirective</c> checks against on the frontend.
/// </summary>
public sealed class Section
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PageId { get; set; }
    public Page? Page { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RoleSectionAccess> RoleAccess { get; set; } = [];
}
