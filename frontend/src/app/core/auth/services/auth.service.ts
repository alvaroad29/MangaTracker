import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { UserDto, RegisterRequest, AuthResponse, LoginRequest } from '../models/auth.models';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';


const ACCESS_TOKEN_KEY = 'mt_access_token';
const REFRESH_TOKEN_KEY = 'mt_refresh_token';
const USER_KEY = 'mt_user';


@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private currentUser = signal<UserDto | null>(this.readStoredUser());
  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', request).pipe(
      tap(res => {
        // console.log("error 1");
        // console.log(res);
        this.persistSession(res)
      })
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', request).pipe(
      tap(
        res => {
          // console.log("error 1");
          // console.log(res);


          this.persistSession(res)
        }
      )
    );
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<AuthResponse>('/api/auth/refresh', JSON.stringify(refreshToken), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      tap(res => this.persistSession(res)),
      catchError(err => {
        // el refresh token también venció (o fue revocado) → no hay forma de recuperar la sesión
        this.forceLogout();
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post('/api/auth/logout', JSON.stringify(refreshToken), {
        headers: { 'Content-Type': 'application/json' }
      }).subscribe();
    }
    this.clearSession();
  }

  me(): Observable<UserDto> {
    return this.http.get<UserDto>('/api/auth/me');
  }

  private forceLogout(): void {
    this.clearSession();
    this.router.navigateByUrl('/login');
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  private persistSession(res: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    this.currentUser.set(res.user);
  }

  private clearSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
  }

  validateSessionOnStartup(): void {
    if (!this.isAuthenticated()) return; // no había nada guardado, nada que validar

    this.me().subscribe({
      next: user => {
        // el backend es la fuente de verdad — resincronizamos localStorage y el signal
        // por si algo cambió (ej: editaste tu displayName desde otro dispositivo)
        localStorage.setItem('mt_user', JSON.stringify(user));
        this.currentUser.set(user);
      }
      
    });
  }

  private readStoredUser(): UserDto | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserDto;
    } catch {
      return null;
    }
  }
}
