import type { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const token = authService.getAccessToken();
  const authReq = token

    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

    // console.log({req});
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // No reintentamos el refresh del propio endpoint de refresh, para evitar loop infinito
      if (error.status === 401 && !req.url.includes('/api/auth/refresh')) {
        return authService.refresh().pipe(
          switchMap(res => {
            const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } });
            return next(retryReq);
          }),
          catchError(refreshError => {
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
