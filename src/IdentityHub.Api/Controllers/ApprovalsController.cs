using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Approvals.Commands;
using IdentityHub.Application.Features.Approvals.Queries;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

/// <summary>
/// Common approval-workflow surface. Requests are linked to any entity via an
/// (entityType, entityId) pair rather than per-entity endpoints, so this single controller
/// can be reused to request/approve/reject changes for Users, Roles, and any future module.
/// </summary>
[ApiController]
[Route("api/approvals")]
[Authorize]
public sealed class ApprovalsController(ISender sender) : ControllerBase
{
    /// <summary>Raises a new approval request against any entity.</summary>
    [HttpPost]
    public async Task<ActionResult<ApprovalDto>> RequestApproval(RequestApprovalRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await sender.Send(new RequestApprovalCommand(request.EntityType, request.EntityId, request.Title, request.Details), ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new[] { ex.Message } });
        }
    }

    /// <summary>Approves a pending approval request.</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApprovalDto>> Approve(Guid id, ApprovalDecisionRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await sender.Send(new ApproveApprovalCommand(id, request.Comment), ct));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new[] { ex.Message } });
        }
    }

    /// <summary>Rejects a pending approval request.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApprovalDto>> Reject(Guid id, ApprovalDecisionRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await sender.Send(new RejectApprovalCommand(id, request.Comment), ct));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new[] { ex.Message } });
        }
    }

    /// <summary>Reassigns the approver for a not-yet-decided stage (the pencil-icon reassign action).</summary>
    [HttpPost("{id:guid}/reassign")]
    public async Task<ActionResult<ApprovalDto>> Reassign(Guid id, ReassignApprovalStageRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await sender.Send(new ReassignApprovalStageCommand(id, request.StageIndex, request.NewApproverUserId), ct));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new[] { ex.Message } });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { errors = new[] { ex.Message } });
        }
    }

    /// <summary>Gets all approval requests for a single entity instance, newest first.</summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalDto>>> GetForEntity(string entityType, string entityId, CancellationToken ct)
        => Ok(await sender.Send(new GetEntityApprovalsQuery(entityType, entityId), ct));

    /// <summary>Gets a paged, optionally filtered approval feed, newest first.</summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PagedResult<ApprovalDto>>> GetPaged(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetApprovalsPagedQuery(entityType, entityId, status, page, pageSize), ct));
}
