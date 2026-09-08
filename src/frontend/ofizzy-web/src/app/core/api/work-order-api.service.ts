import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DashboardSummary, WorkOrder, WorkOrderPage, WorkOrderRequest, WorkOrderStatus } from './work-order.models';
import { cacheKey, SessionDataCacheService } from '../cache/session-data-cache.service';

@Injectable({ providedIn: 'root' })
export class WorkOrderApiService {
  private readonly http = inject(HttpClient);
  private readonly cache = inject(SessionDataCacheService);
  dashboardSummary(): Promise<DashboardSummary> { return this.cache.load(cacheKey.dashboard, () => firstValueFrom(this.http.get<DashboardSummary>('/api/dashboard/summary'))); }
  peekDashboardSummary(): DashboardSummary | undefined { return this.cache.peek(cacheKey.dashboard); }
  list(q = '', page = 1, pageSize = 20, status?: WorkOrderStatus | null): Promise<WorkOrderPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const query = (q ?? '').trim();
    if (query) params = params.set('q', query);
    if (status) params = params.set('status', status);
    const key = cacheKey.workOrders(q, page, pageSize, status);
    return this.cache.load(key, () => firstValueFrom(this.http.get<WorkOrderPage>('/api/work-orders', { params })));
  }
  peekList(q = '', page = 1, pageSize = 20, status?: WorkOrderStatus | null): WorkOrderPage | undefined { return this.cache.peek(cacheKey.workOrders(q, page, pageSize, status)); }
  get(id: string): Promise<WorkOrder> { return this.cache.load(cacheKey.workOrder(id), () => firstValueFrom(this.http.get<WorkOrder>(`/api/work-orders/${id}`))); }
  save(request: WorkOrderRequest, id?: string): Promise<WorkOrder> { return firstValueFrom(id ? this.http.put<WorkOrder>(`/api/work-orders/${id}`, request) : this.http.post<WorkOrder>('/api/work-orders', request)).then((result) => { this.cache.invalidate('work-order:', 'work-orders:', 'dashboard:'); return result; }); }
  changeStatus(id: string, status: WorkOrderStatus): Promise<WorkOrder> { return firstValueFrom(this.http.patch<WorkOrder>(`/api/work-orders/${id}/status`, { status })).then((result) => { this.cache.invalidate('work-order:', 'work-orders:', 'dashboard:'); return result; }); }
  downloadPdf(id: string): Promise<Blob> { return firstValueFrom(this.http.get(`/api/work-orders/${id}/pdf`, { responseType: 'blob' })); }
}

