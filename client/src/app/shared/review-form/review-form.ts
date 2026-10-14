import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReviewService } from '../../core/auth/review.service';
import type { ItemReviewDto } from '../../core/models/auth.models';
import { readImageAsDataUrl } from '../image-utils';
import { StarRating } from '../star-rating/star-rating';

const MAX_PHOTOS = 4;

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
      const added = await Promise.all(files.slice(0, room).map((file) => readImageAsDataUrl(file, 1000, 0.8)));
      this.photos.update((p) => [...p, ...added]);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Could not process the selected image.';
      this.snackBar.open(message, 'Close', { duration: 3000, panelClass: ['snack-error'] });
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

}
