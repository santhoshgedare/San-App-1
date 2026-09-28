import { Component, OnChanges, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ActivityLogService } from '../../core/auth/activity-log.service';
import type { ActivityLogDto } from '../../core/models/activity-log.models';

/**
 * Renders the audit trail for a single entity instance, looked up generically by
 * (entityType, entityId) so it can be embedded on any detail/edit page (Users, Roles, ...).
 */
@Component({
  selector: 'app-activity-log-panel',
  standalone: true,
  imports: [DatePipe, MatIconModule],
  templateUrl: './activity-log-panel.html',
  styleUrl: './activity-log-panel.scss',
})
export class ActivityLogPanel implements OnChanges {
  private readonly activityLogService = inject(ActivityLogService);

  readonly entityType = input.required<string>();
  readonly entityId = input.required<string>();

  readonly entries = signal<ActivityLogDto[]>([]);
  readonly isLoading = signal(true);

  ngOnChanges(): void {
    if (!this.entityType() || !this.entityId()) {
      return;
    }
    this.isLoading.set(true);
    this.activityLogService.getForEntity(this.entityType(), this.entityId()).subscribe({
      next: (entries) => {
        this.entries.set(entries);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
