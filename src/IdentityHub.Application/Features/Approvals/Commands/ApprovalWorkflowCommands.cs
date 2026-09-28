using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Approvals.Commands;

/// <summary>Creates or replaces the approval workflow (stages + eligible roles) for an entity type.</summary>
public sealed record SaveApprovalWorkflowCommand(
    Guid? Id,
    string EntityType,
    string Name,
    bool IsActive,
    IReadOnlyList<SaveApprovalWorkflowStageRequest> Stages) : IRequest<Result<ApprovalWorkflowDto>>;

public sealed class SaveApprovalWorkflowCommandHandler(IApprovalWorkflowService workflowService)
    : IRequestHandler<SaveApprovalWorkflowCommand, Result<ApprovalWorkflowDto>>
{
    public Task<Result<ApprovalWorkflowDto>> Handle(SaveApprovalWorkflowCommand request, CancellationToken cancellationToken)
        => workflowService.SaveAsync(
            new SaveApprovalWorkflowRequest(request.Id, request.EntityType, request.Name, request.IsActive, request.Stages),
            cancellationToken);
}

/// <summary>Deletes an approval workflow configuration.</summary>
public sealed record DeleteApprovalWorkflowCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteApprovalWorkflowCommandHandler(IApprovalWorkflowService workflowService)
    : IRequestHandler<DeleteApprovalWorkflowCommand, Result>
{
    public Task<Result> Handle(DeleteApprovalWorkflowCommand request, CancellationToken cancellationToken)
        => workflowService.DeleteAsync(request.Id, cancellationToken);
}
