import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { Popover } from 'primeng/popover';
import { AuthService } from '../core/auth/auth.service';
import { TenantContextService, TenantContext } from '../core/tenancy/tenant-context.service';
import { CurrentUser } from '../core/auth/auth.models';
import { OrganizationSwitcherComponent } from './organization-switcher.component';

describe('OrganizationSwitcherComponent', () => {
  const mockUser = signal<CurrentUser | null>({
    id: 'user-1',
    name: 'Test User',
    email: 'test@example.com',
    isPlatformAdmin: false,
    tenant: {
      id: 'tenant-1',
      name: 'Oficina Alpha',
      slug: 'alpha',
      status: 'Active',
      vertical: 'Automotive',
      role: 'Owner',
      onboardingCompleted: true,
      modules: ['Customers'],
    },
  });

  const tenant1: TenantContext = {
    id: 'tenant-1',
    name: 'Oficina Alpha',
    slug: 'alpha',
    status: 'Active',
    vertical: 'Automotive',
    role: 'Owner',
    onboardingCompleted: true,
    modules: ['Customers'],
  };

  const tenant2: TenantContext = {
    id: 'tenant-2',
    name: 'Oficina Beta',
    slug: 'beta',
    status: 'Active',
    vertical: 'Automotive',
    role: 'Member',
    onboardingCompleted: true,
    modules: ['Customers'],
  };

  let selectTenantCalledWith: string | null = null;

  beforeEach(() => {
    selectTenantCalledWith = null;
    TestBed.configureTestingModule({
      imports: [OrganizationSwitcherComponent],
      providers: [
        provideRouter([]),
        provideAnimationsAsync(),
        {
          provide: AuthService,
          useValue: {
            user: mockUser,
            selectTenant: (id: string) => {
              selectTenantCalledWith = id;
              return Promise.resolve();
            },
            destination: () => '/',
          },
        },
      ],
    });
  });

  it('renders a simple badge without dropdown when user has only 1 organization', () => {
    const tenantService = TestBed.inject(TenantContextService);
    tenantService.setTenants([tenant1]);

    const fixture = TestBed.createComponent(OrganizationSwitcherComponent);
    fixture.detectChanges();

    const element: HTMLElement = fixture.nativeElement;
    const badge = element.querySelector('.header-org-badge');
    const triggerBtn = element.querySelector('button.org-switcher-trigger');
    const chevron = element.querySelector('.org-switcher-chevron');

    expect(badge).not.toBeNull();
    expect(triggerBtn).toBeNull();
    expect(chevron).toBeNull();
    expect(element.textContent).toContain('Oficina Alpha');
  });

  it('renders a button with chevron trigger when user has multiple organizations', () => {
    const tenantService = TestBed.inject(TenantContextService);
    tenantService.setTenants([tenant1, tenant2]);

    const fixture = TestBed.createComponent(OrganizationSwitcherComponent);
    fixture.detectChanges();

    const element: HTMLElement = fixture.nativeElement;
    const badge = element.querySelector('.header-org-badge');
    const triggerBtn = element.querySelector('button.org-switcher-trigger');
    const chevron = element.querySelector('.org-switcher-chevron');

    expect(badge).toBeNull();
    expect(triggerBtn).not.toBeNull();
    expect(chevron).not.toBeNull();
    expect(element.textContent).toContain('Oficina Alpha');
  });

  it('calls auth.selectTenant when switching to another organization', async () => {
    const tenantService = TestBed.inject(TenantContextService);
    tenantService.setTenants([tenant1, tenant2]);

    const fixture = TestBed.createComponent(OrganizationSwitcherComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    const mockPopover = { hide: () => {} } as unknown as Popover;
    await component.switchTenant(tenant2, mockPopover);

    expect(selectTenantCalledWith).toBe('tenant-2');
  });

  it('does not call auth.selectTenant when selecting the currently active organization', async () => {
    const tenantService = TestBed.inject(TenantContextService);
    tenantService.setTenants([tenant1, tenant2]);

    const fixture = TestBed.createComponent(OrganizationSwitcherComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    let hideCalled = false;
    const mockPopover = {
      hide: () => {
        hideCalled = true;
      },
    } as unknown as Popover;
    await component.switchTenant(tenant1, mockPopover);

    expect(selectTenantCalledWith).toBeNull();
    expect(hideCalled).toBe(true);
  });
});
