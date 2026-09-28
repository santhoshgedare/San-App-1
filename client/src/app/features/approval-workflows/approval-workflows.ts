import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { ApprovalWorkflowService } from '../../core/auth/approval-workflow.service';
import { RoleService } from '../../core/auth/role.service';
import { UserService } from '../../core/auth/user.service';
import { ENTITY_TYPES } from '../../core/models/constants';
import type { ApprovalWorkflowDto } from '../../core/models/approval.models';
import type { RoleDto, UserDto } from '../../core/models/auth.models';

interface StageDraft {
  name: string;
  roles: string[];
  defaultApproverUserId: string | null;
}

/** Any entity type a workflow can be configured for; extend this list as new masters adopt approvals. */
const CONFIGURABLE_ENTITY_TYPES: string[] = [
  ENTITY_TYPES.user,
  ENTITY_TYPES.role,
  ENTITY_TYPES.category,
  ENTITY_TYPES.item,
  ENTITY_TYPES.order,
];

/**
 * Admin-only screen for configuring the multi-stage approval workflow per entity type: pick an
 * entity type, define ordered stages, assign the role(s) eligible to decide each stage, and pick
 * a default approver (a specific user holding one of those roles) — pre-populated on every new
 * request but still reassignable per-request. Any one user holding an assigned role can advance
 * that stage; the next master created/edited for that entity type automatically picks up this
 * workflow (see `ApprovalPanel`).
 */
@Component({
  selector: 'app-approval-workflows',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './approval-workflows.html',
  styleUrl: './approval-workflows.scss',
})
export class ApprovalWorkflows implements OnInit {
  private readonly workflowService = inject(ApprovalWorkflowService);
  private readonly roleService = inject(RoleService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);

  readonly entityTypes = CONFIGURABLE_ENTITY_TYPES;
  readonly workflows = signal<ApprovalWorkflowDto[]>([]);
  readonly allRoles = signal<RoleDto[]>([]);
  readonly allUsers = signal<UserDto[]>([]);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  selectedEntityType: string = CONFIGURABLE_ENTITY_TYPES[0];
  editingId: string | null = null;
  workflowName = '';
  isActive = true;
  stages: StageDraft[] = [];

  ngOnInit(): void {
    this.roleService.getAll().subscribe((roles) => this.allRoles.set(roles));
    this.userService.getAll().subscribe((users) => this.allUsers.set(users));
    this.reload();
  }

  private reload(): void {
    this.isLoading.set(true);
    this.workflowService.getAll().subscribe({
      next: (workflows) => {
        this.workflows.set(workflows);
        this.isLoading.set(false);
        this.loadEntityType(this.selectedEntityType);
      },
      error: () => this.isLoading.set(false),
    });
  }

  /** Loads the existing workflow for the picked entity type into the editor, or starts a blank one. */
  loadEntityType(entityType: string): void {
    this.selectedEntityType = entityType;
    this.errorMessage.set(null);
    const existing = this.workflows().find((w) => w.entityType === entityType);
    if (existing) {
      this.editingId = existing.id;
      this.workflowName = existing.name;
      this.isActive = existing.isActive;
      this.stages = existing.stages.map((s) => ({
        name: s.name,
        roles: [...s.roles],
        defaultApproverUserId: s.defaultApproverUserId,
      }));
    } else {
      this.editingId = null;
      this.workflowName = `${entityType} Approval`;
      this.isActive = true;
      this.stages = [{ name: 'Manager Review', roles: [], defaultApproverUserId: null }];
    }
  }

  addStage(): void {
    this.stages.push({ name: `Stage ${this.stages.length + 1}`, roles: [], defaultApproverUserId: null });
  }

  removeStage(index: number): void {
    this.stages.splice(index, 1);
  }

  isRoleSelected(stage: StageDraft, roleName: string): boolean {
    return stage.roles.includes(roleName);
  }

  toggleStageRole(stage: StageDraft, roleName: string, checked: boolean): void {
    if (checked) {
      if (!stage.roles.includes(roleName)) {
        stage.roles.push(roleName);
      }
    } else {
      stage.roles = stage.roles.filter((r) => r !== roleName);
    }
    // The default approver must hold one of the stage's eligible roles; clear it if it no longer does.
    if (stage.defaultApproverUserId && !this.usersForStage(stage).some((u) => u.id === stage.defaultApproverUserId)) {
      stage.defaultApproverUserId = null;
    }
  }

  /** Users holding at least one of the stage's eligible roles — candidates for the default-approver picker. */
  usersForStage(stage: StageDraft): UserDto[] {
    if (!stage.roles.length) {
      return this.allUsers();
    }
    return this.allUsers().filter((u) => u.roles.some((r) => stage.roles.includes(r)));
  }

  save(): void {
    this.errorMessage.set(null);

    if (!this.workflowName.trim()) {
      this.errorMessage.set('Workflow name is required.');
      return;
    }
    if (!this.stages.length) {
      this.errorMessage.set('At least one stage is required.');
      return;
    }
    if (this.stages.some((s) => !s.name.trim())) {
      this.errorMessage.set('Every stage needs a name.');
      return;
    }
    if (this.stages.some((s) => s.roles.length === 0)) {
      this.errorMessage.set('Every stage needs at least one eligible role.');
      return;
    }

    this.isSaving.set(true);
    this.workflowService
      .save({
        id: this.editingId,
        entityType: this.selectedEntityType,
        name: this.workflowName.trim(),
        isActive: this.isActive,
        stages: this.stages.map((s) => ({ name: s.name.trim(), roles: s.roles, defaultApproverUserId: s.defaultApproverUserId })),
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.reload();
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('Could not save workflow.');
        },
      });
  }

  remove(): void {
    if (!this.editingId) {
      return;
    }
    if (!confirm('Delete this approval workflow? Entities of this type will fall back to a single Admin/Manager decision.')) {
      return;
    }
    this.workflowService.delete(this.editingId).subscribe(() => this.reload());
  }

  cancel(): void {
    this.router.navigate(['/dashboard']);
  }
}
