import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Customer, CustomerRequest, PagedResponse, Part, PartRequest, ServiceItem, ServiceRequest, Vehicle, VehicleRequest } from './catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  customers(q = '', page = 1, pageSize = 20): Promise<PagedResponse<Customer>> { return firstValueFrom(this.http.get<PagedResponse<Customer>>('/api/customers', { params: this.params(q, page, pageSize) })); }
  saveCustomer(request: CustomerRequest, id?: string): Promise<Customer> { return firstValueFrom(id ? this.http.put<Customer>(`/api/customers/${id}`, request) : this.http.post<Customer>('/api/customers', request)); }
  archiveCustomer(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/customers/${id}`)); }
  vehicles(q = '', page = 1, pageSize = 20, customerId?: string): Promise<PagedResponse<Vehicle>> { let params = this.params(q, page, pageSize); if (customerId) params = params.set('customerId', customerId); return firstValueFrom(this.http.get<PagedResponse<Vehicle>>('/api/vehicles', { params })); }
  saveVehicle(request: VehicleRequest, id?: string): Promise<Vehicle> { return firstValueFrom(id ? this.http.put<Vehicle>(`/api/vehicles/${id}`, request) : this.http.post<Vehicle>('/api/vehicles', request)); }
  archiveVehicle(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/vehicles/${id}`)); }
  services(q = '', pageSize = 50): Promise<PagedResponse<ServiceItem>> { return firstValueFrom(this.http.get<PagedResponse<ServiceItem>>('/api/services', { params: this.params(q, 1, pageSize) })); }
  saveService(request: ServiceRequest, id?: string): Promise<ServiceItem> { return firstValueFrom(id ? this.http.put<ServiceItem>(`/api/services/${id}`, request) : this.http.post<ServiceItem>('/api/services', request)); }
  archiveService(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/services/${id}`)); }
  parts(q = '', pageSize = 50): Promise<PagedResponse<Part>> { return firstValueFrom(this.http.get<PagedResponse<Part>>('/api/parts', { params: this.params(q, 1, pageSize) })); }
  savePart(request: PartRequest, id?: string): Promise<Part> { return firstValueFrom(id ? this.http.put<Part>(`/api/parts/${id}`, request) : this.http.post<Part>('/api/parts', request)); }
  archivePart(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/parts/${id}`)); }
  private params(q: string | undefined | null, page: number, pageSize: number): HttpParams {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const query = (q ?? '').trim();
    if (query) params = params.set('q', query);
    return params;
  }
}
