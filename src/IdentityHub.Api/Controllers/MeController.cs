using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Api.Contracts;
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

    /// <summary>Lets the signed-in user update their own name, email and phone number.</summary>
    [HttpPut]
    public async Task<ActionResult<UserDto>> Update(UpdateProfileRequest request, [FromServices] IIdentityService identityService, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return BadRequest(new { errors = new[] { "First name is required." } });
        }
        if (!string.IsNullOrWhiteSpace(request.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email))
        {
            return BadRequest(new { errors = new[] { "Enter a valid email address." } });
        }
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(request.PhoneNumber, @"^[+\d][\d\s-]{6,19}$"))
        {
            return BadRequest(new { errors = new[] { "Enter a valid phone number." } });
        }

        var result = await identityService.UpdateOwnProfileAsync(userId, request.FirstName, request.LastName ?? string.Empty, request.Email, request.PhoneNumber, ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
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
