import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { PLATFORM_ID } from '@angular/core';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthApiService } from './auth-api.service';
import { User, AuthResponse } from '../models/auth.model';

describe('AuthService', () => {
    let authService: AuthService;
    let authApiSpy: jasmine.SpyObj<AuthApiService>;
    let routerSpy: jasmine.SpyObj<Router>;

    const mockUser: User = {
        id: 'user-1',
        fullName: 'Test User',
        email: 'test@example.com',
        roles: ['User']
    };

    const mockAuthResponse: AuthResponse = {
        token: 'jwt-token-123',
        refreshToken: 'refresh-token-456',
        expiresAt: new Date().toISOString(),
        user: mockUser
    };

    beforeEach(() => {
        authApiSpy = jasmine.createSpyObj('AuthApiService', ['login', 'register', 'getCurrentUser']);
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                AuthService,
                { provide: AuthApiService, useValue: authApiSpy },
                { provide: Router, useValue: routerSpy },
                { provide: PLATFORM_ID, useValue: 'browser' }
            ]
        });

        authService = TestBed.inject(AuthService);
        // Clear localStorage and reset auth state between tests
        if (typeof localStorage !== 'undefined') {
            localStorage.clear();
        }
        authService.logout();
    });

    afterEach(() => {
        authService.logout();
    });

    it('should be created', () => {
        expect(authService).toBeTruthy();
    });

    it('should not be authenticated initially', () => {
        expect(authService.authenticated).toBe(false);
        expect(authService.user).toBeNull();
    });

    it('should store token and user on successful login', (done) => {
        authApiSpy.login.and.returnValue(of(mockAuthResponse));

        authService.login({ email: 'test@example.com', password: 'pass' }).subscribe({
            next: () => {
                expect(authService.authenticated).toBe(true);
                expect(authService.user?.email).toBe('test@example.com');
                expect(authService.token).toBe('jwt-token-123');
                if (typeof localStorage !== 'undefined') {
                    expect(localStorage.getItem('auth_token')).toBe('jwt-token-123');
                }
                done();
            },
            error: done.fail
        });
    });

    it('should clear auth and navigate to login on logout', () => {
        authApiSpy.login.and.returnValue(of(mockAuthResponse));
        authService.login({ email: 'test@example.com', password: 'pass' }).subscribe();
        expect(authService.authenticated).toBe(true);

        authService.logout();
        expect(authService.authenticated).toBe(false);
        expect(authService.user).toBeNull();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });

    it('should report hasRole correctly after login', (done) => {
        authApiSpy.login.and.returnValue(of(mockAuthResponse));
        authService.login({ email: 'test@example.com', password: 'pass' }).subscribe({
            next: () => {
                expect(authService.hasRole('User')).toBe(true);
                expect(authService.hasRole('Admin')).toBe(false);
                done();
            },
            error: done.fail
        });
    });
});
