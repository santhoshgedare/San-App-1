import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PasswordField } from '../../../shared/password-field/password-field';
import { SocialLogin } from '../../../shared/social-login/social-login';
import type { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [PasswordField, ReactiveFormsModule, RouterLink, SocialLogin],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly returnUrl = signal<string | null>(this.route.snapshot.queryParamMap.get('returnUrl'));

  private readonly validateIdentifier = (control: AbstractControl): ValidationErrors | null => {
    const identifier = String(control.value ?? '').trim();
    if (!identifier) return null;
    return identifier.includes('@')
      ? Validators.email(control)
      : /^[+\d][\d\s-]{6,19}$/.test(identifier)
        ? null
        : { phone: true };
  };

  readonly form = this.fb.nonNullable.group({
    identifier: ['', [Validators.required, this.validateIdentifier]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { identifier, password } = this.form.getRawValue();
    const trimmedIdentifier = identifier.trim();
    const request = trimmedIdentifier.includes('@')
      ? { email: trimmedIdentifier, password }
      : { phoneNumber: trimmedIdentifier, password };

    this.auth.register(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.onRegistered();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = error.error as ApiError | undefined;
        this.errorMessage.set(apiError?.errors?.[0] ?? 'Unable to create account. Please try again.');
      },
    });
  }

  onRegistered(): void {
    this.router.navigateByUrl(this.auth.postAuthUrl(this.returnUrl()));
  }
}
