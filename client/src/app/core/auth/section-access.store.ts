import { Injectable, computed, inject, signal } from '@angular/core';
import { AuthService } from './auth.service';
import { ModuleAccessService } from './module-access.service';
import { ROLES } from '../models/constants';
import { Observable, catchError, map, of, shareReplay, tap } from 'rxjs';

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
  private pendingLoad: Observable<boolean> | null = null;

  readonly isReady = computed(() => this.loaded() || this.isAdmin());

  private isAdmin(): boolean {
    return this.auth.currentUser()?.roles.includes(ROLES.admin) ?? false;
  }

  /** Loads the current user's granted sections. Call once after login / on app start. */
  load(): void {
    this.ensureLoaded().subscribe();
  }

  ensureLoaded(): Observable<boolean> {
    if (!this.auth.isAuthenticated()) {
      this.reset();
      return of(false);
    }
    if (this.isAdmin()) {
      this.loaded.set(true);
      return of(true);
    }
    if (this.loaded()) return of(true);
    if (this.pendingLoad) return this.pendingLoad;

    this.pendingLoad = this.moduleAccess.getMySections().pipe(
      tap((keys) => this.sectionKeys.set(keys)),
      map(() => true),
      catchError(() => of(false)),
      tap(() => this.loaded.set(true)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.pendingLoad;
  }

  reset(): void {
    this.sectionKeys.set([]);
    this.loaded.set(false);
    this.pendingLoad = null;
  }

  can(sectionKey: string): boolean {
    if (this.isAdmin()) {
      return true;
    }
    return this.sectionKeys().includes(sectionKey);
  }
}
