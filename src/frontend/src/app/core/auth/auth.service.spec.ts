import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { AUTH_SESSION_STORAGE_KEY, AuthService } from './auth.service';

describe('AuthService', () => {
  let authService: AuthService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });

    authService = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
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
});
