import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { CreateReviewRequest, ItemReviewDto, ItemReviewSummaryDto } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly http = inject(HttpClient);

  getForItem(itemId: string, take = 20): Observable<ItemReviewSummaryDto> {
    return this.http.get<ItemReviewSummaryDto>(`${environment.apiUrl}/items/${itemId}/reviews`, { params: { take } });
  }

  getForOrder(orderId: string): Observable<ItemReviewDto[]> {
    return this.http.get<ItemReviewDto[]>(`${environment.apiUrl}/reviews/order/${orderId}`);
  }

  create(request: CreateReviewRequest): Observable<ItemReviewDto> {
    return this.http.post<ItemReviewDto>(`${environment.apiUrl}/reviews`, request);
  }
}
