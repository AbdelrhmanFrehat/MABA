import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthApiService } from '../../../shared/services/auth-api.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        PasswordModule,
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
                        <i class="pi pi-key"></i>
                        {{ languageService.language === 'ar' ? 'تعيين كلمة مرور جديدة' : 'Set New Password' }}
                    </span>
                    <h1 class="hero-title">{{ 'auth.resetPassword.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'auth.resetPassword.description' | translate }}</p>
                </div>
            </section>

            <!-- Form Section -->
            <section class="form-section">
                <div class="container">
                    <div class="auth-card">
                        <form [formGroup]="resetPasswordForm" (ngSubmit)="onSubmit()">
                            <div class="form-group">
                                <label>{{ 'auth.resetPassword.newPassword' | translate }} *</label>
                                <div class="input-wrapper password-wrapper">
                                    <i class="pi pi-lock"></i>
                                    <p-password 
                                        formControlName="newPassword"
                                        [placeholder]="'auth.passwordPlaceholder' | translate"
                                        [toggleMask]="true"
                                        [feedback]="true"
                                        styleClass="password-input">
                                    </p-password>
                                </div>
                                <small *ngIf="resetPasswordForm.get('newPassword')?.hasError('required') && resetPasswordForm.get('newPassword')?.touched" 
                                       class="error-text">
                                    {{ 'auth.passwordRequired' | translate }}
                                </small>
                                <small *ngIf="resetPasswordForm.get('newPassword')?.hasError('minlength') && resetPasswordForm.get('newPassword')?.touched" 
                                       class="error-text">
                                    {{ 'auth.passwordMinLength' | translate }}
                                </small>
                            </div>

                            <div class="form-group">
                                <label>{{ 'auth.resetPassword.confirmPassword' | translate }} *</label>
                                <div class="input-wrapper password-wrapper">
                                    <i class="pi pi-lock"></i>
                                    <p-password 
                                        formControlName="confirmPassword"
                                        [placeholder]="'auth.confirmPasswordPlaceholder' | translate"
                                        [toggleMask]="true"
                                        [feedback]="false"
                                        styleClass="password-input">
                                    </p-password>
                                </div>
                                <small *ngIf="resetPasswordForm.get('confirmPassword')?.hasError('required') && resetPasswordForm.get('confirmPassword')?.touched" 
                                       class="error-text">
                                    {{ 'auth.confirmPasswordRequired' | translate }}
                                </small>
                                <small *ngIf="resetPasswordForm.hasError('passwordMismatch') && resetPasswordForm.get('confirmPassword')?.touched" 
                                       class="error-text">
                                    {{ 'auth.passwordsDoNotMatch' | translate }}
                                </small>
                            </div>

                            <button 
                                type="submit" 
                                class="submit-btn"
                                [disabled]="!resetPasswordForm.valid || submitting">
                                <i *ngIf="submitting" class="pi pi-spin pi-spinner"></i>
                                <i *ngIf="!submitting" class="pi pi-check"></i>
                                {{ 'auth.resetPassword.submit' | translate }}
                            </button>
                        </form>

                        <div *ngIf="passwordReset" class="success-card">
                            <div class="success-icon">
                                <i class="pi pi-check-circle"></i>
                            </div>
                            <h3>{{ 'auth.resetPassword.success' | translate }}</h3>
                            <p>{{ languageService.language === 'ar' 
                                ? 'تم تغيير كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول.'
                                : 'Your password has been changed successfully. You can now login.' }}</p>
                            <button class="login-btn" routerLink="/auth/login">
                                <i class="pi pi-sign-in"></i>
                                {{ 'auth.login' | translate }}
                            </button>
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
        }

        .input-wrapper > i {
            position: absolute;
            left: 1rem;
            top: 50%;
            transform: translateY(-50%);
            color: #6c757d;
            z-index: 10;
        }

        :host ::ng-deep .password-input {
            width: 100%;
        }

        :host ::ng-deep .password-input input {
            width: 100%;
            padding: 1rem 1rem 1rem 3rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            font-size: 1rem;
            transition: all 0.3s;
        }

        :host ::ng-deep .password-input input:focus {
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

        .submit-btn {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            font-size: 1rem;
            cursor: pointer;
            transition: all 0.3s;
            margin-top: 2rem;
        }

        .submit-btn:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        .submit-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        .success-card {
            text-align: center;
            padding: 2rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            border-radius: 16px;
            margin-top: 2rem;
        }

        .success-icon {
            width: 80px;
            height: 80px;
            background: rgba(102, 126, 234, 0.2);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .success-icon i {
            font-size: 2.5rem;
            color: #667eea;
        }

        .success-card h3 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .success-card p {
            color: #6c757d;
            margin-bottom: 1.5rem;
        }

        .login-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .login-btn:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }
    `]
})
export class ResetPasswordComponent implements OnInit {
    resetPasswordForm: FormGroup;
    submitting = false;
    passwordReset = false;
    token = '';

    private formBuilder = inject(FormBuilder);
    private authApiService = inject(AuthApiService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.resetPasswordForm = this.formBuilder.group({
            newPassword: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', [Validators.required]]
        }, { validators: this.passwordMatchValidator });
    }

    ngOnInit() {
        this.token = this.route.snapshot.queryParams['token'] || '';
        if (!this.token) {
            this.messageService.add({
                severity: 'error',
                summary: this.translateService.instant('messages.error'),
                detail: this.translateService.instant('auth.resetPassword.invalidToken')
            });
            this.router.navigate(['/auth/login']);
        }
    }

    passwordMatchValidator(form: FormGroup) {
        const password = form.get('newPassword');
        const confirmPassword = form.get('confirmPassword');
        if (password && confirmPassword && password.value !== confirmPassword.value) {
            confirmPassword.setErrors({ passwordMismatch: true });
            return { passwordMismatch: true };
        }
        return null;
    }

    onSubmit() {
        if (!this.resetPasswordForm.valid || !this.token) return;

        this.submitting = true;
        this.authApiService.resetPassword(this.token, this.resetPasswordForm.get('newPassword')!.value).subscribe({
            next: () => {
                this.passwordReset = true;
                this.submitting = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('auth.resetPassword.success')
                });
            },
            error: () => {
                this.submitting = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('auth.resetPassword.error')
                });
            }
        });
    }
}
