import { Injectable } from '@angular/core';
import { map, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { LoginCredentials, TokenData, LoginApiResponse } from '../models/auth.models';

const TOKEN_KEY = 'token';
const USER_KEY  = 'user';
const EXPIRES_AT_KEY = 'tokenExpiresAt';
const SESSION_DURATION_MS = 8 * 60 * 60 * 1000; // 8 saat

@Injectable({ providedIn: 'root' })
export class AuthService {
  private logoutTimer: ReturnType<typeof setTimeout> | null = null;

  get token(): string | null { return localStorage.getItem(TOKEN_KEY); }
  get currentUser() {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  constructor(private api: ApiService, private router: Router) {
    this.initializeSessionState();
  }

  login(model: LoginCredentials) {
    return this.api.post<TokenData>('user/login', model).pipe(
      map((res: any) => {
        // res: ApiResponse<TokenData>
        if (!res.success || !res.data?.IsSuccessful) throw new Error(res.message || 'Giriş başarısız.');
        return res.data as TokenData;
      }),
      tap((td: TokenData) => {
        localStorage.setItem(TOKEN_KEY, td.Token);
        localStorage.setItem(USER_KEY, JSON.stringify(td.User));
        localStorage.setItem(EXPIRES_AT_KEY, (Date.now() + SESSION_DURATION_MS).toString());
        this.scheduleAutoLogout();
      })
    );
  }

  logout() {
    if (this.logoutTimer) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
  }

  hasRole(role: string): boolean {
    const u = this.currentUser;
    if (!u) return false;
    if (!Array.isArray(u.Roles)) return true;
    return u.Roles.includes(role);
  }

  isAuthenticated(): boolean {
    if (!this.token) return false;
    if (this.isSessionExpired()) {
      this.performAutoLogout();
      return false;
    }
    return true;
  }

  private initializeSessionState(): void {
    if (!this.token) return;

    if (this.isSessionExpired()) {
      this.performAutoLogout();
      return;
    }

    this.scheduleAutoLogout();
  }

  private isSessionExpired(): boolean {
    const expiresAtRaw = localStorage.getItem(EXPIRES_AT_KEY);
    if (!expiresAtRaw) return true;

    const expiresAt = Number(expiresAtRaw);
    return !Number.isFinite(expiresAt) || Date.now() >= expiresAt;
  }

  private scheduleAutoLogout(): void {
    if (this.logoutTimer) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }

    const expiresAtRaw = localStorage.getItem(EXPIRES_AT_KEY);
    const expiresAt = Number(expiresAtRaw);
    if (!Number.isFinite(expiresAt)) return;

    const remainingMs = expiresAt - Date.now();
    if (remainingMs <= 0) {
      this.performAutoLogout();
      return;
    }

    this.logoutTimer = setTimeout(() => {
      this.performAutoLogout();
    }, remainingMs);
  }

  private performAutoLogout(): void {
    this.logout();
    this.router.navigate(['/login']);
  }
}
