import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PasswordField } from '../../../shared/password-field/password-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { UserService } from '../../../core/auth/user.service';
import { RoleService } from '../../../core/auth/role.service';
import { SectionAccessStore } from '../../../core/auth/section-access.store';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ENTITY_TYPES } from '../../../core/models/constants';
import type { RoleDto, UserDto } from '../../../core/models/auth.models';

/**
 * Single page for viewing, editing, or creating a user (routed as `/users/new` or
 * `/users/:id`), replacing the old inline-in-table edit affordances with a proper
 * detail/edit/create screen.
 */
@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [PasswordField, CommonModule, FormsModule, DatePipe, MatIconModule, MatButtonModule, ActivityLogPanel, ApprovalPanel],
  templateUrl: './user-form.html',
  styleUrl: './user-form.scss',
})
export class UserForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly roleService = inject(RoleService);
  protected readonly sectionAccess = inject(SectionAccessStore);

  readonly entityType = ENTITY_TYPES.user;
  readonly isNew = signal(true);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly roles = signal<RoleDto[]>([]);
  readonly user = signal<UserDto | null>(null);

  // Form fields
  email = '';
  password = '';
  firstName = '';
  lastName = '';
  isActive = true;
  selectedRoleNames = new Set<string>();

  ngOnInit(): void {
    if (this.sectionAccess.can('section-users-roles')) {
      this.roleService.getAll().subscribe((roles) => this.roles.set(roles));
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'new') {
      this.isNew.set(true);
      this.isLoading.set(false);
      return;
    }

    this.isNew.set(false);
    this.userService.getById(id).subscribe({
      next: (user) => {
        this.user.set(user);
        this.email = user.email;
        this.firstName = user.firstName;
        this.lastName = user.lastName;
        this.isActive = user.isActive;
        this.selectedRoleNames = new Set(user.roles);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('User not found.');
        this.isLoading.set(false);
      },
    });
  }

  isRoleSelected(roleName: string): boolean {
    return this.selectedRoleNames.has(roleName);
  }

  toggleRole(roleName: string, checked: boolean): void {
    if (checked) {
      this.selectedRoleNames.add(roleName);
    } else {
      this.selectedRoleNames.delete(roleName);
    }
  }

  save(): void {
    this.errorMessage.set(null);

    if (this.isNew()) {
      if (!this.sectionAccess.can('section-users-roles')) {
        this.errorMessage.set('Role assignment permission is required to create users.');
        return;
      }
      if (!this.email.trim() || !this.password.trim() || !this.firstName.trim() || !this.lastName.trim()) {
        this.errorMessage.set('All fields are required.');
        return;
      }

      this.isSaving.set(true);
      this.userService
        .create({
          email: this.email.trim(),
          password: this.password,
          firstName: this.firstName.trim(),
          lastName: this.lastName.trim(),
          roles: Array.from(this.selectedRoleNames),
        })
        .subscribe({
          next: (user) => {
            this.isSaving.set(false);
            this.router.navigate(['/users', user.id]);
          },
          error: (err) => {
            this.isSaving.set(false);
            this.errorMessage.set(err?.error?.errors?.join(', ') ?? 'Could not create user.');
          },
        });
      return;
    }

    const existing = this.user();
    if (!existing) {
      return;
    }

    this.isSaving.set(true);
    this.userService
      .update(existing.id, { firstName: this.firstName.trim(), lastName: this.lastName.trim(), isActive: this.isActive })
      .subscribe({
        next: () => {
          if (!this.sectionAccess.can('section-users-roles')) {
            this.isSaving.set(false);
            this.router.navigate(['/users']);
            return;
          }
          this.userService.assignRoles(existing.id, { roles: Array.from(this.selectedRoleNames) }).subscribe({
            next: () => {
              this.isSaving.set(false);
              this.router.navigate(['/users']);
            },
            error: () => {
              this.isSaving.set(false);
              this.errorMessage.set('Profile saved, but role assignment failed.');
            },
          });
        },
        error: (err) => {
          this.isSaving.set(false);
          this.errorMessage.set(err?.error?.errors?.join(', ') ?? 'Could not save user.');
        },
      });
  }

  remove(): void {
    const existing = this.user();
    if (!existing) {
      return;
    }
    if (!confirm(`Delete user ${existing.email}? This cannot be undone.`)) {
      return;
    }
    this.userService.delete(existing.id).subscribe(() => this.router.navigate(['/users']));
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}
