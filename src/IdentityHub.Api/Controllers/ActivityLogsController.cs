using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.ActivityLogs.Queries;
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
[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
public sealed class ActivityLogsController(ISender sender) : ControllerBase
{
    /// <summary>Gets the full audit trail for a single entity instance, newest first.</summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<IReadOnlyList<ActivityLogDto>>> GetForEntity(string entityType, string entityId, CancellationToken ct)
        => Ok(await sender.Send(new GetEntityActivityQuery(entityType, entityId), ct));

    /// <summary>Gets a paged, optionally entity-filtered activity feed, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetPaged(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetActivityPagedQuery(entityType, entityId, page, pageSize), ct));
}
