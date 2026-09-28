import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { CategoryService } from '../../core/auth/category.service';
import { AuthService } from '../../core/auth/auth.service';
import { ItemService } from '../../core/auth/item.service';
import { OrderService } from '../../core/auth/order.service';

interface DashboardStatistics {
  categories: number;
  activeCategories: number;
  items: number;
  activeItems: number;
  orders: number;
  pendingOrders: number;
  deliveredOrders: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly categoryService = inject(CategoryService);
  private readonly itemService = inject(ItemService);
  private readonly orderService = inject(OrderService);

  readonly statistics = signal<DashboardStatistics | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    forkJoin({
      categories: this.categoryService.getAll(),
      items: this.itemService.getPaged({ page: 1, pageSize: 1 }),
      activeItems: this.itemService.getPaged({ page: 1, pageSize: 1, isActive: true }),
      orders: this.orderService.getPaged({ page: 1, pageSize: 1 }),
      pendingOrders: this.orderService.getPaged({ page: 1, pageSize: 1, status: 'Pending' }),
      deliveredOrders: this.orderService.getPaged({ page: 1, pageSize: 1, status: 'Delivered' }),
    }).subscribe({
      next: (result) => {
        this.statistics.set({
          categories: result.categories.length,
          activeCategories: result.categories.filter((category) => category.isActive).length,
          items: result.items.totalCount,
          activeItems: result.activeItems.totalCount,
          orders: result.orders.totalCount,
          pendingOrders: result.pendingOrders.totalCount,
          deliveredOrders: result.deliveredOrders.totalCount,
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.statistics.set(null);
        this.errorMessage.set('Dashboard statistics could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
