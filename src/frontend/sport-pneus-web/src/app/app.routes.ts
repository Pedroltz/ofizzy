import { Routes } from '@angular/router';
import { authGuard, guestGuard, setupGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: 'setup', canActivate: [setupGuard], loadComponent: () => import('./features/setup/setup.page').then((m) => m.SetupPage) },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./features/login/login.page').then((m) => m.LoginPage) },
  {
    path: '', canActivate: [authGuard], loadComponent: () => import('./layout/app-shell.component').then((m) => m.AppShellComponent),
    children: [{ path: '', pathMatch: 'full', loadComponent: () => import('./features/dashboard/dashboard.page').then((m) => m.DashboardPage) }],
  },
  { path: '**', redirectTo: '' },
];
