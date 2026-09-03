import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { WorkOrder, WorkOrderPage, WorkOrderRequest, WorkOrderStatus } from './work-order.models';

@Injectable({ providedIn: 'root' })
export class WorkOrderApiService {
  private readonly http = inject(HttpClient);
  list(q = '', page = 1, pageSize = 20): Promise<WorkOrderPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    const query = (q ?? '').trim();
    if (query) params = params.set('q', query);
    return firstValueFrom(this.http.get<WorkOrderPage>('/api/work-orders', { params }));
  }
  get(id: string): Promise<WorkOrder> { return firstValueFrom(this.http.get<WorkOrder>(`/api/work-orders/${id}`)); }
  save(request: WorkOrderRequest, id?: string): Promise<WorkOrder> { return firstValueFrom(id ? this.http.put<WorkOrder>(`/api/work-orders/${id}`, request) : this.http.post<WorkOrder>('/api/work-orders', request)); }
  changeStatus(id: string, status: WorkOrderStatus): Promise<WorkOrder> { return firstValueFrom(this.http.patch<WorkOrder>(`/api/work-orders/${id}/status`, { status })); }
  downloadPdf(id: string): Promise<Blob> { return firstValueFrom(this.http.get(`/api/work-orders/${id}/pdf`, { responseType: 'blob' })); }
}
