import { Injectable, signal, Inject, PLATFORM_ID, computed } from '@angular/core';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap, catchError, throwError, of } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';

// Re-export for backward compatibility
export type AuthUser = User;
export type LoginResponse = AuthResponse;

// Role constants
export const ROLES = {
    ADMIN: 'Admin',
    MANAGER: 'Manager',
    VIEWER: 'Viewer',
    USER: 'User'
} as const;

export type UserRole = typeof ROLES[keyof typeof ROLES];

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'auth_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';
    private readonly USER_KEY = 'auth_user';

    private currentUser = signal<User | null>(null);
    private isAuthenticated = signal<boolean>(false);
    private isBrowser: boolean;

    // Computed signals for role checks
    readonly isAdmin = computed(() => this.hasRole(ROLES.ADMIN));
    readonly isManager = computed(() => this.hasRole(ROLES.MANAGER));
    readonly isViewer = computed(() => this.hasRole(ROLES.VIEWER));
    readonly isUser = computed(() => this.hasRole(ROLES.USER));

    // Check if user can edit (Admin, Manager, or User - NOT Viewer)
    readonly canEdit = computed(() => {
        const user = this.currentUser();
        if (!user) return false;
        return !this.hasRole(ROLES.VIEWER);
    });

    // Check if user can delete (Admin or Manager)
    readonly canDelete = computed(() => {
        return this.hasAnyRole([ROLES.ADMIN, ROLES.MANAGER]);
    });

    // Check if user can manage users (Admin only)
    readonly canManageUsers = computed(() => {
        return this.hasRole(ROLES.ADMIN);
    });

    // Check if user can access settings (Admin only)
    readonly canAccessSettings = computed(() => {
        return this.hasRole(ROLES.ADMIN);
    });

    get user() {
        return this.currentUser();
    }

    get userSignal() {
        return this.currentUser;
    }

    get authenticated() {
        return this.isAuthenticated();
    }

    get authenticatedSignal() {
        return this.isAuthenticated;
    }

    get token(): string | null {
        if (!this.isBrowser) return null;
        return localStorage.getItem(this.TOKEN_KEY);
    }

    get userRole(): string {
        const user = this.currentUser();
        if (!user) return '';
        return user.roles && user.roles.length > 0 ? user.roles[0] : '';
    }

    constructor(
        private authApi: AuthApiService,
        private router: Router,
        @Inject(PLATFORM_ID) private platformId: Object
    ) {
        this.isBrowser = isPlatformBrowser(this.platformId);
        this.loadStoredAuth();
    }

    private loadStoredAuth(): void {
        if (!this.isBrowser) return;

        const token = localStorage.getItem(this.TOKEN_KEY);
        const userJson = localStorage.getItem(this.USER_KEY);

        if (token && userJson) {
            try {
                const user = JSON.parse(userJson) as User;
                this.currentUser.set(user);
                this.isAuthenticated.set(true);
            } catch {
                this.clearAuth();
            }
        }
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.authApi.register(request).pipe(
            tap(response => {
                this.storeAuth(response, true);
            }),
            catchError(error => {
                return throwError(() => error);
            })
        );
    }

    login(request: LoginRequest, rememberMe: boolean = false): Observable<AuthResponse> {
        return this.authApi.login(request).pipe(
            tap(response => {
                this.storeAuth(response, rememberMe);
            }),
            catchError(error => {
                return throwError(() => error);
            })
        );
    }

    logout(): void {
        this.clearAuth();
        this.router.navigate(['/auth/login']);
    }

    private storeAuth(response: AuthResponse, rememberMe: boolean): void {
        if (!this.isBrowser) return;

        localStorage.setItem(this.TOKEN_KEY, response.token);
        if (response.refreshToken) {
            localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
        }
        if (response.user) {
            localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
            this.currentUser.set(response.user);
        }

        this.isAuthenticated.set(true);
    }

    private clearAuth(): void {
        if (this.isBrowser) {
            localStorage.removeItem(this.TOKEN_KEY);
            localStorage.removeItem(this.REFRESH_TOKEN_KEY);
            localStorage.removeItem(this.USER_KEY);
        }
        this.currentUser.set(null);
        this.isAuthenticated.set(false);
    }

    getCurrentUser(): Observable<User> {
        return this.authApi.getCurrentUser().pipe(
            tap(user => {
                this.currentUser.set(user);
                if (this.isBrowser) {
                    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
                }
            }),
            catchError(error => {
                this.clearAuth();
                return throwError(() => error);
            })
        );
    }

    /**
     * Check if user has a specific role
     */
    hasRole(role: string): boolean {
        const user = this.currentUser();
        if (!user) return false;
        
        // Check roles array
        if (user.roles && user.roles.length > 0) {
            return user.roles.some(r => r.toLowerCase() === role.toLowerCase());
        }
        
        return false;
    }

    /**
     * Check if user has any of the specified roles
     */
    hasAnyRole(roles: string[]): boolean {
        return roles.some(role => this.hasRole(role));
    }

    /**
     * Check if user has all of the specified roles
     */
    hasAllRoles(roles: string[]): boolean {
        return roles.every(role => this.hasRole(role));
    }

    /**
     * Check permission for specific action
     */
    canPerformAction(action: 'create' | 'read' | 'update' | 'delete'): boolean {
        const user = this.currentUser();
        if (!user) return false;

        switch (action) {
            case 'read':
                return true; // All authenticated users can read
            case 'create':
            case 'update':
                return !this.hasRole(ROLES.VIEWER); // Viewers cannot create/update
            case 'delete':
                return this.hasAnyRole([ROLES.ADMIN, ROLES.MANAGER]); // Only Admin/Manager can delete
            default:
                return false;
        }
    }

    /**
     * Check if user can access a specific route/feature
     */
    canAccessFeature(feature: string): boolean {
        const user = this.currentUser();
        if (!user) return false;

        // Admin has full access
        if (this.hasRole(ROLES.ADMIN)) return true;

        // Define feature access by role
        const featureAccess: Record<string, string[]> = {
            'dashboard': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'items': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'categories': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'warehouses': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'suppliers': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'customers': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'purchases': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'sales': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'adjustments': [ROLES.ADMIN, ROLES.MANAGER],
            'movements': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER, ROLES.USER],
            'payments': [ROLES.ADMIN, ROLES.MANAGER],
            'expenses': [ROLES.ADMIN, ROLES.MANAGER],
            'reports': [ROLES.ADMIN, ROLES.MANAGER, ROLES.VIEWER],
            'users': [ROLES.ADMIN],
            'settings': [ROLES.ADMIN]
        };

        const allowedRoles = featureAccess[feature];
        if (!allowedRoles) return false;

        return this.hasAnyRole(allowedRoles);
    }
}
