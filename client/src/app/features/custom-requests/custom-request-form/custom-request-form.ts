import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/auth/auth.service';
import { CustomRequestService } from '../../../core/auth/custom-request.service';
import { readImageAsDataUrl } from '../../../shared/image-utils';

const MAX_PHOTOS = 5;

interface Photo {
  url: string;
  fileName: string;
}

@Component({
  selector: 'app-custom-request-form',
  standalone: true,
  imports: [FormsModule, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './custom-request-form.html',
  styleUrl: './custom-request-form.scss',
})
export class CustomRequestForm implements OnInit {
  private readonly service = inject(CustomRequestService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly maxPhotos = MAX_PHOTOS;
  readonly photos = signal<Photo[]>([]);
  readonly isSaving = signal(false);
  readonly isProcessingImages = signal(false);
  readonly error = signal<string | null>(null);

  title = '';
  description = '';
  quantity = 1;
  budget: number | null = null;
  neededBy = '';
  customerName = '';
  customerEmail = '';
  customerPhone = '';

  ngOnInit(): void {
    const user = this.auth.currentUser();
    if (user) {
      this.customerName = `${user.firstName} ${user.lastName}`.trim();
      this.customerEmail = user.email;
      this.customerPhone = user.phoneNumber ?? '';
    }
  }

  async onFilesSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (this.isProcessingImages()) return;
    const room = MAX_PHOTOS - this.photos().length;
    if (files.length > room) {
      this.snackBar.open(`You can add up to ${MAX_PHOTOS} photos.`, 'Close');
      return;
    }
    this.isProcessingImages.set(true);
    try {
      const added = await Promise.all(files.map(async (file) => ({
        url: await readImageAsDataUrl(file),
        fileName: file.name,
      })));
      this.photos.update((list) => [...list, ...added]);
    } catch (error) {
      this.snackBar.open(error instanceof Error ? error.message : 'Could not process the selected image.', 'Close');
    } finally {
      this.isProcessingImages.set(false);
    }
  }

  removePhoto(index: number): void {
    this.photos.update((list) => list.filter((_, i) => i !== index));
  }

  submit(): void {
    if (this.isProcessingImages()) return;
    if (!this.title.trim() || !this.description.trim()) {
      this.error.set('Please add a title and describe what you want made.');
      return;
    }
    if (!this.customerName.trim() || !this.customerEmail.trim() || !this.customerPhone.trim()) {
      this.error.set('Please fill in your contact details.');
      return;
    }
    this.error.set(null);
    this.isSaving.set(true);
    this.service
      .create({
        title: this.title,
        description: this.description,
        quantity: this.quantity || 1,
        budget: this.budget,
        neededBy: this.neededBy || null,
        customerName: this.customerName,
        customerEmail: this.customerEmail,
        customerPhone: this.customerPhone,
        images: this.photos().map((p) => ({ url: p.url, fileName: p.fileName })),
      })
      .subscribe({
        next: (request) => {
          this.snackBar.open('Request posted. Sellers can now send you quotes.', 'Close');
          this.router.navigate(['/custom-requests', request.id]);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err?.error?.errors?.join(' ') ?? 'Could not post your request.');
        },
      });
  }
}
