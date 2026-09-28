using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Manages the Module &gt; Page &gt; Section master hierarchy and which Sections each role
/// can access. Drives both the settings navigator (bottom sheet) and the client-side
/// <c>canRender</c> directive used to hide/show UI by section key.
/// </summary>
public interface IModuleAccessService
{
    Task<IReadOnlyList<ModuleDto>> GetModuleTreeAsync(CancellationToken ct);

    Task<Result<ModuleDto>> CreateModuleAsync(string name, string key, int sortOrder, CancellationToken ct);
    Task<Result> DeleteModuleAsync(Guid moduleId, CancellationToken ct);

    Task<Result<PageDto>> CreatePageAsync(Guid moduleId, string name, string url, int sortOrder, CancellationToken ct);
    Task<Result> DeletePageAsync(Guid pageId, CancellationToken ct);

    Task<Result<SectionDto>> CreateSectionAsync(Guid pageId, string name, string key, int sortOrder, CancellationToken ct);
    Task<Result> DeleteSectionAsync(Guid sectionId, CancellationToken ct);

    Task<IReadOnlyList<RoleAccessDto>> GetAllRoleAccessAsync(CancellationToken ct);
    Task<RoleAccessDto?> GetRoleAccessAsync(Guid roleId, CancellationToken ct);
    Task<Result> SetRoleAccessAsync(Guid roleId, IReadOnlyCollection<string> sectionKeys, CancellationToken ct);

    /// <summary>Returns the set of section keys the given roles can access (union across roles).</summary>
    Task<IReadOnlyList<string>> GetSectionKeysForRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken ct);
}
