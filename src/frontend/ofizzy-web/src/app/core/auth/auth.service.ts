import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrentUser, SetupRequest } from './auth.models';
import { SessionDataCacheService } from '../cache/session-data-cache.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly cache = inject(SessionDataCacheService);
  private readonly currentUser = signal<CurrentUser | null>(null);
  private restorePromise?: Promise<boolean>;
  readonly user = this.currentUser.asReadonly();
  readonly authenticated = computed(() => this.currentUser() !== null);

  setupRequired(): Promise<boolean> { return firstValueFrom(this.http.get<{ required: boolean }>('/api/setup/status')).then((response) => response.required); }
  setup(request: SetupRequest): Promise<void> { return firstValueFrom(this.http.post<CurrentUser>('/api/setup', request)).then(() => this.loadAuthenticatedUser()); }
  login(email: string, password: string): Promise<void> { return firstValueFrom(this.http.post<CurrentUser>('/api/auth/login', { email, password })).then(() => this.loadAuthenticatedUser()); }
  restore(): Promise<boolean> {
    if (this.currentUser()) return Promise.resolve(true);
    this.restorePromise ??= firstValueFrom(this.http.get<CurrentUser>('/api/auth/me'))
      .catch(() => firstValueFrom(this.http.post<CurrentUser>('/api/auth/refresh', {})))
      .then((user) => { this.currentUser.set(user); return true; }).catch(() => false).finally(() => { this.restorePromise = undefined; });
    return this.restorePromise;
  }
  logout(): Promise<void> { return firstValueFrom(this.http.post<void>('/api/auth/logout', {})).catch(() => undefined).then(() => { this.cache.clear(); this.currentUser.set(null); }); }
  reload(): Promise<void> { return this.loadAuthenticatedUser(); }
  selectTenant(tenantId: string): Promise<void> { return firstValueFrom(this.http.post<CurrentUser>('/api/auth/tenant', { tenantId })).then(() => { this.cache.clear(); return this.loadAuthenticatedUser(); }); }
  private loadAuthenticatedUser(): Promise<void> { return firstValueFrom(this.http.get<CurrentUser>('/api/auth/me')).then((user) => { this.cache.clear(); this.currentUser.set(user); }); }
}
