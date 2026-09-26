import type { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  // console.log("error in");

  // console.log({req});

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 429) {
        notifications.show('Estamos recibiendo muchas búsquedas ahora mismo. Esperá un momento y probá de nuevo.');
      } else if (error.status === 0 || error.status >= 500) {
        notifications.show('Ocurrió un error de conexión. Probá de nuevo en un momento.');
      }
      return throwError(() => error);
    })
  );
};
