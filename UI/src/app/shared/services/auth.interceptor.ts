import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { normalizeReturnUrl } from '../utils/url.utils';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Get token
    const token = authService.token;

    // Clone request with auth header if token exists
    if (token) {
        req = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                // Clear auth state (token invalid/expired)
                if (typeof localStorage !== 'undefined') {
                    localStorage.removeItem('auth_token');
                    localStorage.removeItem('refresh_token');
                    localStorage.removeItem('auth_user');
                }
                // Do NOT redirect to login when user is browsing public pages.
                // Important: "/" must be exact-match only; using startsWith("/") would match every route.
                const exactPublicPaths = new Set(['/']);
                const publicPrefixPaths = ['/3d-print', '/laser-engraving', '/cnc', '/catalog', '/item/', '/about', '/contact', '/search', '/wishlist', '/compare', '/projects', '/cart'];
                const isPublicBrowsing =
                    exactPublicPaths.has(router.url) ||
                    publicPrefixPaths.some(p => router.url.startsWith(p));
                const alreadyOnLogin = router.url.includes('/auth/login');
                if (!alreadyOnLogin && !isPublicBrowsing) {
                    router.navigate(['/auth/login'], {
                        queryParams: { returnUrl: normalizeReturnUrl(router.url), sessionExpired: true }
                    });
                }
            }
            return throwError(() => error);
        })
    );
};
