import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { CategoryService } from '../../core/auth/category.service';
import { AuthService } from '../../core/auth/auth.service';
import { ItemService } from '../../core/auth/item.service';
import { OrderService } from '../../core/auth/order.service';
import { SectionAccessStore } from '../../core/auth/section-access.store';
import type { ProfitLossReportDto } from '../../core/models/auth.models';

interface DashboardStatistics {
  categories: number;
  activeCategories: number;
  items: number;
  activeItems: number;
  orders: number;
  pendingOrders: number;
  deliveredOrders: number;
  profitLossReport: ProfitLossReportDto | null;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, FormsModule, CurrencyPipe, MatIconModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly categoryService = inject(CategoryService);
  private readonly itemService = inject(ItemService);
  private readonly orderService = inject(OrderService);
  protected readonly sectionAccess = inject(SectionAccessStore);

  private readonly today = new Date().toISOString().slice(0, 10);
  readonly reportStartDate = signal(`${this.today.slice(0, 7)}-01`);
  readonly reportEndDate = signal(this.today);

  readonly statistics = signal<DashboardStatistics | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    const canViewOrders = this.sectionAccess.can('section-orders-view');

    forkJoin({
      categories: this.categoryService.getAll(),
      items: this.itemService.getPaged({ page: 1, pageSize: 1 }),
      activeItems: this.itemService.getPaged({ page: 1, pageSize: 1, isActive: true }),
      orders: canViewOrders ? this.orderService.getPaged({ page: 1, pageSize: 1 }) : of({ totalCount: 0 }),
      pendingOrders: canViewOrders ? this.orderService.getPaged({ page: 1, pageSize: 1, status: 'Pending' }) : of({ totalCount: 0 }),
      deliveredOrders: canViewOrders ? this.orderService.getPaged({ page: 1, pageSize: 1, status: 'Delivered' }) : of({ totalCount: 0 }),
      profitLossReport: this.sectionAccess.can('section-reports-view')
        ? this.orderService.getProfitLossReport(this.reportStartDate(), this.reportEndDate())
        : of(null),
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
          profitLossReport: result.profitLossReport,
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

  downloadReport(report: ProfitLossReportDto): void {
    const rows = [
      ['Metric', 'Value'],
      ['Start date', report.startDate],
      ['End date', report.endDate],
      ['Paid orders', report.paidOrderCount],
      ['Refunded orders', report.refundedOrderCount],
      ['Gross sales', report.grossSales],
      ['Refunds', report.refunds],
      ['Net sales', report.netSales],
      ['Cost of goods sold', report.costOfGoodsSold],
      ['Gross profit/loss', report.grossProfitOrLoss ?? 'Incomplete data'],
    ];
    const csv = rows.map((row) => row.map((value) => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = `profit-loss-${report.startDate}-${report.endDate}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }
}
