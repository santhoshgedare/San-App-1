import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { emptyAddress, validateAddress } from '../../../core/auth/address.service';
import { SellerInput, SellerInviteInfo, SellerService } from '../../../core/auth/seller.service';
import { AddressFields } from '../../../shared/address-fields/address-fields';
import { PasswordField } from '../../../shared/password-field/password-field';

@Component({
  selector: 'app-seller-register',
  standalone: true,
  imports: [FormsModule, RouterLink, AddressFields, PasswordField],
  templateUrl: './seller-register.html',
  styleUrl: '../../auth/register/register.scss',
})
export class SellerRegister implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sellers = inject(SellerService);
  private readonly auth = inject(AuthService);

  private token = '';
  readonly invite = signal<SellerInviteInfo | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  firstName = '';
  lastName = '';
  password = '';
  confirmPassword = '';
  phoneNumber = '';
  readonly address = emptyAddress();
  company: SellerInput = {
    userId: null,
    companyName: '',
    tagline: null,
    description: null,
    logoUrl: null,
    contactEmail: null,
    contactPhone: null,
    website: null,
    addressLine1: null,
    city: null,
    state: null,
    postalCode: null,
    country: 'India',
    upiId: null,
    qrCodeImageUrl: null,
    payeeName: null,
    bankDetails: null,
  };

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.sellers.getInvite(this.token).subscribe({
      next: (info) => {
        this.invite.set(info);
        this.company.companyName = info.companyName;
        this.company.tagline = info.tagline;
        this.company.logoUrl = info.logoUrl;
        this.company.contactEmail = info.email;
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('This invitation is invalid or has expired. Please ask for a new one.');
        this.isLoading.set(false);
      },
    });
  }

  onQrSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      this.errorMessage.set('QR image must be under 2 MB.');
      return;
    }
    const reader = new FileReader();
    reader.onload = () => (this.company.qrCodeImageUrl = reader.result as string);
    reader.readAsDataURL(file);
  }
  submit(): void {
    this.errorMessage.set(null);
    if (!this.firstName.trim() || !this.lastName.trim()) return this.fail('First and last name are required.');
    if (this.password.length < 8) return this.fail('Password must be at least 8 characters.');
    if (this.password !== this.confirmPassword) return this.fail('Passwords do not match.');
    if (!this.phoneNumber.trim()) return this.fail('Phone number is required.');
    if (!this.company.companyName.trim()) return this.fail('Company name is required.');
    const problem = validateAddress(this.address);
    if (problem) return this.fail(problem);
    if (!this.company.upiId?.trim() && !this.company.bankDetails?.trim()) {
      return this.fail('Add a UPI ID or bank details so customers can pay you.');
    }

    this.isSubmitting.set(true);
    this.sellers
      .acceptInvite(this.token, {
        firstName: this.firstName.trim(),
        lastName: this.lastName.trim(),
        password: this.password,
        phoneNumber: this.phoneNumber.trim(),
        address: this.address,
        company: { ...this.company, contactPhone: this.company.contactPhone || this.phoneNumber.trim() },
      })
      .subscribe({
        next: (result) => {
          this.auth.startSession(result);
          this.router.navigateByUrl('/seller-profile');
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.fail(err?.error?.errors?.join(' ') ?? 'Registration failed. Please try again.');
        },
      });
  }

  private fail(message: string): void {
    this.errorMessage.set(message);
  }
}