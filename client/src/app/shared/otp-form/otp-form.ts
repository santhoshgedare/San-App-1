import { Component, OnDestroy, inject, input, output, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import type { ApiError, OtpPurpose } from '../../core/models/auth.models';

/** Two-step email/phone one-time-code form used for both sign-in and registration. */
@Component({
  selector: 'app-otp-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    @if (step() === 'identifier') {
      <form [formGroup]="identifierForm" (ngSubmit)="sendCode()">
        @if (purpose() === 'register') {
          <div class="row">
            <div class="col-6 mb-3">
              <label class="form-label" for="otp-first-name">First name</label>
              <input id="otp-first-name" type="text" class="form-control" formControlName="firstName" autocomplete="given-name" placeholder="Jane" />
            </div>
            <div class="col-6 mb-3">
              <label class="form-label" for="otp-last-name">Last name</label>
              <input id="otp-last-name" type="text" class="form-control" formControlName="lastName" autocomplete="family-name" placeholder="Doe" />
            </div>
          </div>
        }
        <div class="mb-3">
          <label class="form-label" for="otp-identifier">Email or phone number</label>
          <input id="otp-identifier" type="text" class="form-control" formControlName="identifier" autocomplete="username"
            placeholder="you@example.com or +91 98765 43210" />
          @if (identifierForm.controls.identifier.invalid && identifierForm.controls.identifier.touched) {
            <div class="form-text text-danger">Enter your email address or phone number.</div>
          }
        </div>

        @if (errorMessage(); as message) {
          <div class="alert alert-danger py-2 small" role="alert">{{ message }}</div>
        }

        <button type="submit" class="btn btn-primary w-100" [disabled]="isBusy()">
          @if (isBusy()) { <span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span> }
          {{ isBusy() ? 'Sending code...' : 'Send code' }}
        </button>
      </form>
    } @else {
      <form [formGroup]="codeForm" (ngSubmit)="verifyCode()">
        <p class="small text-muted mb-3">
          We sent a 6-digit code to <strong>{{ identifierForm.controls.identifier.value }}</strong>.
        </p>
        <div class="mb-3">
          <label class="form-label" for="otp-code">Verification code</label>
          <input id="otp-code" type="text" inputmode="numeric" maxlength="6" class="form-control text-center fs-4"
            formControlName="code" autocomplete="one-time-code" placeholder="••••••" />
          @if (codeForm.controls.code.invalid && codeForm.controls.code.touched) {
            <div class="form-text text-danger">Enter the 6-digit code.</div>
          }
        </div>

        @if (errorMessage(); as message) {
          <div class="alert alert-danger py-2 small" role="alert">{{ message }}</div>
        }

        <button type="submit" class="btn btn-primary w-100" [disabled]="isBusy()">
          @if (isBusy()) { <span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span> }
          {{ isBusy() ? 'Verifying...' : purpose() === 'register' ? 'Verify & create account' : 'Verify & sign in' }}
        </button>

        <div class="d-flex justify-content-between small mt-3">
          <button type="button" class="btn btn-link btn-sm p-0" (click)="changeIdentifier()">Change email / phone</button>
          <button type="button" class="btn btn-link btn-sm p-0" [disabled]="cooldown() > 0 || isBusy()" (click)="sendCode()">
            {{ cooldown() > 0 ? 'Resend in ' + cooldown() + 's' : 'Resend code' }}
          </button>
        </div>
      </form>
    }
  `,
})
export class OtpForm implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly purpose = input.required<OtpPurpose>();
  readonly completed = output<void>();

  readonly step = signal<'identifier' | 'code'>('identifier');
  readonly isBusy = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly cooldown = signal(0);
  private timer: number | null = null;

  readonly identifierForm = this.fb.nonNullable.group({
    firstName: [''],
    lastName: [''],
    identifier: ['', [Validators.required]],
  });

  readonly codeForm = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  ngOnDestroy(): void {
    this.stopTimer();
  }

  sendCode(): void {
    const { firstName, lastName, identifier } = this.identifierForm.getRawValue();
    const registering = this.purpose() === 'register';
    if (this.identifierForm.controls.identifier.invalid) {
      this.identifierForm.markAllAsTouched();
      return;
    }
    if (registering && (!firstName.trim() || !lastName.trim())) {
      this.errorMessage.set('First and last name are required.');
      return;
    }

    this.isBusy.set(true);
    this.errorMessage.set(null);
    this.auth
      .requestOtp({
        identifier: identifier.trim(),
        purpose: this.purpose(),
        firstName: registering ? firstName.trim() : undefined,
        lastName: registering ? lastName.trim() : undefined,
      })
      .subscribe({
        next: () => {
          this.isBusy.set(false);
          this.codeForm.reset();
          this.step.set('code');
          this.startCooldown();
        },
        error: (error: HttpErrorResponse) => {
          this.isBusy.set(false);
          this.errorMessage.set(this.messageFrom(error, 'Unable to send the code. Please try again.'));
        },
      });
  }

  verifyCode(): void {
    if (this.codeForm.invalid) {
      this.codeForm.markAllAsTouched();
      return;
    }

    const { firstName, lastName, identifier } = this.identifierForm.getRawValue();
    const registering = this.purpose() === 'register';
    this.isBusy.set(true);
    this.errorMessage.set(null);
    this.auth
      .verifyOtp({
        identifier: identifier.trim(),
        code: this.codeForm.getRawValue().code,
        purpose: this.purpose(),
        firstName: registering ? firstName.trim() : undefined,
        lastName: registering ? lastName.trim() : undefined,
      })
      .subscribe({
        next: () => {
          this.isBusy.set(false);
          this.completed.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.isBusy.set(false);
          this.errorMessage.set(this.messageFrom(error, 'Unable to verify the code. Please try again.'));
        },
      });
  }

  changeIdentifier(): void {
    this.stopTimer();
    this.cooldown.set(0);
    this.errorMessage.set(null);
    this.step.set('identifier');
  }

  private startCooldown(): void {
    this.stopTimer();
    this.cooldown.set(30);
    this.timer = window.setInterval(() => {
      this.cooldown.update((value) => Math.max(0, value - 1));
      if (this.cooldown() === 0) this.stopTimer();
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timer !== null) window.clearInterval(this.timer);
    this.timer = null;
  }

  private messageFrom(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 429) return 'Too many attempts. Please wait a few minutes and try again.';
    const apiError = error.error as ApiError | undefined;
    return apiError?.errors?.[0] ?? fallback;
  }
}
