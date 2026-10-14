import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import type { Observable } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { InviteEmailDialog, InviteEmailDialogData } from '../invite-email-dialog/invite-email-dialog';
import { SellerDto, SellerInput, SellerInviteResult, SellerService } from '../../../core/auth/seller.service';
import { readImageAsDataUrl } from '../../../shared/image-utils';
import { UserService } from '../../../core/auth/user.service';
import { ROLES } from '../../../core/models/constants';
import type { UserDto } from '../../../core/models/auth.models';

/** Company profile editor. Used by admins (`/sellers/:id`) and by managers for their own company (`/seller-profile`). */
@Component({
  selector: 'app-seller-form',
  standalone: true,
  imports: [FormsModule, MatIconModule],
  templateUrl: './seller-form.html',
  styleUrl: '../seller-pages.scss',
})
export class SellerForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  protected readonly router = inject(Router);
  private readonly service = inject(SellerService);
  private readonly users = inject(UserService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly mine = this.route.snapshot.routeConfig?.path === 'seller-profile';
  readonly isNew = signal(false);
  readonly isSaving = signal(false);
  readonly isProcessingImages = signal(false);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly managers = signal<UserDto[]>([]);
  private id: string | null = null;
  readonly status = signal<string>('None');
  readonly inviteLink = signal<string | null>(null);
  readonly inviteNote = signal<string | null>(null);
  readonly isInviting = signal(false);
  inviteEmail = '';

  sendInvite(): void {
    if (!this.id) return;
    this.isInviting.set(true);
    this.errorMessage.set(null);
    this.service.invite(this.id, this.inviteEmail).subscribe({
      next: (draft) => {
        this.isInviting.set(false);
        this.status.set('Pending');
        this.inviteLink.set(draft.inviteUrl);
        this.inviteNote.set('Invite link created. Share it directly or use Compose email.');
        this.openComposer(draft);
      },
      error: (err) => {
        this.isInviting.set(false);
        this.errorMessage.set(err?.error?.errors?.join(' ') ?? 'Could not create invitation.');
      },
    });
  }

  private draft: SellerInviteResult | null = null;

  private openComposer(draft: SellerInviteResult): void {
    this.draft = draft;
    this.dialog
      .open<InviteEmailDialog, InviteEmailDialogData, boolean>(InviteEmailDialog, {
        data: { sellerId: this.id!, to: draft.to, subject: draft.subject, body: draft.body },
        width: '720px',
        maxWidth: '96vw',
        autoFocus: false,
      })
      .afterClosed()
      .subscribe((sent) => {
        if (sent) this.inviteNote.set('Invitation emailed. You can also share the link below.');
      });
  }

  composeAgain(): void {
    if (this.draft) this.openComposer(this.draft);
  }
  copyLink(): void {
    const link = this.inviteLink();
    if (link) navigator.clipboard?.writeText(link).then(() => this.snackBar.open('Link copied.', 'Close'));
  }

  model: SellerInput = {
    userId: null,
    companyName: '',
    tagline: null,
    description: null,
    logoUrl: null,
    contactEmail: null,
    contactPhone: null,
    website: null,
    addressLine1: null,
    city: null,
    state: null,
    postalCode: null,
    country: 'India',
    upiId: null,
    payeeName: null,
    qrCodeImageUrl: null,
    bankDetails: null,
  };

  ngOnInit(): void {
    if (this.mine) {
      this.service.getMine().subscribe({
        next: (s) => this.apply(s),
        error: (err) => {
          const errors = err?.error?.errors;
          const message = Array.isArray(errors) ? errors.join(' ') : null;
          this.errorMessage.set(
            message ??
              (err?.status === 404
                ? 'Your company profile could not be found. Please contact an administrator.'
                : 'Could not load your registered company details. Check your connection and try again.'),
          );
          this.isLoading.set(false);
        },
      });
      return;
    }

    this.users.getAll().subscribe((all) => this.managers.set(all.filter((u) => u.roles.includes(ROLES.manager))));

    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'new') {
      this.isNew.set(true);
      this.isLoading.set(false);
      return;
    }

    this.id = id;
    this.service.getById(id).subscribe({
      next: (s) => this.apply(s),
      error: () => {
        this.errorMessage.set('Seller not found.');
        this.isLoading.set(false);
      },
    });
  }

  private apply(s: SellerDto): void {
    const { id: _id, userEmail: _email, itemCount: _count, inviteEmail: _ie, inviteStatus: _is, ...input } = s;
    this.model = input;
    this.status.set(s.inviteStatus);
    this.inviteEmail = s.inviteEmail ?? '';
    this.isLoading.set(false);
  }

  async onQrSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    await this.processImage(file, (url) => (this.model.qrCodeImageUrl = url), 1600, 0.95);
  }
  async onLogoSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    await this.processImage(file, (url) => (this.model.logoUrl = url), 512, 0.85);
  }

  private async processImage(file: File, setImage: (url: string) => void, maxSize: number, quality: number): Promise<void> {
    if (this.isProcessingImages()) return;
    this.isProcessingImages.set(true);
    this.errorMessage.set(null);
    try {
      setImage(await readImageAsDataUrl(file, maxSize, quality));
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Could not process the selected image.');
    } finally {
      this.isProcessingImages.set(false);
    }
  }

  save(): void {
    if (this.isProcessingImages()) return;
    this.errorMessage.set(null);
    if (!this.model.companyName.trim()) {
      this.errorMessage.set('Company name is required.');
      return;
    }

    this.isSaving.set(true);
    const request: Observable<unknown> = this.mine
      ? this.service.updateMine(this.model)
      : this.isNew()
        ? this.service.create(this.model)
        : this.service.update(this.id!, this.model);

    request.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.snackBar.open('Company profile saved.', 'Close');
        if (!this.mine) this.router.navigate(['/sellers']);
      },
      error: (err) => {
        this.isSaving.set(false);
        this.errorMessage.set(err?.error?.errors?.join(' ') ?? 'Could not save company profile.');
      },
    });
  }
}