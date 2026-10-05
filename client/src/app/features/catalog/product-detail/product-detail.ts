import { SellerService, type SellerDto } from '../../../core/auth/seller.service';
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ItemService } from '../../../core/auth/item.service';
import { CartService } from '../../../core/cart/cart.service';
import { FavouritesService } from '../../../core/cart/favourites.service';
import { ReviewService } from '../../../core/auth/review.service';
import { StarRating } from '../../../shared/star-rating/star-rating';
import type { ItemDto, ItemImageDto, ItemReviewSummaryDto, ItemVariantDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule,
    StarRating,
  ],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly itemService = inject(ItemService);
  private readonly cartService = inject(CartService);
  private readonly snackBar = inject(MatSnackBar);
  protected readonly favourites = inject(FavouritesService);
  private readonly reviewService = inject(ReviewService);
  private readonly sellerService = inject(SellerService);
  protected readonly seller = signal<SellerDto | null>(null);

  readonly item = signal<ItemDto | null>(null);
  readonly isLoading = signal(true);
  readonly selectedImage = signal<ItemImageDto | null>(null);
  readonly selectedVariant = signal<ItemVariantDto | null>(null);
  readonly quantity = signal(1);
  readonly similarItems = signal<ItemDto[]>([]);
  readonly similarHasMore = signal(false);
  readonly isLoadingMoreSimilar = signal(false);
  readonly reviewSummary = signal<ItemReviewSummaryDto | null>(null);
  readonly lightboxImage = signal<string | null>(null);
  readonly ratingRows = [5, 4, 3, 2, 1];

  // Parsed attribute maps for interactive variant picker (e.g. { Color: ['Red', 'Blue'], Size: ['M', 'L'] })
  readonly attributeKeys = signal<string[]>([]);
  readonly attributeOptions = signal<Record<string, string[]>>({});
  readonly selectedAttributeValues = signal<Record<string, string>>({});

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const itemId = params.get('id');
      if (!itemId) {
        this.router.navigate(['/catalog']);
        return;
      }
      this.loadItem(itemId);
    });
  }

  private loadItem(itemId: string): void {
    this.isLoading.set(true);
    this.quantity.set(1);
    this.selectedVariant.set(null);
    this.selectedImage.set(null);
    this.similarItems.set([]);
    this.reviewSummary.set(null);
    window.scrollTo({ top: 0 });

    this.itemService.getById(itemId).subscribe({
      next: (item) => {
        this.item.set(item);
        this.seller.set(null);
        if (item.sellerId) {
          this.sellerService.getPublic(item.sellerId).subscribe({ next: (s) => this.seller.set(s), error: () => undefined });
        }
        if (item.images && item.images.length > 0) {
          const primary = item.images.find((img) => img.isPrimary) ?? item.images[0];
          this.selectedImage.set(primary);
        }

        this.parseVariantAttributes(item);

        if (item.variants && item.variants.length > 0) {
          const firstAvailable = item.variants.find((variant) => variant.isActive && variant.stockQuantity > 0)
            ?? item.variants.find((variant) => variant.isActive)
            ?? item.variants[0];
          this.selectVariant(firstAvailable);
        }

        this.isLoading.set(false);
        this.loadSimilar(item);
        this.reviewService.getForItem(item.id).subscribe({ next: (s) => this.reviewSummary.set(s), error: () => this.reviewSummary.set(null) });
      },
      error: () => {
        this.snackBar.open('Item not found', 'Close', { duration: 3000, panelClass: ['snack-error'] });
        this.router.navigate(['/catalog']);
      },
    });
  }

  private loadSimilar(item: ItemDto, take = 5): void {
    this.itemService.getSimilar(item.id, take).subscribe({
      next: (items) => {
        this.similarItems.set(items);
        this.similarHasMore.set(items.length >= take && take < 20);
        this.isLoadingMoreSimilar.set(false);
      },
      error: () => {
        this.similarItems.set([]);
        this.isLoadingMoreSimilar.set(false);
      },
    });
  }

  loadMoreSimilar(): void {
    const itm = this.item();
    if (!itm || this.isLoadingMoreSimilar()) return;
    this.isLoadingMoreSimilar.set(true);
    this.loadSimilar(itm, this.similarItems().length + 5);
  }

  ratingPercent(star: number): number {
    const s = this.reviewSummary();
    return s && s.totalCount ? ((s.distribution[star] ?? 0) / s.totalCount) * 100 : 0;
  }

  getPrimaryImage(item: ItemDto): ItemImageDto | null {
    return item.images?.find((img) => img.isPrimary) ?? item.images?.[0] ?? null;
  }

  openSimilar(item: ItemDto): void {
    this.router.navigate(['/catalog', item.id]);
  }

  private parseVariantAttributes(item: ItemDto): void {
    if (!item.variants || item.variants.length === 0) return;

    const keySet = new Set<string>();
    const optionsMap: Record<string, Set<string>> = {};

    for (const v of item.variants) {
      if (!v.attributesJson) continue;
      try {
        const parsed = JSON.parse(v.attributesJson);
        for (const [key, value] of Object.entries(parsed)) {
          keySet.add(key);
          if (!optionsMap[key]) {
            optionsMap[key] = new Set();
          }
          optionsMap[key].add(String(value));
        }
      } catch { }
    }

    const keys = Array.from(keySet);
    this.attributeKeys.set(keys);

    const finalOptions: Record<string, string[]> = {};
    for (const key of keys) {
      finalOptions[key] = Array.from(optionsMap[key] || []);
    }
    this.attributeOptions.set(finalOptions);
  }

  selectVariant(variant: ItemVariantDto): void {
    this.selectedVariant.set(variant);
    if (variant.stockQuantity > 0) {
      this.quantity.update((quantity) => Math.min(quantity, variant.stockQuantity));
    }

    if (variant.attributesJson) {
      try {
        const parsed = JSON.parse(variant.attributesJson);
        this.selectedAttributeValues.set(parsed);
      } catch { }
    }
  }

  onAttributeValueChange(attrKey: string, attrVal: string): void {
    const currentSelections = { ...this.selectedAttributeValues(), [attrKey]: attrVal };
    this.selectedAttributeValues.set(currentSelections);

    // Find the matching variant
    const item = this.item();
    if (!item || !item.variants) return;

    const matched = item.variants.find((v) => {
      if (!v.isActive) return false;
      if (!v.attributesJson) return false;
      try {
        const parsed = JSON.parse(v.attributesJson);
        return Object.entries(currentSelections).every(([k, val]) => parsed[k] === val);
      } catch {
        return false;
      }
    });

    if (matched) {
      this.selectedVariant.set(matched);
    }
  }

  getEffectivePrice(): number {
    const v = this.selectedVariant();
    if (v) return v.price;
    return this.item()?.price ?? 0;
  }

  getAvailableStock(): number {
    const item = this.item();
    if (!item || !item.isActive) return 0;
    const variant = this.selectedVariant();
    if (variant) return variant.isActive ? variant.stockQuantity : 0;
    return item.variants.length > 0 ? 0 : item.stockQuantity;
  }

  getRemainingStock(): number {
    const item = this.item();
    return item ? this.cartService.getRemainingStock(item, this.selectedVariant()) : 0;
  }

  incrementQuantity(): void {
    this.quantity.update((q) => Math.min(q + 1, this.getRemainingStock()));
  }

  decrementQuantity(): void {
    this.quantity.update((q) => (q > 1 ? q - 1 : 1));
  }

  addToCart(): void {
    const item = this.item();
    if (!item) return;

    const variant = this.selectedVariant();
    if (!this.cartService.addToCart(item, variant, this.quantity())) {
      this.snackBar.open('There is not enough stock available for this quantity.', 'Close', { duration: 3000, panelClass: ['snack-error'] });
      return;
    }

    const variantName = variant ? ` (${variant.name || variant.sku})` : '';
    this.snackBar.open(`Added ${this.quantity()}x "${item.name}${variantName}" to cart!`, 'View Cart', {
      duration: 3500,
    }).onAction().subscribe(() => {
      this.router.navigate(['/cart']);
    });
  }

  toggleFavourite(): void {
    const item = this.item();
    if (!item) return;
    const added = this.favourites.toggle(item.id);
    this.snackBar.open(added ? 'Added to favourites' : 'Removed from favourites', undefined, { duration: 1800 });
  }

  buyNow(): void {
    const item = this.item();
    if (!item) return;

    if (!this.cartService.addToCart(item, this.selectedVariant(), this.quantity())) {
      this.snackBar.open('There is not enough stock available for this quantity.', 'Close', { duration: 3000, panelClass: ['snack-error'] });
      return;
    }
    this.router.navigate(['/checkout']);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  getFileIcon(docType?: string | null): string {
    switch (docType?.toLowerCase()) {
      case 'pdf':
        return 'picture_as_pdf';
      case 'spec':
      case 'specsheet':
        return 'description';
      case 'manual':
        return 'menu_book';
      default:
        return 'insert_drive_file';
    }
  }
}
