import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';
import { AuthService } from '../services/auth.service';

function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return true;
    const payload = JSON.parse(atob(parts[1]));
    if (!payload.exp) return false;
    const exp = payload.exp * 1000; // Convert to milliseconds
    return Date.now() >= exp;
  } catch {
    return true; // Treat invalid tokens as expired
  }
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = localStorage.getItem('token') ?? null;

  if (token) {
    let path = req.url;
    if (path.startsWith(environment.baseUrl)) {
      path = path.substring(environment.baseUrl.length);
    }
    if (path.startsWith('/')) {
      path = path.substring(1);
    }

    const urlPath = path.split('?')[0];
    const isLogoutEndpoint = urlPath === getEndpoint('logout');

    if (!isLogoutEndpoint && isTokenExpired(token)) {
      // Expiration check failed for non-logout endpoint: perform local logout and redirect
      authService.logoutLocal();
      router.navigate(['/']);
      return throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            statusText: 'Unauthorized (Token Expired)',
          }),
      );
    }

    // Attach token to all requests if present and not expired (or if it is logout)
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req);
};
