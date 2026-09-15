import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
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

    await Promise.resolve();

    const refreshReq = httpMock.expectOne('/api/auth/refresh');
    expect(refreshReq.request.method).toBe('POST');
    refreshReq.flush({
      id: 'user1',
      name: 'User',
      email: 'user@test.com',
      isPlatformAdmin: false,
      tenant: null,
    });

    await new Promise((r) => setTimeout(r, 10));

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

    await Promise.resolve();

    const refreshReq = httpMock.expectOne('/api/auth/refresh');
    refreshReq.flush({ title: 'Session expired' }, { status: 401, statusText: 'Unauthorized' });

    await new Promise((r) => setTimeout(r, 10));

    expect(errorStatus).toBe(401);
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
    expect(authService.authenticated()).toBe(false);
  });
});
