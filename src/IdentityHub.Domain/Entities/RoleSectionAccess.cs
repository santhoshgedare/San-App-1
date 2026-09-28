namespace IdentityHub.Domain.Entities;

/// <summary>
/// Grants a role access to a specific Section. This single join table replaces the legacy
/// app's three separate RoleAccessModule/RoleAccessPage/RoleAccessSection tables — granting
/// a Section implicitly grants its parent Page and Module (resolved via the join in queries),
/// which removes the redundant bookkeeping the old schema required.
/// </summary>
public sealed class RoleSectionAccess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoleId { get; set; }
    public Guid SectionId { get; set; }
    public Section? Section { get; set; }
}
