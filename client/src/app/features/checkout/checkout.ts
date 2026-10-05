import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CartItem, CartService } from '../../core/cart/cart.service';
import { OrderService } from '../../core/auth/order.service';
import { AuthService } from '../../core/auth/auth.service';
import { PaymentMethod } from '../../core/models/auth.models';
import type { CreateOrderRequest, OrderItemRequest } from '../../core/models/auth.models';

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
    }

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

    const orderItems: OrderItemRequest[] = this.items().map((line) => ({
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
    }));

    const request: CreateOrderRequest = {
      customerName: this.customerName,
      customerEmail: this.customerEmail,
      customerPhone: this.customerPhone,
      shippingAddress: this.shippingAddress,
      billingAddress: this.billingAddress || null,
      orderNotes: this.orderNotes || null,
      paymentMethod: PaymentMethod.UpiQr,
      items: orderItems,
    };

    this.orderService.create(request).subscribe({
      next: (createdOrder) => {
        this.cartService.clearCart();
        this.snackBar
          .open(
            `Order ${createdOrder.orderNumber} has been placed. Please confirm payment details later to complete verification.`,
            'View Order',
            { duration: 6000 }
          )
          .onAction()
          .subscribe(() => {
            this.router.navigate(['/orders', createdOrder.id]);
          });
        this.router.navigate(['/orders', createdOrder.id]);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const errorList = err?.error?.errors;
        this.errorMessage.set(Array.isArray(errorList) ? errorList.join(' ') : 'Failed to place order. Please try again.');
      },
    });
  }
}

