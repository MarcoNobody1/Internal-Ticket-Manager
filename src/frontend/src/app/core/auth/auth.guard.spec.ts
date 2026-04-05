import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { AuthService } from './auth.service';
import { adminGuard, authGuard } from './auth.guard';

describe('auth guards', () => {
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: jasmine.createSpy('isAuthenticated'),
            hasRole: jasmine.createSpy('hasRole')
          }
        }
      ]
    });

    router = TestBed.inject(Router);
  });

  it('redirects unauthenticated users to login with a reason', () => {
    const authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    authService.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard({} as never, { url: '/workspace/tickets' } as never));

    expect(router.serializeUrl(result as never)).toBe('/login?reason=authenticationRequired&returnUrl=%2Fworkspace%2Ftickets');
  });

  it('redirects authenticated non-admin users to access denied', () => {
    const authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    authService.isAuthenticated.and.returnValue(true);
    authService.hasRole.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, { url: '/workspace/users' } as never));

    expect(router.serializeUrl(result as never)).toBe('/workspace/access-denied?from=%2Fworkspace%2Fusers');
  });
});
