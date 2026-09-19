import { describe, expect, it, vi } from 'vitest';
import { SessionDataCacheService, cacheKey } from './session-data-cache.service';

describe('SessionDataCacheService', () => {
  it('reuses a fresh value and deduplicates concurrent requests', async () => {
    const cache = new SessionDataCacheService();
    const loader = vi.fn(async () => ({ items: [1] }));

    const first = cache.load('customers::1:12', loader);
    const second = cache.load('customers::1:12', loader);

    expect(first).toBe(second);
    await expect(first).resolves.toEqual({ items: [1] });
    await expect(cache.load('customers::1:12', loader)).resolves.toEqual({ items: [1] });
    expect(loader).toHaveBeenCalledTimes(1);
  });

  it('returns the snapshot while an expired entry is revalidated', async () => {
    vi.useFakeTimers();
    const cache = new SessionDataCacheService();
    await cache.load('dashboard:summary', async () => 'old', 30_000);
    vi.advanceTimersByTime(30_001);

    let resolve!: (value: string) => void;
    const refresh = cache.load(
      'dashboard:summary',
      () =>
        new Promise((done) => {
          resolve = done;
        }),
    );

    expect(cache.peek('dashboard:summary')).toBe('old');
    resolve('new');
    await expect(refresh).resolves.toBe('new');
    expect(cache.peek('dashboard:summary')).toBe('new');
    vi.useRealTimers();
  });

  it('invalidates matching resources without caching an obsolete in-flight result', async () => {
    const cache = new SessionDataCacheService();
    let resolve!: (value: string) => void;
    const pending = cache.load(
      'work-orders::1:12:',
      () =>
        new Promise((done) => {
          resolve = done;
        }),
    );

    cache.invalidate('work-orders:');
    resolve('obsolete');
    await pending;

    expect(cache.peek('work-orders::1:12:')).toBeUndefined();
  });

  it('normalizes equivalent search cache keys', () => {
    expect(cacheKey.customers('  JOÃO ', 1, 12)).toBe(cacheKey.customers('joão', 1, 12));
  });
});
