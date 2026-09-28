import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { OrderService } from '../../core/auth/order.service';
import { AuthService } from '../../core/auth/auth.service';
import type { OrderDto, OrderStatus, PaymentStatus } from '../../core/models/auth.models';

const PAGE_SIZE = 15;

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class Orders implements OnInit, OnDestroy {
  private readonly orderService = inject(OrderService);
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly search$ = new Subject<string>();

  readonly orders = signal<OrderDto[]>([]);
  readonly isLoading = signal(true);
  readonly totalCount = signal(0);
  readonly page = signal(1);

  readonly searchTerm = signal('');
  readonly selectedStatus = signal<string>('');
  readonly selectedPaymentStatus = signal<string>('');

  readonly statusTabs = [
    { label: 'All Orders', value: '' },
    { label: 'Pending', value: 'Pending' },
    { label: 'Confirmed', value: 'Confirmed' },
    { label: 'Processing', value: 'Processing' },
    { label: 'Shipped', value: 'Shipped' },
    { label: 'Delivered', value: 'Delivered' },
    { label: 'Cancelled', value: 'Cancelled' },
  ];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((val) => {
      this.searchTerm.set(val);
      this.page.set(1);
      this.loadOrders();
    });
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.search$.complete();
  }

  onSearch(term: string): void {
    this.search$.next(term);
  }

  selectStatus(status: string): void {
    this.selectedStatus.set(status);
    this.page.set(1);
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.orderService
      .getPaged({
        search: this.searchTerm() || undefined,
        status: this.selectedStatus() || undefined,
        paymentStatus: this.selectedPaymentStatus() || undefined,
        page: this.page(),
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (res) => {
          this.orders.set(res.items);
          this.totalCount.set(res.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false),
      });
  }

  getStatusClass(status: OrderStatus | string): string {
    switch (status) {
      case 'Pending':
        return 'status-pending';
      case 'Confirmed':
        return 'status-confirmed';
      case 'Processing':
        return 'status-processing';
      case 'Shipped':
        return 'status-shipped';
      case 'Delivered':
        return 'status-delivered';
      case 'Cancelled':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  getPaymentStatusClass(pStatus: PaymentStatus | string): string {
    switch (pStatus) {
      case 'Paid':
        return 'pay-paid';
      case 'Pending':
        return 'pay-pending';
      case 'Failed':
        return 'pay-failed';
      case 'Refunded':
        return 'pay-refunded';
      default:
        return 'pay-default';
    }
  }

  viewOrder(order: OrderDto): void {
    this.router.navigate(['/orders', order.id]);
  }
}
