using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityHub.Infrastructure.Persistence;

/// <summary>
/// Seeds default roles, the initial administrator account, and the Module &gt; Page &gt; Section
/// master hierarchy (with default role access) on startup.
/// </summary>
public static class DbInitializer
{
    private const string PermissionDefaultsMarkerKey = "system-role-defaults-v2";
    private const string AdminEmailConfigurationKey = "SystemAdmin:Email";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();
        var adminEmail = services.GetRequiredService<IConfiguration>()[AdminEmailConfigurationKey]?.Trim();
        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new InvalidOperationException($"Configuration value '{AdminEmailConfigurationKey}' is required.");

        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            var createdAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(createdAdmin, "Admin@12345");
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create the default administrator: {string.Join(", ", result.Errors.Select(error => error.Description))}");

            admin = createdAdmin;
        }

        await EnsureRoleAsync(userManager, admin, Roles.Admin);
        await SeedModuleAccessAsync(db, roleManager);
    }

    private static async Task EnsureRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
    {
        if (await userManager.IsInRoleAsync(user, role)) return;

        var result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not assign the {role} role to the default administrator: {string.Join(", ", result.Errors.Select(error => error.Description))}");
    }

    private static async Task SeedModuleAccessAsync(AppDbContext db, RoleManager<ApplicationRole> roleManager)
    {
        var shouldSeedRoleDefaults = !await db.Modules.AnyAsync(module => module.Key == PermissionDefaultsMarkerKey);
        var sections = new Dictionary<string, Section>(StringComparer.Ordinal);

        async Task<Module> EnsureModuleAsync(string key, string name, int sortOrder)
        {
            var module = await db.Modules.FirstOrDefaultAsync(value => value.Key == key);
            if (module is not null) return module;

            module = new Module { Name = name, Key = key, SortOrder = sortOrder };
            db.Modules.Add(module);
            return module;
        }

        async Task<Page> EnsurePageAsync(Module module, string name, string url, int sortOrder)
        {
            var page = await db.Pages.FirstOrDefaultAsync(value => value.Url == url);
            if (page is not null) return page;

            page = new Page { Module = module, Name = name, Url = url, SortOrder = sortOrder };
            db.Pages.Add(page);
            return page;
        }

        async Task EnsureSectionAsync(Page page, string name, string key, int sortOrder)
        {
            var section = await db.Sections.FirstOrDefaultAsync(value => value.Key == key);
            if (section is null)
            {
                section = new Section { Page = page, Name = name, Key = key, SortOrder = sortOrder };
                db.Sections.Add(section);
            }
            sections[key] = section;
        }

        async Task<Page> AddPageAsync(Module module, string name, string url, int sortOrder, params (string Name, string Key)[] pageSections)
        {
            var page = await EnsurePageAsync(module, name, url, sortOrder);
            for (var index = 0; index < pageSections.Length; index++)
            {
                var (sectionName, key) = pageSections[index];
                await EnsureSectionAsync(page, sectionName, key, index + 1);
            }
            return page;
        }

        var storefront = await EnsureModuleAsync("module-storefront", "Storefront", 1);
        var inventory = await EnsureModuleAsync("module-inventory", "Inventory", 2);
        var administration = await EnsureModuleAsync("module-administration", "Administration", 3);
        var account = await EnsureModuleAsync("module-account", "Account & Settings", 4);
        var analytics = await EnsureModuleAsync("module-analytics", "Analytics & Reports", 5);

        await AddPageAsync(storefront, "Product Catalog", "/catalog", 1, ("View Catalog", "section-catalog-view"));
        await AddPageAsync(storefront, "Shopping Cart", "/cart", 2, ("View Cart", "section-cart-view"));
        await AddPageAsync(storefront, "Checkout", "/checkout", 3, ("Place Orders", "section-checkout-create"));
        await AddPageAsync(storefront, "Orders", "/orders", 4,
            ("View Own Orders", "section-orders-view"),
            ("Manage Order Fulfillment", "section-orders-manage"),
            ("Verify Payments", "section-orders-payment"),
            ("Process Refunds", "section-orders-refund"));
        var customRequestsAreNew = !await db.Sections.AnyAsync(section => section.Key == "section-custom-requests-view");
        await AddPageAsync(storefront, "Custom Requests", "/custom-requests", 5,
            ("View & Raise Custom Requests", "section-custom-requests-view"),
            ("Quote Custom Requests", "section-custom-requests-quote"));
        await AddPageAsync(inventory, "Categories", "/categories", 1,
            ("View Categories", "section-categories-view"),
            ("Manage Categories", "section-categories-manage"));
        await AddPageAsync(inventory, "Items", "/items", 2,
            ("View Items", "section-items-view"),
            ("Manage Items", "section-items-manage"));
        await AddPageAsync(administration, "Users", "/users", 1,
            ("View Users", "section-users-view"),
            ("Manage Users", "section-users-manage"),
            ("Assign User Roles", "section-users-roles"));
        await AddPageAsync(administration, "Roles", "/roles", 2,
            ("View Roles", "section-roles-view"),
            ("Manage Roles & Access", "section-roles-manage"));
        await AddPageAsync(administration, "Approval Workflows", "/approval-workflows", 3,
            ("View Approval Workflows", "section-approval-workflows-view"),
            ("Manage Approval Workflows", "section-approval-workflows-manage"));
        await AddPageAsync(account, "My Profile", "/profile", 1,
            ("View Profile", "section-profile-view"));
        await AddPageAsync(account, "Payment Settings", "/payment-settings", 2,
            ("View Payment Settings", "section-payment-settings-view"),
            ("Manage Payment Settings", "section-payment-settings-manage"));
        await AddPageAsync(analytics, "Dashboard & Reports", "/dashboard", 1,
            ("View Dashboard", "section-dashboard-view"),
            ("View Profit & Loss Reports", "section-reports-view"));

        await db.SaveChangesAsync();

        async Task GrantMissingAsync(string roleName, params string[] sectionKeys)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) return;

            var sectionIds = sectionKeys.Select(key => sections[key].Id).ToArray();
            var existingIds = await db.RoleSectionAccesses
                .Where(access => access.RoleId == role.Id && sectionIds.Contains(access.SectionId))
                .Select(access => access.SectionId)
                .ToListAsync();

            foreach (var sectionId in sectionIds.Except(existingIds))
            {
                db.RoleSectionAccesses.Add(new RoleSectionAccess { RoleId = role.Id, SectionId = sectionId });
            }
        }

        if (customRequestsAreNew && !shouldSeedRoleDefaults)
        {
            await GrantMissingAsync(Roles.Manager, "section-custom-requests-view", "section-custom-requests-quote");
            await GrantMissingAsync(Roles.User, "section-custom-requests-view");
            await db.SaveChangesAsync();
        }

        if (shouldSeedRoleDefaults)
        {
            await GrantMissingAsync(Roles.Manager,
                "section-custom-requests-view", "section-custom-requests-quote",
                "section-dashboard-view", "section-reports-view",
                "section-catalog-view", "section-cart-view", "section-checkout-create",
                "section-orders-view", "section-orders-manage", "section-orders-payment", "section-orders-refund",
                "section-categories-view", "section-categories-manage", "section-items-view", "section-items-manage",
                "section-users-view", "section-profile-view",
                "section-payment-settings-view", "section-payment-settings-manage");
            await GrantMissingAsync(Roles.User,
                "section-catalog-view", "section-cart-view", "section-checkout-create",
                "section-custom-requests-view",
                "section-orders-view", "section-profile-view");

            db.Modules.Add(new Module
            {
                Name = "Permission Defaults Marker",
                Key = PermissionDefaultsMarkerKey,
                IsActive = false,
                SortOrder = int.MaxValue
            });
            await db.SaveChangesAsync();
        }
    }
}
