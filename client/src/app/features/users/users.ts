import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { UserService } from '../../core/auth/user.service';
import { RoleService } from '../../core/auth/role.service';
import { AuthService } from '../../core/auth/auth.service';
import type { RoleDto, UserDto } from '../../core/models/auth.models';

const PAGE_SIZE = 20;

/**
 * Scroll-paginated, searchable, filterable users list. Loads a page at a time and
 * appends more as the user scrolls near the bottom of the table (infinite scroll),
 * rather than a client-side-only list.
 */
@Component({
  selector: 'app-users',
  standalone: true,
  imports: [FormsModule, DatePipe, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit, OnDestroy {
  private readonly userService = inject(UserService);
  private readonly roleService = inject(RoleService);
  private readonly router = inject(Router);
  private readonly searchInput$ = new Subject<string>();
  protected readonly auth = inject(AuthService);

  @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLDivElement>;

  readonly users = signal<UserDto[]>([]);
  readonly roles = signal<RoleDto[]>([]);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly totalCount = signal(0);

  readonly searchTerm = signal('');
  readonly roleFilter = signal('');
  readonly statusFilter = signal<'' | 'active' | 'inactive'>('');

  private page = 1;
  private hasMore = true;

  ngOnInit(): void {
    this.roleService.getAll().subscribe((roles) => this.roles.set(roles));
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
    this.users.set([]);
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
    this.userService
      .getPaged({
        search: this.searchTerm() || undefined,
        role: this.roleFilter() || undefined,
        isActive: this.statusFilter() === '' ? undefined : this.statusFilter() === 'active',
        page: this.page,
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (result) => {
          this.users.update((existing) => [...existing, ...result.items]);
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

  openDetail(user: UserDto): void {
    this.router.navigate(['/users', user.id]);
  }

  create(): void {
    this.router.navigate(['/users', 'new']);
  }
}
