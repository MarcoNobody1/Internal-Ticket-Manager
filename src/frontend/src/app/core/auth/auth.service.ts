import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';

import { AuthSession, LoginRequest, LoginResponse } from './auth.models';

export const AUTH_SESSION_STORAGE_KEY = 'itm.auth.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionSubject = new BehaviorSubject<AuthSession | null>(this.getStoredSession());
  readonly session$ = this.sessionSubject.asObservable();

  constructor(private readonly httpClient: HttpClient) {
    this.clearSessionIfExpired();
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
    sessionStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
    this.sessionSubject.next(null);
  }

  getSession(): AuthSession | null {
    this.clearSessionIfExpired();
    return this.sessionSubject.value;
  }

  isAuthenticated(): boolean {
    return this.getSession() !== null;
  }

  private setSession(session: AuthSession): void {
    sessionStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));
    this.sessionSubject.next(session);
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
}
