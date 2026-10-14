import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';
import { CustomRequestScope, CustomRequestService, CustomRequestSummary } from '../../core/auth/custom-request.service';

interface Tab {
  scope: CustomRequestScope;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-custom-requests',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './custom-requests.html',
  styleUrl: './custom-requests.scss',
})
export class CustomRequests implements OnInit {
  private readonly service = inject(CustomRequestService);
  private readonly auth = inject(AuthService);

  readonly tabs = computed<Tab[]>(() => {
    const tabs: Tab[] = [{ scope: 'mine', label: 'My requests', icon: 'palette' }];
    if (this.auth.isManager() || this.auth.isAdmin()) {
      tabs.unshift({ scope: 'open', label: 'Open for quotes', icon: 'campaign' });
    }
    if (this.auth.isManager()) {
      tabs.splice(1, 0, { scope: 'joined', label: 'My quotes', icon: 'request_quote' });
    }
    if (this.auth.isAdmin()) {
      tabs.push({ scope: 'all', label: 'All requests', icon: 'list_alt' });
    }
    return tabs;
  });

  readonly scope = signal<CustomRequestScope>('mine');
  readonly requests = signal<CustomRequestSummary[]>([]);
  readonly isLoading = signal(true);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly pageSize = 12;

  ngOnInit(): void {
    this.scope.set(this.tabs()[0].scope);
    this.load();
  }

  select(scope: CustomRequestScope): void {
    this.scope.set(scope);
    this.page.set(1);
    this.load();
  }

  goTo(page: number): void {
    this.page.set(page);
    this.load();
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  private load(): void {
    this.isLoading.set(true);
    this.service.getPaged(this.scope(), undefined, this.page(), this.pageSize).subscribe({
      next: (result) => {
        this.requests.set(result.items);
        this.total.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
