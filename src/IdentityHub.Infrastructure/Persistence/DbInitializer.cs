using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
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
    private const string DefaultCategoriesMarkerKey = "system-default-categories-v1";
    private const string DefaultCategoryVariantsMarkerKey = "system-default-category-variants-v1";

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
        await SeedDefaultCategoriesAsync(db);
        await SeedModuleAccessAsync(db, roleManager);
    }

    // Marker-guarded so categories an admin later deletes or renames are not recreated on restart.
    private static async Task SeedDefaultCategoriesAsync(AppDbContext db)
    {
        if (!await db.Modules.AnyAsync(m => m.Key == DefaultCategoriesMarkerKey))
        {
            var defaults = new (string Name, string Description, UnitOfMeasurement Unit)[]
            {
                ("Clothing & Apparel", "Sarees, ethnic wear, fabrics and everyday clothing.", UnitOfMeasurement.Piece),
                ("Jewellery & Accessories", "Jewellery, bags, scarves and fashion accessories.", UnitOfMeasurement.Piece),
                ("Home & Living", "Furniture, kitchenware, bedding and household essentials.", UnitOfMeasurement.Piece),
                ("Home Decor & Gifts", "Decor, handicrafts, festive and gifting items.", UnitOfMeasurement.Piece),
                ("Food & Groceries", "Packaged foods, spices, sweets and pantry items.", UnitOfMeasurement.Pack),
                ("Beauty & Personal Care", "Skincare, haircare, wellness and grooming products.", UnitOfMeasurement.Piece),
                ("Electronics", "Gadgets, appliances and accessories.", UnitOfMeasurement.Piece),
                ("Stationery & Books", "Books, notebooks, art and office supplies.", UnitOfMeasurement.Piece),
            };

            var existing = await db.Categories.IgnoreQueryFilters().Select(c => c.Name).ToListAsync();
            foreach (var (name, description, unit) in defaults.Where(d => !existing.Contains(d.Name, StringComparer.OrdinalIgnoreCase)))
            {
                db.Categories.Add(new Category { Name = name, Description = description, UnitOfMeasurement = unit });
            }

            db.Modules.Add(new Module { Name = "Default Categories Marker", Key = DefaultCategoriesMarkerKey, IsActive = false, SortOrder = int.MaxValue });
            await db.SaveChangesAsync();
        }

        await SeedDefaultCategoryVariantsAsync(db);
    }

    // Adds default variant definitions to the default categories that have none; runs once.
    private static async Task SeedDefaultCategoryVariantsAsync(AppDbContext db)
    {
        if (await db.Modules.AnyAsync(m => m.Key == DefaultCategoryVariantsMarkerKey)) return;

        var variants = new Dictionary<string, (CategoryVariantType Type, string Name, string[] Values, bool Required)[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Clothing & Apparel"] = [(CategoryVariantType.Size, "Size", ["XS", "S", "M", "L", "XL", "XXL", "Free Size"], true), (CategoryVariantType.Color, "Color", ["Red", "Blue", "Green", "Black", "White", "Yellow", "Pink", "Maroon"], false), (CategoryVariantType.Material, "Fabric", ["Cotton", "Silk", "Linen", "Georgette", "Polyester", "Wool"], false)],
            ["Jewellery & Accessories"] = [(CategoryVariantType.Material, "Material", ["Gold Plated", "Silver", "Brass", "Oxidised", "Beads", "Leather"], false), (CategoryVariantType.Color, "Color", ["Gold", "Silver", "Black", "Multicolor"], false), (CategoryVariantType.Size, "Size", ["Small", "Medium", "Large", "Adjustable"], false)],
            ["Home & Living"] = [(CategoryVariantType.Size, "Size", ["Small", "Medium", "Large", "King", "Queen", "Single"], false), (CategoryVariantType.Color, "Color", ["White", "Beige", "Brown", "Grey", "Blue", "Green"], false), (CategoryVariantType.Material, "Material", ["Wood", "Steel", "Cotton", "Glass", "Ceramic", "Plastic"], false)],
            ["Home Decor & Gifts"] = [(CategoryVariantType.Color, "Color", ["Red", "Gold", "Blue", "Green", "Multicolor", "Natural"], false), (CategoryVariantType.Size, "Size", ["Small", "Medium", "Large"], false), (CategoryVariantType.Material, "Material", ["Wood", "Brass", "Clay", "Metal", "Fabric", "Paper"], false)],
            ["Food & Groceries"] = [(CategoryVariantType.Weight, "Pack Size", ["100 g", "250 g", "500 g", "1 kg", "2 kg", "5 kg"], true), (CategoryVariantType.Custom, "Flavour", ["Original", "Spicy", "Sweet", "Salted"], false)],
            ["Beauty & Personal Care"] = [(CategoryVariantType.Size, "Volume", ["50 ml", "100 ml", "200 ml", "500 ml"], false), (CategoryVariantType.Custom, "Type", ["Dry Skin", "Oily Skin", "Normal", "All Skin Types"], false), (CategoryVariantType.Custom, "Fragrance", ["Rose", "Sandalwood", "Lavender", "Unscented"], false)],
            ["Electronics"] = [(CategoryVariantType.Color, "Color", ["Black", "White", "Silver", "Blue", "Grey"], false), (CategoryVariantType.Custom, "Storage", ["32 GB", "64 GB", "128 GB", "256 GB", "512 GB"], false), (CategoryVariantType.Custom, "Warranty", ["6 Months", "1 Year", "2 Years"], false)],
            ["Stationery & Books"] = [(CategoryVariantType.Custom, "Format", ["Paperback", "Hardcover", "Spiral", "Ruled", "Plain"], false), (CategoryVariantType.Size, "Size", ["A3", "A4", "A5", "A6"], false), (CategoryVariantType.Color, "Color", ["Blue", "Black", "Red", "Multicolor"], false)],
        };

        var categories = await db.Categories.Include(c => c.VariantDefinitions).ToListAsync();
        foreach (var category in categories.Where(c => c.VariantDefinitions.Count == 0 && variants.ContainsKey(c.Name)))
        {
            foreach (var (type, name, values, required) in variants[category.Name])
            {
                db.CategoryVariantDefinitions.Add(new CategoryVariantDefinition
                {
                    CategoryId = category.Id,
                    Name = name,
                    Type = type,
                    ValuesJson = System.Text.Json.JsonSerializer.Serialize(values),
                    IsRequired = required,
                });
            }
        }

        db.Modules.Add(new Module { Name = "Default Category Variants Marker", Key = DefaultCategoryVariantsMarkerKey, IsActive = false, SortOrder = int.MaxValue });
        await db.SaveChangesAsync();
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
        await AddPageAsync(analytics, "Dashboard & Reports", "/dashboard", 1,
            ("View Dashboard", "section-dashboard-view"),
            ("View Profit & Loss Reports", "section-reports-view"));

        // Payment details are managed per seller (My Company); the global Payment Settings page was retired.
        var retiredPage = await db.Pages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Url == "/payment-settings");
        if (retiredPage is not null)
        {
            var retiredIds = retiredPage.Sections.Select(s => s.Id).ToList();
            db.RoleSectionAccesses.RemoveRange(await db.RoleSectionAccesses.Where(a => retiredIds.Contains(a.SectionId)).ToListAsync());
            db.Sections.RemoveRange(retiredPage.Sections);
            db.Pages.Remove(retiredPage);
        }

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
                "section-profile-view");
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
