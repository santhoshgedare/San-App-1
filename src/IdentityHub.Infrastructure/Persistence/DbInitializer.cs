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
    private const string PermissionDefaultsMarkerKey = "system-role-defaults-v2";

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
        await SeedSampleItemsAsync(db);
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

    private static async Task SeedSampleItemsAsync(AppDbContext db)
    {
        if (await db.Items.AnyAsync(i => i.Code.StartsWith("SMP-")))
        {
            return;
        }

        async Task<Category> EnsureCategoryAsync(string name, string description)
        {
            var existing = await db.Categories.FirstOrDefaultAsync(c => c.Name == name);
            if (existing is not null)
            {
                return existing;
            }

            var created = new Category
            {
                Name = name,
                Description = description,
                IsActive = true,
                UnitOfMeasurement = UnitOfMeasurement.Piece
            };
            db.Categories.Add(created);
            await db.SaveChangesAsync();
            return created;
        }

        var bangles = await EnsureCategoryAsync("Bangles", "Handmade bangles and bangle sets.");
        var electronics = await EnsureCategoryAsync("Electronics & Audio", "Premium audio gear and digital accessories.");
        var apparel = await EnsureCategoryAsync("Apparel & Activewear", "Technical fabrics and modern apparel.");

        var samples = new (Guid Category, string Code, string Name, string Description, decimal Price, decimal Cost, int Stock, string[] Options)[]
        {
            (bangles.Id, "SMP-BAN-01", "Royal Maroon Silk Thread Bangles", "Silk thread bangles in deep maroon with fine gold detailing.", 450m, 220m, 40, ["2/4", "2/6", "2/8"]),
            (bangles.Id, "SMP-BAN-02", "Pearl Kundan Bridal Set", "Kundan stone bangles finished with delicate pearl accents.", 1250m, 640m, 25, ["2/4", "2/6"]),
            (bangles.Id, "SMP-BAN-03", "Emerald Green Glass Bangles", "Glossy emerald glass bangles with a subtle metallic edge.", 320m, 150m, 60, ["2/4", "2/6", "2/8"]),
            (bangles.Id, "SMP-BAN-04", "Antique Gold Polish Kada", "Wide kada with an antique gold polish and hand-set stones.", 890m, 430m, 18, []),
            (bangles.Id, "SMP-BAN-05", "Peach Blossom Thread Set", "Soft peach thread bangles with floral beadwork.", 380m, 170m, 35, ["2/4", "2/6"]),
            (bangles.Id, "SMP-BAN-06", "Midnight Blue Kundan Bangles", "Deep blue kundan bangles with gold-toned borders.", 760m, 360m, 22, ["2/4", "2/6", "2/8"]),
            (bangles.Id, "SMP-BAN-07", "Rose Quartz Beaded Bangles", "Rose quartz beads strung on a flexible band.", 540m, 250m, 30, []),
            (bangles.Id, "SMP-BAN-08", "Temple Pattern Stack Set", "A stack of six temple-pattern bangles in antique tones.", 980m, 480m, 15, ["Set of 6"]),
            (bangles.Id, "SMP-BAN-09", "Ivory Lace Thread Bangles", "Ivory thread bangles with lace-style detailing.", 290m, 130m, 50, ["2/4", "2/6"]),
            (bangles.Id, "SMP-BAN-10", "Golden Meenakari Bangles", "Colourful meenakari work on gold-toned bangles.", 1100m, 540m, 12, ["2/4", "2/6"]),
            (electronics.Id, "SMP-ELE-01", "Compact Bluetooth Speaker", "Portable speaker with clear sound and 12-hour battery.", 79m, 38m, 70, ["Black", "Blue"]),
            (electronics.Id, "SMP-ELE-02", "True Wireless Earbuds", "In-ear earbuds with a charging case.", 59m, 27m, 90, ["White", "Black"]),
            (electronics.Id, "SMP-ELE-03", "Fast Charge Power Bank 20000mAh", "High-capacity power bank with dual USB output.", 45m, 21m, 110, []),
            (electronics.Id, "SMP-ELE-04", "Studio Monitor Headphones", "Wired over-ear headphones tuned for accurate sound.", 129m, 64m, 40, []),
            (electronics.Id, "SMP-ELE-05", "Smart Fitness Band", "Activity tracker with heart-rate and sleep monitoring.", 49m, 22m, 80, ["Black", "Teal"]),
            (electronics.Id, "SMP-ELE-06", "USB-C Braided Cable 2m", "Durable braided charging cable.", 12m, 4m, 200, []),
            (apparel.Id, "SMP-APP-01", "Everyday Cotton Tee", "Soft midweight cotton tee with a relaxed fit.", 24m, 9m, 150, ["S", "M", "L", "XL"]),
            (apparel.Id, "SMP-APP-02", "Performance Running Shorts", "Lightweight quick-dry shorts with a zip pocket.", 34m, 14m, 100, ["S", "M", "L"]),
            (apparel.Id, "SMP-APP-03", "Fleece Zip Hoodie", "Warm fleece hoodie with a full zip.", 69m, 31m, 60, ["M", "L", "XL"]),
            (apparel.Id, "SMP-APP-04", "Trail Hiking Pants", "Stretch hiking pants with water-resistant finish.", 84m, 38m, 45, ["S", "M", "L"]),
            (apparel.Id, "SMP-APP-05", "Merino Base Layer", "Breathable merino wool base layer.", 74m, 35m, 55, []),
            (apparel.Id, "SMP-APP-06", "Packable Rain Jacket", "Ultralight rain jacket that folds into its pocket.", 99m, 46m, 38, ["M", "L"]),
        };

        foreach (var s in samples)
        {
            var item = new Item
            {
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                CategoryId = s.Category,
                UnitOfMeasurement = UnitOfMeasurement.Piece,
                Price = s.Price,
                CostPrice = s.Cost,
                StockQuantity = s.Stock,
                IsActive = true,
                Images =
                [
                    new ItemImage
                    {
                        Url = $"https://picsum.photos/seed/{s.Code.ToLowerInvariant()}/800/800",
                        FileName = $"{s.Code.ToLowerInvariant()}.jpg",
                        Caption = s.Name,
                        IsPrimary = true,
                        SortOrder = 1
                    }
                ]
            };

            if (s.Options.Length > 1)
            {
                var perVariant = Math.Max(1, s.Stock / s.Options.Length);
                item.Variants = s.Options.Select((option, index) => new ItemVariant
                {
                    Sku = $"{s.Code}-{index + 1:00}",
                    Name = option,
                    AttributesJson = $"{{\"Option\":\"{option}\"}}",
                    Price = s.Price,
                    CostPrice = s.Cost,
                    StockQuantity = perVariant,
                    IsActive = true
                }).ToList();
            }

            db.Items.Add(item);
        }

        await db.SaveChangesAsync();
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

        if (shouldSeedRoleDefaults)
        {
            await GrantMissingAsync(Roles.Manager,
                "section-dashboard-view", "section-reports-view",
                "section-catalog-view", "section-cart-view", "section-checkout-create",
                "section-orders-view", "section-orders-manage", "section-orders-payment", "section-orders-refund",
                "section-categories-view", "section-categories-manage", "section-items-view", "section-items-manage",
                "section-users-view", "section-profile-view",
                "section-payment-settings-view", "section-payment-settings-manage");
            await GrantMissingAsync(Roles.User,
                "section-catalog-view", "section-cart-view", "section-checkout-create",
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
