import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

const BYPASS_URLS = ['/api/auth/login', '/api/auth/refresh', '/api/auth/logout', '/api/setup'];

export const authRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const messages = inject(MessageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || BYPASS_URLS.some((url) => req.url.includes(url))) {
        return throwError(() => error);
      }

      return from(authService.refreshSession()).pipe(
        switchMap((success) => {
          if (success) {
            return next(req);
          }
          authService.handleSessionExpired();
          void router.navigateByUrl('/login');
          messages.add({
            severity: 'warn',
            summary: 'Sessão expirada',
            detail: 'Sua sessão expirou por inatividade. Faça login novamente.',
          });
          return throwError(() => error);
        }),
      );
    }),
  );
};
