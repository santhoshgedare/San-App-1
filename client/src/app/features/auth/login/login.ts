import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PasswordField } from '../../../shared/password-field/password-field';
import { AuthService } from '../../../core/auth/auth.service';
import { environment } from '../../../../environments/environment';
import type { ApiError, AuthResult, ExternalAuthProvider } from '../../../core/models/auth.models';

interface ExternalAuthPopupMessage {
  type: 'identityhub:external-login';
  state: string;
  result?: AuthResult | null;
  error?: string | null;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [PasswordField, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly externalProviders = signal<ExternalAuthProvider[]>([]);
  readonly providersLoading = signal(true);
  readonly providerDiscoveryFailed = signal(false);
  readonly returnUrl = signal<string | null>(this.route.snapshot.queryParamMap.get('returnUrl'));
  private expectedExternalState: string | null = null;
  private externalPopup: Window | null = null;
  private popupCheck: number | null = null;

  private readonly onExternalMessage = (event: MessageEvent<ExternalAuthPopupMessage>): void => {
    if (event.origin !== new URL(environment.apiUrl).origin || !this.expectedExternalState) return;
    const message = event.data;
    if (message?.type !== 'identityhub:external-login' || message.state !== this.expectedExternalState) return;

    this.finishExternalFlow();
    if (message.result) {
      this.auth.completeExternalLogin(message.result);
      this.router.navigateByUrl(this.returnUrl() || '/catalog');
      return;
    }
    this.errorMessage.set(message.error || 'External sign-in could not be completed.');
  };

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.auth.getExternalProviders().subscribe({
      next: (providers) => {
        this.externalProviders.set(providers.length ? providers : this.fallbackProviders());
        this.providersLoading.set(false);
        this.providerDiscoveryFailed.set(false);
      },
      error: () => {
        this.externalProviders.set(this.fallbackProviders());
        this.providersLoading.set(false);
        this.providerDiscoveryFailed.set(true);
      },
    });
  }

  ngOnDestroy(): void {
    this.finishExternalFlow();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigateByUrl(this.returnUrl() || '/catalog');
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = error.error as ApiError | undefined;
        this.errorMessage.set(apiError?.errors?.[0] ?? 'Unable to sign in. Please try again.');
      },
    });
  }

  beginExternalLogin(provider: ExternalAuthProvider): void {
    if (!provider.enabled || this.isSubmitting()) return;

    const state = crypto.randomUUID();
    const url = new URL(`${environment.apiUrl}/auth/external/${provider.provider}`);
    url.searchParams.set('state', state);
    url.searchParams.set('clientOrigin', window.location.origin);
    this.expectedExternalState = state;
    this.errorMessage.set(null);
    window.addEventListener('message', this.onExternalMessage);

    this.externalPopup = window.open(url.toString(), 'identityhub-external-login', 'popup,width=520,height=680');
    if (!this.externalPopup) {
      this.finishExternalFlow();
      this.errorMessage.set('Allow popups to continue with external sign-in.');
      return;
    }

    this.isSubmitting.set(true);
    this.popupCheck = window.setInterval(() => {
      if (this.externalPopup?.closed) {
        this.finishExternalFlow();
        this.isSubmitting.set(false);
      }
    }, 400);
  }

  private finishExternalFlow(): void {
    window.removeEventListener('message', this.onExternalMessage);
    if (this.popupCheck !== null) window.clearInterval(this.popupCheck);
    this.popupCheck = null;
    this.expectedExternalState = null;
    this.externalPopup = null;
    this.isSubmitting.set(false);
  }

  private fallbackProviders(): ExternalAuthProvider[] {
    return [
      { provider: 'google', displayName: 'Google', enabled: false },
      { provider: 'facebook', displayName: 'Facebook (Meta)', enabled: false },
    ];
  }
}
