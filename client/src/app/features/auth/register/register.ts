import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PasswordField } from '../../../shared/password-field/password-field';
import { AuthService } from '../../../core/auth/auth.service';
import { AddressFields } from '../../../shared/address-fields/address-fields';
import { emptyAddress, validateAddress } from '../../../core/auth/address.service';
import type { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [AddressFields, PasswordField, ReactiveFormsModule, RouterLink],
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

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[+\d][\d\s-]{6,19}$/)]],
  });

  readonly address = emptyAddress();

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const addressProblem = validateAddress(this.address);
    if (addressProblem) {
      this.errorMessage.set(addressProblem);
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();
    const address = {
      ...this.address,
      fullName: ` `.trim(),
      phone: value.phoneNumber,
    };

    this.auth.register({ ...value, address }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigateByUrl(this.returnUrl() || '/catalog');
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = error.error as ApiError | undefined;
        this.errorMessage.set(apiError?.errors?.[0] ?? 'Unable to create account. Please try again.');
      },
    });
  }
}
