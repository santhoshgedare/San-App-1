import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { SELECT_DEFAULTS } from '../../shared/select-defaults';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CanRenderDirective } from '../../core/directives/can-render.directive';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { ItemService } from '../../core/auth/item.service';
import { CategoryService } from '../../core/auth/category.service';
import { SectionAccessStore } from '../../core/auth/section-access.store';
import type { CategoryDto, ItemDto, ItemImageDto } from '../../core/models/auth.models';

const PAGE_SIZE = 20;

/**
 * Scroll-paginated, searchable, filterable items list.
 * Displays items with thumbnail image, category, unit, pricing, variants count, documents count, and status.
 */
@Component({
  selector: 'app-items',
  standalone: true,
  imports: [FormsModule, CurrencyPipe, MatIconModule, MatSelectModule, MatButtonModule, MatTooltipModule, CanRenderDirective],
  providers: [SELECT_DEFAULTS],
  templateUrl: './items.html',
  styleUrl: './items.scss',
})
export class Items implements OnInit, OnDestroy {
  private readonly itemService = inject(ItemService);
  private readonly categoryService = inject(CategoryService);
  protected readonly sectionAccess = inject(SectionAccessStore);
  private readonly router = inject(Router);
  private readonly searchInput$ = new Subject<string>();

  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  readonly items = signal<ItemDto[]>([]);
  readonly categories = signal<CategoryDto[]>([]);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly totalCount = signal(0);

  readonly searchTerm = signal('');
  readonly categoryFilter = signal('');
  readonly statusFilter = signal<'' | 'active' | 'inactive'>('');

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

  onFilterChange(): void {
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

  onScroll(): void {
    const el = this.scrollContainer?.nativeElement;
    if (!el) {
      return;
    }
    const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 200;
    if (nearBottom) {
      this.loadMore();
    }
  }

  private fetchPage(): void {
    const params = {
      search: this.searchTerm() || undefined,
      categoryId: this.categoryFilter() || undefined,
      isActive: this.statusFilter() === '' ? undefined : this.statusFilter() === 'active',
      page: this.page,
      pageSize: PAGE_SIZE,
    };
    const request = this.sectionAccess.can('section-items-manage')
      ? this.itemService.getManagementPaged(params)
      : this.itemService.getPaged(params);
    request
      .subscribe({
        next: (result) => {
          this.items.update((existing) => [...existing, ...result.items]);
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

  getAvailableStock(item: ItemDto): number {
    if (item.variants.length > 0) {
      return item.variants
        .filter((variant) => variant.isActive)
        .reduce((total, variant) => total + variant.stockQuantity, 0);
    }
    return item.stockQuantity;
  }

  openDetail(item: ItemDto): void {
    this.router.navigate(['/items', item.id]);
  }

  create(): void {
    this.router.navigate(['/items', 'new']);
  }
}
