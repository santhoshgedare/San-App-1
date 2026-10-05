using FluentAssertions;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Infrastructure.Identity;
using IdentityHub.Infrastructure.Persistence;
using IdentityHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityHub.Application.Tests.Features;

public sealed class ExternalIdentityTests
{
    [Fact]
    public async Task Creates_external_account_with_default_user_role_and_provider_login()
    {
        await using var provider = CreateIdentityProvider();
        await using var scope = provider.CreateAsyncScope();
        var (identityService, userManager) = await CreateIdentityService(scope.ServiceProvider);

        var result = await identityService.AuthenticateExternalAsync(
            "Google", "google-user-1", "social@example.com", "Social", "User", true, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Email.Should().Be("social@example.com");
        result.Data.PhoneNumber.Should().BeNull();
        result.Data.Roles.Should().ContainSingle().Which.Should().Be(Roles.User);
        var user = await userManager.FindByEmailAsync("social@example.com");
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeTrue();
        (await userManager.GetLoginsAsync(user)).Should().ContainSingle(login =>
            login.LoginProvider == "Google" && login.ProviderKey == "google-user-1");
    }

    [Fact]
    public async Task Links_verified_google_identity_but_rejects_unverified_facebook_email_match()
    {
        await using var provider = CreateIdentityProvider();
        await using var scope = provider.CreateAsyncScope();
        var (identityService, userManager) = await CreateIdentityService(scope.ServiceProvider);
        var existing = new ApplicationUser
        {
            UserName = "member@example.com",
            Email = "member@example.com",
            EmailConfirmed = true,
            FirstName = "Existing",
            LastName = "Member"
        };
        (await userManager.CreateAsync(existing, "Password123!")).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(existing, Roles.User)).Succeeded.Should().BeTrue();

        var unverified = await identityService.AuthenticateExternalAsync(
            "Facebook", "facebook-user-2", "member@example.com", "Existing", "Member", false, CancellationToken.None);
        unverified.Succeeded.Should().BeFalse();
        (await userManager.GetLoginsAsync(existing)).Should().BeEmpty();

        var verified = await identityService.AuthenticateExternalAsync(
            "Google", "google-user-2", "member@example.com", "Existing", "Member", true, CancellationToken.None);

        verified.Succeeded.Should().BeTrue();
        verified.Data!.Id.Should().Be(existing.Id);
        (await userManager.GetLoginsAsync(existing)).Should().ContainSingle(login => login.LoginProvider == "Google");
    }

    private static ServiceProvider CreateIdentityProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        return services.BuildServiceProvider();
    }

    private static async Task<(IdentityService Service, UserManager<ApplicationUser> UserManager)> CreateIdentityService(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await roleManager.CreateAsync(new ApplicationRole(Roles.User));
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        return (new IdentityService(userManager, roleManager, new StubActivityLogService()), userManager);
    }

    private sealed class StubActivityLogService : IActivityLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ActivityLogDto>>([]);
        public Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct) =>
            Task.FromResult(new PagedResult<ActivityLogDto>());
    }
}