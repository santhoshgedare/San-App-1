import { Component, inject } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatBadgeModule } from '@angular/material/badge';
import { map } from 'rxjs/operators';
import { AuthService } from '../../auth/auth.service';
import { SectionAccessStore } from '../../auth/section-access.store';
import { CartService } from '../../cart/cart.service';
import { CanRenderDirective } from '../../directives/can-render.directive';
import { SettingsSheet } from '../settings-sheet/settings-sheet';

/**
 * Layout used for Admins/Managers: a full side navigation with every management
 * section (Users, Roles, Categories, Items, Payment Settings, ...) plus the
 * shared storefront links (Catalog, Cart, Dashboard, Orders, Profile).
 */
@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    MatBadgeModule,
    CanRenderDirective,
  ],
  templateUrl: './admin-shell.html',
  styleUrl: './admin-shell.scss',
})
export class AdminShell {
  protected readonly auth = inject(AuthService);
  protected readonly sectionAccess = inject(SectionAccessStore);
  protected readonly cartService = inject(CartService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly bottomSheet = inject(MatBottomSheet);

  readonly cartCount = this.cartService.totalItemsCount;

  constructor() {
    this.sectionAccess.load();
  }

  protected readonly isScreenSmall = toSignal(
    this.breakpointObserver.observe(Breakpoints.Handset).pipe(map((result) => result.matches)),
    { initialValue: false },
  );

  openSettings(): void {
    this.bottomSheet.open(SettingsSheet);
  }

  logout(): void {
    this.auth.logout();
  }
}
