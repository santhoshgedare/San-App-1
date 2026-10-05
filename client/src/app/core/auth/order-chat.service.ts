import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderChatMessage {
  id: string;
  senderRole: 'Buyer' | 'Seller' | 'Auto';
  senderName: string;
  body: string;
  createdAt: string;
  isMine: boolean;
}

export interface OrderChat {
  myRole: 'Buyer' | 'Seller' | 'Observer';
  canPost: boolean;
  isClosed: boolean;
  sellerName: string;
  messages: OrderChatMessage[];
  faqs: { id: string; question: string }[];
}

@Injectable({ providedIn: 'root' })
export class OrderChatService {
  private readonly http = inject(HttpClient);

  get(orderId: string): Observable<OrderChat> {
    return this.http.get<OrderChat>(`${environment.apiUrl}/orders/${orderId}/messages`);
  }

  send(orderId: string, body?: string, faqId?: string): Observable<OrderChat> {
    return this.http.post<OrderChat>(`${environment.apiUrl}/orders/${orderId}/messages`, { body, faqId });
  }
}