using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Approvals.Queries;

/// <summary>Lists every configured approval workflow.</summary>
public sealed record GetApprovalWorkflowsQuery : IRequest<IReadOnlyList<ApprovalWorkflowDto>>;

public sealed class GetApprovalWorkflowsQueryHandler(IApprovalWorkflowService workflowService)
    : IRequestHandler<GetApprovalWorkflowsQuery, IReadOnlyList<ApprovalWorkflowDto>>
{
    public Task<IReadOnlyList<ApprovalWorkflowDto>> Handle(GetApprovalWorkflowsQuery request, CancellationToken cancellationToken)
        => workflowService.GetAllAsync(cancellationToken);
}

/// <summary>Gets the active approval workflow configured for an entity type, if any.</summary>
public sealed record GetApprovalWorkflowForEntityTypeQuery(string EntityType) : IRequest<ApprovalWorkflowDto?>;

public sealed class GetApprovalWorkflowForEntityTypeQueryHandler(IApprovalWorkflowService workflowService)
    : IRequestHandler<GetApprovalWorkflowForEntityTypeQuery, ApprovalWorkflowDto?>
{
    public Task<ApprovalWorkflowDto?> Handle(GetApprovalWorkflowForEntityTypeQuery request, CancellationToken cancellationToken)
        => workflowService.GetForEntityTypeAsync(request.EntityType, cancellationToken);
}
