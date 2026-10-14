import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class OrderChatWidgetState {
  readonly isOpen = signal(false);
  readonly orderId = signal<string | null>(null);

  open(orderId?: string): void {
    if (orderId !== undefined) this.orderId.set(orderId);
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }
}
