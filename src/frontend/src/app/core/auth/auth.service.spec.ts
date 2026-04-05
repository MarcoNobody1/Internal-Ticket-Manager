import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { AUTH_SESSION_STORAGE_KEY, AuthService } from './auth.service';

describe('AuthService', () => {
  let authService: AuthService;
  let httpTestingController: HttpTestingController;
  let router: Router;

  function configureTestingModule(): void {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
    });

    authService = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  }

  beforeEach(() => {
    sessionStorage.clear();
    configureTestingModule();
  });

  afterEach(() => {
    httpTestingController.verify();
    sessionStorage.clear();
  });

  it('stores the session after a successful login', () => {
    let emittedUsername = '';

    authService
      .login({
        username: 'admin.demo',
        password: 'AdminDemo123!'
      })
      .subscribe((session) => {
        emittedUsername = session.username;
      });

    const request = httpTestingController.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');

    request.flush({
      accessToken: 'demo-token',
      expiresAtUtc: '2099-01-01T00:00:00Z',
      username: 'admin.demo',
      role: 'Admin'
    });

    expect(emittedUsername).toBe('admin.demo');
    expect(authService.isAuthenticated()).toBeTrue();

    const storedSession = sessionStorage.getItem(AUTH_SESSION_STORAGE_KEY);
    expect(storedSession).toContain('demo-token');
    expect(storedSession).toContain('admin.demo');
  });

  it('clears an expired stored session during startup', () => {
    TestBed.resetTestingModule();
    sessionStorage.setItem(
      AUTH_SESSION_STORAGE_KEY,
      JSON.stringify({
        accessToken: 'expired-token',
        expiresAtUtc: '2000-01-01T00:00:00Z',
        username: 'developer.demo',
        role: 'Developer'
      })
    );

    configureTestingModule();

    expect(authService.getSession()).toBeNull();
    expect(sessionStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBeNull();
  });

  it('expires an active session and redirects to login', fakeAsync(() => {
    spyOn(router, 'navigate').and.resolveTo(true);

    authService.login({
      username: 'developer.demo',
      password: 'DeveloperDemo123!'
    }).subscribe();

    const request = httpTestingController.expectOne('/api/auth/login');
    request.flush({
      accessToken: 'short-lived-token',
      expiresAtUtc: new Date(Date.now() + 50).toISOString(),
      username: 'developer.demo',
      role: 'Developer'
    });

    tick(60);

    expect(authService.getSession()).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: {
        reason: 'sessionExpired',
        returnUrl: '/'
      }
    });
  }));
});
