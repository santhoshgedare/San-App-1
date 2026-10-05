import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReviewService } from '../../core/auth/review.service';
import type { ItemReviewDto } from '../../core/models/auth.models';
import { StarRating } from '../star-rating/star-rating';

const MAX_PHOTOS = 4;
const MAX_EDGE = 1000;

@Component({
  selector: 'app-review-form',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatSnackBarModule, StarRating],
  templateUrl: './review-form.html',
  styleUrl: './review-form.scss',
})
export class ReviewForm {
  private readonly reviews = inject(ReviewService);
  private readonly snackBar = inject(MatSnackBar);

  readonly orderItemId = input.required<string>();
  readonly itemName = input('');
  readonly submitted = output<ItemReviewDto>();
  readonly cancelled = output<void>();

  protected readonly rating = signal(0);
  protected readonly photos = signal<string[]>([]);
  protected readonly isSubmitting = signal(false);
  protected readonly isProcessing = signal(false);
  protected readonly maxPhotos = MAX_PHOTOS;
  protected title = '';
  protected comment = '';

  protected async onFilesSelected(event: Event): Promise<void> {
    const fileInput = event.target as HTMLInputElement;
    const files = Array.from(fileInput.files ?? []).filter((f) => f.type.startsWith('image/'));
    fileInput.value = '';
    const room = MAX_PHOTOS - this.photos().length;
    if (files.length === 0 || room <= 0) return;

    this.isProcessing.set(true);
    try {
      const added: string[] = [];
      for (const file of files.slice(0, room)) {
        added.push(await this.compress(file));
      }
      this.photos.update((p) => [...p, ...added]);
    } catch {
      this.snackBar.open('Could not read that image', 'Close', { duration: 3000, panelClass: ['snack-error'] });
    } finally {
      this.isProcessing.set(false);
    }
  }

  protected removePhoto(index: number): void {
    this.photos.update((p) => p.filter((_, i) => i !== index));
  }

  protected submit(): void {
    if (this.rating() < 1) {
      this.snackBar.open('Please choose a star rating', 'Close', { duration: 3000, panelClass: ['snack-error'] });
      return;
    }
    this.isSubmitting.set(true);
    this.reviews
      .create({
        orderItemId: this.orderItemId(),
        rating: this.rating(),
        title: this.title.trim() || undefined,
        comment: this.comment.trim() || undefined,
        images: this.photos(),
      })
      .subscribe({
        next: (review) => {
          this.isSubmitting.set(false);
          this.snackBar.open('Thanks for your review!', 'Close', { duration: 3000 });
          this.submitted.emit(review);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          const msg = err?.error?.errors?.[0] ?? 'Could not submit your review';
          this.snackBar.open(msg, 'Close', { duration: 4000 });
        },
      });
  }

  // Resize and re-encode as JPEG so uploads stay small.
  private compress(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const url = URL.createObjectURL(file);
      const img = new Image();
      img.onload = () => {
        const scale = Math.min(1, MAX_EDGE / Math.max(img.width, img.height));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(img.width * scale);
        canvas.height = Math.round(img.height * scale);
        const ctx = canvas.getContext('2d');
        URL.revokeObjectURL(url);
        if (!ctx) {
          reject(new Error('canvas'));
          return;
        }
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        resolve(canvas.toDataURL('image/jpeg', 0.8));
      };
      img.onerror = () => {
        URL.revokeObjectURL(url);
        reject(new Error('load'));
      };
      img.src = url;
    });
  }
}
