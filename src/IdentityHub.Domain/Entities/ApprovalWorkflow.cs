namespace IdentityHub.Domain.Entities;

/// <summary>
/// Configurable multi-stage approval workflow for a given entity type (see
/// <c>Domain.Constants.EntityTypes</c>). When a workflow exists for an entity type, new
/// approval requests against that type snapshot its stages at request time; entities with
/// no configured workflow fall back to a single-stage Admin/Manager decision.
/// </summary>
public sealed class ApprovalWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Entity type this workflow applies to, e.g. "User", "Role". One active workflow per entity type.</summary>
    public string EntityType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ApprovalWorkflowStage> Stages { get; set; } = [];
}

/// <summary>
/// A single ordered stage in an <see cref="ApprovalWorkflow"/>. Any user holding one of the
/// stage's assigned roles can approve/reject at that stage; a single approval from any
/// eligible user advances the request to the next stage.
/// </summary>
public sealed class ApprovalWorkflowStage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkflowId { get; set; }
    public ApprovalWorkflow? Workflow { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    /// <summary>Role names (from ASP.NET Identity roles) eligible to decide this stage, stored as JSON string[].</summary>
    public string RolesJson { get; set; } = "[]";

    /// <summary>
    /// Default user pre-assigned as this stage's approver when a new request is raised (shown as
    /// "Approver" on the request). Any user holding one of <see cref="RolesJson"/> can still
    /// decide/reassign this stage — this is only the starting default, editable per-request.
    /// </summary>
    public Guid? DefaultApproverUserId { get; set; }
}
