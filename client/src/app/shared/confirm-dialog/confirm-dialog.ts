import { Component, Injectable, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Observable, map } from 'rxjs';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  destructive?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="confirm-dialog">
      <div class="confirm-icon" [class.danger]="data.destructive"><mat-icon>{{ data.destructive ? 'warning_amber' : 'help_outline' }}</mat-icon></div>
      <h2 mat-dialog-title>{{ data.title }}</h2>
      <mat-dialog-content>{{ data.message }}</mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-stroked-button [mat-dialog-close]="false">{{ data.cancelText || 'Keep' }}</button>
        <button mat-flat-button class="confirm-yes" [class.danger]="data.destructive" [mat-dialog-close]="true" cdkFocusInitial>
          {{ data.confirmText || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: `
    .confirm-dialog { padding: .5rem .25rem; }
    .confirm-icon { width: 2.6rem; height: 2.6rem; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: .5rem 0 0 1.4rem; background: #f2e2c9; color: #76283a; }
    .confirm-icon.danger { background: #f6dde1; color: #a3202f; }
    h2 { color: #382a2d; }
    mat-dialog-content { color: #716068; }
    .confirm-yes { background: #76283a; color: #fffdfa; }
    .confirm-yes.danger { background: #a3202f; }
  `,
})
export class ConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly dialog = inject(MatDialog);

  confirm(data: ConfirmDialogData): Observable<boolean> {
    return this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data, width: '420px', maxWidth: '92vw', autoFocus: false })
      .afterClosed()
      .pipe(map((r) => r === true));
  }
}
