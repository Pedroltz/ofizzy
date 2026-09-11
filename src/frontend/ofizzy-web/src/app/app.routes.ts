import { Routes } from '@angular/router';
import { authGuard, guestGuard, setupGuard, tenantGuard, platformGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: 'setup', canActivate: [setupGuard], loadComponent: () => import('./features/setup/setup.page').then((m) => m.SetupPage) },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./features/login/login.page').then((m) => m.LoginPage) },
  { path: '', canMatch: [(_route, segments) => ['organizacoes', 'plataforma', 'onboarding'].includes(segments[0]?.path)], canActivate: [authGuard], data: { platformArea: true }, loadComponent: () => import('./layout/app-shell.component').then(m => m.AppShellComponent), children: [
  { path: 'organizacoes', canActivate: [authGuard], loadComponent: () => import('./features/tenancy/organizations.page').then(m => m.OrganizationsPage) },
  { path: 'plataforma', canActivate: [authGuard, platformGuard], loadComponent: () => import('./features/tenancy/platform.page').then(m => m.PlatformPage) },
  { path: 'onboarding', canActivate: [authGuard], loadComponent: () => import('./features/tenancy/onboarding.page').then(m => m.OnboardingPage) },
  ] },
  {
    path: '', canActivate: [authGuard, tenantGuard], loadComponent: () => import('./layout/app-shell.component').then((m) => m.AppShellComponent),
    children: [
      { path: '', pathMatch: 'full', canActivate: [tenantGuard], data: { modules: ['Customers', 'WorkOrders', 'Automotive'] }, loadComponent: () => import('./features/dashboard/dashboard.page').then((m) => m.DashboardPage) },
      { path: 'clientes', canActivate: [tenantGuard], data: { modules: ['Customers'] }, loadComponent: () => import('./features/customers/customers.page').then((m) => m.CustomersPage) },
      { path: 'veiculos', canActivate: [tenantGuard], data: { modules: ['Automotive', 'Customers'] }, loadComponent: () => import('./features/vehicles/vehicles.page').then((m) => m.VehiclesPage) },
      { path: 'ordens', canActivate: [tenantGuard], data: { modules: ['WorkOrders', 'Catalog', 'Automotive', 'Customers'] }, loadComponent: () => import('./features/work-orders/work-orders.page').then((m) => m.WorkOrdersPage) },
      { path: 'configuracoes', canActivate: [tenantGuard], data: { modules: [] }, loadComponent: () => import('./features/settings/settings.page').then((m) => m.SettingsPage) },
    ],
  },
  { path: '**', redirectTo: '' },
];
