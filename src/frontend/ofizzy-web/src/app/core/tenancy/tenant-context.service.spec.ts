import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { CurrentUser } from '../auth/auth.models';
import { TenantContextService } from './tenant-context.service';

describe('TenantContextService', () => {
  it('fails closed without tenant and updates roles/modules when organization changes', () => {
    const user = signal<CurrentUser | null>(null);
    TestBed.configureTestingModule({ providers: [{ provide: AuthService, useValue: { user } }] });
    const context = TestBed.inject(TenantContextService);
    expect(context.has('Automotive')).toBe(false);
    expect(context.admin()).toBe(false);
    user.set({
      id: 'user',
      name: 'Test',
      email: 'test@example.test',
      isPlatformAdmin: false,
      tenant: {
        id: 'alpha',
        name: 'Alpha',
        slug: 'alpha',
        status: 'Active',
        vertical: 'Automotive',
        role: 'Owner',
        onboardingCompleted: true,
        modules: ['Customers', 'Automotive'],
      },
    });
    expect(context.has('Automotive')).toBe(true);
    expect(context.admin()).toBe(true);
    user.set({
      ...user()!,
      tenant: { ...user()!.tenant!, id: 'beta', role: 'Member', modules: ['Customers'] },
    });
    expect(context.has('Automotive')).toBe(false);
    expect(context.admin()).toBe(false);
  });

  it('determines hasMultipleTenants based on userTenants length', () => {
    const user = signal<CurrentUser | null>(null);
    TestBed.configureTestingModule({ providers: [{ provide: AuthService, useValue: { user } }] });
    const context = TestBed.inject(TenantContextService);

    expect(context.hasMultipleTenants()).toBe(false);

    context.setTenants([
      {
        id: '1',
        name: 'Org 1',
        slug: 'org-1',
        status: 'Active',
        vertical: 'Automotive',
        role: 'Owner',
        onboardingCompleted: true,
        modules: [],
      },
    ]);
    expect(context.hasMultipleTenants()).toBe(false);

    context.setTenants([
      {
        id: '1',
        name: 'Org 1',
        slug: 'org-1',
        status: 'Active',
        vertical: 'Automotive',
        role: 'Owner',
        onboardingCompleted: true,
        modules: [],
      },
      {
        id: '2',
        name: 'Org 2',
        slug: 'org-2',
        status: 'Active',
        vertical: 'Automotive',
        role: 'Member',
        onboardingCompleted: true,
        modules: [],
      },
    ]);
    expect(context.hasMultipleTenants()).toBe(true);
  });
});
