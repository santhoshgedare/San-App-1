using Microsoft.AspNetCore.Identity;

namespace IdentityHub.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user, extended with profile fields. Kept in Infrastructure since it is
/// coupled to the Identity framework; the Application layer only ever sees <c>UserDto</c>.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>Soft-delete flag. Deleted users are excluded via a global EF query filter.</summary>
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }

    /// <summary>Soft-delete flag. Deleted roles are excluded via a global EF query filter.</summary>
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
