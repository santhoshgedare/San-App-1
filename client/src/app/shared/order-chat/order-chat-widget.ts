import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input, OnDestroy, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../core/auth/auth.service';
import { OrderChatRealtimeService } from '../../core/auth/order-chat-realtime.service';
import { OrderService } from '../../core/auth/order.service';
import { OrderChatService, type OrderChat, type OrderChatNotification } from '../../core/auth/order-chat.service';
import type { OrderDto } from '../../core/models/auth.models';
import { OrderChatPanel } from './order-chat';
import { OrderChatWidgetState } from './order-chat-widget-state';

const ORDER_PAGE_SIZE = 25;
const NOTIFICATION_POLL_MS = 30000;

@Component({
  selector: 'app-order-chat-widget',
  standalone: true,
  imports: [DatePipe, MatIconModule, OrderChatPanel],
  templateUrl: './order-chat-widget.html',
  styleUrl: './order-chat-widget.scss',
  host: { '[style.--chat-bottom-offset.px]': 'bottomOffset()' },
})
export class OrderChatWidget implements OnDestroy {
  protected readonly state = inject(OrderChatWidgetState);
  private readonly auth = inject(AuthService);
  private readonly orderService = inject(OrderService);
  private readonly chatService = inject(OrderChatService);
  protected readonly realtime = inject(OrderChatRealtimeService);
  private readonly snackBar = inject(MatSnackBar);

  readonly orders = signal<OrderDto[]>([]);
  readonly notifications = signal<OrderChatNotification[]>([]);
  readonly readMessageIds = signal<Record<string, string>>({});
  readonly notificationError = signal(false);
  readonly chatRefreshKey = signal(0);
  readonly isLoadingOrders = signal(false);
  readonly hasLoadedOrders = signal(false);
  readonly totalOrders = signal(0);
  readonly page = signal(0);
  readonly orderLoadError = signal(false);
  readonly bottomOffset = input(0);
  private readonly resolvingOrderId = signal<string | null>(null);
  readonly selectedOrderLoaded = computed(() => this.orders().some((order) => order.id === this.state.orderId()));
  readonly unreadNotifications = computed(() =>
    this.notifications().filter((notice) =>
      !notice.isMine && this.readMessageIds()[notice.orderId] !== notice.messageId));
  private notificationTimer?: ReturnType<typeof setInterval>;
  private readonly realtimeSubscription: Subscription;
  private readonly orderUpdateSubscription: Subscription;
  private readonly router = inject(Router);
  private notificationUserId: string | null = null;

