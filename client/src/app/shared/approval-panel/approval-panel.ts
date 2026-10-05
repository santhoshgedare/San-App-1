import { Component, OnChanges, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { SELECT_DEFAULTS } from '../select-defaults';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApprovalService } from '../../core/auth/approval.service';
import { ApprovalWorkflowService } from '../../core/auth/approval-workflow.service';
import { AuthService } from '../../core/auth/auth.service';
import { UserService } from '../../core/auth/user.service';
import type { ApprovalDto, ApprovalStageDecisionDto, ApprovalWorkflowDto } from '../../core/models/approval.models';
import type { UserDto } from '../../core/models/auth.models';

/**
 * Common approval-workflow panel. Reusable on any detail/edit page by supplying an
 * `entityType`/`entityId` pair (see `Domain.Constants.EntityTypes` on the backend) — no
 * per-module wiring needed. Renders each request as a Stage/Approver/Designation/Status/
 * Action Date/Remark table (mirroring the legacy approval-cycle UI), with a reassign
 * (pencil) icon on the current stage's Approver cell and Approve/Reject actions at the
 * bottom. Any authenticated user can raise a new request via the inline "Request approval" form.
 */
@Component({
  selector: 'app-approval-panel',
  standalone: true,
  imports: [DatePipe, FormsModule, MatIconModule, MatSelectModule, MatButtonModule, MatTooltipModule],
  providers: [SELECT_DEFAULTS],
  templateUrl: './approval-panel.html',
  styleUrl: './approval-panel.scss',
})
export class ApprovalPanel implements OnChanges {
  private readonly approvalService = inject(ApprovalService);
  private readonly workflowService = inject(ApprovalWorkflowService);
  private readonly userService = inject(UserService);
  readonly auth = inject(AuthService);

  readonly entityType = input.required<string>();
  readonly entityId = input.required<string>();
  /** Default title used when requesting approval from this panel's inline request form. */
  readonly requestTitle = input<string>('Change approval');

  readonly approvals = signal<ApprovalDto[]>([]);
  /** Workflow config keyed by entityType, so stage role-eligibility can be resolved for the badge/gating logic. */
  readonly workflows = signal<Map<string, ApprovalWorkflowDto>>(new Map());
  readonly allUsers = signal<UserDto[]>([]);
  readonly isLoading = signal(true);
  readonly isBusy = signal(false);
  readonly showRequestForm = signal(false);
  readonly decidingId = signal<string | null>(null);
  /** approvalId of the stage row currently being reassigned (only one at a time). */
  readonly reassigningStage = signal<{ approvalId: string; stageIndex: number } | null>(null);

  requestDetails = '';
  decisionComment = '';
  reassignUserId = '';
  requestError = signal<string | null>(null);

  /** True while a revision of this entity's approval history is still pending — only one may be pending at a time. */
  hasPendingApproval(): boolean {
    return this.approvals().some((a) => a.isCurrent && a.status === 'Pending');
  }

  /** True when the current user can decide the given approval's current stage (or, for legacy workflow-less approvals, is Admin/Manager). */
  canDecide(approval: ApprovalDto): boolean {
    if (approval.entityType === 'Order' && approval.canDecide !== undefined) {
      return approval.canDecide;
    }
    if (this.auth.isAdmin()) {
      return true;
    }
    if (!approval.stages?.length) {
      return this.auth.isManager();
    }
    const stageRoles = this.stageRoles(approval, approval.currentStageIndex);
    const myRoles = this.auth.currentUser()?.roles ?? [];
    return stageRoles.some((role) => myRoles.includes(role));
  }

  /** Eligible roles for a given stage index, resolved from the request's snapshotted workflow, if any. */
  stageRoles(approval: ApprovalDto, stageIndex: number): string[] {
    const workflow = this.workflows().get(approval.entityType);
    return workflow?.stages[stageIndex]?.roles ?? [];
  }

