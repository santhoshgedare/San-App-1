import { Component, computed, effect, inject, untracked } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs/operators';
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

  private readonly router = inject(Router);
  private readonly isAboutUrl = (url: string) => /^\/about(\/|\?|#|$)/.test(url);

  // Our Story is always shown in the storefront layout, even for admins; the logo leads back to the admin layout.
  private readonly onAbout = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map((e) => this.isAboutUrl(e.urlAfterRedirects)),
    ),
    { initialValue: this.isAboutUrl(this.router.url) },
  );

  readonly showAdminLayout = computed(() => {
    if (this.onAbout()) return false;
    const roles = this.auth.currentUser()?.roles ?? [];
    return roles.some((role) => role !== ROLES.user);
  });
}
