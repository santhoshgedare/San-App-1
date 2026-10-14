import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AddressService, formatAddress } from '../../../core/auth/address.service';
import {
  CustomRequestDetail,
  CustomRequestService,
  CustomRequestThread,
} from '../../../core/auth/custom-request.service';

@Component({
  selector: 'app-custom-request-detail',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, FormsModule, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './custom-request-detail.html',
  styleUrl: './custom-request-detail.scss',
})
export class CustomRequestDetailPage implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(CustomRequestService);
  private readonly addresses = inject(AddressService);
  private readonly snackBar = inject(MatSnackBar);

  readonly request = signal<CustomRequestDetail | null>(null);
  readonly isLoading = signal(true);
  readonly busy = signal(false);
  readonly activeImage = signal<string | null>(null);
  readonly selectedThreadId = signal<string | null>(null);

  readonly selectedThread = computed<CustomRequestThread | null>(() => {
    const r = this.request();
    if (!r) return null;
    const id = this.selectedThreadId();
    return r.threads.find((t) => t.id === id) ?? r.threads[0] ?? null;
  });

  readonly isCustomer = computed(() => this.request()?.myRole === 'Customer');
  readonly isSeller = computed(() => this.request()?.myRole === 'Seller');
  readonly canAward = computed(() => {
    const r = this.request();
    return !!r && r.myRole === 'Customer' && (r.status === 'Open' || r.status === 'Quoted');
  });

  interestMessage = '';
  messageText = '';
  quoteUnitPrice: number | null = null;
  quoteShippingFee: number | null = null;
  quoteLeadDays: number | null = null;
  quoteNote = '';

  awardingThread: CustomRequestThread | null = null;
  shippingAddress = '';
  orderNotes = '';

  private requestId = '';
  private timer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.requestId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load(true);
    this.timer = setInterval(() => {
      if (!this.busy() && !this.awardingThread) this.load(false);
    }, 15000);
  }

  ngOnDestroy(): void {
    clearInterval(this.timer);
  }

  private load(initial: boolean): void {
    this.service.get(this.requestId).subscribe({
      next: (r) => this.apply(r, initial),
      error: () => this.isLoading.set(false),
    });
  }

  private apply(r: CustomRequestDetail, initial = false): void {
    this.request.set(r);
    this.isLoading.set(false);
    if (initial || !this.activeImage()) this.activeImage.set(r.images[0] ?? null);
    const mine = r.threads.find((t) => t.isMine);
    if (mine && initial) {
      this.quoteUnitPrice = mine.quoteUnitPrice ?? null;
      this.quoteShippingFee = mine.quoteShippingFee ?? null;
      this.quoteLeadDays = mine.quoteLeadTimeDays ?? null;
      this.quoteNote = mine.quoteNote ?? '';
    }
  }

  private run(action: () => ReturnType<CustomRequestService['get']>, done?: string): void {
    this.busy.set(true);
    action().subscribe({
      next: (r) => {
        this.busy.set(false);
        this.apply(r);
        if (done) this.snackBar.open(done, 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.busy.set(false);
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Something went wrong.', 'Close');
      },
    });
  }

  expressInterest(): void {
    this.run(() => this.service.expressInterest(this.requestId, this.interestMessage.trim() || null), 'You joined this request.');
    this.interestMessage = '';
  }

  sendMessage(): void {
    const thread = this.selectedThread();
    const body = this.messageText.trim();
    if (!thread || !body) return;
    this.messageText = '';
    this.run(() => this.service.sendMessage(this.requestId, thread.id, body));
  }

  submitQuote(): void {
    const thread = this.selectedThread();
    if (!thread || !this.quoteUnitPrice || this.quoteUnitPrice <= 0) {
      this.snackBar.open('Enter a unit price.', 'Close');
      return;
    }
    this.run(
      () =>
        this.service.submitQuote(this.requestId, thread.id, {
          unitPrice: this.quoteUnitPrice!,
          shippingFee: this.quoteShippingFee,
          leadTimeDays: this.quoteLeadDays,
          note: this.quoteNote.trim() || null,
        }),
      'Quote sent.',
    );
  }

  withdraw(): void {
    const thread = this.selectedThread();
    if (!thread || !confirm('Withdraw from this request?')) return;
    this.run(() => this.service.withdraw(this.requestId, thread.id), 'Withdrawn.');
  }

  cancelRequest(): void {
    if (!confirm('Cancel this request? Sellers will no longer be able to quote.')) return;
    this.run(() => this.service.cancel(this.requestId), 'Request cancelled.');
  }

  startAward(thread: CustomRequestThread): void {
    this.awardingThread = thread;
    this.addresses.getMine().subscribe({
      next: (list) => {
        const pick = list.find((a) => a.isDefault) ?? list[0];
        if (pick && !this.shippingAddress) this.shippingAddress = formatAddress(pick);
      },
    });
  }

  confirmAward(): void {
    const thread = this.awardingThread;
    if (!thread) return;
    if (!this.shippingAddress.trim()) {
      this.snackBar.open('Please enter a shipping address.', 'Close');
      return;
    }
    this.busy.set(true);
    this.service.award(this.requestId, thread.id, this.shippingAddress.trim(), this.orderNotes.trim() || null).subscribe({
      next: ({ orderId }) => {
        this.busy.set(false);
        this.snackBar.open('Quote accepted. Your order has been created.', 'Close', { duration: 4000 });
        this.router.navigate(['/orders', orderId]);
      },
      error: (err) => {
        this.busy.set(false);
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not accept the quote.', 'Close');
      },
    });
  }
}
