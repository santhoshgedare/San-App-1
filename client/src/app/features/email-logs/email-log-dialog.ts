import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import type { EmailLogDto } from '../../core/auth/email-log.service';

@Component({
  selector: 'app-email-log-dialog',
  standalone: true,
  imports: [DatePipe, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="head">
      <div class="min-w-0">
        <div class="small text-uppercase opacity-75">{{ m.category }}</div>
        <h2 class="subject">{{ m.subject }}</h2>
      </div>
      <button type="button" mat-icon-button mat-dialog-close aria-label="Close"><mat-icon>close</mat-icon></button>
    </div>
    <div class="body">
      <span class="badge" [class.text-bg-success]="m.status === 'Sent'" [class.text-bg-danger]="m.status === 'Failed'">{{ m.status }}</span>
      <div class="meta">
        <div><span>To</span>{{ m.toAddresses }}</div>
        @if (m.ccAddresses) { <div><span>Cc</span>{{ m.ccAddresses }}</div> }
        @if (m.attachments) { <div><span>Files</span>{{ m.attachments }}</div> }
        <div><span>Created</span>{{ m.createdAt | date: 'medium' }}{{ m.createdByEmail ? ' by ' + m.createdByEmail : '' }}</div>
        @if (m.sentAt) { <div><span>Sent</span>{{ m.sentAt | date: 'medium' }}</div> }
      </div>
      @if (m.error) { <div class="alert alert-danger small py-2">{{ m.error }}</div> }
      <pre class="text">{{ m.textBody }}</pre>
    </div>
    <div class="foot"><button mat-stroked-button mat-dialog-close>Close</button></div>
  `,
  styles: `
    :host { display: block; }
    .head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; padding: .9rem .75rem .9rem 1.25rem; background: #76283a; color: #fffdfa; }
    .head button { color: inherit; }
    .subject { font-size: 1.1rem; margin: 0; color: inherit; word-break: break-word; }
    .body { padding: 1.1rem 1.25rem; max-height: 60vh; overflow: auto; }
    .meta { margin: .75rem 0; font-size: .85rem; display: grid; gap: .25rem; }
    .meta span { display: inline-block; width: 4.5rem; color: #716068; }
    .text { white-space: pre-wrap; background: #faf5ec; border-radius: 8px; padding: .9rem; font-size: .85rem; margin: 0; font-family: inherit; }
    .foot { display: flex; justify-content: flex-end; padding: .75rem 1.25rem; border-top: 1px solid #eadfd0; }
  `,
})
export class EmailLogDialog {
  protected readonly m = inject<EmailLogDto>(MAT_DIALOG_DATA);
}
