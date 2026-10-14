import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PasswordField } from '../../../shared/password-field/password-field';
import { SocialLogin } from '../../../shared/social-login/social-login';
import { AuthService } from '../../../core/auth/auth.service';
import type { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [PasswordField, ReactiveFormsModule, RouterLink, SocialLogin],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly returnUrl = signal<string | null>(this.route.snapshot.queryParamMap.get('returnUrl'));

  readonly form = this.fb.nonNullable.group({
    identifier: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  onSignedIn(): void {
    this.router.navigateByUrl(this.auth.postAuthUrl(this.returnUrl()));
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
        this.onSignedIn();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = error.error as ApiError | undefined;
        this.errorMessage.set(apiError?.errors?.[0] ?? 'Unable to sign in. Please try again.');
      },
    });
  }
}
