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
    path: 'ai-search',
    loadComponent: () => import('./features/ai-search/ai-search.component').then(m => m.AiSearchComponent)
  },
  {
    path: 'my-list',
    canActivate: [authGuard],
    loadComponent: () => import('./features/my-entries/my-entries.component').then(m => m.MyEntriesComponent )
  },
  {
    path: 'manga/:malId',
    loadComponent: () => import('./features/manga-detail/manga-detail.component').then(m => m.MangaDetailComponent)
  },
  {
    path: 'lists',
    canActivate: [authGuard],
    loadComponent: () => import('./features/my-lists/my-lists.component').then(m => m.MyListsComponent)
  },
  {
    path: 'lists/followed',
    canActivate: [authGuard],
    loadComponent: () => import('./features/followed-lists/followed-lists.component').then(m => m.FollowedListsComponent)
  },
  {
    path: 'lists/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/list-form/list-form.component').then(m => m.ListFormComponent)
  },
  {
    path: 'lists/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/list-form/list-form.component').then(m => m.ListFormComponent)
  },
  {
    path: 'lists/:id',
    loadComponent: () => import('./features/list-detail/list-detail.component').then(m => m.ListDetailComponent)
  },
  {
    path: 'explore',
    loadComponent: () => import('./features/explore-lists/explore-lists.component').then(m => m.ExploreListsComponent)
  },
  {
    path: 'users/:id',
    loadComponent: () => import('./features/user-profile/user-profile.component').then(m => m.UserProfileComponent)
  },
  { path: '**', redirectTo: 'search' }
];
