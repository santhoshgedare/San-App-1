import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
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

  readonly showAdminLayout = computed(() => this.auth.isAdmin() || this.auth.isManager());
}
