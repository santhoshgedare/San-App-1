using System.Security.Claims;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityHub.Api.Authorization;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireSectionAttribute : TypeFilterAttribute
{
    public RequireSectionAttribute(string sectionKey) : base(typeof(RequireSectionFilter))
    {
        Arguments = [sectionKey];
    }
}

public sealed class RequireSectionFilter(
    string sectionKey,
    IModuleAccessService moduleAccess) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (user.IsInRole(Roles.Admin)) return;

        var roleNames = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sections = await moduleAccess.GetSectionKeysForRolesAsync(roleNames, context.HttpContext.RequestAborted);

        if (!sections.Contains(sectionKey, StringComparer.Ordinal))
        {
            context.Result = new ForbidResult();
        }
    }
}