  /** Users eligible to be reassigned as approver for a given stage (holding one of its roles). */
  eligibleUsersForStage(approval: ApprovalDto, stageIndex: number): UserDto[] {
    const roles = this.stageRoles(approval, stageIndex);
    if (!roles.length) {
      return this.allUsers();
    }
    return this.allUsers().filter((u) => u.roles.some((r) => roles.includes(r)));
  }

  /** Whether the pencil-icon reassign action should show for this stage row (not yet decided, and workflow-configured). */
  canReassign(approval: ApprovalDto, row: ApprovalStageDecisionDto): boolean {
    return approval.status === 'Pending' && (row.decision === 'Pending' || row.decision === 'Waiting') && this.canDecide(approval);
  }

  startReassign(approval: ApprovalDto, row: ApprovalStageDecisionDto): void {
    this.reassigningStage.set({ approvalId: approval.id, stageIndex: row.stageIndex });
    this.reassignUserId = row.assignedApproverUserId ?? '';
    if (!this.allUsers().length) {
      this.userService.getAll().subscribe((users) => this.allUsers.set(users));
    }
  }

  cancelReassign(): void {
    this.reassigningStage.set(null);
    this.reassignUserId = '';
  }

  confirmReassign(approval: ApprovalDto): void {
    const target = this.reassigningStage();
    if (!target || !this.reassignUserId) {
      return;
    }
    this.isBusy.set(true);
    this.approvalService.reassignStage(approval.id, target.stageIndex, this.reassignUserId).subscribe({
      next: () => {
        this.isBusy.set(false);
        this.cancelReassign();
        this.reload();
      },
      error: () => this.isBusy.set(false),
    });
  }

  ngOnChanges(): void {
    this.reload();
  }

  private reload(): void {
    if (!this.entityType() || !this.entityId()) {
      return;
    }
    this.isLoading.set(true);
    this.approvalService.getForEntity(this.entityType(), this.entityId()).subscribe({
      next: (approvals) => {
        this.approvals.set(approvals);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
    this.workflowService.getForEntityType(this.entityType()).subscribe({
      next: (workflow) => {
        const map = new Map(this.workflows());
        if (workflow) {
          map.set(this.entityType(), workflow);
        }
        this.workflows.set(map);
      },
      error: () => {},
    });
  }

  toggleRequestForm(): void {
    this.showRequestForm.set(!this.showRequestForm());
    this.requestDetails = '';
    this.requestError.set(null);
  }

  submitRequest(): void {
    this.isBusy.set(true);
    this.requestError.set(null);
    this.approvalService
      .request(this.entityType(), this.entityId(), this.requestTitle(), this.requestDetails.trim() || undefined)
      .subscribe({
        next: () => {
          this.isBusy.set(false);
          this.showRequestForm.set(false);
          this.requestDetails = '';
          this.reload();
        },
        error: (err) => {
          this.isBusy.set(false);
          this.requestError.set(err?.error?.errors?.[0] ?? 'Failed to raise approval request.');
        },
      });
  }

  startDecision(approval: ApprovalDto): void {
    this.decidingId.set(approval.id);
    this.decisionComment = '';
  }

  cancelDecision(): void {
    this.decidingId.set(null);
    this.decisionComment = '';
  }

  confirmApprove(approval: ApprovalDto): void {
    this.isBusy.set(true);
    this.approvalService.approve(approval.id, this.decisionComment.trim() || undefined).subscribe({
      next: () => {
        this.isBusy.set(false);
        this.decidingId.set(null);
        this.reload();
      },
      error: () => this.isBusy.set(false),
    });
  }

  confirmReject(approval: ApprovalDto): void {
    this.isBusy.set(true);
    this.approvalService.reject(approval.id, this.decisionComment.trim() || undefined).subscribe({
      next: () => {
        this.isBusy.set(false);
        this.decidingId.set(null);
        this.reload();
      },
      error: () => this.isBusy.set(false),
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'bg-success-subtle text-success-emphasis';
      case 'Rejected':
        return 'bg-danger-subtle text-danger-emphasis';
      case 'Waiting':
        return 'bg-secondary-subtle text-secondary-emphasis';
      default:
        return 'bg-warning-subtle text-warning-emphasis';
    }
  }
}
