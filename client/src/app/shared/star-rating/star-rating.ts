import { Component, input, model } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/** Read-only or interactive 1-5 star control. */
@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <span class="stars" [class.interactive]="interactive()" role="img" [attr.aria-label]="value() + ' out of 5 stars'">
      @for (n of [1, 2, 3, 4, 5]; track n) {
        @if (interactive()) {
          <button type="button" class="star" [class.on]="n <= value()" (click)="value.set(n)"
            [attr.aria-label]="n + (n === 1 ? ' star' : ' stars')">
            <mat-icon>{{ n <= value() ? 'star' : 'star_border' }}</mat-icon>
          </button>
        } @else {
          <mat-icon class="star-static" [class.on]="n <= roundedValue()">{{ n <= roundedValue() ? 'star' : 'star_border' }}</mat-icon>
        }
      }
    </span>
  `,
  styles: `
    .stars { display: inline-flex; align-items: center; color: #c9a45c; }
    .star { background: none; border: 0; padding: 0; cursor: pointer; color: #cdbfae; line-height: 1; }
    .star.on { color: #c9a45c; }
    mat-icon { width: 1.1rem; height: 1.1rem; font-size: 1.1rem; }
    .interactive mat-icon { width: 2rem; height: 2rem; font-size: 2rem; }
    .star-static:not(.on) { color: #cdbfae; }
  `,
})
export class StarRating {
  readonly value = model(0);
  readonly interactive = input(false);

  roundedValue(): number {
    return Math.round(this.value());
  }
}
