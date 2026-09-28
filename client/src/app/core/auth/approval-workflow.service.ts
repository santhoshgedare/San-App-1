import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApprovalWorkflowDto, SaveApprovalWorkflowRequest } from '../models/approval.models';

/**
 * Admin-only client for configuring the multi-stage approval workflow (stages + eligible
 * roles per stage) used for a given entity type. Any master's create/edit page can call
 * `getForEntityType` to know which stages will apply before/while raising an approval.
 */
@Injectable({ providedIn: 'root' })
export class ApprovalWorkflowService {
  private readonly baseUrl = `${environment.apiUrl}/approval-workflows`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<ApprovalWorkflowDto[]> {
    return this.http.get<ApprovalWorkflowDto[]>(this.baseUrl);
  }

  getForEntityType(entityType: string): Observable<ApprovalWorkflowDto | null> {
    return this.http.get<ApprovalWorkflowDto | null>(`${this.baseUrl}/${entityType}`);
  }

  save(request: SaveApprovalWorkflowRequest): Observable<ApprovalWorkflowDto> {
    return this.http.post<ApprovalWorkflowDto>(this.baseUrl, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
