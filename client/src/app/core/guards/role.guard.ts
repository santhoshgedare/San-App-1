import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Route guard factory that restricts a route to users holding at least one of the given roles.
 * Usage: `canActivate: [roleGuard(['Admin'])]`
 */
export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const userRoles = authService.currentUser()?.roles ?? [];
    if (userRoles.some((role) => allowedRoles.includes(role))) {
      return true;
    }

    return router.parseUrl('/forbidden');
  };
}
