import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { PaymentSettingsDto, UpdatePaymentSettingsRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class PaymentSettingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/paymentsettings`;

  get(): Observable<PaymentSettingsDto> {
    return this.http.get<PaymentSettingsDto>(this.baseUrl);
  }

  update(request: UpdatePaymentSettingsRequest): Observable<PaymentSettingsDto> {
    return this.http.put<PaymentSettingsDto>(this.baseUrl, request);
  }
}
