import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { AuthApiService } from '../../../shared/services/auth-api.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-check-email',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="check-email-page" [dir]="languageService.direction">
            <div class="card">
                <div class="icon">✉</div>
                <h1>{{ languageService.language === 'ar' ? 'تحقق من بريدك الإلكتروني' : 'Check your email' }}</h1>
                <p>
                    {{ languageService.language === 'ar'
                        ? 'أرسلنا رابط التحقق إلى بريدك الإلكتروني. افتح الرسالة واضغط الرابط لتفعيل الحساب.'
                        : 'We sent a verification link to your email. Open the message and click the link to activate your account.' }}
                </p>
                <p class="email" *ngIf="email">{{ email }}</p>
                <div class="actions">
                    <p-button
                        [label]="languageService.language === 'ar' ? 'إعادة إرسال رابط التحقق' : 'Resend verification email'"
                        [loading]="resending"
                        (onClick)="resendVerification()">
                    </p-button>
                    <p-button
                        [label]="languageService.language === 'ar' ? 'الذهاب لتسجيل الدخول' : 'Go to sign in'"
                        [outlined]="true"
                        routerLink="/auth/login">
                    </p-button>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .check-email-page {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
            background: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
        }
        .card {
            background: rgba(255, 255, 255, 0.96);
            border-radius: 24px;
            padding: 2rem;
            max-width: 520px;
            width: 100%;
            text-align: center;
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
        }
        .icon {
            width: 72px;
            height: 72px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1rem;
            font-size: 2rem;
            color: #667eea;
            background: rgba(102, 126, 234, 0.15);
        }
        .card h1 { margin: 0 0 .75rem; color: #1a1a2e; }
        .card p { margin: 0 0 1rem; color: #5c6574; line-height: 1.5; }
        .email { font-weight: 600; color: #1a1a2e; }
        .actions { display: flex; gap: .75rem; flex-wrap: wrap; justify-content: center; }
    `]
})
export class CheckEmailComponent {
    private route = inject(ActivatedRoute);
    private authApi = inject(AuthApiService);
    private messageService = inject(MessageService);
    public languageService = inject(LanguageService);

    email = this.route.snapshot.queryParamMap.get('email') ?? '';
    resending = false;

    resendVerification() {
        if (!this.email) {
            this.messageService.add({
                severity: 'warn',
                summary: '',
                detail: this.languageService.language === 'ar'
                    ? 'يرجى العودة للتسجيل وإدخال البريد الإلكتروني.'
                    : 'Please go back to register and enter your email.'
            });
            return;
        }

        this.resending = true;
        this.authApi.resendVerification(this.email).subscribe({
            next: (res) => {
                this.resending = false;
                this.messageService.add({
                    severity: 'success',
                    summary: '',
                    detail: res?.message || (this.languageService.language === 'ar'
                        ? 'تم إرسال رابط تحقق جديد.'
                        : 'A new verification link has been sent.')
                });
            },
            error: () => {
                this.resending = false;
                this.messageService.add({
                    severity: 'error',
                    summary: '',
                    detail: this.languageService.language === 'ar'
                        ? 'تعذر إعادة إرسال البريد الآن.'
                        : 'Unable to resend verification email right now.'
                });
            }
        });
    }
}
