import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { AddressInput } from '../../core/auth/address.service';

/** Plain template-driven address editor; mutates the provided model. */
@Component({
  selector: 'app-address-fields',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="row g-2">
      <div class="col-12">
        <label class="form-label">Address line 1 <span class="text-danger">*</span></label>
        <input class="form-control" name="line1" [(ngModel)]="model.line1" [ngModelOptions]="{ standalone: true }" autocomplete="address-line1" placeholder="House / street" />
      </div>
      <div class="col-12">
        <label class="form-label">Address line 2</label>
        <input class="form-control" name="line2" [(ngModel)]="model.line2" [ngModelOptions]="{ standalone: true }" autocomplete="address-line2" placeholder="Area, landmark (optional)" />
      </div>
      <div class="col-6">
        <label class="form-label">City <span class="text-danger">*</span></label>
        <input class="form-control" name="city" [(ngModel)]="model.city" [ngModelOptions]="{ standalone: true }" autocomplete="address-level2" />
      </div>
      <div class="col-6">
        <label class="form-label">State <span class="text-danger">*</span></label>
        <input class="form-control" name="state" [(ngModel)]="model.state" [ngModelOptions]="{ standalone: true }" autocomplete="address-level1" />
      </div>
      <div class="col-6">
        <label class="form-label">Postal code <span class="text-danger">*</span></label>
        <input class="form-control" name="postalCode" [(ngModel)]="model.postalCode" [ngModelOptions]="{ standalone: true }" autocomplete="postal-code" />
      </div>
      <div class="col-6">
        <label class="form-label">Country <span class="text-danger">*</span></label>
        <input class="form-control" name="country" [(ngModel)]="model.country" [ngModelOptions]="{ standalone: true }" autocomplete="country-name" />
      </div>
    </div>
  `,
})
export class AddressFields {
  @Input({ required: true }) model!: AddressInput;
}