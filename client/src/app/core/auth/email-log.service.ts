import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { PagedResult } from '../models/pagination.models';

export interface EmailLogDto {
  id: string;
  toAddresses: string;
  ccAddresses: string | null;
  subject: string;
  htmlBody: string | null;
  textBody: string | null;
  attachments: string | null;
  status: 'Sent' | 'Failed';
  error: string | null;
  category: string;
  relatedEntityType: string | null;
  relatedEntityId: string | null;
  createdByEmail: string | null;
  createdAt: string;
  sentAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class EmailLogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/email-logs`;

  getPaged(page: number, pageSize: number, status: string, search: string): Observable<PagedResult<EmailLogDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    if (search.trim()) params = params.set('search', search.trim());
    return this.http.get<PagedResult<EmailLogDto>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<EmailLogDto> {
    return this.http.get<EmailLogDto>(`${this.baseUrl}/${id}`);
  }
}
