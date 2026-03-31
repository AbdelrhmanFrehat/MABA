import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthApiService } from '../../../shared/services/auth-api.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="auth-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-lock"></i>
                        {{ languageService.language === 'ar' ? 'استعادة كلمة المرور' : 'Password Recovery' }}
                    </span>
                    <h1 class="hero-title">{{ 'auth.forgotPassword.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'auth.forgotPassword.description' | translate }}</p>
                </div>
            </section>

            <!-- Form Section -->
            <section class="form-section">
                <div class="container">
                    <div class="auth-card">
                        <form [formGroup]="forgotPasswordForm" (ngSubmit)="onSubmit()">
                            <div class="form-group">
                                <label>{{ 'auth.email' | translate }} *</label>
                                <div class="input-wrapper">
                                    <i class="pi pi-envelope"></i>
                                    <input 
                                        type="email" 
                                        pInputText
                                        formControlName="email"
                                        [placeholder]="'auth.emailPlaceholder' | translate"
                                    />
                                </div>
                                <small *ngIf="forgotPasswordForm.get('email')?.hasError('required') && forgotPasswordForm.get('email')?.touched" 
                                       class="error-text">
                                    {{ 'auth.emailRequired' | translate }}
                                </small>
                                <small *ngIf="forgotPasswordForm.get('email')?.hasError('email') && forgotPasswordForm.get('email')?.touched" 
                                       class="error-text">
                                    {{ 'auth.emailInvalid' | translate }}
                                </small>
                            </div>

                            <div class="button-group">
                                <button type="button" class="cancel-btn" routerLink="/auth/login">
                                    {{ 'common.cancel' | translate }}
                                </button>
                                <button 
                                    type="submit" 
                                    class="submit-btn"
                                    [disabled]="!forgotPasswordForm.valid || submitting">
                                    <i *ngIf="submitting" class="pi pi-spin pi-spinner"></i>
                                    <i *ngIf="!submitting" class="pi pi-send"></i>
                                    {{ 'auth.forgotPassword.submit' | translate }}
                                </button>
                            </div>
                        </form>

                        <div *ngIf="emailSent" class="success-message">
                            <i class="pi pi-check-circle"></i>
                            <span>{{ 'auth.forgotPassword.emailSent' | translate }}</span>
                        </div>

                        <div class="back-to-login">
                            <a routerLink="/auth/login">
                                <i class="pi pi-arrow-left"></i>
                                {{ languageService.language === 'ar' ? 'العودة لتسجيل الدخول' : 'Back to Login' }}
                            </a>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .auth-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 500px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            padding: 4rem 2rem;
            overflow: hidden;
        }

        .hero-bg-gradient {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            z-index: 1;
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: rgba(255,255,255,0.1);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
            border-radius: 50px;
            color: white;
            font-size: 0.875rem;
            margin-bottom: 1.5rem;
        }

        .hero-title {
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 0.75rem;
        }

        .hero-subtitle {
            color: rgba(255,255,255,0.7);
            font-size: 1rem;
            max-width: 400px;
            margin: 0 auto;
        }

        /* ============ FORM SECTION ============ */
        .form-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .auth-card {
            background: white;
            border-radius: 24px;
            padding: 3rem 2.5rem;
            box-shadow: 0 20px 60px rgba(0,0,0,0.12);
        }

        .form-group {
            margin-bottom: 1.5rem;
        }

        .form-group label {
            display: block;
            margin-bottom: 0.5rem;
            font-weight: 600;
            color: #1a1a2e;
        }

        .input-wrapper {
            position: relative;
            display: flex;
            align-items: center;
        }

        .input-wrapper i {
            position: absolute;
            left: 1rem;
            color: #6c757d;
        }

        .input-wrapper input {
            width: 100%;
            padding: 1rem 1rem 1rem 3rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            font-size: 1rem;
            transition: all 0.3s;
        }

        .input-wrapper input:focus {
            outline: none;
            border-color: var(--color-primary);
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
        }

        .error-text {
            display: block;
            color: #ef4444;
            font-size: 0.85rem;
            margin-top: 0.5rem;
        }

        .button-group {
            display: flex;
            gap: 1rem;
            margin-top: 2rem;
        }

        .cancel-btn {
            flex: 1;
            padding: 1rem 1.5rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .cancel-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .submit-btn {
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 1rem 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .submit-btn:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        .submit-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        .success-message {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1.25rem;
            background: rgba(102, 126, 234, 0.1);
            border: 1px solid rgba(118, 75, 162, 0.3);
            border-radius: 12px;
            margin-top: 2rem;
            color: #667eea;
        }

        .success-message i {
            font-size: 1.25rem;
        }

        .back-to-login {
            text-align: center;
            margin-top: 2rem;
            padding-top: 2rem;
            border-top: 1px solid #e9ecef;
        }

        .back-to-login a {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            color: var(--color-primary);
            text-decoration: none;
            font-weight: 600;
            transition: color 0.3s;
        }

        .back-to-login a:hover {
            text-decoration: underline;
        }
    `]
})
export class ForgotPasswordComponent {
    forgotPasswordForm: FormGroup;
    submitting = false;
    emailSent = false;

    private formBuilder = inject(FormBuilder);
    private authApiService = inject(AuthApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.forgotPasswordForm = this.formBuilder.group({
            email: ['', [Validators.required, Validators.email]]
        });
    }

    onSubmit() {
        if (!this.forgotPasswordForm.valid) return;

        this.submitting = true;
        this.authApiService.forgotPassword(this.forgotPasswordForm.get('email')!.value).subscribe({
            next: () => {
                this.emailSent = true;
                this.submitting = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('auth.forgotPassword.emailSent')
                });
            },
            error: () => {
                this.submitting = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('auth.forgotPassword.error')
                });
            }
        });
    }
}
