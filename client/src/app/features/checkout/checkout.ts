import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CartItem, CartService } from '../../core/cart/cart.service';
import { AddressService, formatAddress } from '../../core/auth/address.service';
import { OrderService } from '../../core/auth/order.service';
import { AuthService } from '../../core/auth/auth.service';
import { PaymentMethod } from '../../core/models/auth.models';
import type { CreateOrderRequest, OrderDto, OrderItemRequest } from '../../core/models/auth.models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly addressService = inject(AddressService);

  readonly items = this.cartService.items;
  readonly subtotal = this.cartService.subtotal;
  readonly totalItemsCount = this.cartService.totalItemsCount;
  readonly hasInsufficientStock = this.cartService.hasInsufficientStock;

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  getAvailableStock(line: CartItem): number {
    return this.cartService.getAvailableStock(line.item, line.variant);
  }

  // Form Fields
  customerName = '';
  customerEmail = '';
  customerPhone = '';
  shippingAddress = '';
  billingAddress = '';
  orderNotes = '';

  readonly PaymentMethod = PaymentMethod;

  ngOnInit(): void {
    if (this.items().length === 0) {
      this.router.navigate(['/cart']);
      return;
    }

    const user = this.authService.currentUser();
    if (user) {
      this.customerName = `${user.firstName} ${user.lastName}`.trim();
      this.customerEmail = user.email;
      this.customerPhone = user.phoneNumber ?? '';
    }

    this.addressService.getMine().subscribe((list) => {
      const preferred = list.find((a) => a.isDefault) ?? list[0];
      if (preferred && !this.shippingAddress) {
        this.shippingAddress = formatAddress(preferred);
        this.customerPhone = this.customerPhone || preferred.phone;
      }
    });

  }

  placeOrder(): void {
    if (this.hasInsufficientStock()) {
      this.errorMessage.set('One or more cart quantities exceed available stock. Return to your cart and adjust them before ordering.');
      return;
    }

    if (!this.customerName.trim() || !this.customerEmail.trim() || !this.customerPhone.trim() || !this.shippingAddress.trim()) {
      this.errorMessage.set('Please fill in all required customer and delivery details.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    // Each seller fulfils its own order, so the cart is split per seller.
    const groups = new Map<string, CartItem[]>();
    for (const line of this.items()) {
      const key = line.item.sellerId ?? '';
      groups.set(key, [...(groups.get(key) ?? []), line]);
    }

    const created: OrderDto[] = [];
    const queue = Array.from(groups.values());

    const placeNext = (): void => {
      const lines = queue.shift();
      if (!lines) {
        this.finish(created);
        return;
      }

      const request: CreateOrderRequest = {
        customerName: this.customerName,
        customerEmail: this.customerEmail,
        customerPhone: this.customerPhone,
        shippingAddress: this.shippingAddress,
        billingAddress: this.billingAddress || null,
        orderNotes: this.orderNotes || null,
        paymentMethod: PaymentMethod.UpiQr,
        items: lines.map((line) => this.toOrderItem(line)),
      };

      this.orderService.create(request).subscribe({
        next: (order) => {
          created.push(order);
          lines.forEach((line) => this.cartService.removeFromCart(line.id));
          placeNext();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          const errorList = err?.error?.errors;
          const message = Array.isArray(errorList) ? errorList.join(' ') : 'Failed to place order. Please try again.';
          this.errorMessage.set(created.length ? `${created.length} order(s) were placed. ${message}` : message);
          if (created.length) {
            this.snackBar.open('Some orders were placed. Check My Orders.', 'Close', { panelClass: ['snack-error'] });
          }
        },
      });
    };

    placeNext();
  }

  private toOrderItem(line: CartItem): OrderItemRequest {
    return {
      itemId: line.item.id,
      itemVariantId: line.variant?.id ?? null,
      itemCode: line.item.code,
      itemName: line.item.name,
      variantSku: line.variant?.sku ?? null,
      variantName: line.variant?.name ?? null,
      attributesJson: line.variant?.attributesJson ?? null,
      imageUrl: line.item.images && line.item.images.length > 0 ? line.item.images[0].url : null,
      unitPrice: line.unitPrice,
      quantity: line.quantity,
    };
  }

  private finish(created: OrderDto[]): void {
    this.cartService.clearCart();
    const message =
      created.length === 1
        ? `Order ${created[0].orderNumber} has been placed. Please confirm payment details later to complete verification.`
        : `${created.length} orders were placed, one for each seller.`;
    this.snackBar.open(message, 'Close', { duration: 6000 });
    this.router.navigate(created.length === 1 ? ['/orders', created[0].id] : ['/orders']);
  }
}