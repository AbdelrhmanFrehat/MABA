import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { normalizeReturnUrl } from '../utils/url.utils';

/**
 * Guard for protected routes - requires authentication
 */
export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.authenticated) {
        // Check if route requires specific role
        const requiredRoles = route.data?.['roles'] as string[] | undefined;
        if (requiredRoles && requiredRoles.length > 0) {
            if (!authService.hasAnyRole(requiredRoles)) {
                // User doesn't have required role - redirect to access denied
                router.navigate(['/auth/access']);
                return false;
            }
        }

        // Check if route requires specific feature access
        const feature = route.data?.['feature'] as string | undefined;
        if (feature && !authService.canAccessFeature(feature)) {
            router.navigate(['/auth/access']);
            return false;
        }

        return true;
    }

    // Store the attempted URL for redirecting after login (absolute path)
    router.navigate(['/auth/login'], { queryParams: { returnUrl: normalizeReturnUrl(state.url) } });
    return false;
};

/**
 * Guard for login page - redirects to dashboard if already logged in
 */
export const loginGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.authenticated) {
        return true;
    }

    // Already logged in, redirect to dashboard
    router.navigate(['/']);
    return false;
};

/**
 * Guard for admin-only routes
 */
export const adminGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.authenticated) {
        router.navigate(['/auth/login'], { queryParams: { returnUrl: normalizeReturnUrl(state.url) } });
        return false;
    }

    if (!authService.isAdmin()) {
        router.navigate(['/auth/access']);
        return false;
    }

    return true;
};

/**
 * Guard for manager and admin routes
 */
export const managerGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.authenticated) {
        router.navigate(['/auth/login'], { queryParams: { returnUrl: normalizeReturnUrl(state.url) } });
        return false;
    }

    if (!authService.hasAnyRole(['Admin', 'Manager'])) {
        router.navigate(['/auth/access']);
        return false;
    }

    return true;
};

/**
 * Guard for Admin and StoreOwner routes - blocks Buyer from accessing admin panel
 */
export const storeOwnerGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.authenticated) {
        router.navigate(['/auth/login'], { queryParams: { returnUrl: normalizeReturnUrl(state.url) } });
        return false;
    }

    // Only Admin and StoreOwner can access the admin panel
    if (!authService.hasAnyRole(['Admin', 'StoreOwner'])) {
        router.navigate(['/auth/access']);
        return false;
    }

    return true;
};