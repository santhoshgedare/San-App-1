import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PaymentSettingsService } from '../../core/auth/payment-settings.service';
import { SectionAccessStore } from '../../core/auth/section-access.store';
import { CanRenderDirective } from '../../core/directives/can-render.directive';
import { readImageAsDataUrl } from '../../shared/image-utils';

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
  readonly isProcessingImage = signal(false);
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
      error: (err) => {
        this.errorMessage.set(err?.error?.errors?.join(' ') ?? 'Could not load payment settings.');
        this.isLoading.set(false);
      },
    });
  }

  async onQrFileSelected(event: Event): Promise<void> {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    target.value = '';
    if (!file || this.isProcessingImage()) return;

    this.isProcessingImage.set(true);
    this.errorMessage.set(null);
    try {
      this.qrCodeImageUrl = await readImageAsDataUrl(file, 1600, 0.95);
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Could not process the QR image.');
    } finally {
      this.isProcessingImage.set(false);
    }
  }

  removeQrImage(): void {
    this.qrCodeImageUrl = null;
  }

  save(): void {
    if (this.isProcessingImage()) return;
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
