import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ROLES, APP_CONSTANTS } from '../models/constants';
import type { AuthResult, ExternalAuthProvider, LoginRequest, RegisterRequest, UserDto } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  /** Currently authenticated user, or null when signed out. Restored from storage on load. */
  readonly currentUser = signal<UserDto | null>(this.readStoredUser());
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes(ROLES.admin) ?? false);
  readonly isManager = computed(() => this.currentUser()?.roles.includes(ROLES.manager) ?? false);

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) { }

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/login`, request).pipe(tap((result) => this.persistSession(result)));
  }

  register(request: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/register`, request).pipe(tap((result) => this.persistSession(result)));
  }

  getExternalProviders(): Observable<ExternalAuthProvider[]> {
    return this.http.get<ExternalAuthProvider[]>(`${this.baseUrl}/external/providers`);
  }

  requestPasswordReset(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reset-password`, { email, token, newPassword });
  }

  completeExternalLogin(result: AuthResult): void {
    this.persistSession(result);
  }

  refreshToken(): Observable<AuthResult> {
    const body = {
      accessToken: this.getAccessToken() ?? '',
      refreshToken: this.getRefreshToken() ?? '',
    };
    return this.http.post<AuthResult>(`${this.baseUrl}/refresh`, body).pipe(tap((result) => this.persistSession(result)));
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.baseUrl}/logout`, { refreshToken }).subscribe({ complete: () => this.clearSession() });
    } else {
      this.clearSession();
    }
    this.router.navigateByUrl('/login');
  }

  getAccessToken(): string | null {
    return localStorage.getItem(APP_CONSTANTS.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(APP_CONSTANTS.refreshTokenKey);
  }

  private persistSession(result: AuthResult): void {
    localStorage.setItem(APP_CONSTANTS.accessTokenKey, result.accessToken);
    localStorage.setItem(APP_CONSTANTS.refreshTokenKey, result.refreshToken);
    localStorage.setItem(APP_CONSTANTS.userKey, JSON.stringify(result.user));
    this.currentUser.set(result.user);
  }

  private clearSession(): void {
    localStorage.removeItem(APP_CONSTANTS.accessTokenKey);
    localStorage.removeItem(APP_CONSTANTS.refreshTokenKey);
    localStorage.removeItem(APP_CONSTANTS.userKey);
    this.currentUser.set(null);
  }

  private readStoredUser(): UserDto | null {
    const raw = localStorage.getItem(APP_CONSTANTS.userKey);
    return raw ? (JSON.parse(raw) as UserDto) : null;
  }
}
