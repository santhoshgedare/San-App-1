using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ActivityLogs.Queries;

/// <summary>Gets the full audit trail for a single entity instance, newest first.</summary>
public sealed record GetEntityActivityQuery(string EntityType, string EntityId) : IRequest<IReadOnlyList<ActivityLogDto>>;

public sealed class GetEntityActivityQueryHandler(IActivityLogService activityLogService)
    : IRequestHandler<GetEntityActivityQuery, IReadOnlyList<ActivityLogDto>>
{
    public Task<IReadOnlyList<ActivityLogDto>> Handle(GetEntityActivityQuery request, CancellationToken cancellationToken)
        => activityLogService.GetForEntityAsync(request.EntityType, request.EntityId, cancellationToken);
}

/// <summary>Gets a paged, optionally filtered activity feed across all entities, newest first.</summary>
public sealed record GetActivityPagedQuery(string? EntityType, string? EntityId, int Page, int PageSize)
    : IRequest<PagedResult<ActivityLogDto>>;

public sealed class GetActivityPagedQueryHandler(IActivityLogService activityLogService)
    : IRequestHandler<GetActivityPagedQuery, PagedResult<ActivityLogDto>>
{
    public Task<PagedResult<ActivityLogDto>> Handle(GetActivityPagedQuery request, CancellationToken cancellationToken)
        => activityLogService.GetPagedAsync(
            new ActivityLogQuery
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Page = request.Page,
                PageSize = request.PageSize
            },
            cancellationToken);
}
