import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CategoryService } from '../../../core/auth/category.service';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ENTITY_TYPES } from '../../../core/models/constants';
import {
  CategoryVariantType,
  UnitOfMeasurement,
  type CategoryDto,
  type CategoryVariantDefinition,
} from '../../../core/models/auth.models';

/** Single page for viewing/editing category details, or creating a new category. */
@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatButtonModule, ActivityLogPanel, ApprovalPanel],
  templateUrl: './category-form.html',
  styleUrl: './category-form.scss',
})
export class CategoryForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly categoryService = inject(CategoryService);

  readonly entityType = ENTITY_TYPES.category;
  readonly isNew = signal(true);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly category = signal<CategoryDto | null>(null);
  readonly unitOptions = Object.values(UnitOfMeasurement);
  readonly variantTypeOptions = Object.values(CategoryVariantType);
  readonly variantDefinitions = signal<CategoryVariantDefinition[]>([]);

  name = '';
  description = '';
  isActive = true;
  unitOfMeasurement = UnitOfMeasurement.Piece;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'new') {
      this.isNew.set(true);
      this.unitOfMeasurement = UnitOfMeasurement.Piece;
      this.variantDefinitions.set([this.createVariantDefinition()]);
      this.isLoading.set(false);
      return;
    }

    this.isNew.set(false);
    this.categoryService.getById(id).subscribe({
      next: (match) => {
        this.category.set(match);
        this.name = match.name;
        this.description = match.description ?? '';
        this.isActive = match.isActive;
        this.unitOfMeasurement = match.unitOfMeasurement ?? UnitOfMeasurement.Piece;
        this.variantDefinitions.set((match.variantDefinitions ?? []).map((variant) => ({ ...variant, values: [...variant.values] })));
        if (!this.variantDefinitions().length) {
          this.variantDefinitions.set([this.createVariantDefinition()]);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load category.');
        this.isLoading.set(false);
      },
    });
  }

  addVariant(): void {
    this.variantDefinitions.update((items) => [...items, this.createVariantDefinition()]);
  }

  removeVariant(index: number): void {
    this.variantDefinitions.update((items) => items.filter((_, itemIndex) => itemIndex !== index));
    if (!this.variantDefinitions().length) {
      this.variantDefinitions.set([this.createVariantDefinition()]);
    }
  }

  updateVariant(index: number, updates: Partial<CategoryVariantDefinition>): void {
    this.variantDefinitions.update((items) =>
      items.map((variant, itemIndex) => (itemIndex === index ? { ...variant, ...updates } : variant)),
    );
  }

  save(): void {
    this.errorMessage.set(null);
    const trimmedName = this.name.trim();
    if (!trimmedName) {
      this.errorMessage.set('Category name is required.');
      return;
    }

    const variants = this.variantDefinitions()
      .map((variant) => ({
        ...variant,
        name: variant.name.trim() || variant.type,
        values: (variant.values ?? []).map((value) => value.trim()).filter(Boolean),
      }))
      .filter((variant) => variant.name || variant.values.length > 0);

    this.isSaving.set(true);
    const payload = {
      name: trimmedName,
      description: this.description.trim() || null,
      isActive: this.isActive,
      unitOfMeasurement: this.unitOfMeasurement,
      variantDefinitions: variants,
    };

    if (this.isNew()) {
      this.categoryService.create(payload).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/categories']);
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('Could not create category.');
        },
      });
      return;
    }

    const existing = this.category();
    if (!existing) {
      return;
    }

    this.categoryService.update(existing.id, payload).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.router.navigate(['/categories']);
      },
      error: () => {
        this.isSaving.set(false);
        this.errorMessage.set('Could not update category.');
      },
    });
  }

  remove(): void {
    const existing = this.category();
    if (!existing) {
      return;
    }
    if (!confirm(`Delete category "${existing.name}"?`)) {
      return;
    }
    this.categoryService.delete(existing.id).subscribe(() => this.router.navigate(['/categories']));
  }

  cancel(): void {
    this.router.navigate(['/categories']);
  }

  private createVariantDefinition(): CategoryVariantDefinition {
    return {
      id: crypto.randomUUID(),
      name: 'Color',
      type: CategoryVariantType.Color,
      values: ['Black', 'White'],
      isRequired: false,
    };
  }
}
