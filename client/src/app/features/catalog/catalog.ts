import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { ItemService } from '../../core/auth/item.service';
import { CategoryService } from '../../core/auth/category.service';
import { CartService } from '../../core/cart/cart.service';
import type { CategoryDto, ItemDto, ItemImageDto, ItemVariantDto } from '../../core/models/auth.models';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatBadgeModule,
    MatSnackBarModule,
  ],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss',
})
export class Catalog implements OnInit, OnDestroy {
  private readonly itemService = inject(ItemService);
  private readonly categoryService = inject(CategoryService);
  private readonly cartService = inject(CartService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly searchInput$ = new Subject<string>();

  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  readonly items = signal<ItemDto[]>([]);
  readonly categories = signal<CategoryDto[]>([]);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly totalCount = signal(0);

  readonly searchTerm = signal('');
  readonly selectedCategoryId = signal('');
  readonly selectedSort = signal<'popular' | 'price-asc' | 'price-desc' | 'name'>('popular');

  // Track user-selected variant per item card: itemId -> selectedVariant
  readonly selectedVariants = signal<Record<string, ItemVariantDto>>({});

  readonly cartCount = this.cartService.totalItemsCount;

  private page = 1;
  private hasMore = true;

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => this.categories.set(categories));
    this.searchInput$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
      this.searchTerm.set(term);
      this.reload();
    });
    this.reload();
  }

  ngOnDestroy(): void {
    this.searchInput$.complete();
  }

  onSearchInput(value: string): void {
    this.searchInput$.next(value);
  }

  selectCategory(categoryId: string): void {
    this.selectedCategoryId.set(this.selectedCategoryId() === categoryId ? '' : categoryId);
    this.reload();
  }

  onSortChange(): void {
    this.reload();
  }

  private reload(): void {
    this.page = 1;
    this.hasMore = true;
    this.isLoading.set(true);
    this.items.set([]);
    this.fetchPage();
  }

  loadMore(): void {
    if (this.isLoading() || this.isLoadingMore() || !this.hasMore) {
      return;
    }
    this.isLoadingMore.set(true);
    this.fetchPage();
  }

  private fetchPage(): void {
    this.itemService
      .getPaged({
        search: this.searchTerm() || undefined,
        categoryId: this.selectedCategoryId() || undefined,
        isActive: true,
        page: this.page,
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (result) => {
          let loadedItems = result.items;
          // Client sorting for refined display
          if (this.selectedSort() === 'price-asc') {
            loadedItems.sort((a, b) => a.price - b.price);
          } else if (this.selectedSort() === 'price-desc') {
            loadedItems.sort((a, b) => b.price - a.price);
          } else if (this.selectedSort() === 'name') {
            loadedItems.sort((a, b) => a.name.localeCompare(b.name));
          }

          this.items.update((existing) => [...existing, ...loadedItems]);
          this.totalCount.set(result.totalCount);
          this.hasMore = result.hasMore;
          this.page += 1;
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
        },
      });
  }

  getPrimaryImage(item: ItemDto): ItemImageDto | null {
    if (!item.images || item.images.length === 0) {
      return null;
    }
    return item.images.find((img) => img.isPrimary) ?? item.images[0];
  }

  getSelectedVariant(item: ItemDto): ItemVariantDto | null {
    const map = this.selectedVariants();
    const selectedVariant = map[item.id];
    const selected = item.variants?.find((variant) => variant.id === selectedVariant?.id && variant.isActive);
    if (selected) {
      return selected;
    }
    return item.variants?.find((variant) => variant.isActive && variant.stockQuantity > 0)
      ?? item.variants?.find((variant) => variant.isActive)
      ?? null;
  }

  getSelectedStock(item: ItemDto): number {
    return this.cartService.getAvailableStock(item, this.getSelectedVariant(item));
  }

  getRemainingStock(item: ItemDto): number {
    return this.cartService.getRemainingStock(item, this.getSelectedVariant(item));
  }

  onSelectVariant(item: ItemDto, variant: ItemVariantDto): void {
    this.selectedVariants.update((map) => ({ ...map, [item.id]: variant }));
  }

  onVariantChange(item: ItemDto, variantId: string): void {
    const variant = item.variants.find((candidate) => candidate.id === variantId && candidate.isActive);
    if (variant) {
      this.onSelectVariant(item, variant);
    }
  }

  getEffectivePrice(item: ItemDto): number {
    const variant = this.getSelectedVariant(item);
    return variant ? variant.price : item.price;
  }

  getPriceRange(item: ItemDto): { min: number; max: number; hasRange: boolean } {
    if (!item.variants || item.variants.length === 0) {
      return { min: item.price, max: item.price, hasRange: false };
    }
    const prices = item.variants.map((v) => v.price);
    const min = Math.min(...prices);
    const max = Math.max(...prices);
    return { min, max, hasRange: min !== max };
  }

  addToCart(item: ItemDto, event: MouseEvent): void {
    event.stopPropagation();
    const variant = this.getSelectedVariant(item);
    if (!this.cartService.addToCart(item, variant, 1)) {
      this.snackBar.open('No additional stock is available for this item.', 'Close', { duration: 3000 });
      return;
    }
    const variantName = variant ? ` (${variant.name || variant.sku})` : '';
    this.snackBar.open(`Added "${item.name}${variantName}" to cart!`, 'View Cart', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
    }).onAction().subscribe(() => {
      this.router.navigate(['/cart']);
    });
  }

  viewDetail(item: ItemDto): void {
    this.router.navigate(['/catalog', item.id]);
  }
}
