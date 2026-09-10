import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { SessionDataCacheService } from '../cache/session-data-cache.service';

describe('AuthService tenant selection', () => {
  it('clears prior tenant caches and reloads authenticated context after switching', async () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const auth = TestBed.inject(AuthService); const http = TestBed.inject(HttpTestingController); const cache = TestBed.inject(SessionDataCacheService);
    await cache.load('customers', () => Promise.resolve(['Alpha customer']));
    const switchPromise = auth.selectTenant('beta');
    const selection = http.expectOne('/api/auth/tenant'); expect(selection.request.body).toEqual({ tenantId: 'beta' }); selection.flush({});
    await Promise.resolve();
    http.expectOne('/api/auth/me').flush({ id: 'user', name: 'Test', email: 'test@example.test', isPlatformAdmin: false, tenant: { id: 'beta', modules: [] } });
    await switchPromise;
    expect(cache.peek('customers')).toBeUndefined(); expect(auth.user()?.tenant?.id).toBe('beta'); http.verify();
  });
});
