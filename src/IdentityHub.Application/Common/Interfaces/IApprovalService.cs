using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Generic approval-workflow abstraction. Requests are linked to any entity via a short
/// <c>entityType</c> discriminator (see <c>Domain.Constants.EntityTypes</c>) plus that
/// entity's primary key, so any current or future module can raise/approve/reject requests
/// through the same service without adding new tables or endpoints. When a
/// <see cref="IApprovalWorkflowService"/> workflow is configured for the entity type, the
/// request follows that workflow's ordered stages (advancing on any eligible approval);
/// otherwise it falls back to a single Admin/Manager decision.
/// </summary>
public interface IApprovalService
{
    Task<ApprovalDto> RequestAsync(string entityType, string entityId, string title, string? details, CancellationToken ct);

    Task<ApprovalDto> ApproveAsync(Guid approvalId, string? comment, CancellationToken ct);

    Task<ApprovalDto> RejectAsync(Guid approvalId, string? comment, CancellationToken ct);

    /// <summary>Reassigns the approver for a not-yet-decided stage row (the pencil-icon action) to another user holding that stage's eligible role(s).</summary>
    Task<ApprovalDto> ReassignStageApproverAsync(Guid approvalId, int stageIndex, Guid newApproverUserId, CancellationToken ct);

    /// <summary>Gets all approval requests for a single entity instance, newest first.</summary>
    Task<IReadOnlyList<ApprovalDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct);

    /// <summary>Gets a paged, optionally filtered approval feed across all entities, newest first.</summary>
    Task<PagedResult<ApprovalDto>> GetPagedAsync(ApprovalQuery query, CancellationToken ct);
}

/// <summary>Filter/pagination parameters for the approvals feed.</summary>
public sealed record ApprovalQuery
{
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
