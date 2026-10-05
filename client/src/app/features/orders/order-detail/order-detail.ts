import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { SELECT_DEFAULTS } from '../../../shared/select-defaults';
import { OrderService } from '../../../core/auth/order.service';
import { AuthService } from '../../../core/auth/auth.service';
import { SectionAccessStore } from '../../../core/auth/section-access.store';
import { PaymentSettingsService } from '../../../core/auth/payment-settings.service';
import { ENTITY_TYPES } from '../../../core/models/constants';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ReviewService } from '../../../core/auth/review.service';
import { ReviewForm } from '../../../shared/review-form/review-form';
import { StarRating } from '../../../shared/star-rating/star-rating';
import type { ItemReviewDto, OrderDto, PaymentSettingsDto } from '../../../core/models/auth.models';

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
    ReviewForm,
    StarRating,
  ],
  providers: [SELECT_DEFAULTS],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  protected readonly auth = inject(AuthService);
  private readonly sectionAccess = inject(SectionAccessStore);
  private readonly paymentSettingsService = inject(PaymentSettingsService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly reviewService = inject(ReviewService);

  readonly order = signal<OrderDto | null>(null);
  readonly paymentSettings = signal<PaymentSettingsDto | null>(null);
  readonly isLoading = signal(true);
  readonly reviews = signal<Record<string, ItemReviewDto>>({});
  readonly reviewingItemId = signal<string | null>(null);
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
  refundReferenceNumber = '';
  refundNotes = '';

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
        this.refundReferenceNumber = ord.refundReferenceNumber || '';
        this.refundNotes = ord.refundNotes || '';
        this.isLoading.set(false);
        if (ord.status === 'Delivered' && !this.isPrivileged()) {
          this.reviewService.getForOrder(ord.id).subscribe({
            next: (list) => this.reviews.set(Object.fromEntries(list.map((r) => [r.orderItemId, r]))),
          });
        }
        if (!this.isPrivileged() && this.isPaymentAvailable(ord) && ['Pending', 'Failed'].includes(ord.paymentStatus) && !this.paymentSettings()) {
          this.paymentSettingsService.get().subscribe({
            next: (settings) => this.paymentSettings.set(settings),
            error: () => this.paymentSettings.set(null),
          });
        } else if (this.isPrivileged() || ['Paid', 'Refunded'].includes(ord.paymentStatus)) {
          this.paymentSettings.set(null);
        }
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

  canReview(order: OrderDto): boolean {
    return order.status === 'Delivered' && !this.isPrivileged();
  }

  onReviewSubmitted(review: ItemReviewDto): void {
    this.reviews.update((m) => ({ ...m, [review.orderItemId]: review }));
    this.reviewingItemId.set(null);
  }

  isPrivileged(): boolean {
    return this.auth.isAdmin() ||
      this.sectionAccess.can('section-orders-manage') ||
      this.sectionAccess.can('section-orders-payment') ||
      this.sectionAccess.can('section-orders-refund');
  }

  canSubmitPaymentDetails(order: OrderDto | null): boolean {
    if (!order || this.isPrivileged() || !this.isPaymentAvailable(order) || !['Pending', 'Failed'].includes(order.paymentStatus)) return false;
    const currentUser = this.auth.currentUser();
    return order.customerId === currentUser?.id;
  }

  canUpdatePaymentStatus(order: OrderDto | null): boolean {
    return this.sectionAccess.can('section-orders-payment') && !!order && this.isPaymentAvailable(order) && ['Pending', 'Failed'].includes(order.paymentStatus);
  }

  canUpdateOrderStatus(order: OrderDto | null): boolean {
    return this.sectionAccess.can('section-orders-manage') && !!order;
  }

  canRecordRefund(order: OrderDto | null): boolean {
    return this.sectionAccess.can('section-orders-refund') && !!order && order.paymentStatus === 'Paid';
  }

  isPaymentAvailable(order: OrderDto | null): boolean {
    return !!order && ['Confirmed', 'Processing', 'Shipped', 'Delivered'].includes(order.status);
  }

  submitPaymentDetails(): void {
    const ord = this.order();
    const reference = this.paymentReferenceNumber.trim();
    if (!ord || !reference) {
      this.snackBar.open('Enter the UPI transaction reference ID.', 'Dismiss', { duration: 3000 });
      return;
    }

    this.isUpdating.set(true);
    this.orderService
      .submitPaymentDetails(ord.id, {
        paymentReferenceNumber: reference,
        offlinePaymentNotes: this.offlinePaymentNotes.trim() || null,
      })
      .subscribe({
        next: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Payment details sent for verification.', 'Dismiss', { duration: 3000 });
          this.loadOrder(ord.id);
        },
        error: () => {
          this.isUpdating.set(false);
          this.snackBar.open('Failed to submit payment details.', 'Dismiss', { duration: 3000 });
        },
      });
  }

  savePaymentStatus(): void {
    const ord = this.order();
    if (!ord) return;
    if (this.newPaymentStatus === 'Paid' && !this.paymentReferenceNumber.trim() && !ord.paymentReferenceNumber) {
      this.snackBar.open('A payment reference is required before confirming payment.', 'Dismiss', { duration: 3500 });
      return;
    }

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

  recordRefund(): void {
    const ord = this.order();
    const reference = this.refundReferenceNumber.trim();
    if (!ord || !this.canRecordRefund(ord)) return;
    if (!reference) {
      this.snackBar.open('Enter the refund transaction reference.', 'Dismiss', { duration: 3500 });
      return;
    }

    this.isUpdating.set(true);
    this.orderService.recordRefund(ord.id, {
      refundAmount: ord.totalAmount,
      refundReferenceNumber: reference,
      refundNotes: this.refundNotes.trim() || null,
    }).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.snackBar.open('Refund recorded.', 'Dismiss', { duration: 3000 });
        this.loadOrder(ord.id);
      },
      error: (error) => {
        this.isUpdating.set(false);
        const errors = error?.error?.errors;
        this.snackBar.open(Array.isArray(errors) ? errors.join(' ') : 'Failed to record refund.', 'Dismiss', { duration: 4000 });
      },
    });
  }
}
