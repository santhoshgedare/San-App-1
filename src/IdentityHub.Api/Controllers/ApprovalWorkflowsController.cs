using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Approvals.Commands;
using IdentityHub.Application.Features.Approvals.Queries;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

/// <summary>
/// Configures the multi-stage approval workflow (stages + eligible roles per stage) used for
/// any entity type. Admin only — wiring a new master into the approval cycle is a config
/// change here, not a code change in the master's own module.
/// </summary>
[ApiController]
[Route("api/approval-workflows")]
[Authorize]
public sealed class ApprovalWorkflowsController(ISender sender) : ControllerBase
{
    /// <summary>Lists every configured approval workflow.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<ApprovalWorkflowDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetApprovalWorkflowsQuery(), ct));

    /// <summary>Gets the active workflow for a single entity type, if any is configured. Any authenticated user (used by the request form to know available stages).</summary>
    [HttpGet("{entityType}")]
    public async Task<ActionResult<ApprovalWorkflowDto?>> GetForEntityType(string entityType, CancellationToken ct)
        => Ok(await sender.Send(new GetApprovalWorkflowForEntityTypeQuery(entityType), ct));

    /// <summary>Creates or replaces the workflow (stages + eligible roles) for an entity type.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Save(SaveApprovalWorkflowRequestDto request, CancellationToken ct)
    {
        var stages = request.Stages.Select(s => new SaveApprovalWorkflowStageRequest(s.Name, s.Roles, s.DefaultApproverUserId)).ToList();
        var result = await sender.Send(new SaveApprovalWorkflowCommand(request.Id, request.EntityType, request.Name, request.IsActive, stages), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Deletes an approval workflow configuration.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteApprovalWorkflowCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
