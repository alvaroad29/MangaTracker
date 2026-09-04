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


      // Solo mostramos el toast genérico para errores INESPERADOS (red caída, 500).
      // Los errores esperables (400 de validación, 401, 403, 404, 409) los sigue
      // manejando cada componente puntualmente, porque ahí sí importa el mensaje específico.
      if (error.status === 0 || error.status >= 500) {
        notifications.show('Ocurrió un error de conexión. Probá de nuevo en un momento.');
      }
      return throwError(() => error);
    })
  );
};
