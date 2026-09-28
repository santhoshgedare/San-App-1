import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { ModuleAccessService } from '../../auth/module-access.service';
import { SectionAccessStore } from '../../auth/section-access.store';
import { AuthService } from '../../auth/auth.service';
import type { ModuleDto } from '../../models/module-access.models';

interface SettingOption {
  name: string;
  link: string;
  icon: string;
}

interface SettingGroup {
  category: string;
  options: SettingOption[];
}

/** Fallback icons keyed by page url, for pages the seed data doesn't set an icon for. */
const ICON_BY_URL: Record<string, string> = {
  '/users': 'group',
  '/roles': 'admin_panel_settings',
  '/profile': 'person',
};

/**
 * Bottom sheet for quick access to sections the current user is allowed to see,
 * built from the live Module &gt; Page &gt; Section master tree (mirrors the legacy
 * buyer-spa settings bottom sheet, but data-driven instead of hardcoded).
 */
@Component({
  selector: 'app-settings-sheet',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatListModule],
  templateUrl: './settings-sheet.html',
  styleUrl: './settings-sheet.scss',
})
export class SettingsSheet {
  private readonly router = inject(Router);
  private readonly sheetRef = inject(MatBottomSheetRef<SettingsSheet>);
  private readonly moduleAccess = inject(ModuleAccessService);
  private readonly sectionAccess = inject(SectionAccessStore);
  readonly auth = inject(AuthService);

  private readonly tree = signal<ModuleDto[]>([]);

  protected readonly groups = computed<SettingGroup[]>(() => {
    // Track store readiness so this recomputes once section keys finish loading.
    this.sectionAccess.isReady();

    return this.tree()
      .filter((module) => module.isActive)
      .map((module) => ({
        category: module.name.toUpperCase(),
        options: module.pages
          .filter((page) => page.isActive && page.sections.some((section) => section.isActive && this.sectionAccess.can(section.key)))
          .sort((a, b) => a.sortOrder - b.sortOrder)
          .map((page) => ({
            name: page.name,
            link: page.url,
            icon: ICON_BY_URL[page.url] ?? 'chevron_right',
          })),
      }))
      .filter((group) => group.options.length > 0);
  });

  constructor() {
    this.moduleAccess.getTree().subscribe((tree) => this.tree.set(tree));
  }

  navigate(option: SettingOption): void {
    this.router.navigate([option.link]);
    this.sheetRef.dismiss();
  }

  navigateToWorkflows(): void {
    this.router.navigate(['/approval-workflows']);
    this.sheetRef.dismiss();
  }

  dismiss(): void {
    this.sheetRef.dismiss();
  }
}
