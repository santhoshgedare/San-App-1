using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IActivityLogService activityLog) : IIdentityService
{
    public async Task<Result<UserDto>> RegisterAsync(string? email, string password, string firstName, string lastName, string? phoneNumber, CancellationToken ct)
    {
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        var identifier = normalizedEmail ?? normalizedPhone;
        if (identifier is null)
        {
            return Result<UserDto>.Failure("An email address or phone number is required.");
        }

        var existing = normalizedEmail is null ? null : await userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            return Result<UserDto>.Failure("An account with this email already exists.");
        }
        if (normalizedPhone is not null &&
            await userManager.Users.AnyAsync(user => user.PhoneNumber == normalizedPhone, ct))
        {
            return Result<UserDto>.Failure("An account with this phone number already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = identifier,
            Email = normalizedEmail,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = normalizedPhone
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result<UserDto>.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await userManager.AddToRoleAsync(user, Domain.Constants.Roles.User);

        await activityLog.LogAsync(EntityTypes.User, user.Id.ToString(), "Created", $"User {identifier} registered.", ct);

        return Result<UserDto>.Success(await ToDto(user));
    }

    public async Task<Result<UserDto>> ValidateCredentialsAsync(string identifier, string password, CancellationToken ct)
    {
        var loginIdentifier = identifier.Trim();
        var user = await userManager.FindByEmailAsync(loginIdentifier)
            ?? await userManager.FindByNameAsync(loginIdentifier)
            ?? await userManager.Users.FirstOrDefaultAsync(candidate => candidate.PhoneNumber == loginIdentifier, ct);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return Result<UserDto>.Failure("Invalid email/phone number or password.");
        }

        return Result<UserDto>.Success(await ToDto(user));
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive) return null;
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return Result.Failure("The reset link is invalid or has expired.");
        }

        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!resetResult.Succeeded)
        {
            return Result.Failure(resetResult.Errors.Select(error => error.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.User, user.Id.ToString(), "PasswordReset", "Password reset completed.", ct);
        return Result.Success();
    }

    public async Task<Result<UserDto>> AuthenticateExternalAsync(
        string provider,
        string providerKey,
        string email,
        string firstName,
        string lastName,
        bool emailVerified,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
        {
            return Result<UserDto>.Failure("The identity provider did not return a usable account email and ID.");
        }

        var user = await userManager.FindByLoginAsync(provider, providerKey);
        var created = false;
        var linked = false;
        if (user is null)
        {
            user = await userManager.FindByEmailAsync(email);
            if (user is not null && !user.IsActive)
            {
                return Result<UserDto>.Failure("This account has been deactivated.");
            }
            if (user is not null && !emailVerified)
            {
                return Result<UserDto>.Failure("An account already uses this email. Sign in to that account first; this provider did not verify the email for linking.");
            }

            if (user is null)
            {
                var safeEmail = email.Trim();
                user = new ApplicationUser
                {
                    UserName = safeEmail,
                    Email = safeEmail,
                    EmailConfirmed = emailVerified,
                    FirstName = string.IsNullOrWhiteSpace(firstName) ? safeEmail.Split('@')[0] : firstName.Trim(),
                    LastName = lastName?.Trim() ?? string.Empty,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Result<UserDto>.Failure(createResult.Errors.Select(error => error.Description).ToArray());
                }
                created = true;
            }

            var loginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
            if (!loginResult.Succeeded)
            {
                if (created) await userManager.DeleteAsync(user);
                return Result<UserDto>.Failure(loginResult.Errors.Select(error => error.Description).ToArray());
            }
            linked = !created;

            if (created)
            {
                var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
                if (!roleResult.Succeeded)
                {
                    await userManager.RemoveLoginAsync(user, provider, providerKey);
                    await userManager.DeleteAsync(user);
                    return Result<UserDto>.Failure(roleResult.Errors.Select(error => error.Description).ToArray());
                }

                await activityLog.LogAsync(EntityTypes.User, user.Id.ToString(), "Created", $"User {user.Email} registered with {provider}.", ct);
            }
            else if (linked)
            {
                await activityLog.LogAsync(EntityTypes.User, user.Id.ToString(), "ExternalLoginLinked", $"{provider} sign-in linked to the account.", ct);
            }
        }

        if (!user.IsActive)
        {
            return Result<UserDto>.Failure("This account has been deactivated.");
        }

        return Result<UserDto>.Success(await ToDto(user));
    }

    public async Task<bool> AccountExistsAsync(OtpTarget target, CancellationToken ct)
        => await FindByTargetAsync(target, ct) is not null;

    public async Task<Result<UserDto>> AuthenticateOtpAsync(OtpTarget target, string firstName, string lastName, bool allowCreate, CancellationToken ct)
    {
        var user = await FindByTargetAsync(target, ct);
        if (user is null)
        {
            if (!allowCreate) return Result<UserDto>.Failure("No account found. Please register first.");

            var isEmail = target.Channel == OtpChannel.Email;
            user = new ApplicationUser
            {
                UserName = target.Value,
                Email = isEmail ? target.Value : null,
                EmailConfirmed = isEmail,
                PhoneNumber = isEmail ? null : target.Value,
                PhoneNumberConfirmed = !isEmail,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Result<UserDto>.Failure(createResult.Errors.Select(error => error.Description).ToArray());
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result<UserDto>.Failure(roleResult.Errors.Select(error => error.Description).ToArray());
            }

            await activityLog.LogAsync(EntityTypes.User, user.Id.ToString(), "Created", $"User {target.Value} registered with a one-time passcode.", ct);
        }
        else
        {
            if (!user.IsActive) return Result<UserDto>.Failure("This account has been deactivated.");

            // Receiving the code proves ownership of the contact point.
            var confirmed = target.Channel == OtpChannel.Email ? user.EmailConfirmed : user.PhoneNumberConfirmed;
            if (!confirmed)
            {
                if (target.Channel == OtpChannel.Email) user.EmailConfirmed = true;
                else user.PhoneNumberConfirmed = true;
                await userManager.UpdateAsync(user);
            }
        }

        return Result<UserDto>.Success(await ToDto(user));
    }

    private Task<ApplicationUser?> FindByTargetAsync(OtpTarget target, CancellationToken ct)
        => target.Channel == OtpChannel.Email
            ? userManager.FindByEmailAsync(target.Value)
            : userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == target.Value, ct);

    public async Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToDto(user);
    }

    public async Task<UserDto?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : await ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct)
    {
        var users = await userManager.Users.OrderBy(u => u.Email).ToListAsync(ct);
        var dtos = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            dtos.Add(await ToDto(user));
        }
        return dtos;
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(UserListQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var usersQuery = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            usersQuery = usersQuery.Where(u =>
                u.Email!.Contains(term) ||
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(query.Role);
            var idsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            usersQuery = usersQuery.Where(u => idsInRole.Contains(u.Id));
        }

        var totalCount = await usersQuery.CountAsync(ct);
        var pageOfUsers = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = new List<UserDto>(pageOfUsers.Count);
        foreach (var user in pageOfUsers)
        {
            dtos.Add(await ToDto(user));
        }

        return new PagedResult<UserDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result> UpdateUserAsync(Guid userId, string firstName, string lastName, bool isActive, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.IsActive = isActive;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.User, userId.ToString(), "Updated", $"Profile updated; active={isActive}.", ct);
        return Result.Success();
    }

    public async Task<Result<UserDto>> UpdateOwnProfileAsync(Guid userId, string firstName, string lastName, string? email, string? phoneNumber, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        var newEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        var newPhone = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        if (newEmail is null && newPhone is null)
        {
            return Result<UserDto>.Failure("Keep at least an email address or a phone number.");
        }

        var emailChanged = !string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase);
        var phoneChanged = newPhone != user.PhoneNumber;
        if (emailChanged && newEmail is not null)
        {
            var owner = await userManager.FindByEmailAsync(newEmail);
            if (owner is not null && owner.Id != userId)
            {
                return Result<UserDto>.Failure("Another account already uses this email address.");
            }
        }
        if (phoneChanged && newPhone is not null
            && await userManager.Users.AnyAsync(u => u.PhoneNumber == newPhone && u.Id != userId, ct))
        {
            return Result<UserDto>.Failure("Another account already uses this phone number.");
        }

        // The login name mirrors the original identifier, so keep it in sync when that identifier changes.
        var loginWasEmail = user.UserName is not null && string.Equals(user.UserName, user.Email, StringComparison.OrdinalIgnoreCase);
        var loginWasPhone = user.UserName is not null && user.UserName == user.PhoneNumber;

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        if (emailChanged)
        {
            user.Email = newEmail;
            user.EmailConfirmed = false;
        }
        if (phoneChanged)
        {
            user.PhoneNumber = newPhone;
            user.PhoneNumberConfirmed = false;
        }
        if ((emailChanged && loginWasEmail) || (phoneChanged && loginWasPhone))
        {
            var desired = newEmail ?? newPhone!;
            var taken = await userManager.FindByNameAsync(desired);
            if (taken is null || taken.Id == userId)
            {
                user.UserName = desired;
            }
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result<UserDto>.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.User, userId.ToString(), "Updated", "Profile updated by the user.", ct);
        return Result<UserDto>.Success(await ToDto(user));
    }

    /// <summary>Soft-deletes a user: marks it deleted/inactive rather than removing the row, so its history/audit trail is preserved.</summary>
    public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTimeOffset.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.User, userId.ToString(), "Deleted", $"User {user.Email} soft-deleted.", ct);
        return Result.Success();
    }

    public async Task<Result> AssignRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                return Result.Failure($"Role '{role}' does not exist.");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return Result.Failure(removeResult.Errors.Select(e => e.Description).ToArray());
        }

        var addResult = await userManager.AddToRolesAsync(user, roles);
        if (!addResult.Succeeded)
        {
            return Result.Failure(addResult.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.User, userId.ToString(), "RolesAssigned", $"Roles set to: {string.Join(", ", roles)}.", ct);
        return Result.Success();
    }

    public async Task SetLastLoginAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);
        }
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct)
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync(ct);
        var dtos = new List<RoleDto>(roles.Count);
        foreach (var role in roles)
        {
            var usersInRole = role.Name is null ? [] : await userManager.GetUsersInRoleAsync(role.Name);
            dtos.Add(new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty, UserCount = usersInRole.Count });
        }
        return dtos;
    }

    public async Task<PagedResult<RoleDto>> GetRolesPagedAsync(RoleListQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var rolesQuery = roleManager.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rolesQuery = rolesQuery.Where(r => r.Name!.Contains(term));
        }

        var totalCount = await rolesQuery.CountAsync(ct);
        var pageOfRoles = await rolesQuery
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = new List<RoleDto>(pageOfRoles.Count);
        foreach (var role in pageOfRoles)
        {
            var usersInRole = role.Name is null ? [] : await userManager.GetUsersInRoleAsync(role.Name);
            dtos.Add(new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty, UserCount = usersInRole.Count });
        }

        return new PagedResult<RoleDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result> CreateRoleAsync(string roleName, CancellationToken ct)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return Result.Failure("A role with this name already exists.");
        }

        var role = new ApplicationRole(roleName);
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.Role, role.Id.ToString(), "Created", $"Role \"{roleName}\" created.", ct);
        return Result.Success();
    }

    /// <summary>Soft-deletes a role: marks it deleted rather than removing the row, so its history/audit trail is preserved.</summary>
    public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return Result.Failure("Role not found.");
        }

        role.IsDeleted = true;
        role.DeletedAt = DateTimeOffset.UtcNow;

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        await activityLog.LogAsync(EntityTypes.Role, roleId.ToString(), "Deleted", $"Role \"{role.Name}\" soft-deleted.", ct);
        return Result.Success();
    }

    private async Task<UserDto> ToDto(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToArray()
        };
    }
}
