using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

/// <summary>
/// Common audit-trail writer/reader. Every entry is linked to an entity purely by
/// (entityType, entityId) so any part of the system can be audited without new tables.
/// </summary>
public sealed class ActivityLogService(AppDbContext db, ICurrentUserService currentUser) : IActivityLogService
{
    public async Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct)
    {
        db.ActivityLogs.Add(new ActivityLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Details = details,
            PerformedByUserId = currentUser.UserId,
            PerformedByEmail = currentUser.Email
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct)
    {
        return await db.ActivityLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => ToDto(a))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var logsQuery = db.ActivityLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            logsQuery = logsQuery.Where(a => a.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            logsQuery = logsQuery.Where(a => a.EntityId == query.EntityId);
        }

        var totalCount = await logsQuery.CountAsync(ct);
        var pageOfLogs = await logsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => ToDto(a))
            .ToListAsync(ct);

        return new PagedResult<ActivityLogDto>
        {
            Items = pageOfLogs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static ActivityLogDto ToDto(ActivityLog a) => new()
    {
        Id = a.Id,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        Action = a.Action,
        Details = a.Details,
        PerformedByUserId = a.PerformedByUserId,
        PerformedByEmail = a.PerformedByEmail,
        CreatedAt = a.CreatedAt
    };
}
