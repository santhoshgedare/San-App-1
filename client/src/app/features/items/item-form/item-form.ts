import { MatSnackBar } from '@angular/material/snack-bar';
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { SELECT_DEFAULTS } from '../../../shared/select-defaults';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { ItemService } from '../../../core/auth/item.service';
import { CategoryService } from '../../../core/auth/category.service';
import { AuthService } from '../../../core/auth/auth.service';
import { SellerService, type SellerDto } from '../../../core/auth/seller.service';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ENTITY_TYPES } from '../../../core/models/constants';
import { ConfirmService } from '../../../shared/confirm-dialog/confirm-dialog';
import { readImageAsDataUrl } from '../../../shared/image-utils';
import {
  UnitOfMeasurement,
  type CategoryDto,
  type ItemDocumentDto,
  type ItemDto,
  type ItemImageDto,
  type ItemVariantDto,
} from '../../../core/models/auth.models';

interface VariantAttributeEntry {
  name: string;
  value: string;
}

@Component({
  selector: 'app-item-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule, MatSelectModule,
    MatButtonModule,
    MatTooltipModule,
    MatTabsModule,
    ActivityLogPanel,
    ApprovalPanel,
  ],
  providers: [SELECT_DEFAULTS],
  templateUrl: './item-form.html',
  styleUrl: './item-form.scss',
})
export class ItemForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly confirmService = inject(ConfirmService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly itemService = inject(ItemService);
  private readonly categoryService = inject(CategoryService);
  private readonly authService = inject(AuthService);
  private readonly sellerService = inject(SellerService);

  readonly isAdmin = this.authService.isAdmin;
  readonly entityType = ENTITY_TYPES.item;
  readonly isNew = signal(true);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly isProcessingImages = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly tabIndex = signal(0);
  readonly lastTabIndex = 3;
  readonly item = signal<ItemDto | null>(null);
  readonly categories = signal<CategoryDto[]>([]);
  readonly sellers = signal<SellerDto[]>([]);
  readonly unitOptions = Object.values(UnitOfMeasurement);

  // Form Fields - Basic
  code = '';
  name = '';
  description = '';
  barcode = '';
  categoryId = '';
  sellerId = '';
  unitOfMeasurement = UnitOfMeasurement.Piece;
  price = 0;
  costPrice = 0;
  stockQuantity = 0;
  isActive = true;

  // 1:N Collections
  readonly images = signal<ItemImageDto[]>([]);
  readonly documents = signal<ItemDocumentDto[]>([]);
  readonly variants = signal<ItemVariantDto[]>([]);

  // Selected Category's variant definitions
  readonly selectedCategory = signal<CategoryDto | null>(null);

  ngOnInit(): void {
    forkJoin({
      categories: this.categoryService.getAll(),
      sellers: this.isAdmin() ? this.sellerService.getAll() : of([] as SellerDto[]),
    }).subscribe({
      next: ({ categories, sellers }) => {
        this.categories.set(categories);
        this.sellers.set(sellers);
        this.initForm();
      },
      error: () => {
        this.errorMessage.set('Could not load item form data.');
        this.isLoading.set(false);
      },
    });
  }

  private initForm(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'new') {
      this.isNew.set(true);
      if (this.categories().length > 0) {
        this.onCategoryChange(this.categories()[0].id);
      }
      this.isLoading.set(false);
      return;
    }

    this.isNew.set(false);
    this.itemService.getManagementById(id).subscribe({
      next: (item) => {
        this.item.set(item);
        this.code = item.code;
        this.name = item.name;
        this.description = item.description ?? '';
        this.barcode = item.barcode ?? '';
        this.categoryId = item.categoryId;
        this.sellerId = item.sellerId ?? '';
        this.unitOfMeasurement = item.unitOfMeasurement ?? UnitOfMeasurement.Piece;
        this.price = item.price;
        this.costPrice = item.costPrice ?? 0;
        this.stockQuantity = item.stockQuantity;
        this.isActive = item.isActive;
        this.images.set((item.images ?? []).map((img) => ({ ...img })));
        this.documents.set((item.documents ?? []).map((doc) => ({ ...doc })));
        this.variants.set((item.variants ?? []).map((v) => ({ ...v })));

        const cat = this.categories().find((c) => c.id === item.categoryId);
        this.selectedCategory.set(cat ?? null);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load item details.');
        this.isLoading.set(false);
      },
    });
  }

  onCategoryChange(categoryId: string): void {
    this.categoryId = categoryId;
    const cat = this.categories().find((c) => c.id === categoryId);
    this.selectedCategory.set(cat ?? null);
    if (cat && this.isNew()) {
      this.unitOfMeasurement = cat.unitOfMeasurement ?? UnitOfMeasurement.Piece;
    }
  }

  // --- Variant Management ---
  parseVariantAttributes(attributesJson: string): VariantAttributeEntry[] {
    try {
      const parsed = JSON.parse(attributesJson || '{}');
      if (Array.isArray(parsed)) {
        return parsed;
      }
      return Object.entries(parsed).map(([k, v]) => ({ name: k, value: String(v) }));
    } catch {
      return [];
    }
  }

  variantAttributeMap(variant: ItemVariantDto): Record<string, string> {
    const map: Record<string, string> = {};
    for (const entry of this.parseVariantAttributes(variant.attributesJson)) {
      map[entry.name] = entry.value;
    }
    return map;
  }

  /** Attributes on a variant that are not kinds defined by the selected category. */
  customAttributes(variant: ItemVariantDto): VariantAttributeEntry[] {
    const defined = new Set((this.selectedCategory()?.variantDefinitions ?? []).map((d) => d.name));
    return this.parseVariantAttributes(variant.attributesJson).filter((a) => !defined.has(a.name));
  }

  setVariantAttribute(index: number, name: string, value: string): void {
    const variant = this.variants()[index];
    if (!variant) return;
    const map = this.variantAttributeMap(variant);
    if (value) {
      map[name] = value;
    } else {
      delete map[name];
    }
    this.updateVariant(index, { attributesJson: JSON.stringify(map) });
  }

  renameVariantAttribute(index: number, oldName: string, newName: string): void {
    const variant = this.variants()[index];
    const trimmed = newName.trim();
    if (!variant || !trimmed || trimmed === oldName) return;
    const renamed: Record<string, string> = {};
    for (const [key, value] of Object.entries(this.variantAttributeMap(variant))) {
      renamed[key === oldName ? trimmed : key] = value;
    }
    this.updateVariant(index, { attributesJson: JSON.stringify(renamed) });
  }

  addCustomAttribute(index: number): void {
    const variant = this.variants()[index];
    if (!variant) return;
    const map = this.variantAttributeMap(variant);
    let n = 1;
    while (map[`New kind ${n}`] !== undefined) n++;
    map[`New kind ${n}`] = 'Value';
    this.updateVariant(index, { attributesJson: JSON.stringify(map) });
  }

  addManualVariant(): void {
    const defaultSku = `${this.code || 'ITEM'}-VAR-${this.variants().length + 1}`;
    this.variants.update((list) => [
      ...list,
      {
        sku: defaultSku,
        name: `Variant ${list.length + 1}`,
        attributesJson: '{}',
        price: this.price,
        costPrice: this.costPrice,
        stockQuantity: this.stockQuantity,
        isActive: true,
      },
    ]);
  }

  generateVariantsFromCategory(): void {
    const cat = this.selectedCategory();
    if (!cat || !cat.variantDefinitions || cat.variantDefinitions.length === 0) {
      this.errorMessage.set('Selected category has no variant definitions configured.');
      return;
    }

    const definitions = cat.variantDefinitions.filter((d) => d.values && d.values.length > 0);
    if (definitions.length === 0) {
      this.errorMessage.set('Category variant definitions have no available options.');
      return;
    }

    // Cartesian product of variant definition values
    let combinations: Record<string, string>[] = [{}];
    for (const def of definitions) {
      const nextCombos: Record<string, string>[] = [];
      for (const current of combinations) {
        for (const val of def.values) {
          nextCombos.push({ ...current, [def.name]: val });
        }
      }
      combinations = nextCombos;
    }

    const generated: ItemVariantDto[] = combinations.map((combo, index) => {
      const comboName = Object.values(combo).join(' / ');
      const skuSuffix = Object.values(combo)
        .map((v) => v.substring(0, 3).toUpperCase())
        .join('-');
      return {
        sku: `${this.code || 'ITEM'}-${skuSuffix || index + 1}`,
        name: `${this.name || 'Item'} - ${comboName}`,
        attributesJson: JSON.stringify(combo),
        price: this.price,
        costPrice: this.costPrice,
        stockQuantity: Math.floor(this.stockQuantity / combinations.length) || 0,
        isActive: true,
      };
    });

    this.variants.set(generated);
  }

  removeVariant(index: number): void {
    this.variants.update((list) => list.filter((_, i) => i !== index));
  }

  updateVariant(index: number, updates: Partial<ItemVariantDto>): void {
    this.variants.update((list) =>
      list.map((v, i) => (i === index ? { ...v, ...updates } : v)),
    );
  }

  // --- Image Management ---
  async onImageFileSelected(event: Event): Promise<void> {
    const target = event.target as HTMLInputElement;
    const files = Array.from(target.files ?? []);
    target.value = '';
    if (files.length === 0) return;

    this.isProcessingImages.set(true);
    this.errorMessage.set(null);
    try {
      const encoded = await Promise.all(files.map(async (file) => ({
        file,
        url: await readImageAsDataUrl(file),
      })));
      this.images.update((list) => [
        ...list,
        ...encoded.map(({ file, url }, index) => ({
          url,
          fileName: file.name,
          caption: file.name.split('.')[0],
          isPrimary: list.length === 0 && index === 0,
          sortOrder: list.length + index + 1,
          uploadedAt: new Date().toISOString(),
        })),
      ]);
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Could not process the selected image.');
    } finally {
      this.isProcessingImages.set(false);
    }
  }

  addImageByUrl(): void {
    const url = prompt('Enter Image URL:');
    if (!url || !url.trim()) {
      return;
    }
    const isFirst = this.images().length === 0;
    this.images.update((list) => [
      ...list,
      {
        url: url.trim(),
        fileName: 'External Image',
        caption: 'External image',
        isPrimary: isFirst,
        sortOrder: list.length + 1,
        uploadedAt: new Date().toISOString(),
      },
    ]);
  }

  setPrimaryImage(index: number): void {
    this.images.update((list) =>
      list.map((img, i) => ({ ...img, isPrimary: i === index })),
    );
  }

  removeImage(index: number): void {
    this.images.update((list) => {
      const updated = list.filter((_, i) => i !== index);
      if (updated.length > 0 && !updated.some((img) => img.isPrimary)) {
        updated[0].isPrimary = true;
      }
      return updated;
    });
  }

  // --- Document Management ---
  onDocumentFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (!target.files || target.files.length === 0) {
      return;
    }

    Array.from(target.files).forEach((file) => {
      const reader = new FileReader();
      reader.onload = () => {
        const url = reader.result as string;
        const extension = file.name.split('.').pop()?.toUpperCase() ?? 'DOC';
        this.documents.update((list) => [
          ...list,
          {
            url,
            fileName: file.name,
            documentType: extension,
            fileSizeBytes: file.size,
            description: file.name,
            uploadedAt: new Date().toISOString(),
          },
        ]);
      };
      reader.readAsDataURL(file);
    });
    target.value = '';
  }

  addDocumentByUrl(): void {
    const url = prompt('Enter Document URL:');
    if (!url || !url.trim()) {
      return;
    }
    const fileName = prompt('Enter Document File Name:', 'Document.pdf') || 'Document.pdf';
    this.documents.update((list) => [
      ...list,
      {
        url: url.trim(),
        fileName: fileName.trim(),
        documentType: fileName.split('.').pop()?.toUpperCase() ?? 'LINK',
        fileSizeBytes: 0,
        description: fileName.trim(),
        uploadedAt: new Date().toISOString(),
      },
    ]);
  }

  removeDocument(index: number): void {
    this.documents.update((list) => list.filter((_, i) => i !== index));
  }

  formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '—';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  // --- Save / Delete / Cancel ---
  private validateGeneral(): boolean {
    if (!this.code.trim()) {
      this.errorMessage.set('Item Code is required.');
    } else if (!this.name.trim()) {
      this.errorMessage.set('Item Name is required.');
    } else if (!this.categoryId) {
      this.errorMessage.set('Please select a category.');
    } else {
      this.errorMessage.set(null);
      return true;
    }
    return false;
  }

  onTabChange(index: number): void {
    if (this.isNew() && index > 0 && !this.validateGeneral()) {
      this.tabIndex.set(0);
      return;
    }
    this.tabIndex.set(index);
  }

  nextTab(): void {
    this.onTabChange(Math.min(this.tabIndex() + 1, this.lastTabIndex));
  }

  previousTab(): void {
    this.tabIndex.set(Math.max(this.tabIndex() - 1, 0));
  }

  save(): void {
    this.errorMessage.set(null);

    if (this.isNew() && this.tabIndex() !== this.lastTabIndex) {
      return;
    }

    const trimmedCode = this.code.trim();
    const trimmedName = this.name.trim();

    if (!trimmedCode) {
      this.errorMessage.set('Item Code is required.');
      return;
    }
    if (!trimmedName) {
      this.errorMessage.set('Item Name is required.');
      return;
    }
    if (!this.categoryId) {
      this.errorMessage.set('Please select a category.');
      return;
    }
    if (this.isAdmin() && !this.sellerId) {
      this.errorMessage.set('Please select a seller.');
      return;
    }

    if (this.isProcessingImages()) return;
    this.isSaving.set(true);

    const payload = {
      code: trimmedCode,
      name: trimmedName,
      description: this.description.trim() || null,
      barcode: this.barcode.trim() || null,
      categoryId: this.categoryId,
      sellerId: this.isAdmin() ? this.sellerId : null,
      unitOfMeasurement: this.unitOfMeasurement,
      price: Number(this.price) || 0,
      costPrice: Number(this.costPrice) || 0,
      stockQuantity: Number(this.stockQuantity) || 0,
      isActive: this.isActive,
      images: this.images(),
      documents: this.documents(),
      variants: this.variants(),
    };

    if (this.isNew()) {
      this.itemService.create(payload).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.snackBar.open(this.isAdmin() ? 'Item created.' : 'Item created. It will show in the catalog once an admin approves it.', 'Close', { duration: 4000 });
          this.router.navigate(['/items']);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.errorMessage.set(err?.error?.errors?.[0] ?? 'Could not create item.');
        },
      });
      return;
    }

    const existing = this.item();
    if (!existing) {
      return;
    }

    this.itemService.update(existing.id, payload).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.snackBar.open(this.isAdmin() ? 'Item saved.' : 'Item saved. Changes need admin approval before they show in the catalog.', 'Close', { duration: 4000 });
        this.router.navigate(['/items']);
      },
      error: (err) => {
        this.isSaving.set(false);
        this.errorMessage.set(err?.error?.errors?.[0] ?? 'Could not update item.');
      },
    });
  }

  remove(): void {
    const existing = this.item();
    if (!existing) {
      return;
    }

    this.confirmService
      .confirm({ title: 'Delete item?', message: `Delete item "${existing.code} - ${existing.name}"?`, confirmText: 'Delete', cancelText: 'Keep', destructive: true })
      .subscribe((ok) => {
        if (ok) {
          this.itemService.delete(existing.id).subscribe(() => this.router.navigate(['/items']));
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/items']);
  }
}
