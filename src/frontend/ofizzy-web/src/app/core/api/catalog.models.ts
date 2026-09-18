export interface PagedResponse<T> { items: T[]; page: number; pageSize: number; total: number; }
export interface Customer {
  id: string;
  name: string;
  document: string | null;
  phone: string | null;
  whatsApp: string | null;
  email: string | null;
  address: string | null;
  notes: string | null;
  isActive: boolean;
  createdAt: string;
  postalCode?: string | null;
  street?: string | null;
  number?: string | null;
  district?: string | null;
  city?: string | null;
  state?: string | null;
  cityCode?: string | null;
  stateRegistration?: string | null;
}
export type CustomerRequest = Omit<Customer, 'id' | 'isActive' | 'createdAt'>;
export interface Vehicle { id: string; customerId: string; customerName: string; plate: string; brand: string | null; model: string; year: number | null; color: string | null; mileage: number | null; chassis: string | null; notes: string | null; isActive: boolean; createdAt: string; }
export interface VehicleRequest { customerId: string; plate: string; brand: string | null; model: string; year: number | null; color: string | null; mileage: number | null; chassis: string | null; notes: string | null; }
export interface ServiceItem { id: string; name: string; description: string | null; defaultPrice: number; isActive: boolean; }
export interface ServiceRequest { name: string; description: string | null; defaultPrice: number; }
export interface Part { id: string; name: string; code: string; costPrice: number; salePrice: number; isActive: boolean; }
export interface PartRequest { name: string; code: string; costPrice: number; salePrice: number; }
