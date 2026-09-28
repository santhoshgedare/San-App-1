using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Generic activity/audit log abstraction. Entries are linked to any entity via a short
/// <c>entityType</c> discriminator (e.g. "User", "Role" — see <c>Domain.Constants.EntityTypes</c>)
/// plus that entity's primary key, so the same log table can audit any part of the system.
/// </summary>
public interface IActivityLogService
{
    Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct);

    /// <summary>Gets the audit trail for a single entity instance, newest first.</summary>
    Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct);

    /// <summary>Gets a paged, optionally filtered activity feed across all entities, newest first.</summary>
    Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct);
}

/// <summary>Filter/pagination parameters for the activity log feed.</summary>
public sealed record ActivityLogQuery
{
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
