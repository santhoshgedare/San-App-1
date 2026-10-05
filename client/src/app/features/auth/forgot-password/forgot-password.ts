import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './forgot-password.html',
    styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
    private readonly fb = inject(FormBuilder);
    private readonly auth = inject(AuthService);

    readonly isSubmitting = signal(false);
    readonly requestSent = signal(false);
    readonly errorMessage = signal('');

    readonly form = this.fb.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
    });

    submit(): void {
        if (this.form.invalid || this.isSubmitting()) {
            this.form.markAllAsTouched();
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set('');
        this.auth.requestPasswordReset(this.form.controls.email.value.trim()).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.requestSent.set(true);
            },
            error: () => {
                this.isSubmitting.set(false);
                this.errorMessage.set('Password recovery email is temporarily unavailable. Please try again later.');
            },
        });
    }
}