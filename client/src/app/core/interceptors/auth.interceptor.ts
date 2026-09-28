import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

/**
 * Attaches the bearer access token to outgoing API requests and transparently refreshes it
 * once on a 401, retrying the original request. Concurrent 401s wait for the single refresh call.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const accessToken = authService.getAccessToken();
  const authorizedReq = accessToken ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } }) : req;

  return next(authorizedReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && authService.getRefreshToken() && !req.url.includes('/auth/')) {
        return handleUnauthorized(req, next, authService, router);
      }
      return throwError(() => error);
    }),
  );
};

function handleUnauthorized(req: Parameters<HttpInterceptorFn>[0], next: Parameters<HttpInterceptorFn>[1], authService: AuthService, router: Router) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshedToken$.next(null);

    return authService.refreshToken().pipe(
      switchMap((result) => {
        isRefreshing = false;
        refreshedToken$.next(result.accessToken);
        return next(req.clone({ setHeaders: { Authorization: `Bearer ${result.accessToken}` } }));
      }),
      catchError((refreshError) => {
        isRefreshing = false;
        authService.logout();
        router.navigateByUrl('/login');
        return throwError(() => refreshError);
      }),
    );
  }

  return refreshedToken$.pipe(
    filter((token): token is string => token !== null),
    take(1),
    switchMap((token) => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))),
  );
}
