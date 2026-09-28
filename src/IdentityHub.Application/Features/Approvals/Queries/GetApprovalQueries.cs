using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Approvals.Queries;

/// <summary>Gets all approval requests for a single entity instance, newest first.</summary>
public sealed record GetEntityApprovalsQuery(string EntityType, string EntityId) : IRequest<IReadOnlyList<ApprovalDto>>;

public sealed class GetEntityApprovalsQueryHandler(IApprovalService approvalService)
    : IRequestHandler<GetEntityApprovalsQuery, IReadOnlyList<ApprovalDto>>
{
    public Task<IReadOnlyList<ApprovalDto>> Handle(GetEntityApprovalsQuery request, CancellationToken cancellationToken)
        => approvalService.GetForEntityAsync(request.EntityType, request.EntityId, cancellationToken);
}

/// <summary>Gets a paged, optionally filtered approval feed across all entities, newest first.</summary>
public sealed record GetApprovalsPagedQuery(string? EntityType, string? EntityId, string? Status, int Page, int PageSize)
    : IRequest<PagedResult<ApprovalDto>>;

public sealed class GetApprovalsPagedQueryHandler(IApprovalService approvalService)
    : IRequestHandler<GetApprovalsPagedQuery, PagedResult<ApprovalDto>>
{
    public Task<PagedResult<ApprovalDto>> Handle(GetApprovalsPagedQuery request, CancellationToken cancellationToken)
        => approvalService.GetPagedAsync(
            new ApprovalQuery
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Status = request.Status,
                Page = request.Page,
                PageSize = request.PageSize
            },
            cancellationToken);
}
