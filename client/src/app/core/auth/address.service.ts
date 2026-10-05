import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AddressInput {
  label: string;
  fullName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  formattedAddress: string | null;
  latitude: number | null;
  longitude: number | null;
  isDefault: boolean;
}

export interface AddressDto extends AddressInput {
  id: string;
}

export function emptyAddress(fullName = '', phone = ''): AddressInput {
  return {
    label: 'Home',
    fullName,
    phone,
    line1: '',
    line2: null,
    city: '',
    state: '',
    postalCode: '',
    country: 'India',
    formattedAddress: null,
    latitude: null,
    longitude: null,
    isDefault: true,
  };
}

/** Returns a human readable problem with the address, or null when complete. */
export function validateAddress(a: AddressInput): string | null {
  if (!a.line1?.trim()) return 'Address line is required.';
  if (!a.city?.trim()) return 'City is required.';
  if (!a.state?.trim()) return 'State is required.';
  if (!a.postalCode?.trim()) return 'Postal code is required.';
  if (!a.country?.trim()) return 'Country is required.';
  return null;
}

export function formatAddress(a: AddressInput): string {
  return [a.line1, a.line2, a.city, a.state, a.postalCode, a.country].filter((p) => !!p?.trim()).join(', ');
}

@Injectable({ providedIn: 'root' })
export class AddressService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/addresses`;

  getMine(): Observable<AddressDto[]> {
    return this.http.get<AddressDto[]>(this.baseUrl);
  }

  create(address: AddressInput): Observable<AddressDto> {
    return this.http.post<AddressDto>(this.baseUrl, address);
  }

  update(id: string, address: AddressInput): Observable<AddressDto> {
    return this.http.put<AddressDto>(`${this.baseUrl}/${id}`, address);
  }

  setDefault(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/set-default`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}