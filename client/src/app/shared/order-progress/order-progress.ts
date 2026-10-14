import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CustomRequestService } from '../../core/auth/custom-request.service';
import { OrderProgressUpdateDto } from '../../core/models/auth.models';
import { readImageAsDataUrl } from '../image-utils';

export const PROGRESS_STAGES: { key: string; label: string }[] = [
  { key: 'Accepted', label: 'Accepted' },
  { key: 'DesignFinalized', label: 'Design finalized' },
  { key: 'InProduction', label: 'In production' },
  { key: 'QualityCheck', label: 'Quality check' },
  { key: 'ReadyToShip', label: 'Ready to ship' },
];

@Component({
  selector: 'app-order-progress',
  standalone: true,
  imports: [DatePipe, FormsModule, MatButtonModule, MatIconModule],
  templateUrl: './order-progress.html',
  styleUrl: './order-progress.scss',
})
export class OrderProgress {
  private readonly service = inject(CustomRequestService);
  private readonly snackBar = inject(MatSnackBar);

  readonly orderId = input.required<string>();
  readonly updates = input<OrderProgressUpdateDto[]>([]);
  readonly canPost = input(false);
  readonly changed = output<void>();

  readonly stages = PROGRESS_STAGES;
  readonly saving = signal(false);
  readonly processingImage = signal(false);
  readonly image = signal<string | null>(null);
  stage = '';
  note = '';

  readonly currentIndex = computed(() => {
    const keys = this.updates().map((u) => PROGRESS_STAGES.findIndex((s) => s.key === u.stage));
    return keys.length ? Math.max(...keys) : -1;
  });

  readonly available = computed(() => PROGRESS_STAGES.filter((_, i) => i >= Math.max(this.currentIndex(), 0)));

  readonly sorted = computed(() =>
    [...this.updates()].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()),
  );

  label(stage: string): string {
    return PROGRESS_STAGES.find((s) => s.key === stage)?.label ?? stage;
  }

  async onFile(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || this.processingImage()) return;
    this.processingImage.set(true);
    try {
      this.image.set(await readImageAsDataUrl(file));
    } catch (e) {
      this.snackBar.open(e instanceof Error ? e.message : 'Could not process the selected image.', 'Close');
    } finally {
      this.processingImage.set(false);
    }
  }

  post(): void {
    if (this.processingImage()) return;
    const stage = this.stage || this.available()[0]?.key;
    if (!stage) return;
    this.saving.set(true);
    this.service.addProgress(this.orderId(), stage, this.note.trim() || null, this.image()).subscribe({
      next: () => {
        this.saving.set(false);
        this.note = '';
        this.stage = '';
        this.image.set(null);
        this.snackBar.open('Progress posted.', 'Close', { duration: 3000 });
        this.changed.emit();
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not post progress.', 'Close');
      },
    });
  }
}
