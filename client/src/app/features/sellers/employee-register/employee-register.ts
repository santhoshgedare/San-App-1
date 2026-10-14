import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { SellerService } from '../../../core/auth/seller.service';
import { PasswordField } from '../../../shared/password-field/password-field';

@Component({
  selector: 'app-employee-register',
  standalone: true,
  imports: [FormsModule, RouterLink, PasswordField],
  templateUrl: './employee-register.html',
  styleUrl: '../../auth/register/register.scss',
})
export class EmployeeRegister implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sellers = inject(SellerService);
  private readonly auth = inject(AuthService);

  private token = '';
  readonly invite = signal<{ email: string; companyName: string } | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  firstName = '';
  lastName = '';
  phoneNumber = '';
  password = '';
  confirmPassword = '';

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.sellers.getEmployeeInvite(this.token).subscribe({
      next: (info) => {
        this.invite.set(info);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('This invitation is invalid or has expired. Please ask for a new one.');
        this.isLoading.set(false);
      },
    });
  }

  submit(): void {
    this.errorMessage.set(null);
    if (!this.firstName.trim() || !this.lastName.trim()) return this.errorMessage.set('First and last name are required.');
    if (this.password.length < 8) return this.errorMessage.set('Password must be at least 8 characters.');
    if (this.password !== this.confirmPassword) return this.errorMessage.set('Passwords do not match.');

    this.isSubmitting.set(true);
    this.sellers
      .acceptEmployeeInvite(this.token, {
        firstName: this.firstName.trim(),
        lastName: this.lastName.trim(),
        password: this.password,
        phoneNumber: this.phoneNumber.trim() || null,
      })
      .subscribe({
        next: (result) => {
          this.auth.startSession(result);
          this.router.navigateByUrl('/orders');
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(err?.error?.errors?.join(' ') ?? 'Registration failed. Please try again.');
        },
      });
  }
}
