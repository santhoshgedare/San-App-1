using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityHub.Infrastructure.Persistence;

/// <summary>
/// Seeds default roles, an initial administrator account, and the Module &gt; Page &gt; Section
/// master hierarchy (with default role access) on startup.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        const string adminEmail = "admin@identityhub.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@12345");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }

        await SeedModuleAccessAsync(db, roleManager);
        await SeedCatalogDataAsync(db);
    }

    private static async Task SeedCatalogDataAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var electronicsCategory = new Category
        {
            Name = "Electronics & Audio",
            Description = "Premium high-fidelity audio gear and digital accessories.",
            IsActive = true,
            UnitOfMeasurement = UnitOfMeasurement.Piece,
            VariantDefinitions =
            [
                new CategoryVariantDefinition
                {
                    Name = "Finish",
                    Type = CategoryVariantType.Color,
                    ValuesJson = "[\"Space Gray\",\"Matte Black\",\"Silver\"]",
                    IsRequired = true
                },
                new CategoryVariantDefinition
                {
                    Name = "Edition",
                    Type = CategoryVariantType.Style,
                    ValuesJson = "[\"Standard\",\"Pro Wireless\"]",
                    IsRequired = false
                }
            ]
        };

        var apparelCategory = new Category
        {
            Name = "Apparel & Activewear",
            Description = "Engineered technical fabrics and modern apparel.",
            IsActive = true,
            UnitOfMeasurement = UnitOfMeasurement.Piece,
            VariantDefinitions =
            [
                new CategoryVariantDefinition
                {
                    Name = "Color",
                    Type = CategoryVariantType.Color,
                    ValuesJson = "[\"Midnight Blue\",\"Charcoal\",\"Forest Green\"]",
                    IsRequired = true
                },
                new CategoryVariantDefinition
                {
                    Name = "Size",
                    Type = CategoryVariantType.Size,
                    ValuesJson = "[\"S\",\"M\",\"L\",\"XL\"]",
                    IsRequired = true
                }
            ]
        };

        db.Categories.AddRange(electronicsCategory, apparelCategory);
        await db.SaveChangesAsync();

        var headphoneItem = new Item
        {
            Code = "AUD-PRO-01",
            Name = "Acoustic Pro Wireless ANC Headphones",
            Description = "Studio-grade wireless over-ear headphones with hybrid active noise cancellation, 40-hour battery life, and spatial audio drivers.",
            Barcode = "8901234567890",
            CategoryId = electronicsCategory.Id,
            UnitOfMeasurement = UnitOfMeasurement.Piece,
            Price = 249.99m,
            CostPrice = 140.00m,
            StockQuantity = 85,
            IsActive = true,
            Images =
            [
                new ItemImage
                {
                    Url = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80",
                    FileName = "headphones-main.jpg",
                    Caption = "Acoustic Pro Headphones Front View",
                    IsPrimary = true,
                    SortOrder = 1
                },
                new ItemImage
                {
                    Url = "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=800&auto=format&fit=crop&q=80",
                    FileName = "headphones-side.jpg",
                    Caption = "Acoustic Pro Headphones Side Profile",
                    IsPrimary = false,
                    SortOrder = 2
                }
            ],
            Documents =
            [
                new ItemDocument
                {
                    Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                    FileName = "User_Manual_and_Safety_Guide.pdf",
                    DocumentType = "Manual",
                    FileSizeBytes = 2450000,
                    Description = "Official Acoustic Pro User Manual and Warranty Information"
                },
                new ItemDocument
                {
                    Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                    FileName = "Technical_Data_Sheet.pdf",
                    DocumentType = "SpecSheet",
                    FileSizeBytes = 820000,
                    Description = "Frequency response curve, driver specs, and RF compliance"
                }
            ],
            Variants =
            [
                new ItemVariant
                {
                    Sku = "AUD-PRO-01-GRY-STD",
                    Name = "Space Gray / Standard",
                    AttributesJson = "{\"Finish\":\"Space Gray\",\"Edition\":\"Standard\"}",
                    Price = 249.99m,
                    CostPrice = 140.00m,
                    StockQuantity = 30,
                    IsActive = true
                },
                new ItemVariant
                {
                    Sku = "AUD-PRO-01-BLK-PRO",
                    Name = "Matte Black / Pro Wireless",
                    AttributesJson = "{\"Finish\":\"Matte Black\",\"Edition\":\"Pro Wireless\"}",
                    Price = 299.99m,
                    CostPrice = 165.00m,
                    StockQuantity = 35,
                    IsActive = true
                },
                new ItemVariant
                {
                    Sku = "AUD-PRO-01-SLV-PRO",
                    Name = "Silver / Pro Wireless",
                    AttributesJson = "{\"Finish\":\"Silver\",\"Edition\":\"Pro Wireless\"}",
                    Price = 299.99m,
                    CostPrice = 165.00m,
                    StockQuantity = 20,
                    IsActive = true
                }
            ]
        };

        var jacketItem = new Item
        {
            Code = "APP-JKT-02",
            Name = "All-Weather Technical Softshell Jacket",
            Description = "Waterproof, wind-resistant breathable membrane jacket designed for alpine conditions and urban commuting.",
            Barcode = "8909876543210",
            CategoryId = apparelCategory.Id,
            UnitOfMeasurement = UnitOfMeasurement.Piece,
            Price = 189.00m,
            CostPrice = 90.00m,
            StockQuantity = 120,
            IsActive = true,
            Images =
            [
                new ItemImage
                {
                    Url = "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=800&auto=format&fit=crop&q=80",
                    FileName = "jacket-front.jpg",
                    Caption = "All-Weather Jacket Midnight Blue",
                    IsPrimary = true,
                    SortOrder = 1
                },
                new ItemImage
                {
                    Url = "https://images.unsplash.com/photo-1544441893-675973e31985?w=800&auto=format&fit=crop&q=80",
                    FileName = "jacket-lifestyle.jpg",
                    Caption = "Technical Softshell In Action",
                    IsPrimary = false,
                    SortOrder = 2
                }
            ],
            Documents =
            [
                new ItemDocument
                {
                    Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                    FileName = "Fabric_Care_and_Sizing_Guide.pdf",
                    DocumentType = "Manual",
                    FileSizeBytes = 540000,
                    Description = "Garment care instructions and anatomical sizing chart"
                }
            ],
            Variants =
            [
                new ItemVariant
                {
                    Sku = "APP-JKT-02-BLU-M",
                    Name = "Midnight Blue / M",
                    AttributesJson = "{\"Color\":\"Midnight Blue\",\"Size\":\"M\"}",
                    Price = 189.00m,
                    CostPrice = 90.00m,
                    StockQuantity = 40,
                    IsActive = true
                },
                new ItemVariant
                {
                    Sku = "APP-JKT-02-BLU-L",
                    Name = "Midnight Blue / L",
                    AttributesJson = "{\"Color\":\"Midnight Blue\",\"Size\":\"L\"}",
                    Price = 189.00m,
                    CostPrice = 90.00m,
                    StockQuantity = 40,
                    IsActive = true
                },
                new ItemVariant
                {
                    Sku = "APP-JKT-02-CHR-XL",
                    Name = "Charcoal / XL",
                    AttributesJson = "{\"Color\":\"Charcoal\",\"Size\":\"XL\"}",
                    Price = 199.00m,
                    CostPrice = 95.00m,
                    StockQuantity = 25,
                    IsActive = true
                },
                new ItemVariant
                {
                    Sku = "APP-JKT-02-GRN-L",
                    Name = "Forest Green / L",
                    AttributesJson = "{\"Color\":\"Forest Green\",\"Size\":\"L\"}",
                    Price = 189.00m,
                    CostPrice = 90.00m,
                    StockQuantity = 15,
                    IsActive = true
                }
            ]
        };

        db.Items.AddRange(headphoneItem, jacketItem);
        await db.SaveChangesAsync();
    }

    private static async Task SeedModuleAccessAsync(AppDbContext db, RoleManager<ApplicationRole> roleManager)
    {
        if (await db.Modules.AnyAsync())
        {
            return;
        }

        var userAccessModule = new Module { Name = "User Access Configuration", Key = "module-user-access", SortOrder = 1 };
        var accountModule = new Module { Name = "Account", Key = "module-account", SortOrder = 2 };

        var usersPage = new Page { Module = userAccessModule, Name = "Users", Url = "/users", SortOrder = 1 };
        var rolesPage = new Page { Module = userAccessModule, Name = "Roles", Url = "/roles", SortOrder = 2 };
        var profilePage = new Page { Module = accountModule, Name = "My Profile", Url = "/profile", SortOrder = 1 };

        var usersView = new Section { Page = usersPage, Name = "View Users", Key = "section-users-view", SortOrder = 1 };
        var usersEditRoles = new Section { Page = usersPage, Name = "Edit User Roles", Key = "section-users-edit-roles", SortOrder = 2 };
        var usersDeactivate = new Section { Page = usersPage, Name = "Deactivate/Delete Users", Key = "section-users-manage", SortOrder = 3 };
        var rolesView = new Section { Page = rolesPage, Name = "View Roles", Key = "section-roles-view", SortOrder = 1 };
        var rolesManage = new Section { Page = rolesPage, Name = "Create/Delete Roles", Key = "section-roles-manage", SortOrder = 2 };
        var profileView = new Section { Page = profilePage, Name = "View Profile", Key = "section-profile-view", SortOrder = 1 };

        db.Modules.AddRange(userAccessModule, accountModule);
        db.Pages.AddRange(usersPage, rolesPage, profilePage);
        db.Sections.AddRange(usersView, usersEditRoles, usersDeactivate, rolesView, rolesManage, profileView);
        await db.SaveChangesAsync();

        var managerRole = await roleManager.FindByNameAsync(Roles.Manager);
        if (managerRole is not null)
        {
            db.RoleSectionAccesses.AddRange(
                new RoleSectionAccess { RoleId = managerRole.Id, SectionId = usersView.Id },
                new RoleSectionAccess { RoleId = managerRole.Id, SectionId = usersEditRoles.Id },
                new RoleSectionAccess { RoleId = managerRole.Id, SectionId = profileView.Id });
        }

        var userRole = await roleManager.FindByNameAsync(Roles.User);
        if (userRole is not null)
        {
            db.RoleSectionAccesses.Add(new RoleSectionAccess { RoleId = userRole.Id, SectionId = profileView.Id });
        }

        await db.SaveChangesAsync();
    }
}
