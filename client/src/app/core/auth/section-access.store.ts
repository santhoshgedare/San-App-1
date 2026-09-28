import { Injectable, computed, inject, signal } from '@angular/core';
import { AuthService } from './auth.service';
import { ModuleAccessService } from './module-access.service';
import { ROLES } from '../models/constants';

/**
 * Holds the current user's granted section keys and exposes `can(key)` for the
 * `canRender` structural directive. Admins implicitly have every section (mirrors
 * the legacy app's SuperAdmin bypass), so no network call is needed for them.
 */
@Injectable({ providedIn: 'root' })
export class SectionAccessStore {
  private readonly auth = inject(AuthService);
  private readonly moduleAccess = inject(ModuleAccessService);

  private readonly sectionKeys = signal<string[]>([]);
  private readonly loaded = signal(false);

  readonly isReady = computed(() => this.loaded() || this.isAdmin());

  private isAdmin(): boolean {
    return this.auth.currentUser()?.roles.includes(ROLES.admin) ?? false;
  }

  /** Loads the current user's granted sections. Call once after login / on app start. */
  load(): void {
    if (!this.auth.isAuthenticated()) {
      this.reset();
      return;
    }

    if (this.isAdmin()) {
      this.loaded.set(true);
      return;
    }

    this.moduleAccess.getMySections().subscribe({
      next: (keys) => {
        this.sectionKeys.set(keys);
        this.loaded.set(true);
      },
      error: () => this.loaded.set(true),
    });
  }

  reset(): void {
    this.sectionKeys.set([]);
    this.loaded.set(false);
  }

  can(sectionKey: string): boolean {
    if (this.isAdmin()) {
      return true;
    }
    return this.sectionKeys().includes(sectionKey);
  }
}
