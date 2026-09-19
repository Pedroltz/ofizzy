import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const messages = inject(MessageService);
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        let detail = 'Tente novamente em instantes.';
        if (error.error?.detail) {
          detail = error.error.detail;
        } else if (error.error?.errors && typeof error.error.errors === 'object') {
          const errors = Object.values(error.error.errors).flat() as string[];
          if (errors.length > 0) detail = errors.join(' ');
        } else if (error.error?.title) {
          detail = error.error.title;
        }
        messages.add({ severity: 'error', summary: 'Não foi possível concluir', detail });
      }
      return throwError(() => error);
    }),
  );
};
