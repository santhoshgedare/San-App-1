import { Component, computed, inject } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { AuthService } from '../../auth/auth.service';
import { SectionAccessStore } from '../../auth/section-access.store';
import { CartService } from '../../cart/cart.service';
import { FavouritesService } from '../../cart/favourites.service';

/**
 * Layout used for plain Users and guests: a clean, storefront-style single page
 * (no side navigation) that maximizes space for browsing the catalog. Navigation
 * is a lightweight top bar that collapses into a menu on small screens.
 */
@Component({
  selector: 'app-user-shell',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    MatBadgeModule,
    MatDividerModule,
  ],
  templateUrl: './user-shell.html',
  styleUrl: './user-shell.scss',
})
export class UserShell {
  protected readonly auth = inject(AuthService);
  protected readonly sectionAccess = inject(SectionAccessStore);
  protected readonly cartService = inject(CartService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly cartCount = this.cartService.totalItemsCount;
  protected readonly favourites = inject(FavouritesService);

  protected readonly isCompact = toSignal(
    this.breakpointObserver.observe(['(max-width: 900px)']).pipe(map((result) => result.matches)),
    { initialValue: false },
  );

  protected readonly navLinks = computed(() => {
    this.sectionAccess.isReady();
    const links = [] as { path: string; label: string; icon: string; exact: boolean }[];
    if (!this.auth.isAuthenticated() || this.sectionAccess.can('section-catalog-view')) {
      links.push({ path: '/catalog', label: 'Catalog', icon: 'storefront', exact: true });
    }
    links.push({ path: '/about', label: 'Our Story', icon: 'auto_awesome', exact: true });
    if (this.auth.isAuthenticated() && this.sectionAccess.can('section-orders-view')) {
      links.push({ path: '/orders', label: 'My Orders', icon: 'receipt_long', exact: false });
    }
    return links;
  });

  logout(): void {
    this.auth.logout();
  }
}
