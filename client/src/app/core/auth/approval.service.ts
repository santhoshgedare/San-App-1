import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApprovalDto } from '../models/approval.models';
import type { PagedResult } from '../models/pagination.models';

export interface ApprovalListParams {
  entityType?: string;
  entityId?: string;
  status?: string;
  page: number;
  pageSize: number;
}

/**
 * Common approval-workflow client. Requests are raised/looked up by (entityType, entityId),
 * not per-entity endpoints, so this same service can be reused by any module/page that needs
 * an approval step (e.g. embed `<app-approval-panel>` and call `request()` from any feature).
 */
@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private readonly baseUrl = `${environment.apiUrl}/approvals`;

  constructor(private readonly http: HttpClient) {}

  request(entityType: string, entityId: string, title: string, details?: string): Observable<ApprovalDto> {
    return this.http.post<ApprovalDto>(this.baseUrl, { entityType, entityId, title, details });
  }

  approve(approvalId: string, comment?: string): Observable<ApprovalDto> {
    return this.http.post<ApprovalDto>(`${this.baseUrl}/${approvalId}/approve`, { comment });
  }

  reject(approvalId: string, comment?: string): Observable<ApprovalDto> {
    return this.http.post<ApprovalDto>(`${this.baseUrl}/${approvalId}/reject`, { comment });
  }

  /** Reassigns the approver for a not-yet-decided stage (the pencil-icon reassign action). */
  reassignStage(approvalId: string, stageIndex: number, newApproverUserId: string): Observable<ApprovalDto> {
    return this.http.post<ApprovalDto>(`${this.baseUrl}/${approvalId}/reassign`, { stageIndex, newApproverUserId });
  }

  getForEntity(entityType: string, entityId: string): Observable<ApprovalDto[]> {
    return this.http.get<ApprovalDto[]>(`${this.baseUrl}/${entityType}/${entityId}`);
  }

  getPaged(params: ApprovalListParams): Observable<PagedResult<ApprovalDto>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.entityType) {
      httpParams = httpParams.set('entityType', params.entityType);
    }
    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }
    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }
    return this.http.get<PagedResult<ApprovalDto>>(this.baseUrl, { params: httpParams });
  }
}
