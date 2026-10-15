import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth/auth.service';

export type ProductModule = 'Customers' | 'WorkOrders' | 'Catalog' | 'Automotive';
export interface TenantContext {
  id: string;
  name: string;
  slug: string;
  status: string;
  vertical: string;
  role: 'Owner' | 'Admin' | 'Member';
  onboardingCompleted: boolean;
  modules: ProductModule[];
}

@Injectable({ providedIn: 'root' })
export class TenantContextService {
  private readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient, { optional: true });

  readonly tenant = computed(() => this.auth.user()?.tenant ?? null);
  readonly admin = computed(() => ['Owner', 'Admin'].includes(this.tenant()?.role ?? ''));
  readonly userTenants = signal<TenantContext[]>([]);
  readonly hasMultipleTenants = computed(() => this.userTenants().length > 1);

  has(module: ProductModule): boolean {
    return this.tenant()?.modules.includes(module) === true;
  }

  async loadUserTenants(): Promise<TenantContext[]> {
    if (!this.http || !this.auth.user()) {
      this.userTenants.set([]);
      return [];
    }
    try {
      const list = await firstValueFrom(this.http.get<TenantContext[]>('/api/auth/tenants'));
      this.userTenants.set(list ?? []);
      return list ?? [];
    } catch {
      this.userTenants.set([]);
      return [];
    }
  }

  setTenants(tenants: TenantContext[]): void {
    this.userTenants.set(tenants);
  }
}
