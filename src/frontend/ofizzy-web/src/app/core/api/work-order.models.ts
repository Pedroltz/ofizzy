import { PagedResponse } from './catalog.models';
export type WorkOrderStatus = 'Open' | 'InProgress' | 'Completed' | 'Cancelled';
export interface WorkOrderLineRequest { catalogId: string | null; description: string; code?: string | null; quantity: number; unitPrice: number; }
export interface WorkOrderRequest { customerId: string; vehicleId: string; mileage: number | null; complaint: string | null; diagnosis: string | null; notes: string | null; services: WorkOrderLineRequest[]; parts: WorkOrderLineRequest[]; }
export interface WorkOrderSummary { id: string; number: number; customerName: string; vehiclePlate: string; vehicleDescription: string; status: WorkOrderStatus; total: number; createdAt: string; }
export interface WorkOrder extends WorkOrderSummary, WorkOrderRequest { customerDocument: string | null; customerPhone: string | null; servicesTotal: number; partsTotal: number; completedAt: string | null; services: (WorkOrderLineRequest & { id: string; total: number })[]; parts: (WorkOrderLineRequest & { id: string; code: string | null; total: number })[]; }
export type WorkOrderPage = PagedResponse<WorkOrderSummary>;
export interface DashboardSummary {
  totalCustomers: number;
  totalVehicles: number;
  totalActiveOrders: number;
  totalCompletedOrders: number;
  activeOrders: WorkOrderSummary[];
}
