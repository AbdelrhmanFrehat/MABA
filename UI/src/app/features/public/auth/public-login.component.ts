import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../shared/services/auth.service';
import { LanguageService } from '../../../shared/services/language.service';
import { sanitizeReturnUrl } from '../../../shared/utils/url.utils';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-public-login',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        TranslateModule,
        InputTextModule,
        PasswordModule,
        ButtonModule,
        CardModule,
        MessageModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <div class="auth-page" [dir]="languageService.direction">
            <div class="auth-bg-gradient"></div>
            <div class="auth-pattern"></div>
            <div class="auth-floating-shapes">
                <div class="shape shape-1"></div>
                <div class="shape shape-2"></div>
            </div>

            <div class="auth-container">
                <div class="auth-card">
                    <div class="auth-header">
                        <div class="auth-icon">
                            <i class="pi pi-user"></i>
                        </div>
                        <h1>{{ 'auth.login' | translate }}</h1>
                        <p>{{ 'auth.signInToContinue' | translate }}</p>
                    </div>

                    <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
                        <div class="form-group">
                            <label for="email">
                                <i class="pi pi-envelope"></i>
                                {{ 'common.email' | translate }} <span class="required">*</span>
                            </label>
                            <input 
                                type="email" 
                                id="email"
                                pInputText
                                formControlName="email"
                                [placeholder]="'auth.emailPlaceholder' | translate"
                                [class.ng-invalid]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched">
                            @if (loginForm.get('email')?.invalid && loginForm.get('email')?.touched) {
                                <small class="p-error">{{ 'validation.required' | translate }}</small>
                            }
                        </div>

                        <div class="form-group">
                            <label for="password">
                                <i class="pi pi-lock"></i>
                                {{ 'common.password' | translate }} <span class="required">*</span>
                            </label>
                            <p-password
                                id="password"
                                formControlName="password"
                                [placeholder]="'auth.passwordPlaceholder' | translate"
                                [feedback]="false"
                                [toggleMask]="true"
                                styleClass="w-full">
                            </p-password>
                            @if (loginForm.get('password')?.invalid && loginForm.get('password')?.touched) {
                                <small class="p-error">{{ 'validation.required' | translate }}</small>
                            }
                        </div>

                        @if (errorMessage) {
                            <div class="error-message">
                                <i class="pi pi-exclamation-circle"></i>
                                {{ errorMessage }}
                            </div>
                        }

                        <p-button 
                            type="submit"
                            [label]="'auth.signIn' | translate"
                            icon="pi pi-sign-in"
                            iconPos="right"
                            [disabled]="loginForm.invalid || loading"
                            [loading]="loading"
                            styleClass="submit-button w-full">
                        </p-button>
                    </form>

                    <div class="auth-footer">
                        <p>
                            {{ 'auth.noAccount' | translate }}
                            <a routerLink="/auth/register" [queryParams]="{ returnUrl: route.snapshot.queryParams['returnUrl'] }">{{ 'auth.register' | translate }}</a>
                        </p>
                        <p class="auth-footer-link">
                            <a routerLink="/auth/forgot-password">{{ 'auth.forgotPasswordLink' | translate }}</a>
                        </p>
                    </div>
                </div>
            </div>
        </div>
        <p-toast></p-toast>
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

        /* Toast success - use blue theme instead of green */
        :host ::ng-deep .p-toast-message-success,
        :host ::ng-deep .p-toast .p-toast-message.p-toast-message-success {
            background: #0066FF !important;
            background-color: #0066FF !important;
            border: 1px solid #0066FF !important;
            border-left: 6px solid #0052CC !important;
            color: #ffffff !important;
        }

        :host ::ng-deep .p-toast-message-success *,
        :host ::ng-deep .p-toast-message-success .p-toast-message-icon,
        :host ::ng-deep .p-toast-message-success .p-toast-close-button,
        :host ::ng-deep .p-toast-message-success .p-toast-summary,
        :host ::ng-deep .p-toast-message-success .p-toast-detail {
            color: #ffffff !important;
        }

        :host ::ng-deep .p-toast-message-success svg {
            fill: #ffffff !important;
            color: #ffffff !important;
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

        .auth-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(20px);
            border-radius: 24px;
            padding: 3rem;
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
        }

        .auth-header {
            text-align: center;
            margin-bottom: 2.5rem;
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

        .error-message {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem;
            background: rgba(229, 62, 62, 0.1);
            border: 1px solid rgba(229, 62, 62, 0.3);
            border-radius: 12px;
            color: #e53e3e;
            font-size: 0.875rem;
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

        :host ::ng-deep .submit-button:hover {
            transform: translateY(-2px) !important;
        }

        .auth-footer {
            text-align: center;
            margin-top: 2rem;
            padding-top: 2rem;
            border-top: 1px solid #e9ecef;
        }

        .auth-footer p {
            color: #6c757d;
        }

        .auth-footer a {
            color: var(--color-primary);
            text-decoration: none;
            font-weight: 700;
            transition: all 0.3s ease;
        }

        .auth-footer a:hover {
            color: var(--color-secondary);
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
        }
    `]
})
export class PublicLoginComponent {
    loginForm: FormGroup;
    loading = false;
    errorMessage = '';

    public languageService = inject(LanguageService);
    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);
    route = inject(ActivatedRoute);
    private messageService = inject(MessageService);

    constructor() {
        this.loginForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]]
        });
    }

    onSubmit() {
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;
        this.errorMessage = '';

        const { email, password } = this.loginForm.value;

        this.authService.login({ email, password }).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.languageService.translate('common.success'),
                    detail: this.languageService.translate('auth.loginSuccess')
                });
                const rawReturnUrl = this.route.snapshot.queryParams['returnUrl'] as string | undefined;
                const returnUrl = sanitizeReturnUrl(rawReturnUrl);
                if (this.authService.hasAnyRole(['Admin', 'StoreOwner'])) {
                    this.router.navigateByUrl(returnUrl === '/' ? '/admin' : returnUrl);
                } else {
                    this.router.navigateByUrl(returnUrl);
                }
            },
            error: (error) => {
                this.loading = false;
                this.errorMessage = error.error?.message || this.languageService.translate('auth.loginError');
            }
        });
    }

}
