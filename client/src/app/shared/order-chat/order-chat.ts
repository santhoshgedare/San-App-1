import { DatePipe } from '@angular/common';
import { AfterViewChecked, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { OrderChat, OrderChatService } from '../../core/auth/order-chat.service';

const POLL_MS = 10000;

const SELLER_REPLIES = [
  'Thank you for your order! We have received it.',
  'Your order is confirmed and being prepared.',
  'Your payment is verified. Thank you!',
  'Your order has been shipped.',
  'Could you please share more details?',
];

/** Buyer ⇄ seller chat on an order, with quick FAQ questions for buyers and quick replies for sellers. */
@Component({
  selector: 'app-order-chat',
  standalone: true,
  imports: [DatePipe, FormsModule, MatIconModule],
  templateUrl: './order-chat.html',
  styleUrl: './order-chat.scss',
})
export class OrderChatPanel implements OnInit, OnDestroy, AfterViewChecked {
  private readonly service = inject(OrderChatService);
  private readonly snackBar = inject(MatSnackBar);

  readonly orderId = input.required<string>();
  readonly chat = signal<OrderChat | null>(null);
  readonly isSending = signal(false);
  readonly replies = SELLER_REPLIES;
  text = '';

  @ViewChild('thread') private thread?: ElementRef<HTMLElement>;
  private timer?: ReturnType<typeof setInterval>;
  private lastCount = 0;

  ngOnInit(): void {
    this.load();
    this.timer = setInterval(() => this.load(), POLL_MS);
  }

  ngOnDestroy(): void {
    clearInterval(this.timer);
  }

  ngAfterViewChecked(): void {
    const count = this.chat()?.messages.length ?? 0;
    if (count !== this.lastCount && this.thread) {
      this.lastCount = count;
      this.thread.nativeElement.scrollTop = this.thread.nativeElement.scrollHeight;
    }
  }

  private load(): void {
    this.service.get(this.orderId()).subscribe({ next: (c) => this.chat.set(c), error: () => undefined });
  }

  send(): void {
    const body = this.text.trim();
    if (!body || this.isSending()) return;
    this.submit(body, undefined);
  }

  ask(faqId: string): void {
    if (!this.isSending()) this.submit(undefined, faqId);
  }

  useReply(reply: string): void {
    this.text = reply;
  }

  onEnter(event: Event): void {
    const e = event as KeyboardEvent;
    if (!e.shiftKey) {
      e.preventDefault();
      this.send();
    }
  }

  private submit(body?: string, faqId?: string): void {
    this.isSending.set(true);
    this.service.send(this.orderId(), body, faqId).subscribe({
      next: (c) => {
        this.chat.set(c);
        this.text = '';
        this.isSending.set(false);
      },
      error: (err) => {
        this.isSending.set(false);
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not send message.', 'Close', { panelClass: ['snack-error'] });
      },
    });
  }
}