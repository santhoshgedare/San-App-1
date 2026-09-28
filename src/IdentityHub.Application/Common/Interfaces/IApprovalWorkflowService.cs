using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Configures the multi-stage approval workflow used for a given entity type. Any current
/// or future master (User, Role, or new modules) can be wired into a configurable approval
/// cycle purely by referencing its <c>Domain.Constants.EntityTypes</c> discriminator here —
/// no code change needed in the approving module itself.
/// </summary>
public interface IApprovalWorkflowService
{
    Task<IReadOnlyList<ApprovalWorkflowDto>> GetAllAsync(CancellationToken ct);

    Task<ApprovalWorkflowDto?> GetForEntityTypeAsync(string entityType, CancellationToken ct);

    Task<Result<ApprovalWorkflowDto>> SaveAsync(SaveApprovalWorkflowRequest request, CancellationToken ct);

    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}

/// <summary>Full replace-style upsert payload: one active workflow per entity type, stages given in order.</summary>
public sealed record SaveApprovalWorkflowRequest(
    Guid? Id,
    string EntityType,
    string Name,
    bool IsActive,
    IReadOnlyList<SaveApprovalWorkflowStageRequest> Stages);

public sealed record SaveApprovalWorkflowStageRequest(string Name, IReadOnlyList<string> Roles, Guid? DefaultApproverUserId);
