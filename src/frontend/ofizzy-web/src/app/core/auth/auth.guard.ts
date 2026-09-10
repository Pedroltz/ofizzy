import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); if (await auth.setupRequired()) return router.createUrlTree(['/setup']); return (await auth.restore()) || router.createUrlTree(['/login']); };
export const guestGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); if (await auth.setupRequired()) return router.createUrlTree(['/setup']); return (await auth.restore()) ? router.createUrlTree(['/']) : true; };
export const setupGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); return (await auth.setupRequired()) || router.createUrlTree(['/login']); };

export const tenantGuard: CanActivateFn = async (route) => {
  const auth = inject(AuthService); const router = inject(Router);
  if (!await auth.restore()) return router.createUrlTree(['/login']);
  const tenant = auth.user()?.tenant;
  if (!tenant) return router.createUrlTree(['/organizacoes']);
  if (!tenant.onboardingCompleted) return router.createUrlTree(['/onboarding']);
  const modules = route.data['modules'] as string[] | undefined;
  return (!modules || modules.every(m => tenant.modules.some(x => x === m))) || router.createUrlTree(['/organizacoes']);
};
export const platformGuard: CanActivateFn = async () => {
  const auth = inject(AuthService); const router = inject(Router);
  return (await auth.restore() && auth.user()?.isPlatformAdmin) || router.createUrlTree(['/organizacoes']);
};
