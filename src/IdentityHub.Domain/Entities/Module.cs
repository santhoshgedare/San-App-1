namespace IdentityHub.Domain.Entities;

/// <summary>
/// A top-level functional area of the application (e.g. "User Access", "Settings").
/// Modules contain Pages, which contain Sections — the same three-tier master
/// hierarchy used by the legacy app to drive role-based UI visibility.
/// </summary>
public sealed class Module
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Page> Pages { get; set; } = [];
}
