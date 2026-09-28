import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { OrderService } from '../../../core/auth/order.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ENTITY_TYPES } from '../../../core/models/constants';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import type { OrderDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-order-detail',
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
    MatSnackBarModule,
    MatSelectModule,
    ApprovalPanel,
    ActivityLogPanel,
  ],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  protected readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  readonly order = signal<OrderDto | null>(null);
  readonly isLoading = signal(true);
  readonly isUpdating = signal(false);

  readonly entityType = ENTITY_TYPES.order;

  // Status updates
  newStatus = '';
  trackingNumber = '';
  shippingCarrier = '';

  // Payment status updates
  newPaymentStatus = '';
  paymentReferenceNumber = '';
  offlinePaymentNotes = '';

  readonly statusSteps = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/orders']);
      return;
    }
    this.loadOrder(id);
  }

  loadOrder(id: string): void {
    this.orderService.getById(id).subscribe({
      next: (ord) => {
        this.order.set(ord);
        this.newStatus = ord.status;
        this.trackingNumber = ord.trackingNumber || '';
        this.shippingCarrier = ord.shippingCarrier || '';
        this.newPaymentStatus = ord.paymentStatus;
        this.paymentReferenceNumber = ord.paymentReferenceNumber || '';
        this.offlinePaymentNotes = ord.offlinePaymentNotes || '';
        this.isLoading.set(false);
      },
      error: () => {
        this.snackBar.open('Order not found', 'Close', { duration: 3000 });
        this.router.navigate(['/orders']);
      },
    });
  }

  getStepIndex(status: string): number {
    return this.statusSteps.indexOf(status);
  }

  saveStatus(): void {
    const ord = this.order();
    if (!ord) return;

    this.isUpdating.set(true);
    this.orderService
      .updateStatus(ord.id, {
        status: this.newStatus,
        trackingNumber: this.trackingNumber || null,
        shippingCarrier: this.shippingCarrier || null,
      })
      .subscribe({
        next: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Order fulfillment status updated!', 'Dismiss', { duration: 3000 });
          this.loadOrder(ord.id);
        },
        error: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Failed to update status', 'Dismiss', { duration: 3000 });
        },
      });
  }

  savePaymentStatus(): void {
    const ord = this.order();
    if (!ord) return;

    this.isUpdating.set(true);
    this.orderService
      .updatePaymentStatus(ord.id, {
        paymentStatus: this.newPaymentStatus,
        paymentReferenceNumber: this.paymentReferenceNumber || null,
        offlinePaymentNotes: this.offlinePaymentNotes || null,
      })
      .subscribe({
        next: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Payment status updated!', 'Dismiss', { duration: 3000 });
          this.loadOrder(ord.id);
        },
        error: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Failed to update payment status', 'Dismiss', { duration: 3000 });
        },
      });
  }
}
