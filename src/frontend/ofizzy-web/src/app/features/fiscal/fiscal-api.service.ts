import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
export type FiscalValue = string | number | boolean | null;
export interface FiscalAddress {
  street: string;
  number: string;
  district: string;
  city: string;
  cityCode: string;
  state: string;
  postalCode: string;
}
export interface FiscalSettings {
  cnpj: string;
  legalName: string;
  stateRegistration: string;
  municipalRegistration: string;
  regime: string;
  address: FiscalAddress | null;
  nfeEnabled: boolean;
  nfseEnabled: boolean;
  environment: string;
  nfeSeries: number;
  dpsSeries: number;
}
export interface FiscalSettingsResponse {
  settings: FiscalSettings;
  certificate: { subject: string; expiresAt: string; thumbprint: string } | null;
  encryptionConfigured: boolean;
  productionAllowed: boolean;
  devToolsAvailable: boolean;
}
export interface FiscalPreparation {
  address: FiscalAddress | null;
  document: string | null;
  name: string | null;
  stateRegistration: string | null;
  recipientIeIndicator: string;
  competence: string | null;
  paymentCode: string;
  paymentAmount: number | null;
  products: Record<string, Record<string, FiscalValue>> | null;
  services: Record<string, Record<string, FiscalValue>> | null;
}
export interface FiscalDocument {
  id: string;
  kind: 'Nfe' | 'Nfse';
  environment: string;
  state: string;
  number: number;
  schemaPackage: string;
  accessKey: string | null;
  total: number;
  message: string | null;
  canDownload: boolean;
}
export interface FiscalOrder {
  preparation: FiscalPreparation;
  issues: { field: string; message: string }[];
  servicesTotal: number;
  productsTotal: number;
  status: string;
  documents: FiscalDocument[];
}
export interface FiscalInutilization {
  id: string;
  series: number;
  year: number;
  firstNumber: number;
  lastNumber: number;
  environment: string;
  state: string;
  protocol: string | null;
  message: string | null;
  createdAt: string;
}
@Injectable({ providedIn: 'root' })
export class FiscalApiService {
  private readonly http = inject(HttpClient);
  inutilizations() {
    return firstValueFrom(this.http.get<FiscalInutilization[]>('/api/fiscal/nfe/inutilizations'));
  }
  inutilize(data: Record<string, FiscalValue>) {
    return firstValueFrom(this.http.post('/api/fiscal/nfe/inutilizations', data));
  }
  recoverInutilization(id: string) {
    return firstValueFrom(this.http.post(`/api/fiscal/nfe/inutilizations/${id}/sync`, {}));
  }
  settings() {
    return firstValueFrom(this.http.get<FiscalSettingsResponse>('/api/fiscal/settings'));
  }
  saveSettings(data: FiscalSettings) {
    return firstValueFrom(this.http.put('/api/fiscal/settings', data));
  }
  certificate(file: File, password: string) {
    const data = new FormData();
    data.append('file', file);
    data.append('password', password);
    return firstValueFrom(this.http.post('/api/fiscal/certificate', data));
  }
  profile(kind: 'parts' | 'services', id: string) {
    return firstValueFrom(this.http.get<Record<string, FiscalValue>>(`/api/${kind}/${id}/fiscal`));
  }
  saveProfile(kind: 'parts' | 'services', id: string, data: Record<string, FiscalValue>) {
    return firstValueFrom(this.http.put(`/api/${kind}/${id}/fiscal`, data));
  }
  order(id: string) {
    return firstValueFrom(this.http.get<FiscalOrder>(`/api/work-orders/${id}/fiscal`));
  }
  saveOrder(id: string, data: FiscalPreparation) {
    return firstValueFrom(this.http.put(`/api/work-orders/${id}/fiscal`, data));
  }
  issue(id: string) {
    return firstValueFrom(this.http.post(`/api/work-orders/${id}/fiscal/issue`, {}));
  }
  sync(id: string) {
    return firstValueFrom(this.http.post(`/api/fiscal/documents/${id}/sync`, {}));
  }
  cancel(id: string, reason: string) {
    return firstValueFrom(this.http.post(`/api/fiscal/documents/${id}/cancel`, { reason }));
  }
  download(id: string, type: 'xml' | 'pdf') {
    return firstValueFrom(
      this.http.get(`/api/fiscal/documents/${id}/${type}`, { responseType: 'blob' }),
    );
  }
  bundle(id: string, pdf = false) {
    return firstValueFrom(
      this.http.get(`/api/work-orders/${id}/fiscal/download`, {
        params: { includePdf: pdf },
        responseType: 'blob',
      }),
    );
  }
  downloadDevCertificate() {
    return firstValueFrom(this.http.get('/api/fiscal/dev/certificate', { responseType: 'blob' }));
  }
}
export function saveFiscalBlob(blob: Blob, name: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = name;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  setTimeout(() => URL.revokeObjectURL(url), 30000);
}
