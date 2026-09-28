using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Users.Commands.AssignRoles;
using IdentityHub.Application.Features.Users.Commands.CreateUser;
using IdentityHub.Application.Features.Users.Commands.DeleteUser;
using IdentityHub.Application.Features.Users.Commands.UpdateUser;
using IdentityHub.Application.Features.Users.Queries.GetUserById;
using IdentityHub.Application.Features.Users.Queries.GetUsers;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Lists all users. Admin/Manager only.</summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetUsersQuery(), ct));

    /// <summary>
    /// Lists users with search, role/status filters, and offset pagination for scroll-based list pages.
    /// </summary>
    [HttpGet("paged")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetUsersPagedQuery(search, role, isActive, page, pageSize), ct));


    /// <summary>Gets a single user by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Creates a user (admin-provisioned account, no auth tokens issued). Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateUserCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Roles), ct);
        return result.Succeeded ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates a user's profile and active status. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateUserCommand(id, request.FirstName, request.LastName, request.IsActive), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Deletes a user. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteUserCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Replaces a user's role assignments. Admin only.</summary>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> AssignRoles(Guid id, AssignRolesRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AssignRolesCommand(id, request.Roles), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
