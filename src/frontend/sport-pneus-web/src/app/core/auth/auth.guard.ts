import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); if (await auth.setupRequired()) return router.createUrlTree(['/setup']); return (await auth.restore()) || router.createUrlTree(['/login']); };
export const guestGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); if (await auth.setupRequired()) return router.createUrlTree(['/setup']); return (await auth.restore()) ? router.createUrlTree(['/']) : true; };
export const setupGuard: CanActivateFn = async () => { const auth = inject(AuthService); const router = inject(Router); return (await auth.setupRequired()) || router.createUrlTree(['/login']); };
