import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { PagedResult } from '../models/pagination.models';

export type CustomRequestStatus = 'Open' | 'Quoted' | 'Awarded' | 'Cancelled';

export interface CustomRequestSummary {
  id: string;
  requestNumber: string;
  title: string;
  status: CustomRequestStatus;
  quantity: number;
  budget?: number | null;
  neededBy?: string | null;
  customerName: string;
  thumbnailUrl?: string | null;
  interestedCount: number;
  quoteCount: number;
  myThreadId?: string | null;
  orderId?: string | null;
  createdAt: string;
}

export interface CustomRequestMessage {
  id: string;
  senderRole: 'Buyer' | 'Seller' | 'System';
  senderName: string;
  body: string;
  createdAt: string;
  isMine: boolean;
}

export interface CustomRequestThread {
  id: string;
  sellerId: string;
  sellerName: string;
  status: 'Active' | 'Withdrawn' | 'Awarded' | 'Closed';
  quoteUnitPrice?: number | null;
  quoteShippingFee?: number | null;
  quoteLeadTimeDays?: number | null;
  quoteNote?: string | null;
  quotedAt?: string | null;
  quoteRevision: number;
  quoteTotal?: number | null;
  isMine: boolean;
  createdAt: string;
  messages: CustomRequestMessage[];
}

export interface CustomRequestDetail {
  id: string;
  requestNumber: string;
  title: string;
  description: string;
  status: CustomRequestStatus;
  quantity: number;
  budget?: number | null;
  neededBy?: string | null;
  customerName: string;
  createdAt: string;
  orderId?: string | null;
  awardedThreadId?: string | null;
  myRole: 'Customer' | 'Seller' | 'Observer';
  canExpressInterest: boolean;
  images: string[];
  threads: CustomRequestThread[];
}

export interface CreateCustomRequest {
  title: string;
  description: string;
  quantity: number;
  budget: number | null;
  neededBy: string | null;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  images: { url: string; fileName?: string | null }[];
}

export interface CustomQuoteRequest {
  unitPrice: number;
  shippingFee: number | null;
  leadTimeDays: number | null;
  note: string | null;
}

export type CustomRequestScope = 'mine' | 'open' | 'joined' | 'all';

@Injectable({ providedIn: 'root' })
export class CustomRequestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/custom-requests`;

  getPaged(scope: CustomRequestScope, status?: string, page = 1, pageSize = 12): Observable<PagedResult<CustomRequestSummary>> {
    let params = new HttpParams().set('scope', scope).set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<CustomRequestSummary>>(this.baseUrl, { params });
  }

  get(id: string): Observable<CustomRequestDetail> {
    return this.http.get<CustomRequestDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateCustomRequest): Observable<CustomRequestDetail> {
    return this.http.post<CustomRequestDetail>(this.baseUrl, request);
  }

  cancel(id: string): Observable<CustomRequestDetail> {
    return this.http.post<CustomRequestDetail>(`${this.baseUrl}/${id}/cancel`, {});
  }

  expressInterest(id: string, message: string | null): Observable<CustomRequestDetail> {
    return this.http.post<CustomRequestDetail>(`${this.baseUrl}/${id}/interest`, { message });
  }

  sendMessage(id: string, threadId: string, body: string): Observable<CustomRequestDetail> {
    return this.http.post<CustomRequestDetail>(`${this.baseUrl}/${id}/threads/${threadId}/messages`, { body });
  }

  submitQuote(id: string, threadId: string, quote: CustomQuoteRequest): Observable<CustomRequestDetail> {
    return this.http.put<CustomRequestDetail>(`${this.baseUrl}/${id}/threads/${threadId}/quote`, quote);
  }

  withdraw(id: string, threadId: string): Observable<CustomRequestDetail> {
    return this.http.post<CustomRequestDetail>(`${this.baseUrl}/${id}/threads/${threadId}/withdraw`, {});
  }

  award(id: string, threadId: string, shippingAddress: string, orderNotes: string | null): Observable<{ orderId: string }> {
    return this.http.post<{ orderId: string }>(`${this.baseUrl}/${id}/threads/${threadId}/award`, { shippingAddress, orderNotes });
  }

  addProgress(orderId: string, stage: string, note: string | null, imageUrl: string | null): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/orders/${orderId}/progress`, { stage, note, imageUrl });
  }
}
