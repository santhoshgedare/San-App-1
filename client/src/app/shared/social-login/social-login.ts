import { Component, OnDestroy, OnInit, inject, output, signal } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment';
import type { AuthResult, ExternalAuthProvider } from '../../core/models/auth.models';

interface ExternalAuthPopupMessage {
  type: 'identityhub:external-login';
  state: string;
  result?: AuthResult | null;
  error?: string | null;
}

/** Google / Facebook SSO buttons. The API creates the account on first sign-in, so this serves login and registration. */
@Component({
  selector: 'app-social-login',
  standalone: true,
  template: `
    <div class="social-login" aria-label="Other sign-in options">
      <div class="social-divider"><span>or continue with</span></div>
      <div class="social-provider-list">
        @for (provider of externalProviders(); track provider.provider) {
          <button type="button" class="btn btn-outline-secondary social-provider"
            [disabled]="!provider.enabled || isBusy()" (click)="beginExternalLogin(provider)">
            @if (provider.provider === 'google') {
              <i class="bi bi-google google-mark" aria-hidden="true"></i>
            } @else {
              <i class="bi bi-facebook facebook-mark" aria-hidden="true"></i>
            }
            <span>{{ provider.displayName }}</span>
          </button>
        }
      </div>
      @if (errorMessage(); as message) {
        <div class="alert alert-danger py-2 small mb-0" role="alert">{{ message }}</div>
      }
      @if (providerDiscoveryFailed()) {
        <p class="provider-note" role="status">Provider status is unavailable. Restart the API and reload to check sign-in options.</p>
      } @else if (providersLoading()) {
        <p class="provider-note" role="status">Checking sign-in options...</p>
      } @else if (!externalProviders().some((provider) => provider.enabled)) {
        <p class="provider-note">Social sign-in is not configured yet.</p>
      }
    </div>
  `,
  styles: `
    .social-login { display: grid; gap: 0.65rem; margin-top: 1.1rem; }
    .social-divider { display: flex; align-items: center; gap: 0.75rem; color: #75818c; font-size: 0.75rem; }
    .social-divider::before, .social-divider::after { content: ''; height: 1px; flex: 1; background: #dce2e8; }
    .social-provider-list { display: grid; grid-template-columns: 1fr; gap: 0.55rem; }
    .social-provider { display: flex; align-items: center; justify-content: center; gap: 0.65rem; min-height: 2.75rem; font-weight: 600; font-size: 0.85rem; }
    .google-mark { color: #4285f4; font-size: 1rem; }
    .facebook-mark { color: #1877f2; font-size: 1.1rem; }
    .provider-note { margin: 0.1rem 0 0; color: #777a72; font-size: 0.72rem; line-height: 1.45; text-align: center; }
  `,
})
export class SocialLogin implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);

  readonly completed = output<void>();

  readonly externalProviders = signal<ExternalAuthProvider[]>([]);
  readonly providersLoading = signal(true);
  readonly providerDiscoveryFailed = signal(false);
  readonly isBusy = signal(false);
  readonly errorMessage = signal<string | null>(null);
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
      this.completed.emit();
      return;
    }
    this.errorMessage.set(message.error || 'External sign-in could not be completed.');
  };

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

  beginExternalLogin(provider: ExternalAuthProvider): void {
    if (!provider.enabled || this.isBusy()) return;

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

    this.isBusy.set(true);
    this.popupCheck = window.setInterval(() => {
      if (this.externalPopup?.closed) this.finishExternalFlow();
    }, 400);
  }

  private finishExternalFlow(): void {
    window.removeEventListener('message', this.onExternalMessage);
    if (this.popupCheck !== null) window.clearInterval(this.popupCheck);
    this.popupCheck = null;
    this.expectedExternalState = null;
    this.externalPopup = null;
    this.isBusy.set(false);
  }

  private fallbackProviders(): ExternalAuthProvider[] {
    return [
      { provider: 'google', displayName: 'Google', enabled: false },
      { provider: 'facebook', displayName: 'Facebook (Meta)', enabled: false },
    ];
  }
}
