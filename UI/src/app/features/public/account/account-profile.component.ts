import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../shared/services/auth.service';
import { AuthApiService } from '../../../shared/services/auth-api.service';
import { ApiService } from '../../../shared/services/api.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-account-profile',
    standalone: true,
    imports: [
        CommonModule,
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
        <div class="profile-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-user-edit"></i>
                        {{ languageService.language === 'ar' ? 'الملف الشخصي' : 'Profile' }}
                    </span>
                    <h1 class="hero-title">{{ 'account.profile.title' | translate }}</h1>
                </div>
            </section>

            <!-- Form Section -->
            <section class="form-section">
                <div class="container">
                    <form [formGroup]="profileForm" (ngSubmit)="onSubmit()">
                        <div class="profile-card">
                            <div class="form-grid">
                                <div class="form-group">
                                    <label>{{ 'account.profile.firstName' | translate }} *</label>
                                    <div class="input-wrapper">
                                        <i class="pi pi-user"></i>
                                        <input 
                                            type="text" 
                                            pInputText
                                            formControlName="firstName"
                                        />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label>{{ 'account.profile.lastName' | translate }} *</label>
                                    <div class="input-wrapper">
                                        <i class="pi pi-user"></i>
                                        <input 
                                            type="text" 
                                            pInputText
                                            formControlName="lastName"
                                        />
                                    </div>
                                </div>
                                <div class="form-group full-width">
                                    <label>{{ 'account.profile.email' | translate }} *</label>
                                    <div class="input-wrapper">
                                        <i class="pi pi-envelope"></i>
                                        <input 
                                            type="email" 
                                            pInputText
                                            formControlName="email"
                                        />
                                    </div>
                                </div>
                                <div class="form-group full-width">
                                    <label>{{ 'account.profile.phone' | translate }}</label>
                                    <div class="input-wrapper">
                                        <i class="pi pi-phone"></i>
                                        <input 
                                            type="tel" 
                                            pInputText
                                            formControlName="phone"
                                        />
                                    </div>
                                </div>
                            </div>

                            <div class="button-group">
                                <button type="button" class="cancel-btn" (click)="resetForm()">
                                    {{ 'common.cancel' | translate }}
                                </button>
                                <button 
                                    type="submit" 
                                    class="submit-btn"
                                    [disabled]="!profileForm.valid || saving">
                                    <i *ngIf="saving" class="pi pi-spin pi-spinner"></i>
                                    <i *ngIf="!saving" class="pi pi-check"></i>
                                    {{ 'common.save' | translate }}
                                </button>
                            </div>
                        </div>
                    </form>
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

        .profile-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 700px;
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
            font-size: clamp(2rem, 4vw, 3rem);
            font-weight: 800;
            color: white;
        }

        /* ============ FORM SECTION ============ */
        .form-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .profile-card {
            background: white;
            border-radius: 24px;
            padding: 3rem 2.5rem;
            box-shadow: 0 20px 60px rgba(0,0,0,0.12);
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1.5rem;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-group.full-width {
            grid-column: 1 / -1;
        }

        .form-group label {
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

        .button-group {
            display: flex;
            justify-content: flex-end;
            gap: 1rem;
            margin-top: 2rem;
            padding-top: 2rem;
            border-top: 1px solid #e9ecef;
        }

        .cancel-btn {
            padding: 1rem 2rem;
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
            display: flex;
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

        .submit-btn:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        .submit-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .form-grid {
                grid-template-columns: 1fr;
            }
            .form-group.full-width {
                grid-column: 1;
            }
        }
    `]
})
export class AccountProfileComponent implements OnInit {
    profileForm: FormGroup;
    saving = false;

    private formBuilder = inject(FormBuilder);
    private authService = inject(AuthService);
    private authApiService = inject(AuthApiService);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.profileForm = this.formBuilder.group({
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            phone: ['']
        });
    }

    ngOnInit() {
        this.loadProfile();
    }

    loadProfile() {
        const user = this.authService.user;
        if (user) {
            const nameParts = (user.fullName || '').split(' ');
            const firstName = nameParts[0] || '';
            const lastName = nameParts.slice(1).join(' ') || '';
            
            this.profileForm.patchValue({
                firstName: firstName,
                lastName: lastName,
                email: user.email || '',
                phone: user.phone || ''
            });
        }
    }

    onSubmit() {
        if (!this.profileForm.valid) return;

        this.saving = true;
        const formValue = this.profileForm.value;
        const updateData = {
            fullName: `${formValue.firstName} ${formValue.lastName}`.trim(),
            email: formValue.email,
            phone: formValue.phone
        };
        this.apiService.put('/users', 'me', updateData).subscribe({
            next: (updatedUser) => {
                this.authService.getCurrentUser().subscribe();
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('account.profile.updated')
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    resetForm() {
        this.loadProfile();
    }
}
