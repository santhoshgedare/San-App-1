using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over ASP.NET Core Identity's <c>UserManager</c>/<c>SignInManager</c>, kept in Application
/// so use-case handlers don't take a dependency on Infrastructure or ASP.NET Identity types directly.
/// </summary>
public interface IIdentityService
{
    Task<Result<UserDto>> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber, CancellationToken ct);
    Task<Result<UserDto>> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
    Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken ct);
    Task<UserDto?> FindByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct);
    Task<PagedResult<UserDto>> GetUsersPagedAsync(UserListQuery query, CancellationToken ct);
    Task<Result> UpdateUserAsync(Guid userId, string firstName, string lastName, bool isActive, CancellationToken ct);
    Task<Result> DeleteUserAsync(Guid userId, CancellationToken ct);
    Task<Result> AssignRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken ct);
    Task SetLastLoginAsync(Guid userId, CancellationToken ct);

    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct);
    Task<PagedResult<RoleDto>> GetRolesPagedAsync(RoleListQuery query, CancellationToken ct);
    Task<Result> CreateRoleAsync(string roleName, CancellationToken ct);
    Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken ct);
}

/// <summary>Search/filter/pagination parameters for the users list.</summary>
public sealed record UserListQuery
{
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>Search/filter/pagination parameters for the roles list.</summary>
public sealed record RoleListQuery
{
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
