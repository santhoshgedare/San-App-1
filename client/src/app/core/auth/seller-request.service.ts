import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type SellerRequestStatus = 'New' | 'Contacted' | 'Invited' | 'Declined';

export interface SellerRequestInput {
  name: string;
  email: string;
  phone: string;
  businessName: string | null;
  city: string | null;
  craftType: string | null;
  website: string | null;
  message: string | null;
}

export interface SellerRequestDto extends SellerRequestInput {
  id: string;
  status: SellerRequestStatus;
  adminNote: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class SellerRequestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/seller-requests`;

  submit(input: SellerRequestInput): Observable<void> {
    return this.http.post<void>(this.baseUrl, input);
  }

  getAll(): Observable<SellerRequestDto[]> {
    return this.http.get<SellerRequestDto[]>(this.baseUrl);
  }

  setStatus(id: string, status: SellerRequestStatus, adminNote?: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, { status, adminNote: adminNote ?? null });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
