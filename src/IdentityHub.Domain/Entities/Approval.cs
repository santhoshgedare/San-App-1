namespace IdentityHub.Domain.Entities;

/// <summary>
/// Common, entity-agnostic approval-request record. Linked to any entity via
/// <see cref="EntityType"/> (a short discriminator such as "User" or "Role" — see
/// <c>Domain.Constants.EntityTypes</c>) plus <see cref="EntityId"/> (the entity's primary
/// key, stored as a string so this table can reference any key type/shape). A single
/// approval workflow implementation can therefore be reused by any future module simply by
/// creating requests against it — no schema change required.
/// </summary>
public sealed class Approval
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Discriminator for the entity this approval request is about, e.g. "User", "Role".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity, as a string so any key type can be referenced.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Short label for what is being approved, e.g. "Role change", "Deactivation".</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Free-form human-readable detail about what is being requested.</summary>
    public string? Details { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public Guid RequestedByUserId { get; set; }
    public string? RequestedByEmail { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? DecidedByUserId { get; set; }
    public string? DecidedByEmail { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }

    /// <summary>Optional comment left by the approver/rejecter.</summary>
    public string? DecisionComment { get; set; }

    /// <summary>
    /// The workflow this request follows, snapshotted at request time (null when the entity
    /// type has no configured workflow, meaning a single Admin/Manager decision applies).
    /// </summary>
    public Guid? WorkflowId { get; set; }

    /// <summary>Zero-based index of the stage currently awaiting a decision (ignored when <see cref="WorkflowId"/> is null).</summary>
    public int CurrentStageIndex { get; set; }

    /// <summary>
    /// 1-based revision number for this (EntityType, EntityId) pair — increments each time a new
    /// request is raised for the same entity, so the full history of past approval cycles is kept.
    /// </summary>
    public int RevisionNumber { get; set; } = 1;

    /// <summary>
    /// True only for the latest revision of this entity's approval history. Only one approval per
    /// (EntityType, EntityId) may ever be <see cref="ApprovalStatus.Pending"/>/current at a time —
    /// a new request cannot be raised while a prior one for the same entity is still pending.
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    public List<ApprovalStageDecision> StageDecisions { get; set; } = [];
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    /// <summary>Stage row status only: this stage hasn't been reached yet (an earlier stage is still pending).</summary>
    Waiting = 3,
}

/// <summary>
/// One stage row of a specific approval request — created for every workflow stage up front
/// (in "Waiting" status) when the request is raised, so the full stage list/approver plan is
/// visible immediately, mirroring a real approval-cycle table. The current stage is "Pending"
/// and awaiting a decision; a reassignable <see cref="AssignedApproverUserId"/> defaults to the
/// workflow stage's configured default approver but can be changed per-request (the pencil-icon
/// reassign action) before it is decided.
/// </summary>
public sealed class ApprovalStageDecision
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApprovalId { get; set; }
    public Approval? Approval { get; set; }

    public int StageIndex { get; set; }
    public string StageName { get; set; } = string.Empty;

    /// <summary>User currently assigned to decide this stage (defaults from workflow config, reassignable while Pending/Waiting).</summary>
    public Guid? AssignedApproverUserId { get; set; }
    public string? AssignedApproverEmail { get; set; }
    /// <summary>Display label for the assigned approver's role/title at this stage, e.g. "Buyer Manager".</summary>
    public string? Designation { get; set; }

    public ApprovalStatus Decision { get; set; } = ApprovalStatus.Waiting;

    public Guid? DecidedByUserId { get; set; }
    public string? DecidedByEmail { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }

    public string? Comment { get; set; }
}
