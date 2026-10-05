import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
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

  readonly addresses = signal<AddressDto[]>([]);
  readonly editing = signal<AddressInput | null>(null);
  editingId: string | null = null;
  readonly format = formatAddress;

  ngOnInit(): void {
    this.load();
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