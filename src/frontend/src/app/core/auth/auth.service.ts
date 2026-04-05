import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';

import { AuthRedirectReason, AuthSession, LoginRequest, LoginResponse } from './auth.models';

export const AUTH_SESSION_STORAGE_KEY = 'itm.auth.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionSubject = new BehaviorSubject<AuthSession | null>(this.getStoredSession());
  private sessionExpirationTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly session$ = this.sessionSubject.asObservable();

  constructor(private readonly httpClient: HttpClient, private readonly router: Router) {
    this.clearSessionIfExpired();
    this.scheduleSessionExpiration();
  }

  login(request: LoginRequest): Observable<AuthSession> {
    return this.httpClient.post<LoginResponse>('/api/auth/login', request).pipe(
      map((response) => ({
        accessToken: response.accessToken,
        expiresAtUtc: response.expiresAtUtc,
        username: response.username,
        role: response.role
      })),
      tap((session) => this.setSession(session))
    );
  }

  logout(): void {
    this.clearScheduledExpiration();
    sessionStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
    this.sessionSubject.next(null);
  }

  logoutAndRedirect(reason: AuthRedirectReason, returnUrl = this.router.url): void {
    this.logout();

    void this.router.navigate(['/login'], {
      queryParams: {
        reason,
        returnUrl: this.normalizeReturnUrl(returnUrl)
      }
    });
  }

  redirectToAccessDenied(targetUrl = this.router.url): void {
    void this.router.navigate(['/workspace/access-denied'], {
      queryParams: {
        from: this.normalizeReturnUrl(targetUrl)
      }
    });
  }

  getSession(): AuthSession | null {
    this.clearSessionIfExpired();
    return this.sessionSubject.value;
  }

  isAuthenticated(): boolean {
    return this.getSession() !== null;
  }

  hasRole(role: string): boolean {
    return this.getSession()?.role === role;
  }

  private setSession(session: AuthSession): void {
    sessionStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));
    this.sessionSubject.next(session);
    this.scheduleSessionExpiration();
  }

  private getStoredSession(): AuthSession | null {
    const rawSession = sessionStorage.getItem(AUTH_SESSION_STORAGE_KEY);

    if (!rawSession) {
      return null;
    }

    try {
      return JSON.parse(rawSession) as AuthSession;
    } catch {
      sessionStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
      return null;
    }
  }

  private clearSessionIfExpired(): void {
    const session = this.sessionSubject.value;

    if (!session) {
      return;
    }

    if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
      this.logout();
    }
  }

  private scheduleSessionExpiration(): void {
    this.clearScheduledExpiration();

    const session = this.sessionSubject.value;

    if (!session) {
      return;
    }

    const delayMs = new Date(session.expiresAtUtc).getTime() - Date.now();

    if (delayMs <= 0) {
      this.logout();
      return;
    }

    this.sessionExpirationTimeoutId = setTimeout(() => {
      this.logoutAndRedirect('sessionExpired');
    }, delayMs);
  }

  private clearScheduledExpiration(): void {
    if (this.sessionExpirationTimeoutId === null) {
      return;
    }

    clearTimeout(this.sessionExpirationTimeoutId);
    this.sessionExpirationTimeoutId = null;
  }

  private normalizeReturnUrl(returnUrl: string): string | null {
    return returnUrl.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : null;
  }
}
