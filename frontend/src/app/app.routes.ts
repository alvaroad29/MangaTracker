import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'search', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'search',
    loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent)
  },
  {
    path: 'my-list',
    canActivate: [authGuard],
    loadComponent: () => import('./features/my-list/my-list.component').then(m => m.MyListComponent)
  },
  { path: '**', redirectTo: 'search' }
];
