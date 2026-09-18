import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpXsrfTokenExtractor, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { authRefreshInterceptor } from './auth-refresh.interceptor';
import { AuthService } from '../auth/auth.service';

describe('authRefreshInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: Router;
  let authService: AuthService;
  const settle = () => new Promise((resolve) => setTimeout(resolve, 0));
  async function prepareRefresh() {
    httpMock.expectOne('/api/setup/status').flush({ required: false });
    await settle();
  }
  async function finishRefresh() {
    await settle();
    httpMock.expectOne('/api/setup/status').flush({ required: false });
    await settle();
  }

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authRefreshInterceptor])),
        provideHttpClientTesting(),
        MessageService,
        {
          provide: Router,
          useValue: { navigateByUrl: vi.fn().mockResolvedValue(true) },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('permite requisições bem-sucedidas passarem sem refresh', () => {
    let result: unknown;
    http.get('/api/customers').subscribe((data) => (result = data));

    const req = httpMock.expectOne('/api/customers');
    req.flush([{ id: '1', name: 'Cliente 1' }]);

    expect(result).toEqual([{ id: '1', name: 'Cliente 1' }]);
  });

  it('ao receber 401, renova a sessão via /api/auth/refresh e repete a requisição original', async () => {
    let result: unknown;
    http.get('/api/customers').subscribe((data) => (result = data));

    const firstReq = httpMock.expectOne('/api/customers');
    firstReq.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    await prepareRefresh();

    const refreshReq = httpMock.expectOne('/api/auth/refresh');
    expect(refreshReq.request.method).toBe('POST');
    refreshReq.flush({
      id: 'user1',
      name: 'User',
      email: 'user@test.com',
      isPlatformAdmin: false,
      tenant: null,
    });

    await finishRefresh();

    const retryReq = httpMock.expectOne('/api/customers');
    retryReq.flush([{ id: '1', name: 'Cliente Renovado' }]);

    expect(result).toEqual([{ id: '1', name: 'Cliente Renovado' }]);
    expect(authService.authenticated()).toBe(true);
  });

  it('não dispara refresh quando a URL do erro 401 está na lista de bypass', () => {
    let errorStatus = 0;
    http.post('/api/auth/login', { email: 'wrong', password: 'pwd' }).subscribe({
      error: (err) => (errorStatus = err.status),
    });

    const loginReq = httpMock.expectOne('/api/auth/login');
    loginReq.flush({ title: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });

    httpMock.expectNone('/api/auth/refresh');
    expect(errorStatus).toBe(401);
  });

  it('redireciona para /login quando o refresh da sessão falha', async () => {
    let errorStatus = 0;
    http.get('/api/customers').subscribe({
      error: (err) => (errorStatus = err.status),
    });

    const firstReq = httpMock.expectOne('/api/customers');
    firstReq.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    await prepareRefresh();

    const refreshReq = httpMock.expectOne('/api/auth/refresh');
    refreshReq.flush({ title: 'Session expired' }, { status: 401, statusText: 'Unauthorized' });

    await new Promise((r) => setTimeout(r, 10));

    expect(errorStatus).toBe(401);
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
    expect(authService.authenticated()).toBe(false);
    expect(await authService.restore()).toBe(false);
    httpMock.expectNone('/api/auth/me');
    httpMock.expectNone('/api/auth/refresh');
  });

  for (const status of [0, 400, 429, 503]) {
    it(`não anuncia expiração nem redireciona quando refresh falha com ${status}`, async () => {
      const restoring = authService.restore();
      httpMock.expectOne('/api/auth/me').flush({ id: 'user1' });
      await restoring;
      const messages = vi.spyOn(TestBed.inject(MessageService), 'add');
      let errorStatus: number | undefined;
      http.get('/api/customers').subscribe({ error: error => { errorStatus = error.status; } });
      httpMock.expectOne('/api/customers').flush(null, { status: 401, statusText: 'Unauthorized' });
      await prepareRefresh();
      const refresh = httpMock.expectOne('/api/auth/refresh');
      if (status === 0) refresh.error(new ProgressEvent('error'));
      else refresh.flush(null, { status, statusText: 'Failed' });
      await settle();
      expect(errorStatus).toBe(status);
      expect(authService.authenticated()).toBe(true);
      expect(router.navigateByUrl).not.toHaveBeenCalled();
      expect(messages).not.toHaveBeenCalled();
    });
  }

  it('compartilha a renovação e anuncia uma única expiração para requisições concorrentes', async () => {
    const messages = vi.spyOn(TestBed.inject(MessageService), 'add');
    for (const path of ['/api/customers', '/api/vehicles']) {
      http.get(path).subscribe({ error: () => undefined });
      httpMock.expectOne(path).flush(null, { status: 401, statusText: 'Unauthorized' });
    }
    await prepareRefresh();
    httpMock.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    await settle();
    expect(router.navigateByUrl).toHaveBeenCalledTimes(1);
    expect(messages).toHaveBeenCalledTimes(1);
  });

  it('usa o novo token XSRF ao repetir uma escrita e encerra a sessão se receber outro 401', async () => {
    vi.spyOn(TestBed.inject(HttpXsrfTokenExtractor), 'getToken').mockReturnValue('renewed-token');
    http.post('/api/customers', { name: 'Test' }, { headers: { 'X-XSRF-TOKEN': 'old-token' } })
      .subscribe({ error: () => undefined });
    httpMock.expectOne('/api/customers').flush(null, { status: 401, statusText: 'Unauthorized' });
    await prepareRefresh();
    httpMock.expectOne('/api/auth/refresh').flush({ id: 'user1' });
    await finishRefresh();
    const retry = httpMock.expectOne('/api/customers');
    expect(retry.request.headers.get('X-XSRF-TOKEN')).toBe('renewed-token');
    expect(retry.request.body).toEqual({ name: 'Test' });
    retry.flush(null, { status: 401, statusText: 'Unauthorized' });
    await settle();
    expect(authService.authenticated()).toBe(false);
    expect(await authService.restore()).toBe(false);
    expect(router.navigateByUrl).toHaveBeenCalledTimes(1);
  });

  it('restaura com uma única tentativa de refresh e permite login explícito depois da expiração', async () => {
    const restoring = authService.restore();
    httpMock.expectOne('/api/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });
    await prepareRefresh();
    httpMock.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(await restoring).toBe(false);
    expect(await authService.restore()).toBe(false);
    const login = authService.login('test@example.test', 'test-password');
    httpMock.expectOne('/api/auth/login').flush({ id: 'user1' });
    await settle();
    httpMock.expectOne('/api/auth/me').flush({ id: 'user1' });
    await login;
    expect(await authService.restore()).toBe(true);
  });
});
