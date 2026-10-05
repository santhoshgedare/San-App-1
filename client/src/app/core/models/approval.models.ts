export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected' | 'Waiting';

/** One stage row (Stage/Approver/Designation/Status/Action Date/Remark) of a specific approval request. */
export interface ApprovalStageDecisionDto {
  stageIndex: number;
  stageName: string;
  /** User currently assigned to decide this stage — defaults from workflow config, reassignable via the pencil icon while Pending/Waiting. */
  assignedApproverUserId: string | null;
  assignedApproverEmail: string | null;
  /** Display label for the assigned approver's role/title at this stage. */
  designation: string | null;
  decision: ApprovalStatus;
  decidedByUserId: string | null;
  decidedByEmail: string | null;
  decidedAt: string | null;
  comment: string | null;
}

/** Generic approval-request record, keyed by (entityType, entityId) so any module can reuse it. */
export interface ApprovalDto {
  id: string;
  entityType: string;
  entityId: string;
  title: string;
  details: string | null;
  status: ApprovalStatus;
  requestedByUserId: string;
  requestedByEmail: string | null;
  requestedAt: string;
  decidedByUserId: string | null;
  decidedByEmail: string | null;
  decidedAt: string | null;
  decisionComment: string | null;
  /** Ordered stage names snapshotted from the workflow configured for this entity type (empty when no workflow is configured). */
  stages: string[];
  /** Zero-based index of the stage currently awaiting a decision. */
  currentStageIndex: number;
  /** 1-based revision number for this entity's approval history (increments per new request raised). */
  revisionNumber: number;
  /** True only for the latest revision — only one approval per entity may be pending at a time. */
  isCurrent: boolean;
  canDecide?: boolean;
  /** Per-stage row: assigned approver, designation, status, decision date, remark — in stage order. */
  stageDecisions: ApprovalStageDecisionDto[];
}

/** A single stage in an approval workflow: a name plus the roles eligible to decide it. */
export interface ApprovalWorkflowStageDto {
  id: string;
  name: string;
  sortOrder: number;
  roles: string[];
  defaultApproverUserId: string | null;
  defaultApproverEmail: string | null;
}

/** A configured multi-stage approval workflow for one entity type. */
export interface ApprovalWorkflowDto {
  id: string;
  entityType: string;
  name: string;
  isActive: boolean;
  stages: ApprovalWorkflowStageDto[];
}

/** Upsert payload for saving a workflow's stages (used by the workflow config UI). */
export interface SaveApprovalWorkflowRequest {
  id: string | null;
  entityType: string;
  name: string;
  isActive: boolean;
  stages: { name: string; roles: string[]; defaultApproverUserId: string | null }[];
}

/** Reassigns the approver for a not-yet-decided stage (the pencil-icon action on the Approver column). */
export interface ReassignApprovalStageRequest {
  stageIndex: number;
  newApproverUserId: string;
}

