import { TenantContext } from '../tenancy/tenant-context.service';
export interface CurrentUser { id: string; name: string; email: string; isPlatformAdmin: boolean; tenant: TenantContext | null; }
export interface SetupRequest { companyName: string; cnpj: string | null; phone: string | null; adminName: string; email: string; password: string; }
