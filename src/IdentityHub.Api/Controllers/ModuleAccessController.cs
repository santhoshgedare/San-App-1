using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.ModuleAccess.Commands.CreateModule;
using IdentityHub.Application.Features.ModuleAccess.Commands.CreatePage;
using IdentityHub.Application.Features.ModuleAccess.Commands.CreateSection;
using IdentityHub.Application.Features.ModuleAccess.Commands.DeleteModule;
using IdentityHub.Application.Features.ModuleAccess.Commands.DeletePage;
using IdentityHub.Application.Features.ModuleAccess.Commands.DeleteSection;
using IdentityHub.Application.Features.ModuleAccess.Commands.SetRoleAccess;
using IdentityHub.Application.Features.ModuleAccess.Queries.GetModuleTree;
using IdentityHub.Application.Features.ModuleAccess.Queries.GetRoleAccess;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

/// <summary>
/// Manages the Module &gt; Page &gt; Section master hierarchy and per-role section access,
/// the backing data for the settings navigator and the client-side <c>canRender</c> directive.
/// </summary>
[ApiController]
[Route("api/module-access")]
[Authorize]
public sealed class ModuleAccessController(ISender sender) : ControllerBase
{
    /// <summary>Returns the full Module &gt; Page &gt; Section tree.</summary>
    [HttpGet("tree")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ModuleDto>>> GetTree(CancellationToken ct)
        => Ok(await sender.Send(new GetModuleTreeQuery(), ct));

    [HttpPost("modules")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateModule(CreateModuleRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateModuleCommand(request.Name, request.Key, request.SortOrder), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("modules/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteModule(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteModuleCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("pages")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreatePage(CreatePageRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePageCommand(request.ModuleId, request.Name, request.Url, request.SortOrder), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("pages/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeletePage(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeletePageCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("sections")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateSection(CreateSectionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateSectionCommand(request.PageId, request.Name, request.Key, request.SortOrder), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("sections/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteSection(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteSectionCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Lists every role together with the section keys it currently has access to.</summary>
    [HttpGet("role-access")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<RoleAccessDto>>> GetAllRoleAccess(CancellationToken ct)
        => Ok(await sender.Send(new GetAllRoleAccessQuery(), ct));

    /// <summary>Gets the section access granted to a single role.</summary>
    [HttpGet("role-access/{roleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoleAccessDto>> GetRoleAccess(Guid roleId, CancellationToken ct)
    {
        var result = await sender.Send(new GetRoleAccessByIdQuery(roleId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Replaces the full set of section keys a role is granted access to.</summary>
    [HttpPut("role-access/{roleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetRoleAccess(Guid roleId, SetRoleAccessRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SetRoleAccessCommand(roleId, request.SectionKeys), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
