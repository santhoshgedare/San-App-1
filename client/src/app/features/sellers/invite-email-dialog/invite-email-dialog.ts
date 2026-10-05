import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SellerService } from '../../../core/auth/seller.service';

export interface InviteEmailDialogData {
  sellerId: string;
  to: string;
  subject: string;
  body: string;
}

const MAX_FILES = 5;
const MAX_TOTAL_BYTES = 10 * 1024 * 1024;

/** Mail-style composer used to review and send a seller invitation. */
@Component({
  selector: 'app-invite-email-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './invite-email-dialog.html',
  styleUrl: './invite-email-dialog.scss',
})
export class InviteEmailDialog {
  protected readonly data = inject<InviteEmailDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<InviteEmailDialog, boolean>);
  private readonly sellers = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);

  to = this.data.to;
  cc = '';
  subject = this.data.subject;
  body = this.data.body;
  readonly files = signal<File[]>([]);
  readonly isSending = signal(false);
  readonly error = signal<string | null>(null);

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const merged = [...this.files(), ...Array.from(input.files ?? [])];
    input.value = '';
    if (merged.length > MAX_FILES || merged.reduce((sum, f) => sum + f.size, 0) > MAX_TOTAL_BYTES) {
      this.error.set('Attach at most 5 files totalling 10 MB.');
      return;
    }
    this.error.set(null);
    this.files.set(merged);
  }

  removeFile(index: number): void {
    this.files.update((list) => list.filter((_, i) => i !== index));
  }

  size(file: File): string {
    return file.size >= 1024 * 1024 ? (file.size / (1024 * 1024)).toFixed(1) + ' MB' : Math.max(1, Math.round(file.size / 1024)) + ' KB';
  }

  send(): void {
    if (!this.to.trim() || !this.subject.trim() || !this.body.trim()) {
      this.error.set('To, subject and message are required.');
      return;
    }

    const form = new FormData();
    form.append('to', this.to);
    form.append('cc', this.cc);
    form.append('subject', this.subject);
    form.append('body', this.body);
    this.files().forEach((f) => form.append('files', f, f.name));

    this.isSending.set(true);
    this.error.set(null);
    this.sellers.sendInviteEmail(this.data.sellerId, form).subscribe({
      next: () => {
        this.isSending.set(false);
        this.snackBar.open('Invitation email sent.', 'Close');
        this.ref.close(true);
      },
      error: (err) => {
        this.isSending.set(false);
        this.error.set(err?.error?.errors?.join(' ') ?? 'Could not send the email.');
      },
    });
  }
}