  constructor() {
    this.realtime.connect();
    this.orderUpdateSubscription = this.realtime.orderUpdates$.subscribe((update) => {
      this.snackBar.open(`Order ${update.orderNumber}: ${update.message}`, 'View order', { duration: 8000 })
        .onAction().subscribe(() => void this.router.navigate(['/orders', update.orderId]));
    });
    this.realtimeSubscription = this.realtime.messages$.subscribe((event) => {
      this.refreshNotifications();
      if (this.state.orderId() === event.orderId) this.chatRefreshKey.update((key) => key + 1);
    });
    this.refreshNotifications();
    this.notificationTimer = setInterval(() => this.refreshNotifications(), NOTIFICATION_POLL_MS);
    effect(() => {
      if (!this.state.isOpen()) return;
      if (!this.hasLoadedOrders()) {
        this.loadOrders();
        return;
      }

      const selectedId = this.state.orderId();
      if (!selectedId || this.orders().some((order) => order.id === selectedId)) return;
      if (this.resolvingOrderId() === selectedId) return;

      this.resolvingOrderId.set(selectedId);
      this.orderService.getById(selectedId).subscribe({
        next: (order) => {
          this.orders.update((orders) => orders.some((candidate) => candidate.id === order.id)
            ? orders
            : [order, ...orders]);
          this.resolvingOrderId.set(null);
        },
        error: () => {
          this.resolvingOrderId.set(null);
          if (this.state.orderId() === selectedId) this.state.orderId.set(null);
          this.snackBar.open('Could not open this order for chat.', 'Close', {
            duration: 4000,
            panelClass: ['snack-error'],
          });
        },
      });
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.notificationTimer);
    this.realtimeSubscription.unsubscribe();
    this.orderUpdateSubscription.unsubscribe();
    void this.realtime.disconnect();
  }

  toggle(): void {
    if (this.state.isOpen()) {
      this.state.close();
      return;
    }

    this.state.open();
  }

  loadOrders(): void {
    if (this.isLoadingOrders()) return;
    const nextPage = this.page() + 1;
    this.isLoadingOrders.set(true);
    this.orderLoadError.set(false);
    this.orderService.getPaged({ page: nextPage, pageSize: ORDER_PAGE_SIZE }).subscribe({
      next: (result) => {
        this.orders.update((orders) => [...orders, ...result.items]);
        this.totalOrders.set(result.totalCount);
        this.page.set(nextPage);
        this.hasLoadedOrders.set(true);
        this.isLoadingOrders.set(false);
      },
      error: () => {
        this.orderLoadError.set(true);
        this.isLoadingOrders.set(false);
        this.snackBar.open('Could not load orders for chat.', 'Close', {
          duration: 4000,
          panelClass: ['snack-error'],
        });
      },
    });
  }

  selectedOrderChanged(orderId: string): void {
    this.state.orderId.set(orderId || null);
  }

  openNotification(orderId: string): void {
    this.state.open(orderId);
  }

  markConversationRead(chat: OrderChat): void {
    const orderId = this.state.orderId();
    const latestHumanMessage = [...chat.messages].reverse().find((message) => message.senderRole !== 'Auto');
    if (orderId && latestHumanMessage) this.markRead(orderId, latestHumanMessage.id);
  }

  private refreshNotifications(): void {
    const userId = this.auth.currentUser()?.id;
    if (!userId) return;
    if (userId !== this.notificationUserId) {
      this.notificationUserId = userId;
      this.readMessageIds.set({});
      this.notifications.set([]);
    }

    this.chatService.getNotifications().subscribe({
      next: (notifications) => {
        const readIds = { ...this.readMessageIds() };
        for (const notification of notifications) {
          const key = this.readStorageKey(userId, notification.orderId);
          if (readIds[notification.orderId] === undefined) {
            const storedId = localStorage.getItem(key);
            if (storedId) readIds[notification.orderId] = storedId;
          }
        }
        this.readMessageIds.set(readIds);
        this.notificationError.set(false);

        const previous = new Map(this.notifications().map((notice) => [notice.orderId, notice.messageId]));
        this.notifications.set(notifications);
        for (const notice of notifications) {
          if (previous.has(notice.orderId) && previous.get(notice.orderId) !== notice.messageId && !notice.isMine) {
            this.snackBar.open(`New message from ${notice.senderName} on order ${notice.orderNumber}.`, 'Open chat', {
              duration: 7000,
              panelClass: ['snack-info'],
            }).onAction().subscribe(() => this.openNotification(notice.orderId));
          }
        }
      },
      error: () => this.notificationError.set(true),
    });
  }

  private markRead(orderId: string, messageId: string): void {
    if (this.readMessageIds()[orderId] === messageId) return;
    const userId = this.auth.currentUser()?.id;
    if (!userId) return;
    localStorage.setItem(this.readStorageKey(userId, orderId), messageId);
    this.readMessageIds.update((readIds) => ({ ...readIds, [orderId]: messageId }));
  }

  private readStorageKey(userId: string, orderId: string): string {
    return `order-chat-read:${userId}:${orderId}`;
  }

  orderLabel(order: OrderDto): string {
    const counterpart = order.canManage ? order.customerName : order.sellerName || order.customerName;
    return `${order.orderNumber} · ${counterpart}`;
  }
}
