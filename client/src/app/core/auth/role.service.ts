import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { RoleDto } from '../models/auth.models';
import type { PagedResult } from '../models/pagination.models';

export interface RoleListParams {
  search?: string;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly baseUrl = `${environment.apiUrl}/roles`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.baseUrl);
  }

  getPaged(params: RoleListParams): Observable<PagedResult<RoleDto>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    return this.http.get<PagedResult<RoleDto>>(`${this.baseUrl}/paged`, { params: httpParams });
  }

  create(name: string): Observable<void> {
    return this.http.post<void>(this.baseUrl, { name });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
