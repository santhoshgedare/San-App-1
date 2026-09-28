import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ActivityLogDto } from '../models/activity-log.models';
import type { PagedResult } from '../models/pagination.models';

export interface ActivityLogListParams {
  entityType?: string;
  entityId?: string;
  page: number;
  pageSize: number;
}

/** Common audit-trail client. Entries are looked up by (entityType, entityId), not per-entity endpoints. */
@Injectable({ providedIn: 'root' })
export class ActivityLogService {
  private readonly baseUrl = `${environment.apiUrl}/activity-logs`;

  constructor(private readonly http: HttpClient) {}

  getForEntity(entityType: string, entityId: string): Observable<ActivityLogDto[]> {
    return this.http.get<ActivityLogDto[]>(`${this.baseUrl}/${entityType}/${entityId}`);
  }

  getPaged(params: ActivityLogListParams): Observable<PagedResult<ActivityLogDto>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.entityType) {
      httpParams = httpParams.set('entityType', params.entityType);
    }
    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }
    return this.http.get<PagedResult<ActivityLogDto>>(this.baseUrl, { params: httpParams });
  }
}
