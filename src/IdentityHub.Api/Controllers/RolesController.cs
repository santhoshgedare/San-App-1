using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Roles.Commands.CreateRole;
using IdentityHub.Application.Features.Roles.Commands.DeleteRole;
using IdentityHub.Application.Features.Roles.Queries.GetRoles;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public sealed class RolesController(ISender sender) : ControllerBase
{
    /// <summary>Lists all roles with their assigned user counts.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetRolesQuery(), ct));

    /// <summary>Lists roles with search and offset pagination for scroll-based list pages.</summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<RoleDto>>> GetPaged(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetRolesPagedQuery(search, page, pageSize), ct));

    /// <summary>Creates a new role.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateRoleCommand(request.Name), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Deletes a role.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRoleCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
