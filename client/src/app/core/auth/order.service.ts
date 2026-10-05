import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  CreateOrderRequest,
  OrderDto,
  RecordOrderRefundRequest,
  SubmitOrderPaymentDetailsRequest,
  UpdateOrderPaymentStatusRequest,
  UpdateOrderStatusRequest,
} from '../models/auth.models';
import type { PagedResult } from '../models/pagination.models';

export interface OrderQueryFilter {
  search?: string;
  status?: string;
  paymentStatus?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  getPaged(filter: OrderQueryFilter = {}): Observable<PagedResult<OrderDto>> {
    let params = new HttpParams();
    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.paymentStatus) params = params.set('paymentStatus', filter.paymentStatus);
    if (filter.page) params = params.set('page', filter.page);
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize);

    return this.http.get<PagedResult<OrderDto>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateOrderRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(this.baseUrl, request);
  }

  updateStatus(id: string, request: UpdateOrderStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, request);
  }

  updatePaymentStatus(id: string, request: UpdateOrderPaymentStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/payment-status`, request);
  }

  submitPaymentDetails(id: string, request: SubmitOrderPaymentDetailsRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/payment-details`, request);
  }

  recordRefund(id: string, request: RecordOrderRefundRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/refund`, request);
  }
}
