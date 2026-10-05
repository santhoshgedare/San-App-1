import { Component, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CartService, CartItem } from '../../core/cart/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CommonModule,
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule,
  ],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  private readonly cartService = inject(CartService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  readonly items = this.cartService.items;
  readonly totalItemsCount = this.cartService.totalItemsCount;
  readonly subtotal = this.cartService.subtotal;
  readonly hasInsufficientStock = this.cartService.hasInsufficientStock;

  getAvailableStock(line: CartItem): number {
    return this.cartService.getAvailableStock(line.item, line.variant);
  }

  getPrimaryImageUrl(cartItem: CartItem): string | null {
    const item = cartItem.item;
    if (!item.images || item.images.length === 0) return null;
    const primary = item.images.find((img) => img.isPrimary) ?? item.images[0];
    return primary.url;
  }

  updateQuantity(cartItem: CartItem, delta: number): void {
    const newQty = cartItem.quantity + delta;
    if (!this.cartService.updateQuantity(cartItem.id, newQty) && delta > 0) {
      this.snackBar.open(`Only ${this.getAvailableStock(cartItem)} available.`, 'Close', { duration: 3000, panelClass: ['snack-error'] });
    }
  }

  removeItem(cartItem: CartItem): void {
    this.cartService.removeFromCart(cartItem.id);
    this.snackBar.open(`Removed "${cartItem.item.name}" from cart`, 'Dismiss', { duration: 2500 });
  }

  clearCart(): void {
    this.cartService.clearCart();
    this.snackBar.open('Shopping cart cleared', 'Dismiss', { duration: 2500 });
  }

  checkout(): void {
    this.router.navigate(['/checkout']);
  }
}
