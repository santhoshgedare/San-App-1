import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { UserDto } from '../models/auth.models';
import type { PagedResult } from '../models/pagination.models';

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface AssignRolesRequest {
  roles: string[];
}

export interface UserListParams {
  search?: string;
  role?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/users`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.baseUrl);
  }

  getPaged(params: UserListParams): Observable<PagedResult<UserDto>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.role) {
      httpParams = httpParams.set('role', params.role);
    }
    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }
    return this.http.get<PagedResult<UserDto>>(`${this.baseUrl}/paged`, { params: httpParams });
  }

  getById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(this.baseUrl, request);
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignRoles(id: string, request: AssignRolesRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/roles`, request);
  }

  getMe(): Observable<UserDto> {
    return this.http.get<UserDto>(`${environment.apiUrl}/me`);
  }
}
