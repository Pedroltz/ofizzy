import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Customer, CustomerRequest, PagedResponse, Part, PartRequest, ServiceItem, ServiceRequest, Vehicle, VehicleRequest } from './catalog.models';
import { cacheKey, SessionDataCacheService } from '../cache/session-data-cache.service';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly cache = inject(SessionDataCacheService);
  customers(q = '', page = 1, pageSize = 20): Promise<PagedResponse<Customer>> { const key = cacheKey.customers(q, page, pageSize); return this.cache.load(key, () => firstValueFrom(this.http.get<PagedResponse<Customer>>('/api/customers', { params: this.params(q, page, pageSize) }))); }
  peekCustomers(q = '', page = 1, pageSize = 20): PagedResponse<Customer> | undefined { return this.cache.peek(cacheKey.customers(q, page, pageSize)); }
  saveCustomer(request: CustomerRequest, id?: string): Promise<Customer> { return firstValueFrom(id ? this.http.put<Customer>(`/api/customers/${id}`, request) : this.http.post<Customer>('/api/customers', request)).then((result) => { this.cache.invalidate('customers:', 'vehicles:', 'dashboard:'); return result; }); }
  archiveCustomer(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/customers/${id}`)).then(() => this.cache.invalidate('customers:', 'vehicles:', 'dashboard:')); }
  vehicles(q = '', page = 1, pageSize = 20, customerId?: string): Promise<PagedResponse<Vehicle>> { const key = cacheKey.vehicles(q, page, pageSize, customerId); return this.cache.load(key, () => { let params = this.params(q, page, pageSize); if (customerId) params = params.set('customerId', customerId); return firstValueFrom(this.http.get<PagedResponse<Vehicle>>('/api/vehicles', { params })); }); }
  peekVehicles(q = '', page = 1, pageSize = 20, customerId?: string): PagedResponse<Vehicle> | undefined { return this.cache.peek(cacheKey.vehicles(q, page, pageSize, customerId)); }
  saveVehicle(request: VehicleRequest, id?: string): Promise<Vehicle> { return firstValueFrom(id ? this.http.put<Vehicle>(`/api/vehicles/${id}`, request) : this.http.post<Vehicle>('/api/vehicles', request)).then((result) => { this.cache.invalidate('vehicles:', 'dashboard:'); return result; }); }
  archiveVehicle(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/vehicles/${id}`)).then(() => this.cache.invalidate('vehicles:', 'dashboard:')); }
  services(q = '', pageSize = 50): Promise<PagedResponse<ServiceItem>> { const key = cacheKey.services(q, pageSize); return this.cache.load(key, () => firstValueFrom(this.http.get<PagedResponse<ServiceItem>>('/api/services', { params: this.params(q, 1, pageSize) }))); }
  peekServices(q = '', pageSize = 50): PagedResponse<ServiceItem> | undefined { return this.cache.peek(cacheKey.services(q, pageSize)); }
  saveService(request: ServiceRequest, id?: string): Promise<ServiceItem> { return firstValueFrom(id ? this.http.put<ServiceItem>(`/api/services/${id}`, request) : this.http.post<ServiceItem>('/api/services', request)).then((result) => { this.cache.invalidate('services:'); return result; }); }
  archiveService(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/services/${id}`)).then(() => this.cache.invalidate('services:')); }
  parts(q = '', pageSize = 50): Promise<PagedResponse<Part>> { const key = cacheKey.parts(q, pageSize); return this.cache.load(key, () => firstValueFrom(this.http.get<PagedResponse<Part>>('/api/parts', { params: this.params(q, 1, pageSize) }))); }
  peekParts(q = '', pageSize = 50): PagedResponse<Part> | undefined { return this.cache.peek(cacheKey.parts(q, pageSize)); }
  savePart(request: PartRequest, id?: string): Promise<Part> { return firstValueFrom(id ? this.http.put<Part>(`/api/parts/${id}`, request) : this.http.post<Part>('/api/parts', request)).then((result) => { this.cache.invalidate('parts:'); return result; }); }
  archivePart(id: string): Promise<void> { return firstValueFrom(this.http.delete<void>(`/api/parts/${id}`)).then(() => this.cache.invalidate('parts:')); }
  private params(q: string | undefined | null, page: number, pageSize: number): HttpParams {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const query = (q ?? '').trim();
    if (query) params = params.set('q', query);
    return params;
  }
}
