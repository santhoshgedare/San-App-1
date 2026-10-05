import { Component, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-password-field',
  standalone: true,
  imports: [MatIconModule],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => PasswordField), multi: true }],
  template: `
    <div class="password-field">
      <input
        [id]="inputId()"
        [type]="visible() ? 'text' : 'password'"
        class="form-control"
        [value]="value()"
        [disabled]="disabled()"
        [attr.autocomplete]="autocomplete()"
        [attr.placeholder]="placeholder() || null"
        (input)="onInput($event)"
        (blur)="onTouched()"
      />
      <button
        type="button"
        class="toggle"
        [attr.aria-label]="visible() ? 'Hide password' : 'Show password'"
        [attr.aria-pressed]="visible()"
        [disabled]="disabled()"
        (click)="visible.set(!visible())"
      >
        <mat-icon>{{ visible() ? 'visibility_off' : 'visibility' }}</mat-icon>
      </button>
    </div>
  `,
  styles: `
    :host { display: block; }
    .password-field { position: relative; }
    .form-control { padding-right: 2.75rem; }
    .toggle {
      position: absolute; top: 50%; right: 0.4rem; transform: translateY(-50%);
      display: inline-flex; align-items: center; justify-content: center;
      width: 2rem; height: 2rem; padding: 0; border: 0; border-radius: 50%;
      background: transparent; color: #8d7775; cursor: pointer;
    }
    .toggle:hover { color: #76283a; background: #f6e6e9; }
    .toggle:focus-visible { outline: 2px solid #76283a; outline-offset: 1px; }
    mat-icon { width: 1.25rem; height: 1.25rem; font-size: 1.25rem; }
  `,
})
export class PasswordField implements ControlValueAccessor {
  readonly inputId = input<string | null>(null);
  readonly placeholder = input('');
  readonly autocomplete = input('current-password');

  protected readonly value = signal('');
  protected readonly disabled = signal(false);
  protected readonly visible = signal(false);

  private onChange: (value: string) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }
  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected onInput(event: Event): void {
    const v = (event.target as HTMLInputElement).value;
    this.value.set(v);
    this.onChange(v);
  }
}
