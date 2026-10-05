using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Identity;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class ModuleAccessService(AppDbContext db, RoleManager<ApplicationRole> roleManager) : IModuleAccessService
{
    public async Task<IReadOnlyList<ModuleDto>> GetModuleTreeAsync(CancellationToken ct)
    {
        var modules = await db.Modules
            .Where(module => module.IsActive)
            .Include(m => m.Pages)
            .ThenInclude(p => p.Sections)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

        return modules.Select(ToDto).ToList();
    }

    public async Task<Result<ModuleDto>> CreateModuleAsync(string name, string key, int sortOrder, CancellationToken ct)
    {
        if (await db.Modules.AnyAsync(m => m.Key == key, ct))
        {
            return Result<ModuleDto>.Failure("A module with this key already exists.");
        }

        var module = new Module { Name = name, Key = key, SortOrder = sortOrder };
        db.Modules.Add(module);
        await db.SaveChangesAsync(ct);

        return Result<ModuleDto>.Success(ToDto(module));
    }

    public async Task<Result> DeleteModuleAsync(Guid moduleId, CancellationToken ct)
    {
        var module = await db.Modules.FindAsync([moduleId], ct);
        if (module is null)
        {
            return Result.Failure("Module not found.");
        }

        db.Modules.Remove(module);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<PageDto>> CreatePageAsync(Guid moduleId, string name, string url, int sortOrder, CancellationToken ct)
    {
        if (!await db.Modules.AnyAsync(m => m.Id == moduleId, ct))
        {
            return Result<PageDto>.Failure("Module not found.");
        }

        var page = new Page { ModuleId = moduleId, Name = name, Url = url, SortOrder = sortOrder };
        db.Pages.Add(page);
        await db.SaveChangesAsync(ct);

        return Result<PageDto>.Success(ToDto(page));
    }

    public async Task<Result> DeletePageAsync(Guid pageId, CancellationToken ct)
    {
        var page = await db.Pages.FindAsync([pageId], ct);
        if (page is null)
        {
            return Result.Failure("Page not found.");
        }

        db.Pages.Remove(page);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<SectionDto>> CreateSectionAsync(Guid pageId, string name, string key, int sortOrder, CancellationToken ct)
    {
        if (!await db.Pages.AnyAsync(p => p.Id == pageId, ct))
        {
            return Result<SectionDto>.Failure("Page not found.");
        }

        if (await db.Sections.AnyAsync(s => s.Key == key, ct))
        {
            return Result<SectionDto>.Failure("A section with this key already exists.");
        }

        var section = new Section { PageId = pageId, Name = name, Key = key, SortOrder = sortOrder };
        db.Sections.Add(section);
        await db.SaveChangesAsync(ct);

        return Result<SectionDto>.Success(ToDto(section));
    }

    public async Task<Result> DeleteSectionAsync(Guid sectionId, CancellationToken ct)
    {
        var section = await db.Sections.FindAsync([sectionId], ct);
        if (section is null)
        {
            return Result.Failure("Section not found.");
        }

        db.Sections.Remove(section);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<IReadOnlyList<RoleAccessDto>> GetAllRoleAccessAsync(CancellationToken ct)
    {
        var roles = await roleManager.Roles.ToListAsync(ct);
        var accessByRole = await db.RoleSectionAccesses
            .Include(r => r.Section)
            .ToListAsync(ct);

        return roles.Select(role => new RoleAccessDto
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            SectionKeys = accessByRole.Where(a => a.RoleId == role.Id).Select(a => a.Section!.Key).ToList()
        }).ToList();
    }

    public async Task<RoleAccessDto?> GetRoleAccessAsync(Guid roleId, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return null;
        }

        var sectionKeys = await db.RoleSectionAccesses
            .Include(r => r.Section)
            .Where(r => r.RoleId == roleId)
            .Select(r => r.Section!.Key)
            .ToListAsync(ct);

        return new RoleAccessDto { RoleId = role.Id, RoleName = role.Name ?? string.Empty, SectionKeys = sectionKeys };
    }

    public async Task<Result> SetRoleAccessAsync(Guid roleId, IReadOnlyCollection<string> sectionKeys, CancellationToken ct)
    {
        if (await roleManager.FindByIdAsync(roleId.ToString()) is null)
        {
            return Result.Failure("Role not found.");
        }

        var requestedKeys = sectionKeys.Distinct(StringComparer.Ordinal).ToArray();
        var sectionIds = await db.Sections
            .Where(section => requestedKeys.Contains(section.Key) && section.IsActive && section.Page!.IsActive && section.Page.Module!.IsActive)
            .Select(s => s.Id)
            .ToListAsync(ct);
        if (sectionIds.Count != requestedKeys.Length)
        {
            return Result.Failure("One or more permission sections are invalid or inactive.");
        }

        var existing = await db.RoleSectionAccesses.Where(r => r.RoleId == roleId).ToListAsync(ct);
        db.RoleSectionAccesses.RemoveRange(existing);

        foreach (var sectionId in sectionIds)
        {
            db.RoleSectionAccesses.Add(new RoleSectionAccess { RoleId = roleId, SectionId = sectionId });
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<IReadOnlyList<string>> GetSectionKeysForRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken ct)
    {
        if (roleNames.Count == 0)
        {
            return [];
        }

        var roleIds = await roleManager.Roles
            .Where(r => r.Name != null && roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(ct);

        return await db.RoleSectionAccesses
            .Include(r => r.Section)
            .Where(r => roleIds.Contains(r.RoleId) &&
                r.Section!.IsActive &&
                r.Section.Page!.IsActive &&
                r.Section.Page.Module!.IsActive)
            .Select(r => r.Section!.Key)
            .Distinct()
            .ToListAsync(ct);
    }

    private static ModuleDto ToDto(Module module) => new()
    {
        Id = module.Id,
        Name = module.Name,
        Key = module.Key,
        SortOrder = module.SortOrder,
        IsActive = module.IsActive,
        Pages = module.Pages.OrderBy(p => p.SortOrder).Select(ToDto).ToList()
    };

    private static PageDto ToDto(Page page) => new()
    {
        Id = page.Id,
        ModuleId = page.ModuleId,
        Name = page.Name,
        Url = page.Url,
        SortOrder = page.SortOrder,
        IsActive = page.IsActive,
        Sections = page.Sections.OrderBy(s => s.SortOrder).Select(ToDto).ToList()
    };

    private static SectionDto ToDto(Section section) => new()
    {
        Id = section.Id,
        PageId = section.PageId,
        Name = section.Name,
        Key = section.Key,
        SortOrder = section.SortOrder,
        IsActive = section.IsActive
    };
}
