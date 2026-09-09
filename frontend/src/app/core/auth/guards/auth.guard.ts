import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // console.log(authService.isAuthenticated());
  if (authService.isAuthenticated()) return true;

  router.navigate(['/login']);
  return false;
};
