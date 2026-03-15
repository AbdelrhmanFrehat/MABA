import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../shared/services/language.service';

@Component({
    selector: 'app-error',
    standalone: true,
    imports: [CommonModule, ButtonModule, RouterModule, TranslateModule],
    template: `
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
                        <div class="auth-icon error">
                            <i class="pi pi-exclamation-triangle"></i>
                        </div>
                        <h1>{{ langService.language === 'ar' ? 'حدث خطأ' : 'Error Occurred' }}</h1>
                        <p>{{ langService.language === 'ar' ? 'المورد المطلوب غير متاح' : 'Requested resource is not available' }}</p>
                    </div>

                    <div class="illustration">
                        <div class="illustration-circle">
                            <i class="pi pi-times-circle"></i>
                        </div>
                        <p class="illustration-text">
                            {{ langService.language === 'ar' 
                                ? 'نأسف، حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى لاحقاً.' 
                                : 'Sorry, an unexpected error occurred. Please try again later.' }}
                        </p>
                    </div>

                    <div class="action-buttons">
                        <p-button 
                            [label]="langService.language === 'ar' ? 'العودة للرئيسية' : 'Go to Home'" 
                            routerLink="/" 
                            icon="pi pi-home"
                            styleClass="primary-btn">
                        </p-button>
                        <p-button 
                            [label]="langService.language === 'ar' ? 'إعادة المحاولة' : 'Try Again'" 
                            (click)="goBack()" 
                            icon="pi pi-refresh"
                            styleClass="secondary-btn"
                            [outlined]="true">
                        </p-button>
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
            --gradient-error: linear-gradient(135deg, #fc8181 0%, #e53e3e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --color-error: #e53e3e;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
            --shadow-error: 0 0 40px rgba(229, 62, 62, 0.4);
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
                radial-gradient(circle at 25% 25%, rgba(229, 62, 62, 0.1) 0%, transparent 50%),
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
            background: var(--gradient-error);
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
            max-width: 500px;
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
            text-align: center;
        }

        .auth-header {
            margin-bottom: 2rem;
        }

        .auth-icon {
            width: 80px;
            height: 80px;
            border-radius: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
            font-size: 2rem;
            color: white;
        }

        .auth-icon.error {
            background: var(--gradient-error);
            box-shadow: var(--shadow-error);
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

        .illustration {
            margin: 2rem 0;
        }

        .illustration-circle {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, rgba(229, 62, 62, 0.1) 0%, rgba(229, 62, 62, 0.2) 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .illustration-circle i {
            font-size: 3rem;
            color: var(--color-error);
        }

        .illustration-text {
            color: #6c757d;
            font-size: 0.95rem;
            line-height: 1.6;
            max-width: 300px;
            margin: 0 auto;
        }

        .action-buttons {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }

        :host ::ng-deep .primary-btn {
            background: var(--gradient-error) !important;
            border: none !important;
            padding: 0.875rem 2rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: var(--shadow-error) !important;
        }

        :host ::ng-deep .primary-btn:hover {
            transform: translateY(-2px) !important;
        }

        :host ::ng-deep .secondary-btn {
            color: var(--color-primary) !important;
            border-color: var(--color-primary) !important;
            padding: 0.875rem 2rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
        }

        :host ::ng-deep .secondary-btn:hover {
            background: rgba(102, 126, 234, 0.1) !important;
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
                font-size: 1.5rem;
            }
            .action-buttons {
                flex-direction: column;
            }
        }
    `]
})
export class Error {
    constructor(public langService: LanguageService) {}

    goBack() {
        window.history.back();
    }
}
