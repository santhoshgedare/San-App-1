import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { AuthService } from '../auth/auth.service';

const STORAGE_PREFIX = 'srividika_favourites';

@Injectable({ providedIn: 'root' })
export class FavouritesService {
  private readonly auth = inject(AuthService);

  private readonly storageKey = computed(
    () => `${STORAGE_PREFIX}_${this.auth.currentUser()?.email?.toLowerCase() ?? 'guest'}`,
  );
  private readonly _ids = signal<string[]>(this.read(this.storageKey()));

  readonly ids = this._ids.asReadonly();
  readonly count = computed(() => this._ids().length);

  constructor() {
    effect(() => {
      this._ids.set(this.read(this.storageKey()));
    });
  }

  isFavourite(itemId: string): boolean {
    return this._ids().includes(itemId);
  }

  toggle(itemId: string): boolean {
    const exists = this.isFavourite(itemId);
    const next = exists ? this._ids().filter((id) => id !== itemId) : [itemId, ...this._ids()];
    this._ids.set(next);
    this.write(next);
    return !exists;
  }

  remove(itemId: string): void {
    const next = this._ids().filter((id) => id !== itemId);
    this._ids.set(next);
    this.write(next);
  }

  private read(key: string): string[] {
    try {
      const parsed = JSON.parse(localStorage.getItem(key) ?? '[]');
      return Array.isArray(parsed) ? parsed.filter((id) => typeof id === 'string') : [];
    } catch {
      return [];
    }
  }

  private write(ids: string[]): void {
    try {
      localStorage.setItem(this.storageKey(), JSON.stringify(ids));
    } catch {
      // Storage may be unavailable; favourites stay in memory for this session.
    }
  }
}

