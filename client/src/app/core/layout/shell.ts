import { Component, computed, effect, inject, untracked } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { SectionAccessStore } from '../auth/section-access.store';
import { ROLES } from '../models/constants';
import { AdminShell } from './admin-shell/admin-shell';
import { UserShell } from './user-shell/user-shell';

/**
 * Top-level layout dispatcher. Admins/Managers get the full sidenav-based
 * management console (`AdminShell`); plain users and guests get a lightweight,
 * full-page storefront layout (`UserShell`) optimized for browsing/ordering.
 */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [AdminShell, UserShell],
  template: `
    @if (showAdminLayout()) {
      <app-admin-shell />
    } @else {
      <app-user-shell />
    }
  `,
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly sectionAccess = inject(SectionAccessStore);

  constructor() {
    effect(() => {
      const user = this.auth.currentUser();
      untracked(() => {
        this.sectionAccess.reset();
        if (user) this.sectionAccess.load();
      });
    });
  }

  readonly showAdminLayout = computed(() => {
    const roles = this.auth.currentUser()?.roles ?? [];
    return roles.some((role) => role !== ROLES.user);
  });
}
