using System.Security.Claims;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.ActivityLogs.Queries;
using IdentityHub.Application.Features.Orders.Queries.GetOrderById;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

/// <summary>
/// Read-only audit trail surface. Entries are linked to any entity via an
/// (entityType, entityId) pair rather than per-entity endpoints, so this single
/// controller serves the activity history for Users, Roles, and any future entity.
/// </summary>
[ApiController]
[Route("api/activity-logs")]
[Authorize]
public sealed class ActivityLogsController(ISender sender) : ControllerBase
{
    /// <summary>Gets the full audit trail for a single entity instance, newest first.</summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<IReadOnlyList<ActivityLogDto>>> GetForEntity(string entityType, string entityId, CancellationToken ct)
    {
        var isPrivileged = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);
        if (isPrivileged)
        {
            return Ok(await sender.Send(new GetEntityActivityQuery(entityType, entityId), ct));
        }

        if (!string.Equals(entityType, EntityTypes.Order, StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(entityId, out var orderId))
        {
            return Forbid();
        }

        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return Unauthorized();

        var order = await sender.Send(new GetOrderByIdQuery(orderId), ct);
        if (order is null) return NotFound();
        if (order.CustomerId != currentUserId) return Forbid();

        var entries = await sender.Send(new GetEntityActivityQuery(EntityTypes.Order, orderId.ToString()), ct);
        return Ok(entries.Select(entry => new ActivityLogDto
        {
            Id = entry.Id,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Action = entry.Action,
            Details = entry.Details,
            CreatedAt = entry.CreatedAt
        }).ToArray());
    }

    /// <summary>Gets a paged, optionally entity-filtered activity feed, newest first.</summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetPaged(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetActivityPagedQuery(entityType, entityId, page, pageSize), ct));

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
