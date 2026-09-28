using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.ModuleAccess.Queries.GetMySections;
using IdentityHub.Application.Features.Users.Queries.GetUserById;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MeController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet]
    public async Task<ActionResult<UserDto>> Get(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var user = await sender.Send(new GetUserByIdQuery(userId), ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Returns the section keys the current user can access (union across their roles), used by the
    /// Angular <c>canRender</c> directive to decide what to show/hide.
    /// </summary>
    [HttpGet("sections")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetSections(CancellationToken ct)
    {
        var isAdmin = currentUser.IsInRole(Roles.Admin);
        var sections = await sender.Send(new GetMySectionsQuery(currentUser.Roles, isAdmin), ct);
        return Ok(sections);
    }
}
