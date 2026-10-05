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

public sealed class PasswordRecoveryTests
{
    [Fact]
    public async Task Valid_reset_token_changes_password_and_cannot_be_reused()
    {
        await using var provider = CreateIdentityProvider();
        await using var scope = provider.CreateAsyncScope();
        var (identityService, userManager) = await CreateIdentityService(scope.ServiceProvider);
        var user = new ApplicationUser
        {
            UserName = "reset@example.com",
            Email = "reset@example.com",
            EmailConfirmed = true,
            FirstName = "Reset",
            LastName = "User"
        };
        (await userManager.CreateAsync(user, "InitialPass123!")).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(user, Roles.User)).Succeeded.Should().BeTrue();

        var token = await identityService.GeneratePasswordResetTokenAsync(user.Email!, CancellationToken.None);
        token.Should().NotBeNullOrWhiteSpace();
        var reset = await identityService.ResetPasswordAsync(user.Email!, token!, "ReplacementPass123!", CancellationToken.None);
        var reusedToken = await identityService.ResetPasswordAsync(user.Email!, token!, "AnotherPass123!", CancellationToken.None);

        reset.Succeeded.Should().BeTrue();
        reusedToken.Succeeded.Should().BeFalse();
        (await userManager.CheckPasswordAsync(user, "ReplacementPass123!")).Should().BeTrue();
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
        (await roleManager.CreateAsync(new ApplicationRole(Roles.User))).Succeeded.Should().BeTrue();
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