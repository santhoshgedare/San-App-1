import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../core/auth/auth.service';
import { AddressDto, AddressInput, AddressService, emptyAddress, formatAddress, validateAddress } from '../../core/auth/address.service';
import { AddressFields } from '../../shared/address-fields/address-fields';
import { ConfirmService } from '../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, MatIconModule, AddressFields],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly addressService = inject(AddressService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly confirmService = inject(ConfirmService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly editingDetails = signal(false);
  readonly savingDetails = signal(false);
  readonly completing = signal(false);
  detailsError = '';
  details = { firstName: '', lastName: '', email: '', phoneNumber: '' };

  readonly addresses = signal<AddressDto[]>([]);
  readonly editing = signal<AddressInput | null>(null);
  editingId: string | null = null;
  readonly format = formatAddress;

  ngOnInit(): void {
    this.load();
    if (this.route.snapshot.queryParamMap.get('complete') === '1' || this.auth.profileIncomplete()) {
      this.completing.set(this.route.snapshot.queryParamMap.get('complete') === '1');
      this.startEditDetails();
    }
  }

  startEditDetails(): void {
    const user = this.auth.currentUser();
    this.details = {
      firstName: user?.firstName ?? '',
      lastName: user?.lastName ?? '',
      email: user?.email ?? '',
      phoneNumber: user?.phoneNumber ?? '',
    };
    this.detailsError = '';
    this.editingDetails.set(true);
  }

  cancelDetails(): void {
    this.editingDetails.set(false);
    this.completing.set(false);
  }

  saveDetails(): void {
    const d = this.details;
    if (!d.firstName.trim()) {
      this.detailsError = 'First name is required.';
      return;
    }
    if (!d.email.trim() && !d.phoneNumber.trim()) {
      this.detailsError = 'Provide an email address or a phone number.';
      return;
    }
    this.detailsError = '';
    this.savingDetails.set(true);
    this.auth.updateProfile(d).subscribe({
      next: () => {
        this.savingDetails.set(false);
        this.editingDetails.set(false);
        this.snackBar.open('Profile updated.', 'Close');
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (this.completing()) {
          this.completing.set(false);
          this.router.navigateByUrl(returnUrl || '/catalog');
        }
      },
      error: (err) => {
        this.savingDetails.set(false);
        this.detailsError = err?.error?.errors?.join(' ') ?? 'Could not update your profile.';
      },
    });
  }

  private load(): void {
    this.addressService.getMine().subscribe((list) => this.addresses.set(list));
  }

  startAdd(): void {
    const user = this.auth.currentUser();
    this.editingId = null;
    this.editing.set({
      ...emptyAddress(`${user?.firstName ?? ''} ${user?.lastName ?? ''}`.trim(), user?.phoneNumber ?? ''),
      label: 'Home',
      isDefault: this.addresses().length === 0,
    });
  }

  startEdit(address: AddressDto): void {
    this.editingId = address.id;
    this.editing.set({ ...address });
  }

  cancel(): void {
    this.editing.set(null);
  }

  save(): void {
    const model = this.editing();
    if (!model) return;
    const problem = validateAddress(model);
    if (problem) {
      this.snackBar.open(problem, 'Close', { panelClass: ['snack-error'] });
      return;
    }
    const request = this.editingId ? this.addressService.update(this.editingId, model) : this.addressService.create(model);
    request.subscribe({
      next: () => {
        this.editing.set(null);
        this.snackBar.open('Address saved.', 'Close');
        this.load();
      },
      error: (err) =>
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not save address.', 'Close', { panelClass: ['snack-error'] }),
    });
  }

  makeDefault(address: AddressDto): void {
    this.addressService.setDefault(address.id).subscribe(() => this.load());
  }

  remove(address: AddressDto): void {
    if (this.addresses().length <= 1) {
      this.snackBar.open('You need at least one address.', 'Close', { panelClass: ['snack-error'] });
      return;
    }
    this.confirmService
      .confirm({ title: 'Delete address', message: 'Remove this address from your account?', confirmText: 'Delete', destructive: true })
      .subscribe((ok) => {
        if (!ok) return;
        this.addressService.delete(address.id).subscribe({
          next: () => this.load(),
          error: () => this.snackBar.open('Could not delete address.', 'Close', { panelClass: ['snack-error'] }),
        });
      });
  }
}