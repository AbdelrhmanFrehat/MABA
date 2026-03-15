import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../shared/services/auth.service';
import { LanguageService } from '../../shared/services/language.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        ButtonModule,
        CheckboxModule,
        InputTextModule,
        PasswordModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="auth-page" [dir]="langService.direction">
            <div class="auth-bg-gradient"></div>
            <div class="auth-pattern"></div>
            <div class="auth-floating-shapes">
                <div class="shape shape-1"></div>
                <div class="shape shape-2"></div>
            </div>

            <div class="auth-container">
                <!-- Clickable Logo -->
                <a routerLink="/" class="logo-link">
                    <img src="assets/img/logo.jpeg" alt="MABA Logo" class="logo-img" />
                    <span class="logo-text">MABA</span>
                </a>

                <div class="auth-card">
                    <div class="auth-header">
                        <div class="auth-icon">
                            <i class="pi pi-user"></i>
                        </div>
                        <h1>{{ 'auth.welcome' | translate }}</h1>
                        <p>{{ 'auth.signInToContinue' | translate }}</p>
                    </div>

                    <!-- Session Expired Message -->
                    @if (sessionExpired) {
                        <div class="warning-message">
                            <i class="pi pi-exclamation-triangle"></i>
                            {{ 'messages.sessionExpired' | translate }}
                        </div>
                    }

                    <!-- Error Message -->
                    @if (errorMessage) {
                        <div class="error-message">
                            <i class="pi pi-exclamation-circle"></i>
                            {{ errorMessage }}
                        </div>
                    }

                    <form (ngSubmit)="onSubmit()" class="auth-form">
                        <div class="form-group">
                            <label for="email">
                                <i class="pi pi-envelope"></i>
                                {{ 'auth.email' | translate }} <span class="required">*</span>
                            </label>
                            <input 
                                pInputText 
                                id="email" 
                                type="email" 
                                [placeholder]="'auth.email' | translate" 
                                [(ngModel)]="email"
                                name="email"
                                required
                                autofocus
                                (input)="clearError()"
                            />
                        </div>

                        <div class="form-group">
                            <label for="password">
                                <i class="pi pi-lock"></i>
                                {{ 'auth.password' | translate }} <span class="required">*</span>
                            </label>
                            <p-password 
                                id="password" 
                                [(ngModel)]="password" 
                                name="password"
                                [placeholder]="'auth.password' | translate" 
                                [toggleMask]="true" 
                                styleClass="w-full" 
                                [feedback]="false"
                                required
                                (onInput)="clearError()"
                            ></p-password>
                        </div>

                        <div class="form-options">
                            <div class="remember-me">
                                <p-checkbox [(ngModel)]="rememberMe" name="rememberMe" id="rememberme" binary></p-checkbox>
                                <label for="rememberme">{{ 'auth.rememberMe' | translate }}</label>
                            </div>
                            <a routerLink="/forgot-password" class="forgot-password">{{ 'auth.forgotPassword' | translate }}</a>
                        </div>

                        <p-button 
                            type="submit"
                            [label]="'auth.signIn' | translate" 
                            icon="pi pi-sign-in"
                            iconPos="right"
                            styleClass="submit-button w-full" 
                            [loading]="loading"
                            [disabled]="!email || !password"
                        ></p-button>
                    </form>

                    <!-- Language Switcher -->
                    <div class="lang-switcher">
                        <button 
                            type="button"
                            class="lang-btn"
                            (click)="toggleLanguage()"
                        >
                            <i class="pi pi-globe"></i>
                            {{ langService.language === 'en' ? 'العربية' : 'English' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-accent: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .auth-page {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem;
            position: relative;
            overflow: hidden;
        }

        .auth-bg-gradient {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .auth-pattern {
            position: absolute;
            inset: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.1) 0%, transparent 50%),
                linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px),
                linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px);
            background-size: 100% 100%, 100% 100%, 50px 50px, 50px 50px;
            z-index: 1;
        }

        .auth-floating-shapes {
            position: absolute;
            inset: 0;
            z-index: 2;
            pointer-events: none;
        }

        .shape {
            position: absolute;
            border-radius: 50%;
            filter: blur(60px);
            opacity: 0.4;
        }

        .shape-1 {
            width: 400px;
            height: 400px;
            background: var(--gradient-primary);
            top: -100px;
            right: -100px;
            animation: float 8s ease-in-out infinite;
        }

        .shape-2 {
            width: 300px;
            height: 300px;
            background: var(--gradient-accent);
            bottom: -50px;
            left: -50px;
            animation: float 6s ease-in-out infinite reverse;
        }

        @keyframes float {
            0%, 100% { transform: translateY(0) scale(1); }
            50% { transform: translateY(-30px) scale(1.05); }
        }

        .auth-container {
            width: 100%;
            max-width: 450px;
            position: relative;
            z-index: 10;
            animation: slideUp 0.6s ease-out;
        }

        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Logo Link */
        .logo-link {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
            text-decoration: none;
            margin-bottom: 2rem;
            transition: transform 0.3s ease;
        }

        .logo-link:hover {
            transform: scale(1.05);
        }

        .logo-img {
            width: 50px;
            height: 50px;
            border-radius: 12px;
            object-fit: cover;
            box-shadow: 0 4px 15px rgba(0,0,0,0.3);
        }

        .logo-text {
            font-size: 2rem;
            font-weight: 800;
            color: white;
            text-shadow: 0 2px 10px rgba(0,0,0,0.3);
        }

        .auth-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(20px);
            border-radius: 24px;
            padding: 3rem;
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
        }

        .auth-header {
            text-align: center;
            margin-bottom: 2rem;
        }

        .auth-icon {
            width: 80px;
            height: 80px;
            background: var(--gradient-primary);
            border-radius: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
            font-size: 2rem;
            color: white;
            box-shadow: var(--shadow-glow);
        }

        .auth-header h1 {
            font-size: 2rem;
            font-weight: 800;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .auth-header p {
            color: #6c757d;
            font-size: 1rem;
        }

        .auth-form {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .form-group {
            display: flex;
            flex-direction: column;
        }

        .form-group label {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-weight: 600;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .form-group label i {
            color: var(--color-primary);
        }

        .required {
            color: #e53e3e;
        }

        .form-group input {
            border-radius: 12px !important;
            padding: 0.875rem 1rem !important;
        }

        :host ::ng-deep .p-password {
            width: 100%;
        }

        :host ::ng-deep .p-password input {
            width: 100%;
            border-radius: 12px !important;
            padding: 0.875rem 1rem !important;
        }

        .form-options {
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .remember-me {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .remember-me label {
            color: #4a5568;
            cursor: pointer;
        }

        .forgot-password {
            color: var(--color-primary);
            text-decoration: none;
            font-weight: 600;
            transition: color 0.3s ease;
        }

        .forgot-password:hover {
            color: var(--color-secondary);
        }

        .error-message,
        .warning-message {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem;
            border-radius: 12px;
            font-size: 0.875rem;
            margin-bottom: 1rem;
        }

        .error-message {
            background: rgba(229, 62, 62, 0.1);
            border: 1px solid rgba(229, 62, 62, 0.3);
            color: #e53e3e;
        }

        .warning-message {
            background: rgba(237, 137, 54, 0.1);
            border: 1px solid rgba(237, 137, 54, 0.3);
            color: #dd6b20;
        }

        :host ::ng-deep .submit-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 2rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: var(--shadow-glow) !important;
            margin-top: 0.5rem;
        }

        :host ::ng-deep .submit-button:hover:not(:disabled) {
            transform: translateY(-2px) !important;
        }

        .lang-switcher {
            text-align: center;
            margin-top: 1.5rem;
        }

        .lang-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            background: transparent;
            border: 1px solid #e9ecef;
            border-radius: 50px;
            color: #6c757d;
            font-weight: 500;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .lang-btn:hover {
            background: rgba(102, 126, 234, 0.1);
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        @media (max-width: 480px) {
            .auth-card {
                padding: 2rem;
            }
            .auth-icon {
                width: 60px;
                height: 60px;
                font-size: 1.5rem;
            }
            .auth-header h1 {
                font-size: 1.75rem;
            }
            .form-options {
                flex-direction: column;
                align-items: flex-start;
            }
        }
    `]
})
export class Login implements OnInit {
    email: string = '';
    password: string = '';
    rememberMe: boolean = false;
    loading: boolean = false;
    sessionExpired: boolean = false;
    errorMessage: string = '';

    private returnUrl: string = '/';

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService,
        private translateService: TranslateService,
        public langService: LanguageService
    ) {}

    ngOnInit() {
        // Get return URL from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        
        // Check if session expired
        this.sessionExpired = this.route.snapshot.queryParams['sessionExpired'] === 'true';

        // If already logged in, redirect (admin/store owner → admin panel)
        if (this.authService.authenticated) {
            if (this.authService.hasAnyRole(['Admin', 'StoreOwner'])) {
                this.router.navigate(['/admin']);
            } else {
                this.router.navigate([this.returnUrl]);
            }
        }
    }

    clearError() {
        this.errorMessage = '';
        this.sessionExpired = false;
    }

    onSubmit() {
        if (!this.email || !this.password) {
            return;
        }

        this.loading = true;
        this.errorMessage = '';

        this.authService.login({ email: this.email, password: this.password }, this.rememberMe).subscribe({
            next: (response) => {
                const user = response.user;
                const role = user?.roles?.[0] || '';
                
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('auth.loginSuccessWithRole', { 
                        name: user?.fullName || this.email,
                        role: role
                    }),
                    life: 3000
                });
                // Admin or StoreOwner → redirect to admin panel
                const targetUrl = this.authService.hasAnyRole(['Admin', 'StoreOwner']) ? '/admin' : this.returnUrl;
                setTimeout(() => {
                    this.router.navigate([targetUrl]);
                }, 500);
            },
            error: (error) => {
                this.loading = false;
                
                // Handle different error types
                if (error.status === 401) {
                    this.errorMessage = this.translateService.instant('auth.loginError');
                } else if (error.status === 0) {
                    this.errorMessage = this.translateService.instant('messages.networkError');
                } else {
                    this.errorMessage = error.error?.message || this.translateService.instant('auth.loginError');
                }
                
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.errorMessage,
                    life: 5000
                });
            }
        });
    }

    toggleLanguage() {
        this.langService.toggleLanguage();
    }
}
