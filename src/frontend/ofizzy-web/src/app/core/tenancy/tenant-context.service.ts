import { Injectable, computed, inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
export type ProductModule = 'Customers' | 'WorkOrders' | 'Catalog' | 'Automotive';
export interface TenantContext { id: string; name: string; slug: string; status: string; vertical: string; role: 'Owner' | 'Admin' | 'Member'; onboardingCompleted: boolean; modules: ProductModule[]; }
@Injectable({ providedIn: 'root' })
export class TenantContextService {
  private readonly auth = inject(AuthService);
  readonly tenant = computed(() => this.auth.user()?.tenant ?? null);
  readonly admin = computed(() => ['Owner', 'Admin'].includes(this.tenant()?.role ?? ''));
  has(module: ProductModule): boolean { return this.tenant()?.modules.includes(module) === true; }
}
