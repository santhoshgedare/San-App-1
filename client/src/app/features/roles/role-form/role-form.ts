import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RoleService } from '../../../core/auth/role.service';
import { ModuleAccessService } from '../../../core/auth/module-access.service';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ENTITY_TYPES } from '../../../core/models/constants';
import type { RoleDto } from '../../../core/models/auth.models';
import type { ModuleDto } from '../../../core/models/module-access.models';

/**
 * Single page for viewing/editing a role's section access, or creating a new role
 * (routed as `/roles/new` or `/roles/:id`), replacing the old inline "Manage Access"
 * panel with a proper detail/edit/create screen.
 */
@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatButtonModule, ActivityLogPanel, ApprovalPanel],
  templateUrl: './role-form.html',
  styleUrl: './role-form.scss',
})
export class RoleForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly roleService = inject(RoleService);
  private readonly moduleAccess = inject(ModuleAccessService);

  readonly entityType = ENTITY_TYPES.role;
  readonly isNew = signal(true);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly role = signal<RoleDto | null>(null);
  readonly tree = signal<ModuleDto[]>([]);
  readonly selectedSectionKeys = signal<Set<string>>(new Set());

  newRoleName = '';

  ngOnInit(): void {
    this.moduleAccess.getTree().subscribe((tree) => this.tree.set(tree));

    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'new') {
      this.isNew.set(true);
      this.isLoading.set(false);
      return;
    }

    this.isNew.set(false);
    this.roleService.getAll().subscribe({
      next: (roles) => {
        const match = roles.find((r) => r.id === id);
        if (!match) {
          this.errorMessage.set('Role not found.');
          this.isLoading.set(false);
          return;
        }
        this.role.set(match);
        this.loadAccess(id);
      },
      error: () => {
        this.errorMessage.set('Could not load role.');
        this.isLoading.set(false);
      },
    });
  }

  private loadAccess(roleId: string): void {
    this.moduleAccess.getRoleAccess(roleId).subscribe({
      next: (access) => {
        this.selectedSectionKeys.set(new Set(access.sectionKeys));
        this.isLoading.set(false);
      },
      error: () => {
        this.selectedSectionKeys.set(new Set());
        this.isLoading.set(false);
      },
    });
  }

  isSectionChecked(key: string): boolean {
    return this.selectedSectionKeys().has(key);
  }

  toggleSection(key: string, checked: boolean): void {
    const keys = new Set(this.selectedSectionKeys());
    if (checked) {
      keys.add(key);
    } else {
      keys.delete(key);
    }
    this.selectedSectionKeys.set(keys);
  }

  save(): void {
    this.errorMessage.set(null);

    if (this.isNew()) {
      const name = this.newRoleName.trim();
      if (!name) {
        this.errorMessage.set('Role name is required.');
        return;
      }

      this.isSaving.set(true);
      this.roleService.create(name).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/roles']);
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('Could not create role. It may already exist.');
        },
      });
      return;
    }

    const existing = this.role();
    if (!existing) {
      return;
    }

    this.isSaving.set(true);
    this.moduleAccess.setRoleAccess(existing.id, { sectionKeys: Array.from(this.selectedSectionKeys()) }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.router.navigate(['/roles']);
      },
      error: () => {
        this.isSaving.set(false);
        this.errorMessage.set('Could not save section access.');
      },
    });
  }

  remove(): void {
    const existing = this.role();
    if (!existing) {
      return;
    }
    if (!confirm(`Delete role "${existing.name}"?`)) {
      return;
    }
    this.roleService.delete(existing.id).subscribe(() => this.router.navigate(['/roles']));
  }

  cancel(): void {
    this.router.navigate(['/roles']);
  }
}
