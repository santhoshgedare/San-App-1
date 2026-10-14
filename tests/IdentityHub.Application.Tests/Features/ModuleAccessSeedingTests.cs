using FluentAssertions;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Identity;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityHub.Application.Tests.Features;

public sealed class ModuleAccessSeedingTests
{
    [Fact]
    public async Task Seeds_all_pages_once_and_preserves_permission_revocations_on_restart()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SystemAdmin:Email"] = "info@srividika.com"
            })
            .Build());
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await DbInitializer.SeedAsync(scope.ServiceProvider);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("info@srividika.com");
        admin.Should().NotBeNull();
        var defaultAdmin = admin ?? throw new InvalidOperationException("The default administrator was not seeded.");
        (await userManager.IsInRoleAsync(defaultAdmin, Roles.Admin)).Should().BeTrue();
        (await userManager.IsInRoleAsync(defaultAdmin, Roles.Manager)).Should().BeFalse();
        (await db.SellerProfiles.CountAsync()).Should().Be(0);
        (await db.Categories.CountAsync()).Should().Be(8);
        (await db.Items.CountAsync()).Should().Be(0);

        var expectedRoutes = new[]
        {
            "/catalog", "/cart", "/checkout", "/orders", "/custom-requests", "/categories", "/items",
            "/users", "/roles", "/approval-workflows", "/profile", "/dashboard"
        };
        var seededRoutes = await db.Pages.Select(page => page.Url).ToListAsync();
        seededRoutes.Should().Contain(expectedRoutes);
        seededRoutes.Should().OnlyHaveUniqueItems();

        var manager = await scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>()
            .FindByNameAsync(Roles.Manager);
        manager.Should().NotBeNull();
        var managerRole = manager ?? throw new InvalidOperationException("The default Manager role was not seeded.");
        var itemManagementSection = await db.Sections.SingleAsync(section => section.Key == "section-items-manage");
        var managerGrant = await db.RoleSectionAccesses.SingleAsync(access =>
            access.RoleId == managerRole.Id && access.SectionId == itemManagementSection.Id);
        db.RoleSectionAccesses.Remove(managerGrant);
        var unseededItem = new Item
        {
            Code = "CUSTOM-UNASSIGNED",
            Name = "Manually added item"
        };
        db.Items.Add(unseededItem);
        await db.SaveChangesAsync();

        await DbInitializer.SeedAsync(scope.ServiceProvider);

        (await db.SellerProfiles.CountAsync()).Should().Be(0);
        (await db.Items.SingleAsync(item => item.Id == unseededItem.Id)).SellerId.Should().BeNull();
        (await db.Pages.CountAsync()).Should().Be(expectedRoutes.Length);
        (await db.RoleSectionAccesses.AnyAsync(access =>
            access.RoleId == managerRole.Id && access.SectionId == itemManagementSection.Id)).Should().BeFalse();
        (await db.Modules.AnyAsync(module => module.Key == "system-role-defaults-v2" && !module.IsActive)).Should().BeTrue();
    }
}