import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { authGuard, loginGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

function createAuthServiceMock(authenticated: boolean, hasAnyRole = true): AuthService {
    return {
        get authenticated(): boolean {
            return authenticated;
        },
        hasAnyRole: () => hasAnyRole,
        canAccessFeature: () => true
    } as unknown as AuthService;
}

describe('authGuard', () => {
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);
        TestBed.configureTestingModule({
            providers: [
                { provide: Router, useValue: routerSpy },
                { provide: AuthService, useValue: createAuthServiceMock(false) }
            ]
        });
    });

    it('should allow access when authenticated and no roles required', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(true) });
        const route = { data: {} } as unknown as ActivatedRouteSnapshot;
        const state = {} as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => authGuard(route, state));
        expect(result).toBe(true);
        expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should redirect to login when not authenticated', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(false) });
        const route = { data: {} } as unknown as ActivatedRouteSnapshot;
        const state = { url: '/admin' } as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => authGuard(route, state));
        expect(result).toBe(false);
        expect(routerSpy.navigate).toHaveBeenCalledWith(
            ['/auth/login'],
            jasmine.objectContaining({ queryParams: { returnUrl: '/admin' } })
        );
    });

    it('should allow access when user has required role', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(true, true) });
        const route = { data: { roles: ['Admin'] } } as unknown as ActivatedRouteSnapshot;
        const state = {} as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => authGuard(route, state));
        expect(result).toBe(true);
    });

    it('should redirect to access denied when user lacks required role', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(true, false) });
        const route = { data: { roles: ['Admin'] } } as unknown as ActivatedRouteSnapshot;
        const state = {} as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => authGuard(route, state));
        expect(result).toBe(false);
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/access']);
    });
});

describe('loginGuard', () => {
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);
        TestBed.configureTestingModule({
            providers: [
                { provide: Router, useValue: routerSpy },
                { provide: AuthService, useValue: createAuthServiceMock(false) }
            ]
        });
    });

    it('should allow access to login page when not authenticated', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(false) });
        const route = {} as unknown as ActivatedRouteSnapshot;
        const state = {} as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => loginGuard(route, state));
        expect(result).toBe(true);
    });

    it('should redirect to home when already authenticated', () => {
        TestBed.overrideProvider(AuthService, { useValue: createAuthServiceMock(true) });
        const route = {} as unknown as ActivatedRouteSnapshot;
        const state = {} as unknown as import('@angular/router').RouterStateSnapshot;
        const result = TestBed.runInInjectionContext(() => loginGuard(route, state));
        expect(result).toBe(false);
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
    });
});
