import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderChatRealtimeService } from '../../../core/auth/order-chat-realtime.service';
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
import { ENTITY_TYPES } from '../../../core/models/constants';
import { OrderProgress } from '../../../shared/order-progress/order-progress';
import { OrderChatWidgetState } from '../../../shared/order-chat/order-chat-widget-state';
import { ApprovalPanel } from '../../../shared/approval-panel/approval-panel';
import { ActivityLogPanel } from '../../../shared/activity-log-panel/activity-log-panel';
import { ReviewService } from '../../../core/auth/review.service';
import { ReviewForm } from '../../../shared/review-form/review-form';
import { StarRating } from '../../../shared/star-rating/star-rating';
import type { ItemReviewDto, OrderDto, PaymentSettingsDto, UpdateOrderStatusRequest } from '../../../core/models/auth.models';
import { ConfirmService } from '../../../shared/confirm-dialog/confirm-dialog';

type JourneyKey = 'approval' | 'charge' | 'accept' | 'advance' | 'fulfilment' | 'final' | 'ship' | 'deliver';
interface JourneyStep {
  key: JourneyKey;
  title: string;
  summary: string;
  state: 'done' | 'current' | 'upcoming' | 'blocked';
}

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
    OrderProgress,
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
  private readonly snackBar = inject(MatSnackBar);
  private readonly reviewService = inject(ReviewService);
  private readonly confirmService = inject(ConfirmService);
  private readonly orderChatWidget = inject(OrderChatWidgetState);
  private readonly destroyRef = inject(DestroyRef);
  private readonly realtime = inject(OrderChatRealtimeService);

  readonly order = signal<OrderDto | null>(null);
  readonly paymentSettings = signal<PaymentSettingsDto | null>(null);
  readonly isLoading = signal(true);
  readonly reviews = signal<Record<string, ItemReviewDto>>({});
  readonly reviewingItemId = signal<string | null>(null);
  readonly isUpdating = signal(false);
  readonly paymentUnavailable = signal<string | null>(null);

  readonly entityType = ENTITY_TYPES.order;

  openOrderChat(orderId: string): void {
    this.orderChatWidget.open(orderId);
  }

  // Status updates
  advanceInput: number | null = null;
  paymentMode: 'advance' | 'full' = 'advance';
  trackingNumber = '';
  shippingCarrier = '';
  shippingFeeInput: number | null = null;
  shippingFeeNote = '';

  // Payment updates
  newPaymentStatus = 'Paid';
  paymentReferenceNumber = '';
  offlinePaymentNotes = '';
  refundReferenceNumber = '';
  refundNotes = '';

  /** Default share of the order total requested as the advance booking. */
  private readonly defaultAdvanceRate = 0.3;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/orders']);
      return;
    }
    this.loadOrder(id);
    this.realtime.orderUpdates$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((update) => {
      if (update.orderId === id) this.loadOrder(id);
    });
  }

  loadOrder(id: string): void {
    this.orderService.getById(id).subscribe({
      next: (ord) => {
        this.order.set(ord);
        this.trackingNumber = ord.trackingNumber || '';
        this.shippingCarrier = ord.shippingCarrier || '';
        this.shippingFeeInput = ord.shippingFeeConfirmed ? ord.shippingFee : null;
        this.shippingFeeNote = ord.shippingFeeNote || '';
        this.advanceInput = ord.advanceAmount > 0 ? ord.advanceAmount : Math.round(ord.totalAmount * this.defaultAdvanceRate);
        this.newPaymentStatus = 'Paid';
        this.paymentReferenceNumber = (ord.paymentStage === 'Advance' ? ord.advanceReferenceNumber : ord.paymentReferenceNumber) || '';
        this.offlinePaymentNotes = ord.offlinePaymentNotes || '';
        this.refundReferenceNumber = ord.refundReferenceNumber || '';
        this.refundNotes = ord.refundNotes || '';
        this.isLoading.set(false);
        if (ord.status === 'Delivered' && !this.isPrivileged()) {
          this.reviewService.getForOrder(ord.id).subscribe({
            next: (list) => this.reviews.set(Object.fromEntries(list.map((r) => [r.orderItemId, r]))),
          });
        }
        if (!this.isPrivileged() && ord.paymentStage !== 'None' && !this.paymentSettings()) {
          this.paymentUnavailable.set(null);
          this.orderService.getPaymentInfo(ord.id).subscribe({
            next: (settings) => this.paymentSettings.set(settings),
            error: (err) => {
              this.paymentSettings.set(null);
              this.paymentUnavailable.set(err?.error?.errors?.[0] ?? 'Payment details are not available yet.');
            },
          });
        }
      },
      error: () => {
        this.snackBar.open('Order not found', 'Close', { duration: 3000, panelClass: ['snack-error'] });
        this.router.navigate(['/orders']);
      },
    });
  }

  /** The order as one ordered list of steps; the first unfinished step is the one to act on now. */
  journey(ord: OrderDto): JourneyStep[] {
    const progress = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered'].indexOf(ord.status);
    const advanceSettled = ord.advanceAmount > 0 ? ord.advanceStatus === 'Paid' : progress >= 2 || ord.paymentStatus === 'Paid';
    const rejected = ord.approvalStatus === 'Rejected';
    const steps: { key: JourneyKey; title: string; done: boolean; summary: string }[] = [
      { key: 'approval', title: 'Approval', done: this.isApprovedForProcessing(ord), summary: 'Approved' },
      { key: 'charge', title: 'Delivery charge', done: ord.shippingFeeConfirmed || progress >= 1, summary: ord.shippingFee > 0 ? `${this.money(ord.shippingFee)} set` : 'Free delivery' },
      { key: 'accept', title: 'Buyer confirmation', done: !!ord.chargesAcceptedAt || progress >= 1, summary: 'Buyer accepted the total' },
      { key: 'advance', title: 'Confirm order & advance', done: progress >= 1 && advanceSettled, summary: ord.advanceAmount > 0 ? `Advance ${this.money(ord.advanceAmount)} received` : 'Confirmed, paid in full' },
      { key: 'fulfilment', title: 'Fulfilment', done: progress >= 2, summary: 'Fulfilment started' },
      { key: 'final', title: 'Final payment', done: ord.paymentStatus === 'Paid', summary: 'Payment complete' },
      { key: 'ship', title: 'Shipping', done: progress >= 3, summary: ord.trackingNumber ? `Shipped · ${ord.trackingNumber}` : 'Shipped' },
      { key: 'deliver', title: 'Delivery', done: progress >= 4, summary: 'Delivered' },
    ];
    const currentIndex = steps.findIndex((s) => !s.done);
    return steps.map((s, i) => ({
      key: s.key,
      title: s.title,
      summary: s.summary,
      state: s.done ? 'done' : i === currentIndex ? (rejected && s.key === 'approval' ? 'blocked' : 'current') : 'upcoming',
    }));
  }

  private money(value: number): string {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 }).format(value);
  }

  isBuyer(order: OrderDto | null): boolean {
    return this.isBuyerOfOrder(order);
  }

  /** Payment status of the payment being collected right now. */
  stagePaymentStatus(order: OrderDto): string {
    return order.paymentStage === 'Advance' ? order.advanceStatus : order.paymentStatus;
  }

  hasMoneyHeld(order: OrderDto): boolean {
    return order.amountPaid > 0;
  }

  private runStatus(ord: OrderDto, request: UpdateOrderStatusRequest, success: string): void {
    this.isUpdating.set(true);
    this.orderService.updateStatus(ord.id, request).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.snackBar.open(success, 'Dismiss', { duration: 3000 });
        this.loadOrder(ord.id);
      },
      error: (err) => {
        this.isUpdating.set(false);
        this.snackBar.open(err?.error?.errors?.[0] ?? 'Failed to update the order', 'Dismiss', { duration: 4000, panelClass: ['snack-error'] });
      },
    });
  }

  confirmOrder(): void {
    const ord = this.order();
    if (!ord) return;
    const advance = ord.customRequestId && this.paymentMode === 'advance' ? Number(this.advanceInput ?? 0) : 0;
    if (Number.isNaN(advance) || advance < 0 || advance > ord.totalAmount || (ord.customRequestId && this.paymentMode === 'advance' && advance <= 0)) {
      this.snackBar.open('Advance must be between 1 and the order total.', 'Dismiss', { duration: 3500, panelClass: ['snack-error'] });
      return;
    }
    this.runStatus(ord, { status: 'Confirmed', advanceAmount: advance }, advance > 0 ? 'Order confirmed. Advance requested from the buyer.' : 'Order confirmed. Full payment requested from the buyer.');
  }

  startFulfilment(): void {
    const ord = this.order();
    if (ord) this.runStatus(ord, { status: 'Processing' }, 'Fulfilment started.');
  }

  markShipped(): void {
    const ord = this.order();
    if (ord) this.runStatus(ord, { status: 'Shipped', trackingNumber: this.trackingNumber || null, shippingCarrier: this.shippingCarrier || null }, 'Order marked as shipped.');
  }

  markDelivered(): void {
    const ord = this.order();
    if (ord) this.runStatus(ord, { status: 'Delivered' }, 'Order marked as delivered.');
  }

  cancelOrder(): void {
    const ord = this.order();
    if (!ord) return;
    this.confirmService
      .confirm({ title: 'Cancel this order?', message: 'This cannot be undone. Stock will be released. Any payment received must be refunded first.', confirmText: 'Cancel order', cancelText: 'Keep order', destructive: true })
      .subscribe((ok) => {
        if (ok) this.runStatus(ord, { status: 'Cancelled' }, 'Order cancelled.');
      });
  }

  acceptCharges(): void {
    const ord = this.order();
    if (!ord) return;
    this.isUpdating.set(true);
    this.orderService.acceptCharges(ord.id).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.snackBar.open('Thanks! You accepted the delivery charge and total.', 'Dismiss', { duration: 3000 });
        this.loadOrder(ord.id);
      },
      error: (err) => {
        this.isUpdating.set(false);
        this.snackBar.open(err?.error?.errors?.[0] ?? 'Could not accept the delivery charge', 'Dismiss', { duration: 4000, panelClass: ['snack-error'] });
      },
    });
  }
  onApprovalChanged(): void {
    const ord = this.order();
    if (ord) this.loadOrder(ord.id);
  }

  canReview(order: OrderDto): boolean {
    return order.status === 'Delivered' && !this.isPrivileged();
  }

  onReviewSubmitted(review: ItemReviewDto): void {
    this.reviews.update((m) => ({ ...m, [review.orderItemId]: review }));
    this.reviewingItemId.set(null);
  }

  /** Any staff member (admin or seller) viewing an order they placed themselves is treated as the buyer. */
  private isBuyerOfOrder(order: OrderDto | null): boolean {
    return !!order && order.customerId === this.auth.currentUser()?.id;
  }

  isPrivileged(): boolean {
    if (this.isBuyerOfOrder(this.order())) return false;
    return this.auth.isAdmin() ||
      this.sectionAccess.can('section-orders-manage') ||
      this.sectionAccess.can('section-orders-payment') ||
      this.sectionAccess.can('section-orders-refund');
  }

  /** Next valid statuses for the order (mirrors the server's forward-only rules), including the current one. */
  statusOptions(order: OrderDto): string[] {
    const next: Record<string, string[]> = {
      Pending: ['Confirmed', 'Cancelled'],
      Confirmed: ['Processing', 'Cancelled'],
      Processing: ['Shipped', 'Cancelled'],
      Shipped: ['Delivered'],
    };
    const options = [order.status, ...(next[order.status] ?? [])];
    // Until the order is approved it can only be held or cancelled.
    return order.status === 'Pending' && !this.isApprovedForProcessing(order) ? options.filter((s) => s === 'Pending' || s === 'Cancelled') : options;
  }

  /** Orders with no approval record (legacy) are not blocked. */
  isApprovedForProcessing(order: OrderDto): boolean {
    return !order.approvalStatus || order.approvalStatus === 'Approved';
  }

  canCancelOwn(order: OrderDto | null): boolean {
    return !!order && order.status === 'Pending' && order.customerId === this.auth.currentUser()?.id;
  }

  cancelMyOrder(): void {
    const ord = this.order();
    if (!ord) return;
    this.confirmService
      .confirm({ title: 'Cancel this order?', message: 'This cannot be undone. Your items will be released.', confirmText: 'Cancel order', cancelText: 'Keep order', destructive: true })
      .subscribe((ok) => {
        if (!ok) return;
    this.isUpdating.set(true);
    this.orderService.cancelMine(ord.id).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.snackBar.open('Your order has been cancelled', 'Dismiss', { duration: 3000 });
        this.loadOrder(ord.id);
      },
      error: (err) => {
        this.isUpdating.set(false);
        this.snackBar.open(err?.error?.errors?.[0] ?? 'Could not cancel the order', 'Dismiss', { duration: 4000, panelClass: ['snack-error'] });
      },
    });
      });
  }

  saveShippingFee(): void {
    const ord = this.order();
    const fee = Number(this.shippingFeeInput);
    if (!ord || this.shippingFeeInput === null || this.shippingFeeInput === ('' as unknown) || Number.isNaN(fee) || fee < 0) {
      this.snackBar.open('Enter a delivery charge (0 for free delivery).', 'Dismiss', { duration: 3000, panelClass: ['snack-error'] });
      return;
    }
    this.isUpdating.set(true);
    this.orderService.setShippingFee(ord.id, { shippingFee: fee, note: this.shippingFeeNote.trim() || null }).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.snackBar.open('Delivery charge saved', 'Dismiss', { duration: 3000 });
        this.loadOrder(ord.id);
      },
      error: (err) => {
        this.isUpdating.set(false);
        this.snackBar.open(err?.error?.errors?.[0] ?? 'Could not save delivery charge', 'Dismiss', { duration: 4000, panelClass: ['snack-error'] });
      },
    });
  }

  canSubmitPaymentDetails(order: OrderDto | null): boolean {
    if (!order || this.isPrivileged() || !this.isPaymentAvailable(order) || !['Pending', 'Failed'].includes(this.stagePaymentStatus(order))) return false;
    return order.customerId === this.auth.currentUser()?.id;
  }

  canUpdatePaymentStatus(order: OrderDto | null): boolean {
    return !!order?.canManage && !this.isBuyerOfOrder(order) && this.sectionAccess.can('section-orders-payment') && this.isPaymentAvailable(order) && ['Pending', 'Failed'].includes(this.stagePaymentStatus(order!));
  }

  canUpdateOrderStatus(order: OrderDto | null): boolean {
    return !!order?.canManage && !this.isBuyerOfOrder(order) && this.sectionAccess.can('section-orders-manage') && !!order;
  }

  canRecordRefund(order: OrderDto | null): boolean {
    return !!order?.canManage && !this.isBuyerOfOrder(order) && this.sectionAccess.can('section-orders-refund') &&     !!order && order.amountPaid > 0 && order.paymentStatus !== 'Refunded';
  }

  isPaymentAvailable(order: OrderDto | null): boolean {
        return !!order && order.paymentStage !== 'None' && !['Pending', 'Cancelled'].includes(order.status);
  }

  submitPaymentDetails(): void {
    const ord = this.order();
    const reference = this.paymentReferenceNumber.trim();
    if (!ord || !reference) {
      this.snackBar.open('Enter the UPI transaction reference ID.', 'Dismiss', { duration: 3000, panelClass: ['snack-error'] });
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
        error: (err) => {
          this.isUpdating.set(false);
          this.snackBar.open(err?.error?.errors?.[0] ?? 'Failed to submit payment details.', 'Dismiss', { duration: 3500, panelClass: ['snack-error'] });
        },
      });
  }

  savePaymentStatus(): void {
    const ord = this.order();
    if (!ord) return;
    const existingRef = ord.paymentStage === 'Advance' ? ord.advanceReferenceNumber : ord.paymentReferenceNumber;
    if (this.newPaymentStatus === 'Paid' && !this.paymentReferenceNumber.trim() && !existingRef) {
      this.snackBar.open('A payment reference is required before confirming payment.', 'Dismiss', { duration: 3500, panelClass: ['snack-error'] });
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
        error: (err) => {
          this.isUpdating.set(false);
          this.snackBar.open(err?.error?.errors?.[0] ?? 'Failed to update payment status', 'Dismiss', { duration: 3500, panelClass: ['snack-error'] });
        },
      });
  }

  recordRefund(): void {
    const ord = this.order();
    const reference = this.refundReferenceNumber.trim();
    if (!ord || !this.canRecordRefund(ord)) return;
    if (!reference) {
      this.snackBar.open('Enter the refund transaction reference.', 'Dismiss', { duration: 3500, panelClass: ['snack-error'] });
      return;
    }

    this.isUpdating.set(true);
    this.orderService.recordRefund(ord.id, {
      refundAmount: ord.amountPaid,
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
        this.snackBar.open(Array.isArray(errors) ? errors.join(' ') : 'Failed to record refund.', 'Dismiss', { duration: 4000, panelClass: ['snack-error'] });
      },
    });
  }
}
