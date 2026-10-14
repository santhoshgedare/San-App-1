import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import type { Observable } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { ItemService } from '../../../core/auth/item.service';
import { SectionAccessStore } from '../../../core/auth/section-access.store';
import type { ItemDto, ItemVariantDto } from '../../../core/models/auth.models';

/** Read-only item details; editing happens on the item form. */
@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, MatIconModule],
  templateUrl: './item-detail.html',
  styleUrl: './item-detail.scss',
})
export class ItemDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly itemService = inject(ItemService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly auth = inject(AuthService);
  protected readonly sectionAccess = inject(SectionAccessStore);

  protected readonly isAdmin = this.auth.isAdmin;
  readonly item = signal<ItemDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly selectedImage = signal(0);

  readonly canManage = computed(() => this.sectionAccess.can('section-items-manage'));
  readonly images = computed(() => {
    const imgs = [...(this.item()?.images ?? [])];
    return imgs.sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary) || a.sortOrder - b.sortOrder);
  });
  readonly stock = computed(() => {
    const i = this.item();
    if (!i) return 0;
    return i.variants?.length ? i.variants.reduce((sum, v) => sum + (v.isActive ? v.stockQuantity : 0), 0) : i.stockQuantity;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    const request = this.canManage() ? this.itemService.getManagementById(id) : this.itemService.getById(id);
    request.subscribe({
      next: (item) => {
        this.item.set(item);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Item not found.');
        this.isLoading.set(false);
      },
    });
  }

  attributes(v: ItemVariantDto): string {
    try {
      const parsed = JSON.parse(v.attributesJson || '{}') as Record<string, unknown>;
      return Object.entries(parsed).map(([k, val]) => `${k}: ${val}`).join(' · ');
    } catch {
      return '';
    }
  }

  back(): void {
    this.router.navigate(['/items']);
  }

  edit(): void {
    this.router.navigate(['/items', this.item()!.id, 'edit']);
  }

  approve(): void {
    this.run(this.itemService.approve(this.item()!.id), 'Item approved.');
  }

  reject(): void {
    this.run(this.itemService.reject(this.item()!.id), 'Item rejected.');
  }

  toggleActive(): void {
    const i = this.item()!;
    this.run(this.itemService.setActive(i.id, !i.isActive), i.isActive ? 'Item hidden from catalog.' : 'Item is now on the catalog.');
  }

  private run(call: Observable<unknown>, message: string): void {
    call.subscribe({
      next: () => {
        this.snackBar.open(message, 'Close', { duration: 3000 });
        this.itemService.getManagementById(this.item()!.id).subscribe((i) => this.item.set(i));
      },
      error: (err) => this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Action failed.', 'Close', { duration: 4000 }),
    });
  }
}
