import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { CreateItemRequest, ItemDto, UpdateItemRequest } from '../models/auth.models';
import type { PagedResult } from '../models/pagination.models';

export interface ItemListParams {
  search?: string;
  categoryId?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly baseUrl = `${environment.apiUrl}/items`;

  constructor(private readonly http: HttpClient) { }

  getAll(): Observable<ItemDto[]> {
    return this.http.get<ItemDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<ItemDto> {
    return this.http.get<ItemDto>(`${this.baseUrl}/${id}`);
  }

  getSimilar(id: string, take = 5): Observable<ItemDto[]> {
    return this.http.get<ItemDto[]>(`${this.baseUrl}/${id}/similar`, { params: { take } });
  }

  getManagementById(id: string): Observable<ItemDto> {
    return this.http.get<ItemDto>(`${this.baseUrl}/management/${id}`);
  }

  getPaged(params: ItemListParams): Observable<PagedResult<ItemDto>> {
    return this.getPagedFrom(`${this.baseUrl}/paged`, params);
  }

  getManagementPaged(params: ItemListParams): Observable<PagedResult<ItemDto>> {
    return this.getPagedFrom(`${this.baseUrl}/management/paged`, params);
  }

  private getPagedFrom(url: string, params: ItemListParams): Observable<PagedResult<ItemDto>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }
    return this.http.get<PagedResult<ItemDto>>(url, { params: httpParams });
  }

  create(request: CreateItemRequest): Observable<ItemDto> {
    return this.http.post<ItemDto>(this.baseUrl, request);
  }

  update(id: string, request: UpdateItemRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
