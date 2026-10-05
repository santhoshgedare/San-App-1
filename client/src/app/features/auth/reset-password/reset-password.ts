import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PasswordField } from '../../../shared/password-field/password-field';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [PasswordField, ReactiveFormsModule, RouterLink],
    templateUrl: './reset-password.html',
    styleUrl: './reset-password.scss',
})
export class ResetPassword {
    private readonly fb = inject(FormBuilder);
    private readonly auth = inject(AuthService);
    private readonly route = inject(ActivatedRoute);

    readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
    readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
    readonly isSubmitting = signal(false);
    readonly isComplete = signal(false);
    readonly errorMessage = signal('');

    readonly form = this.fb.nonNullable.group({
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]],
    });

    get hasResetLink(): boolean {
        return !!this.email && !!this.token;
    }

    submit(): void {
        if (!this.hasResetLink) return;
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }
        if (this.form.controls.newPassword.value !== this.form.controls.confirmPassword.value) {
            this.errorMessage.set('The passwords do not match.');
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set('');
        this.auth.resetPassword(this.email, this.token, this.form.controls.newPassword.value).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.isComplete.set(true);
            },
            error: (error) => {
                this.isSubmitting.set(false);
                const messages = error?.error?.errors;
                this.errorMessage.set(Array.isArray(messages) ? messages.join(' ') : 'This reset link is invalid or has expired. Request a new link.');
            },
        });
    }
}