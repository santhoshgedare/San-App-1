import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { catchError, forkJoin, of } from 'rxjs';
import { ItemService } from '../../core/auth/item.service';
import { CartService } from '../../core/cart/cart.service';
import { FavouritesService } from '../../core/cart/favourites.service';
import type { ItemDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-favourites',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, MatIconModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './favourites.html',
  styleUrl: './favourites.scss',
})
export class Favourites implements OnInit {
  private readonly itemService = inject(ItemService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  protected readonly favourites = inject(FavouritesService);

  protected readonly items = signal<ItemDto[]>([]);
  protected readonly isLoading = signal(true);

  ngOnInit(): void {
    const ids = this.favourites.ids();
    if (ids.length === 0) {
      this.isLoading.set(false);
      return;
    }

    forkJoin(ids.map((id) => this.itemService.getById(id).pipe(catchError(() => of(null))))).subscribe((result) => {
      const loaded = result.filter((item): item is ItemDto => item !== null);
      // Items that no longer exist are dropped from the saved list.
      ids.filter((id) => !loaded.some((item) => item.id === id)).forEach((id) => this.favourites.remove(id));
      this.items.set(loaded);
      this.isLoading.set(false);
    });
  }

  protected image(item: ItemDto): string | null {
    return (item.images.find((img) => img.isPrimary) ?? item.images[0])?.url ?? null;
  }

  protected startingPrice(item: ItemDto): number {
    return item.variants.length > 0 ? Math.min(...item.variants.map((v) => v.price)) : item.price;
  }

  protected remove(item: ItemDto): void {
    this.favourites.remove(item.id);
    this.items.update((list) => list.filter((i) => i.id !== item.id));
  }

  protected open(item: ItemDto): void {
    this.router.navigate(['/catalog', item.id]);
  }

  protected addToCart(item: ItemDto): void {
    if (item.variants.length > 0) {
      this.open(item);
      return;
    }
    if (!this.cartService.addToCart(item, null, 1)) {
      this.snackBar.open('No additional stock is available for this item.', 'Close', { duration: 3000 });
      return;
    }
    this.snackBar.open(`Added "${item.name}" to cart!`, 'View Cart', { duration: 3000 })
      .onAction()
      .subscribe(() => this.router.navigate(['/cart']));
  }
}
