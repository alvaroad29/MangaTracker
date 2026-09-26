import type { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, finalize, Observable, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { AuthResponse } from '../models/auth.models';

let refreshInProgress$: Observable<AuthResponse> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getAccessToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

    // console.log({req});
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // No reintentamos el refresh del propio endpoint de refresh, para evitar loop infinito
      if (error.status === 401 && !req.url.includes('/api/auth/refresh')) {
         if (!authService.getRefreshToken()) {
          authService.logout();
          router.navigateByUrl('/login');
          return throwError(() => error);
        }

        if (!refreshInProgress$) {
          refreshInProgress$ = authService.refresh().pipe(
            shareReplay(1), // el resultado se comparte con quien se "sume" después de que ya terminó
            finalize(() => { refreshInProgress$ = null; }) // se limpia al terminar, para el próximo 401 futuro
          );
        }


         return refreshInProgress$.pipe(
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
