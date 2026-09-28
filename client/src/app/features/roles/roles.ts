import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { RoleService } from '../../core/auth/role.service';
import type { RoleDto } from '../../core/models/auth.models';

const PAGE_SIZE = 20;

/** Scroll-paginated, searchable roles list. Detail/section-access editing lives on `/roles/:id`. */
@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './roles.html',
  styleUrl: './roles.scss',
})
export class Roles implements OnInit, OnDestroy {
  private readonly roleService = inject(RoleService);
  private readonly router = inject(Router);
  private readonly searchInput$ = new Subject<string>();

  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  readonly roles = signal<RoleDto[]>([]);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');

  private page = 1;
  private hasMore = true;

  ngOnInit(): void {
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

  private reload(): void {
    this.page = 1;
    this.hasMore = true;
    this.isLoading.set(true);
    this.roles.set([]);
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
    this.roleService.getPaged({ search: this.searchTerm() || undefined, page: this.page, pageSize: PAGE_SIZE }).subscribe({
      next: (result) => {
        this.roles.update((existing) => [...existing, ...result.items]);
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

  openDetail(role: RoleDto): void {
    this.router.navigate(['/roles', role.id]);
  }

  create(): void {
    this.router.navigate(['/roles', 'new']);
  }
}
