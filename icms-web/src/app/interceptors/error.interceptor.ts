import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';

const RETRY_HEADER = 'X-Retry-Auth';

/** Requests that must never trigger a silent refresh loop. */
const isAuthCall = (url: string) =>
  url.includes('/auth/login') ||
  url.includes('/auth/refresh') ||
  url.includes('/auth/logout') ||
  url.includes('/auth/change-password');

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const snack = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isAuthCall(req.url) && !req.headers.has(RETRY_HEADER)) {
        // The access token expired (or a fresh page hit an API before boot
        // restore finished). Rotate the refresh cookie and replay once.
        return from(auth.refresh()).pipe(
          switchMap((refreshed) => {
            if (refreshed) {
              return next(
                req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${auth.getAccessToken()}`,
                    [RETRY_HEADER]: 'true',
                  },
                }),
              );
            }
            auth.clearSession();
            snack.open('Session expired. Please sign in again.', 'Login');
            router.navigate(['/login']);
            return throwError(() => err);
          }),
        );
      }

      if (err.status === 401) {
        // Login/refresh failures or an already-retried request.
        if (err.error?.detail && !err.error?.errors) {
          snack.open(err.error.detail, 'OK');
        }
      } else if (err.status === 403) {
        snack.open('You are not allowed to perform that action.', 'OK');
      } else {
        console.error('API Error Response:', err.error?.detail ?? err.message);
      }
      return throwError(() => err);
    }),
  );
};