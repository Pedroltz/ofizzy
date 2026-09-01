import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrentUser, SetupRequest } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUser = signal<CurrentUser | null>(null);
  private restorePromise?: Promise<boolean>;
  readonly user = this.currentUser.asReadonly();
  readonly authenticated = computed(() => this.currentUser() !== null);

  setupRequired(): Promise<boolean> { return firstValueFrom(this.http.get<{ required: boolean }>('/api/setup/status')).then((response) => response.required); }
  setup(request: SetupRequest): Promise<void> { return firstValueFrom(this.http.post<CurrentUser>('/api/setup', request)).then((user) => this.currentUser.set(user)); }
  login(email: string, password: string): Promise<void> { return firstValueFrom(this.http.post<CurrentUser>('/api/auth/login', { email, password })).then((user) => this.currentUser.set(user)); }
  restore(): Promise<boolean> {
    if (this.currentUser()) return Promise.resolve(true);
    this.restorePromise ??= firstValueFrom(this.http.get<CurrentUser>('/api/auth/me'))
      .catch(() => firstValueFrom(this.http.post<CurrentUser>('/api/auth/refresh', {})))
      .then((user) => { this.currentUser.set(user); return true; }).catch(() => false).finally(() => { this.restorePromise = undefined; });
    return this.restorePromise;
  }
  logout(): Promise<void> { return firstValueFrom(this.http.post<void>('/api/auth/logout', {})).catch(() => undefined).then(() => this.currentUser.set(null)); }
}
