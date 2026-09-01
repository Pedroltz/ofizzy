import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const messages = inject(MessageService);
  return next(request).pipe(catchError((error: HttpErrorResponse) => {
    if (error.status !== 401) messages.add({ severity: 'error', summary: 'Não foi possível concluir', detail: error.error?.detail ?? 'Tente novamente em instantes.' });
    return throwError(() => error);
  }));
};
