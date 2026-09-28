namespace IdentityHub.Domain.Entities;

/// <summary>
/// A screen/route within a Module (e.g. "Users list", "Roles list"). Pages contain Sections,
/// the finest-grained permission unit (e.g. a button or a tab on that screen).
/// </summary>
public sealed class Page
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public Module? Module { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Section> Sections { get; set; } = [];
}
