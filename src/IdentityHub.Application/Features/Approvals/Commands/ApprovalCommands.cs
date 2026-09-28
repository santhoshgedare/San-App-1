using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Approvals.Commands;

/// <summary>Raises a new approval request against any entity, identified generically by (entityType, entityId).</summary>
public sealed record RequestApprovalCommand(string EntityType, string EntityId, string Title, string? Details) : IRequest<ApprovalDto>;

public sealed class RequestApprovalCommandHandler(IApprovalService approvalService)
    : IRequestHandler<RequestApprovalCommand, ApprovalDto>
{
    public Task<ApprovalDto> Handle(RequestApprovalCommand request, CancellationToken cancellationToken)
        => approvalService.RequestAsync(request.EntityType, request.EntityId, request.Title, request.Details, cancellationToken);
}

/// <summary>Approves a pending approval request.</summary>
public sealed record ApproveApprovalCommand(Guid ApprovalId, string? Comment) : IRequest<ApprovalDto>;

public sealed class ApproveApprovalCommandHandler(IApprovalService approvalService)
    : IRequestHandler<ApproveApprovalCommand, ApprovalDto>
{
    public Task<ApprovalDto> Handle(ApproveApprovalCommand request, CancellationToken cancellationToken)
        => approvalService.ApproveAsync(request.ApprovalId, request.Comment, cancellationToken);
}

/// <summary>Rejects a pending approval request.</summary>
public sealed record RejectApprovalCommand(Guid ApprovalId, string? Comment) : IRequest<ApprovalDto>;

public sealed class RejectApprovalCommandHandler(IApprovalService approvalService)
    : IRequestHandler<RejectApprovalCommand, ApprovalDto>
{
    public Task<ApprovalDto> Handle(RejectApprovalCommand request, CancellationToken cancellationToken)
        => approvalService.RejectAsync(request.ApprovalId, request.Comment, cancellationToken);
}

/// <summary>Reassigns the approver for a not-yet-decided stage to another user eligible for that stage.</summary>
public sealed record ReassignApprovalStageCommand(Guid ApprovalId, int StageIndex, Guid NewApproverUserId) : IRequest<ApprovalDto>;

public sealed class ReassignApprovalStageCommandHandler(IApprovalService approvalService)
    : IRequestHandler<ReassignApprovalStageCommand, ApprovalDto>
{
    public Task<ApprovalDto> Handle(ReassignApprovalStageCommand request, CancellationToken cancellationToken)
        => approvalService.ReassignStageApproverAsync(request.ApprovalId, request.StageIndex, request.NewApproverUserId, cancellationToken);
}
