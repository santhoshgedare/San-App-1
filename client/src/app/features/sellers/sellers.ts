import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SellerDto, SellerService } from '../../core/auth/seller.service';
import { ConfirmService } from '../../shared/confirm-dialog/confirm-dialog';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-sellers',
  standalone: true,
  imports: [RouterLink, FormsModule, MatIconModule],
  templateUrl: './sellers.html',
  styleUrl: './seller-pages.scss',
})
export class Sellers implements OnInit {
  private readonly service = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly confirmService = inject(ConfirmService);

  readonly sellers = signal<SellerDto[]>([]);
  readonly search = signal('');
  readonly page = signal(1);

  readonly filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) return this.sellers();
    return this.sellers().filter((s) =>
      [s.companyName, s.userEmail, s.inviteEmail, s.city, s.state].some((v) => v?.toLowerCase().includes(term)),
    );
  });
  readonly stats = computed(() => {
    const all = this.sellers();
    return {
      total: all.length,
      active: all.filter((s) => !!s.userEmail).length,
      invited: all.filter((s) => !s.userEmail && s.inviteStatus === 'Pending').length,
      items: all.reduce((sum, s) => sum + (s.itemCount ?? 0), 0),
    };
  });
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));
  private readonly start = computed(() => (Math.min(this.page(), this.totalPages()) - 1) * PAGE_SIZE);
  readonly paged = computed(() => this.filtered().slice(this.start(), this.start() + PAGE_SIZE));
  readonly rangeLabel = computed(() => {
    const total = this.filtered().length;
    return total ? `${this.start() + 1}–${Math.min(this.start() + PAGE_SIZE, total)} of ${total}` : '0';
  });

  ngOnInit(): void {
    this.load();
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
  }

  location(s: SellerDto): string {
    return [s.city, s.state].filter(Boolean).join(', ') || '—';
  }

  private load(): void {
    this.service.getAll().subscribe((list) => this.sellers.set(list));
  }

  remove(seller: SellerDto): void {
    this.confirmService
      .confirm({ title: 'Delete seller', message: `Delete ${seller.companyName}?`, confirmText: 'Delete', destructive: true })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(seller.id).subscribe({
          next: () => {
            this.snackBar.open('Seller deleted.', 'Close');
            this.load();
          },
          error: (err) => this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not delete seller.', 'Close', { panelClass: ['snack-error'] }),
        });
      });
  }
}