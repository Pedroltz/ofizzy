import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { cacheKey, SessionDataCacheService } from '../cache/session-data-cache.service';

export interface CompanyResponse {
  id: string;
  name: string;
  legalName: string | null;
  cnpj: string | null;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  address: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  warrantyTerms: string | null;
  receiptNotes: string | null;
  updatedAt: string;
}

export interface UpdateCompanyRequest {
  name: string;
  legalName?: string | null;
  cnpj?: string | null;
  phone?: string | null;
  whatsApp?: string | null;
  email?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  warrantyTerms?: string | null;
  receiptNotes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CompanyApiService {
  private readonly http = inject(HttpClient);
  private readonly cache = inject(SessionDataCacheService);

  get(): Promise<CompanyResponse> {
    return this.cache.load(cacheKey.company, () => firstValueFrom(this.http.get<CompanyResponse>('/api/company')));
  }

  peek(): CompanyResponse | undefined { return this.cache.peek(cacheKey.company); }

  update(request: UpdateCompanyRequest): Promise<CompanyResponse> {
    return firstValueFrom(this.http.put<CompanyResponse>('/api/company', request)).then((result) => { this.cache.invalidate('company:'); return result; });
  }
}
