import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const session = authService.getSession();

  const authenticatedRequest = session
    ? request.clone({
        setHeaders: {
          Authorization: `Bearer ${session.accessToken}`
        }
      })
    : request;

  return next(authenticatedRequest).pipe(
    tap({
      error: (error: unknown) => {
        if (!(error instanceof HttpErrorResponse) || error.status !== 401 || request.url.endsWith('/api/auth/login')) {
          return;
        }

        authService.logout();
        void router.navigate(['/login']);
      }
    })
  );
};
