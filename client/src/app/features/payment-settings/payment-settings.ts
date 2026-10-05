import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PaymentSettingsService } from '../../core/auth/payment-settings.service';
import { SectionAccessStore } from '../../core/auth/section-access.store';
import { CanRenderDirective } from '../../core/directives/can-render.directive';

@Component({
  selector: 'app-payment-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, MatSnackBarModule, CanRenderDirective],
  templateUrl: './payment-settings.html',
  styleUrl: './payment-settings.scss',
})
export class PaymentSettingsComponent implements OnInit {
  private readonly paymentSettingsService = inject(PaymentSettingsService);
  protected readonly sectionAccess = inject(SectionAccessStore);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  upiId = '';
  payeeName = '';
  instructions = '';
  qrCodeImageUrl: string | null = null;

  canManageSettings(): boolean {
    return this.sectionAccess.can('section-payment-settings-manage');
  }

  ngOnInit(): void {
    this.paymentSettingsService.get().subscribe({
      next: (settings) => {
        this.upiId = settings.upiId ?? '';
        this.payeeName = settings.payeeName ?? '';
        this.instructions = settings.instructions ?? '';
        this.qrCodeImageUrl = settings.qrCodeImageUrl ?? null;
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  onQrFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (!target.files || target.files.length === 0) {
      return;
    }

    const file = target.files[0];
    const reader = new FileReader();
    reader.onload = () => {
      this.qrCodeImageUrl = reader.result as string;
    };
    reader.readAsDataURL(file);
    target.value = '';
  }

  removeQrImage(): void {
    this.qrCodeImageUrl = null;
  }

  save(): void {
    if (!this.upiId.trim()) {
      this.errorMessage.set('UPI ID is required.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    this.paymentSettingsService
      .update({
        upiId: this.upiId.trim(),
        payeeName: this.payeeName.trim() || null,
        qrCodeImageUrl: this.qrCodeImageUrl,
        instructions: this.instructions.trim() || null,
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.snackBar.open('Payment settings saved successfully!', 'Close', { duration: 4000 });
        },
        error: (err) => {
          this.isSaving.set(false);
          const errorList = err?.error?.errors;
          this.errorMessage.set(Array.isArray(errorList) ? errorList.join(' ') : 'Failed to save payment settings.');
        },
      });
  }
}
