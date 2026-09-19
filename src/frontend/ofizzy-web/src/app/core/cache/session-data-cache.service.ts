import { Injectable } from '@angular/core';

interface CacheEntry<T> {
  value: T;
  expiresAt: number;
}

@Injectable({ providedIn: 'root' })
export class SessionDataCacheService {
  private readonly entries = new Map<string, CacheEntry<unknown>>();
  private readonly inFlight = new Map<string, Promise<unknown>>();
  private readonly versions = new Map<string, number>();

  peek<T>(key: string): T | undefined {
    return this.entries.get(key)?.value as T | undefined;
  }

  load<T>(key: string, loader: () => Promise<T>, ttlMs = 30_000): Promise<T> {
    const entry = this.entries.get(key);
    if (entry && entry.expiresAt > Date.now()) {
      return Promise.resolve(entry.value as T);
    }

    const pending = this.inFlight.get(key);
    if (pending) return pending as Promise<T>;

    const version = this.versions.get(key) ?? 0;
    const request = loader()
      .then((value) => {
        if ((this.versions.get(key) ?? 0) === version) {
          this.entries.set(key, { value, expiresAt: Date.now() + ttlMs });
        }
        return value;
      })
      .finally(() => {
        if (this.inFlight.get(key) === request) this.inFlight.delete(key);
      });

    this.inFlight.set(key, request);
    return request;
  }

  invalidate(...prefixes: string[]): void {
    const keys = new Set([...this.entries.keys(), ...this.inFlight.keys()]);
    for (const key of keys) {
      if (prefixes.some((prefix) => key.startsWith(prefix))) {
        this.entries.delete(key);
        this.inFlight.delete(key);
        this.versions.set(key, (this.versions.get(key) ?? 0) + 1);
      }
    }
  }

  clear(): void {
    const keys = new Set([
      ...this.entries.keys(),
      ...this.inFlight.keys(),
      ...this.versions.keys(),
    ]);
    this.entries.clear();
    this.inFlight.clear();
    for (const key of keys) {
      this.versions.set(key, (this.versions.get(key) ?? 0) + 1);
    }
  }
}

export const cacheKey = {
  customers: (q: string, page: number, pageSize: number) =>
    `customers:${q.trim().toLocaleLowerCase('pt-BR')}:${page}:${pageSize}`,
  vehicles: (q: string, page: number, pageSize: number, customerId?: string) =>
    `vehicles:${q.trim().toLocaleLowerCase('pt-BR')}:${page}:${pageSize}:${customerId ?? ''}`,
  services: (q: string, page: number, pageSize: number) =>
    `services:${q.trim().toLocaleLowerCase('pt-BR')}:${page}:${pageSize}`,
  parts: (q: string, page: number, pageSize: number) =>
    `parts:${q.trim().toLocaleLowerCase('pt-BR')}:${page}:${pageSize}`,
  workOrders: (q: string, page: number, pageSize: number, status?: string | null) =>
    `work-orders:${q.trim().toLocaleLowerCase('pt-BR')}:${page}:${pageSize}:${status ?? ''}`,
  workOrder: (id: string) => `work-order:${id}`,
  dashboard: 'dashboard:summary',
  company: 'company:current',
} as const;
