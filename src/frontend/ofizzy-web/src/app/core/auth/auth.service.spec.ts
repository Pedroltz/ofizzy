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

describe('AuthService destination', () => {
  for (const scenario of [
    { name: 'operador sem vínculo', isPlatformAdmin: true, tenant: null, expected: '/plataforma' },
    { name: 'usuário sem seleção', isPlatformAdmin: false, tenant: null, expected: '/organizacoes' },
    { name: 'empresa pendente', isPlatformAdmin: false, tenant: { onboardingCompleted: false, modules: [] }, expected: '/onboarding' },
    { name: 'empresa operacional', isPlatformAdmin: false, tenant: { onboardingCompleted: true, modules: ['WorkOrders'] }, expected: '/' },
    { name: 'empresa sem ordens', isPlatformAdmin: false, tenant: { onboardingCompleted: true, modules: ['Customers'] }, expected: '/configuracoes' },
  ]) {
    it(`direciona ${scenario.name}`, async () => {
      TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
      const auth = TestBed.inject(AuthService); const http = TestBed.inject(HttpTestingController);
      const restoring = auth.restore();
      http.expectOne('/api/auth/me').flush({ id: 'test', name: 'Test', email: 'test@example.test', isPlatformAdmin: scenario.isPlatformAdmin, tenant: scenario.tenant });
      await restoring; expect(auth.destination()).toBe(scenario.expected); http.verify();
    });
  }
});
