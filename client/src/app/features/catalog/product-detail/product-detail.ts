import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { ItemService } from '../../../core/auth/item.service';
import { CartService } from '../../../core/cart/cart.service';
import type { ItemDto, ItemImageDto, ItemVariantDto } from '../../../core/models/auth.models';

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
    MatTabsModule,
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

  readonly item = signal<ItemDto | null>(null);
  readonly isLoading = signal(true);
  readonly selectedImage = signal<ItemImageDto | null>(null);
  readonly selectedVariant = signal<ItemVariantDto | null>(null);
  readonly quantity = signal(1);

  // Parsed attribute maps for interactive variant picker (e.g. { Color: ['Red', 'Blue'], Size: ['M', 'L'] })
  readonly attributeKeys = signal<string[]>([]);
  readonly attributeOptions = signal<Record<string, string[]>>({});
  readonly selectedAttributeValues = signal<Record<string, string>>({});

  ngOnInit(): void {
    const itemId = this.route.snapshot.paramMap.get('id');
    if (!itemId) {
      this.router.navigate(['/catalog']);
      return;
    }

    this.itemService.getById(itemId).subscribe({
      next: (item) => {
        this.item.set(item);
        if (item.images && item.images.length > 0) {
          const primary = item.images.find((img) => img.isPrimary) ?? item.images[0];
          this.selectedImage.set(primary);
        }

        this.parseVariantAttributes(item);

        if (item.variants && item.variants.length > 0) {
          this.selectVariant(item.variants[0]);
        }

        this.isLoading.set(false);
      },
      error: () => {
        this.snackBar.open('Item not found', 'Close', { duration: 3000 });
        this.router.navigate(['/catalog']);
      },
    });
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
      } catch {}
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

    if (variant.attributesJson) {
      try {
        const parsed = JSON.parse(variant.attributesJson);
        this.selectedAttributeValues.set(parsed);
      } catch {}
    }
  }

  onAttributeValueChange(attrKey: string, attrVal: string): void {
    const currentSelections = { ...this.selectedAttributeValues(), [attrKey]: attrVal };
    this.selectedAttributeValues.set(currentSelections);

    // Find the matching variant
    const item = this.item();
    if (!item || !item.variants) return;

    const matched = item.variants.find((v) => {
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

  incrementQuantity(): void {
    this.quantity.update((q) => q + 1);
  }

  decrementQuantity(): void {
    this.quantity.update((q) => (q > 1 ? q - 1 : 1));
  }

  addToCart(): void {
    const item = this.item();
    if (!item) return;

    const variant = this.selectedVariant();
    this.cartService.addToCart(item, variant, this.quantity());

    const variantName = variant ? ` (${variant.name || variant.sku})` : '';
    this.snackBar.open(`Added ${this.quantity()}x "${item.name}${variantName}" to cart!`, 'View Cart', {
      duration: 3500,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
    }).onAction().subscribe(() => {
      this.router.navigate(['/cart']);
    });
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
