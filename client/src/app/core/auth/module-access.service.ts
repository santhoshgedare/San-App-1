import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ModuleDto, RoleAccessDto, SectionDto, PageDto } from '../models/module-access.models';

export interface CreateModuleRequest {
  name: string;
  key: string;
  sortOrder: number;
}

export interface CreatePageRequest {
  moduleId: string;
  name: string;
  url: string;
  sortOrder: number;
}

export interface CreateSectionRequest {
  pageId: string;
  name: string;
  key: string;
  sortOrder: number;
}

export interface SetRoleAccessRequest {
  sectionKeys: string[];
}

/**
 * Client for the Module &gt; Page &gt; Section master hierarchy and per-role section access —
 * the data that drives the settings navigator and the `canRender` directive.
 */
@Injectable({ providedIn: 'root' })
export class ModuleAccessService {
  private readonly baseUrl = `${environment.apiUrl}/module-access`;

  constructor(private readonly http: HttpClient) {}

  getTree(): Observable<ModuleDto[]> {
    return this.http.get<ModuleDto[]>(`${this.baseUrl}/tree`);
  }

  createModule(request: CreateModuleRequest): Observable<ModuleDto> {
    return this.http.post<ModuleDto>(`${this.baseUrl}/modules`, request);
  }

  deleteModule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/modules/${id}`);
  }

  createPage(request: CreatePageRequest): Observable<PageDto> {
    return this.http.post<PageDto>(`${this.baseUrl}/pages`, request);
  }

  deletePage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/pages/${id}`);
  }

  createSection(request: CreateSectionRequest): Observable<SectionDto> {
    return this.http.post<SectionDto>(`${this.baseUrl}/sections`, request);
  }

  deleteSection(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/sections/${id}`);
  }

  getAllRoleAccess(): Observable<RoleAccessDto[]> {
    return this.http.get<RoleAccessDto[]>(`${this.baseUrl}/role-access`);
  }

  getRoleAccess(roleId: string): Observable<RoleAccessDto> {
    return this.http.get<RoleAccessDto>(`${this.baseUrl}/role-access/${roleId}`);
  }

  setRoleAccess(roleId: string, request: SetRoleAccessRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/role-access/${roleId}`, request);
  }

  /** Section keys the current user can access, for the `canRender` directive. */
  getMySections(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiUrl}/me/sections`);
  }
}
