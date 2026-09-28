import { Injectable, computed, signal } from '@angular/core';
import type { ItemDto, ItemVariantDto } from '../models/auth.models';

export interface CartItem {
  id: string; // unique cart line id (e.g. itemId + variantId)
  item: ItemDto;
  variant?: ItemVariantDto | null;
  unitPrice: number;
  quantity: number;
  selectedAttributesText?: string;
}

const CART_STORAGE_KEY = 'ih_shopping_cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly _items = signal<CartItem[]>(this.loadFromStorage());

  readonly items = this._items.asReadonly();

  readonly totalItemsCount = computed(() =>
    this._items().reduce((total, cartItem) => total + cartItem.quantity, 0)
  );

  readonly subtotal = computed(() =>
    this._items().reduce((sum, cartItem) => sum + cartItem.unitPrice * cartItem.quantity, 0)
  );

  private loadFromStorage(): CartItem[] {
    try {
      const data = localStorage.getItem(CART_STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private saveToStorage(): void {
    try {
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(this._items()));
    } catch {}
  }

  addToCart(item: ItemDto, variant: ItemVariantDto | null, quantity = 1): void {
    if (quantity <= 0) return;

    const variantId = variant?.id ?? variant?.sku ?? 'base';
    const cartLineId = `${item.id}_${variantId}`;
    const unitPrice = variant ? variant.price : item.price;

    let selectedAttributesText = '';
    if (variant && variant.attributesJson) {
      try {
        const parsed = JSON.parse(variant.attributesJson);
        selectedAttributesText = Object.entries(parsed)
          .map(([k, v]) => `${k}: ${v}`)
          .join(', ');
      } catch {
        selectedAttributesText = variant.name || '';
      }
    }

    this._items.update((current) => {
      const existingIndex = current.findIndex((line) => line.id === cartLineId);
      if (existingIndex >= 0) {
        const updated = [...current];
        updated[existingIndex] = {
          ...updated[existingIndex],
          quantity: updated[existingIndex].quantity + quantity,
        };
        return updated;
      } else {
        return [
          ...current,
          {
            id: cartLineId,
            item,
            variant,
            unitPrice,
            quantity,
            selectedAttributesText,
          },
        ];
      }
    });

    this.saveToStorage();
  }

  updateQuantity(cartLineId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(cartLineId);
      return;
    }

    this._items.update((current) =>
      current.map((line) => (line.id === cartLineId ? { ...line, quantity } : line))
    );
    this.saveToStorage();
  }

  removeFromCart(cartLineId: string): void {
    this._items.update((current) => current.filter((line) => line.id !== cartLineId));
    this.saveToStorage();
  }

  clearCart(): void {
    this._items.set([]);
    this.saveToStorage();
  }
}
