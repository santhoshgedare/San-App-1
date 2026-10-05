import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { EmailLogDialog } from './email-log-dialog';
import { EmailLogDto, EmailLogService } from '../../core/auth/email-log.service';

@Component({
  selector: 'app-email-logs',
  standalone: true,
  imports: [FormsModule, DatePipe, MatIconModule],
  templateUrl: './email-logs.html',
})
export class EmailLogs implements OnInit {
  private readonly service = inject(EmailLogService);
  private readonly dialog = inject(MatDialog);

  readonly logs = signal<EmailLogDto[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly isLoading = signal(false);
  readonly pageSize = 15;
  status = '';
  search = '';

  ngOnInit(): void {
    this.load();
  }

  load(page = 1): void {
    this.page.set(page);
    this.isLoading.set(true);
    this.service.getPaged(page, this.pageSize, this.status, this.search).subscribe({
      next: (r) => {
        this.logs.set(r.items);
        this.total.set(r.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  open(log: EmailLogDto): void {
    this.service.getById(log.id).subscribe((full) => this.dialog.open(EmailLogDialog, { data: full, width: '680px', maxWidth: '94vw', autoFocus: false, panelClass: 'flush-dialog' }));
  }
}
