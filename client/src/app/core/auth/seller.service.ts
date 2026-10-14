import type { AuthResult } from '../models/auth.models';
import type { AddressInput } from './address.service';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SellerDto {
  id: string;
  userId: string | null;
  userEmail: string | null;
  companyName: string;
  tagline: string | null;
  description: string | null;
  logoUrl: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  website: string | null;
  addressLine1: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  itemCount: number;
  upiId: string | null;
  payeeName: string | null;
  qrCodeImageUrl: string | null;
  bankDetails: string | null;
  inviteEmail: string | null;
  inviteStatus: 'None' | 'Pending' | 'Expired' | 'Accepted';
  status: 'Pending' | 'Approved' | 'Rejected';
  isActive: boolean;
  statusNote: string | null;
  canSell: boolean;
}

export type SellerInput = Omit<
  SellerDto,
  'id' | 'userEmail' | 'itemCount' | 'inviteEmail' | 'inviteStatus' | 'status' | 'isActive' | 'statusNote' | 'canSell'
>;

export interface SellerInviteResult {
  inviteUrl: string;
  expiresAt: string;
  to: string;
  subject: string;
  body: string;
}

export interface SellerInviteInfo {
  email: string;
  companyName: string;
  tagline: string | null;
  logoUrl: string | null;
}

export interface SellerEmployee {
  id: string;
  userId: string | null;
  name: string;
  email: string;
  phoneNumber: string | null;
  isActive: boolean;
  isPending: boolean;
  createdAt: string;
}

export interface SellerRegistration {
  firstName: string;
  lastName: string;
  password: string;
  phoneNumber: string;
  address: AddressInput;
  company: SellerInput;
}

@Injectable({ providedIn: 'root' })
export class SellerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/sellers`;

  getAll(): Observable<SellerDto[]> {
    return this.http.get<SellerDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<SellerDto> {
    return this.http.get<SellerDto>(`${this.baseUrl}/${id}`);
  }



  getPublic(id: string): Observable<SellerDto> {
    return this.http.get<SellerDto>(`${this.baseUrl}/${id}/public`);
  }

  getMine(): Observable<SellerDto> {
    return this.http.get<SellerDto>(`${this.baseUrl}/mine`);
  }

  approve(id: string, note?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, { note: note ?? null });
  }

  reject(id: string, note?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, { note: note ?? null });
  }

  setActive(id: string, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/active`, { isActive });
  }

  updateMine(input: SellerInput): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/mine`, input);
  }

  create(input: SellerInput): Observable<SellerDto> {
    return this.http.post<SellerDto>(this.baseUrl, input);
  }

  update(id: string, input: SellerInput): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, input);
  }

  inviteNew(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/invite`, { email });
  }

  invite(id: string, email: string): Observable<SellerInviteResult> {
    return this.http.post<SellerInviteResult>(`${this.baseUrl}/${id}/invite`, { email });
  }

  sendInviteEmail(id: string, form: FormData): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/invite/send`, form);
  }

  getInvite(token: string): Observable<SellerInviteInfo> {
    return this.http.get<SellerInviteInfo>(`${this.baseUrl}/invite/${encodeURIComponent(token)}`);
  }

  acceptInvite(token: string, registration: SellerRegistration): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/invite/${encodeURIComponent(token)}/accept`, registration);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getEmployees(): Observable<SellerEmployee[]> {
    return this.http.get<SellerEmployee[]>(`${this.baseUrl}/mine/employees`);
  }

  inviteEmployee(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/mine/employees/invite`, { email });
  }

  setEmployeeActive(id: string, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/mine/employees/${id}/active`, { isActive });
  }

  removeEmployee(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/mine/employees/${id}`);
  }

  getEmployeeInvite(token: string): Observable<{ email: string; companyName: string }> {
    return this.http.get<{ email: string; companyName: string }>(`${this.baseUrl}/employee-invite/${encodeURIComponent(token)}`);
  }

  acceptEmployeeInvite(
    token: string,
    body: { firstName: string; lastName: string; password: string; phoneNumber: string | null },
  ): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/employee-invite/${encodeURIComponent(token)}/accept`, body);
  }
}