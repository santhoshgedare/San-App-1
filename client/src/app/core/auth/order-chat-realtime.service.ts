import { Injectable, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export interface OrderChatLiveEvent {
  orderId: string;
}

export interface OrderLiveUpdate {
  orderId: string;
  orderNumber: string;
  notice: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class OrderChatRealtimeService {
  private readonly auth = inject(AuthService);
  private readonly incomingMessages = new Subject<OrderChatLiveEvent>();
  private readonly incomingOrderUpdates = new Subject<OrderLiveUpdate>();
  private connection: HubConnection | null = null;
  private retryTimer?: ReturnType<typeof setTimeout>;
  private active = false;

  readonly isConnected = signal(false);
  readonly hasConnectionError = signal(false);
  readonly messages$ = this.incomingMessages.asObservable();
  readonly orderUpdates$ = this.incomingOrderUpdates.asObservable();

  connect(): void {
    this.active = true;
    if (!this.connection) this.createConnection();
    this.startConnection();
  }

  async disconnect(): Promise<void> {
    this.active = false;
    clearTimeout(this.retryTimer);
    this.retryTimer = undefined;
    this.isConnected.set(false);
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  private createConnection(): void {
    const hubUrl = environment.apiUrl.replace(/\/api\/?$/, '/hubs/order-chat');
    this.connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('OrderChatMessage', (event: OrderChatLiveEvent) => {
      if (!this.active) return;
      this.incomingMessages.next(event);
    });
    this.connection.on('OrderUpdated', (event: OrderLiveUpdate) => {
      if (!this.active) return;
      this.incomingOrderUpdates.next(event);
    });
    this.connection.onreconnecting(() => this.isConnected.set(false));
    this.connection.onreconnected(() => {
      if (!this.active) return;
      this.isConnected.set(true);
      this.hasConnectionError.set(false);
    });
    this.connection.onclose(() => {
      this.isConnected.set(false);
      if (this.active) this.scheduleReconnect();
    });
  }

  private startConnection(): void {
    if (!this.active || !this.connection || this.connection.state !== HubConnectionState.Disconnected) return;
    if (!this.auth.getAccessToken()) {
      this.hasConnectionError.set(true);
      this.scheduleReconnect();
      return;
    }

    this.connection.start().then(() => {
      if (!this.active) return;
      this.isConnected.set(true);
      this.hasConnectionError.set(false);
    }).catch(() => {
      if (!this.active) return;
      this.isConnected.set(false);
      this.hasConnectionError.set(true);
      this.scheduleReconnect();
    });
  }

  private scheduleReconnect(): void {
    if (this.retryTimer || !this.active) return;
    this.retryTimer = setTimeout(() => {
      this.retryTimer = undefined;
      this.startConnection();
    }, 10000);
  }
}
