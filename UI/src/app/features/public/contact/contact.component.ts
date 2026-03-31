import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-contact',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <div class="contact-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-floating-shapes">
                    <div class="shape shape-1"></div>
                    <div class="shape shape-2"></div>
                </div>
                <div class="hero-content">
                    <span class="hero-badge animate-fade-in">
                        <i class="pi pi-envelope"></i>
                        {{ 'contact.getInTouch' | translate }}
                    </span>
                    <h1 class="hero-title animate-fade-in-delay">
                        {{ 'contact.hereToHelp' | translate }}
                    </h1>
                    <p class="hero-description animate-fade-in-delay-2">
                        {{ 'contact.description' | translate }}
                    </p>
                </div>
            </section>

            <!-- Map + Contact Form Section (same row) -->
            <section class="map-form-section">
                <div class="container">
                    <div class="map-form-grid">
                        <!-- Map Column: live map for the new location -->
                        <div class="map-wrapper map-static">
                            <iframe
                                title="MABA Location Map"
                                src="https://maps.google.com/maps?q=31.9233856,35.2081461&z=17&output=embed"
                                width="100%"
                                height="100%"
                                style="border:0"
                                loading="lazy"
                                referrerpolicy="no-referrer-when-downgrade">
                            </iframe>
                            <a
                                href="https://maps.app.goo.gl/K37Par84yA9Y6oWe9"
                                target="_blank"
                                rel="noopener noreferrer"
                                class="map-static-overlay"
                                title="فتح في Google Maps / Open in Google Maps">
                                <i class="pi pi-map-marker"></i>
                                {{ 'contact.openInGoogleMaps' | translate }}
                            </a>
                        </div>
                        <!-- Form Column -->
                        <div class="form-column">
                            <div class="form-header">
                                <span class="section-badge">{{ 'contact.sendMessage' | translate }}</span>
                                <h2 class="section-title">{{ 'contact.sendUsMessage' | translate }}</h2>
                                <p class="section-description">{{ 'contact.weWillReply' | translate }}</p>
                            </div>
                            <form class="contact-form" (ngSubmit)="submitForm()">
                                <div class="form-row">
                                    <div class="form-group">
                                        <label>
                                            <i class="pi pi-user"></i>
                                            {{ 'contact.fullName' | translate }}
                                        </label>
                                        <input 
                                            type="text" 
                                            pInputText 
                                            [(ngModel)]="formData.name"
                                            name="name"
                                            [placeholder]="'contact.enterName' | translate"
                                            required
                                        />
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            <i class="pi pi-envelope"></i>
                                            {{ 'contact.emailAddress' | translate }}
                                        </label>
                                        <input 
                                            type="email" 
                                            pInputText 
                                            [(ngModel)]="formData.email"
                                            name="email"
                                            [placeholder]="'contact.enterEmail' | translate"
                                            required
                                        />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label>
                                        <i class="pi pi-tag"></i>
                                        {{ 'contact.subject' | translate }}
                                    </label>
                                    <input 
                                        type="text" 
                                        pInputText 
                                        [(ngModel)]="formData.subject"
                                        name="subject"
                                        [placeholder]="'contact.messageSubject' | translate"
                                        required
                                    />
                                </div>
                                <div class="form-group">
                                    <label>
                                        <i class="pi pi-align-left"></i>
                                        {{ 'contact.message' | translate }}
                                    </label>
                                    <textarea 
                                        pTextarea 
                                        [(ngModel)]="formData.message"
                                        name="message"
                                        rows="6"
                                        [placeholder]="'contact.writeMessage' | translate"
                                        required
                                    ></textarea>
                                </div>
                                <button type="submit" class="submit-button" [disabled]="sending">
                                    <i *ngIf="sending" class="pi pi-spin pi-spinner"></i>
                                    <i *ngIf="!sending" class="pi pi-send"></i>
                                    {{ 'contact.send' | translate }}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Contact Info + Social (below map & form) - new design -->
            <section class="contact-info-section">
                <div class="container">
                    <div class="contact-info-cards">
                        <div class="contact-info-card" *ngFor="let info of contactInfo">
                            <div class="contact-info-icon">
                                <i [class]="info.icon"></i>
                            </div>
                            <div class="contact-info-content">
                                <h4>{{ info.titleKey | translate }}</h4>
                                <p class="contact-info-value">{{ (languageService.language === 'ar' ? info.value : (info.valueEn || info.value)) }}</p>
                                <a *ngIf="info.link" [href]="info.link" class="contact-info-action" target="_blank" rel="noopener">
                                    {{ 'contact.contactNow' | translate }}
                                    <i class="pi pi-arrow-right"></i>
                                </a>
                            </div>
                        </div>
                    </div>
                    <div class="social-block">
                        <h3 class="social-block-title">{{ 'contact.followUs' | translate }}</h3>
                        <div class="social-buttons">
                            <a href="https://www.facebook.com/MABA.Engineering.Solutions" target="_blank" rel="noopener" class="social-btn facebook" [attr.aria-label]="'Facebook'">
                                <i class="pi pi-facebook"></i>
                                <span>Facebook</span>
                            </a>
                            <a href="https://www.instagram.com/maba_tech" target="_blank" rel="noopener" class="social-btn instagram" [attr.aria-label]="'Instagram'">
                                <i class="pi pi-instagram"></i>
                                <span>Instagram</span>
                            </a>
                            <a href="https://wa.me/qr/DHGKCLYUCIOUD1" target="_blank" rel="noopener" class="social-btn whatsapp" [attr.aria-label]="'WhatsApp'">
                                <i class="pi pi-whatsapp"></i>
                                <span>WhatsApp</span>
                            </a>
                        </div>
                    </div>
                </div>
            </section>
        </div>
        <p-toast></p-toast>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-secondary: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-accent: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
            
            /* Social Brand Colors */
            --facebook-color: #1877F2;
            --instagram-gradient: linear-gradient(45deg, #f09433 0%,#e6683c 25%,#dc2743 50%,#cc2366 75%,#bc1888 100%);
            --whatsapp-color: #25D366;
        }

        .contact-page {
            width: 100%;
            overflow-x: hidden;
            background: #fafbfc;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            min-height: 50vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 6rem 2rem;
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
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%),
                linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px),
                linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px);
            background-size: 100% 100%, 100% 100%, 60px 60px, 60px 60px;
            z-index: 1;
        }

        .hero-floating-shapes {
            position: absolute;
            inset: 0;
            z-index: 2;
            pointer-events: none;
        }

        .shape {
            position: absolute;
            border-radius: 50%;
            filter: blur(60px);
            opacity: 0.5;
        }

        .shape-1 {
            width: 300px;
            height: 300px;
            background: var(--gradient-primary);
            top: -50px;
            right: -50px;
            animation: float 8s ease-in-out infinite;
        }

        .shape-2 {
            width: 200px;
            height: 200px;
            background: var(--gradient-accent);
            bottom: -30px;
            left: -30px;
            animation: float 6s ease-in-out infinite reverse;
        }

        @keyframes float {
            0%, 100% { transform: translateY(0) scale(1); }
            50% { transform: translateY(-30px) scale(1.05); }
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
            max-width: 700px;
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
            font-weight: 500;
            margin-bottom: 2rem;
        }

        .hero-title {
            font-size: clamp(2rem, 5vw, 3.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1rem;
            line-height: 1.2;
        }

        .hero-description {
            font-size: 1.2rem;
            color: rgba(255,255,255,0.8);
            line-height: 1.7;
        }

        .animate-fade-in { animation: fadeIn 0.8s ease-out forwards; }
        .animate-fade-in-delay { animation: fadeIn 0.8s ease-out 0.2s forwards; opacity: 0; }
        .animate-fade-in-delay-2 { animation: fadeIn 0.8s ease-out 0.4s forwards; opacity: 0; }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }

        /* ============ MAP + FORM SECTION (same row) ============ */
        .map-form-section {
            padding: 4rem 2rem;
            background: white;
        }

        .map-form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
            align-items: start;
            max-width: 1400px;
            margin: 0 auto;
        }

        .map-form-grid .map-wrapper {
            position: sticky;
            top: 1rem;
            border-radius: 24px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0,0,0,0.1);
            min-height: 500px;
        }

        .map-form-grid .map-wrapper iframe {
            display: block;
            min-height: 500px;
        }

        /* Static map: no interaction, click opens Google Maps */
        .map-wrapper.map-static {
            min-height: 320px;
        }
        .map-static .map-static-link {
            display: block;
            position: relative;
            width: 100%;
            height: 100%;
            min-height: 320px;
            cursor: pointer;
            text-decoration: none;
            color: inherit;
        }
        .map-static .map-static-img {
            display: block;
            width: 100%;
            height: 100%;
            min-height: 320px;
            object-fit: cover;
            user-select: none;
            -webkit-user-select: none;
            -webkit-user-drag: none;
            pointer-events: none;
        }
        .map-static .map-static-overlay {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            padding: 1rem 1.5rem;
            background: linear-gradient(transparent, rgba(0,0,0,0.7));
            color: white;
            font-weight: 600;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            pointer-events: none;
        }
        .map-static .map-static-overlay i {
            font-size: 1.25rem;
        }

        .map-form-grid .form-column {
            min-width: 0;
        }

        .map-form-grid .form-header {
            text-align: left;
            margin-bottom: 1.5rem;
        }

        [dir="rtl"] .map-form-grid .form-header {
            text-align: right;
        }

        .section-badge {
            display: inline-block;
            padding: 0.5rem 1.25rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.875rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 1rem;
        }

        .section-title {
            font-size: 2.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .section-description {
            color: #6c757d;
            font-size: 1.1rem;
        }

        .contact-form {
            background: #f8f9fa;
            padding: 3rem;
            border-radius: 24px;
        }

        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1.5rem;
        }

        .form-group {
            margin-bottom: 1.5rem;
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

        .form-group input,
        .form-group textarea {
            width: 100%;
            padding: 1rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            font-size: 1rem;
            transition: all 0.3s;
        }

        .form-group input:focus,
        .form-group textarea:focus {
            outline: none;
            border-color: var(--color-primary);
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
        }

        .submit-button {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
            padding: 1.25rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 50px;
            color: white;
            font-size: 1.1rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
            box-shadow: var(--shadow-glow);
        }

        .submit-button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 15px 40px rgba(102, 126, 234, 0.5);
        }

        .submit-button:disabled {
            opacity: 0.7;
            cursor: not-allowed;
        }

        /* ============ CONTACT INFO + SOCIAL (below map & form) ============ */
        .contact-info-section {
            padding: 4rem 2rem;
            background: linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%);
        }

        .contact-info-cards {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 1.5rem;
            margin-bottom: 3rem;
        }

        .contact-info-card {
            display: flex;
            align-items: flex-start;
            gap: 1.25rem;
            padding: 1.5rem 1.75rem;
            background: white;
            border-radius: 16px;
            box-shadow: 0 2px 12px rgba(0,0,0,0.06);
            border: 1px solid rgba(0,0,0,0.06);
            transition: all 0.3s ease;
        }

        .contact-info-card:hover {
            box-shadow: 0 8px 24px rgba(102, 126, 234, 0.12);
            border-color: rgba(102, 126, 234, 0.2);
        }

        .contact-info-icon {
            flex-shrink: 0;
            align-self: flex-start;
            width: 52px;
            height: 52px;
            min-width: 52px;
            min-height: 52px;
            background: var(--gradient-primary);
            border-radius: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.4rem;
            color: white;
        }

        .contact-info-content {
            flex: 1;
            min-width: 0;
        }

        .contact-info-content h4 {
            font-size: 1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0 0 0.5rem 0;
        }

        .contact-info-value {
            font-size: 0.95rem;
            color: #495057;
            line-height: 1.5;
            margin: 0 0 0.5rem 0;
        }

        .contact-info-action {
            display: inline-flex;
            align-items: center;
            gap: 0.35rem;
            font-size: 0.9rem;
            font-weight: 600;
            color: var(--color-primary);
            text-decoration: none;
            transition: gap 0.2s ease;
        }

        .contact-info-action:hover {
            gap: 0.5rem;
        }

        .social-block {
            text-align: center;
        }

        .social-block-title {
            font-size: 1.35rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0 0 1.5rem 0;
        }

        .social-buttons {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 1rem;
        }

        .social-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.6rem;
            padding: 0.85rem 1.5rem;
            border-radius: 50px;
            font-size: 0.95rem;
            font-weight: 600;
            color: white;
            text-decoration: none;
            box-shadow: 0 4px 14px rgba(0,0,0,0.15);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .social-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0,0,0,0.2);
        }

        .social-btn i {
            font-size: 1.2rem;
        }

        .social-btn.facebook {
            background: var(--facebook-color);
        }

        .social-btn.instagram {
            background: linear-gradient(135deg, #f09433 0%, #e6683c 50%, #dc2743 100%);
        }

        .social-btn.whatsapp {
            background: var(--whatsapp-color);
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 992px) {
            .contact-info-cards {
                grid-template-columns: 1fr;
            }
            .map-form-grid {
                grid-template-columns: 1fr;
            }
            .map-form-grid .map-wrapper {
                position: static;
                min-height: 400px;
            }
            .map-form-grid .map-wrapper.map-static {
                min-height: 400px;
            }
            .map-static .map-static-link,
            .map-static .map-static-img {
                min-height: 400px;
            }
            .map-form-grid .map-wrapper iframe {
                min-height: 400px;
            }
            .form-row {
                grid-template-columns: 1fr;
            }
        }

        @media (max-width: 768px) {
            .hero-section {
                padding: 4rem 1.5rem;
                min-height: 40vh;
            }
            .contact-form {
                padding: 2rem;
            }
            .section-title {
                font-size: 2rem;
            }
            .contact-info-section {
                padding: 3rem 1rem;
            }
            .contact-info-card {
                flex-direction: column;
                text-align: center;
            }
            .contact-info-icon {
                margin: 0 auto;
            }
            .social-buttons {
                flex-direction: column;
            }
            .social-btn {
                justify-content: center;
            }
        }
    `]
})
export class ContactComponent {
    public languageService = inject(LanguageService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    sending = false;
    formData = {
        name: '',
        email: '',
        subject: '',
        message: ''
    };

    contactInfo = [
        {
            icon: 'pi pi-envelope',
            titleKey: 'contact.email',
            value: 'mabaengsol@gmail.com',
            link: 'mailto:mabaengsol@gmail.com'
        },
        {
            icon: 'pi pi-phone',
            titleKey: 'contact.phone',
            value: '0598595969',
            link: 'tel:+970598595969'
        },
        {
            icon: 'pi pi-map-marker',
            titleKey: 'contact.address',
            value: 'عمارة الارسال، شارع الإذاعة، البيرة',
            valueEn: 'Al-Irsal Building, Al-Ithaa Street, Al-Bireh',
            link: 'https://maps.app.goo.gl/K37Par84yA9Y6oWe9'
        }
    ];

    submitForm() {
        if (!this.formData.name || !this.formData.email || !this.formData.message) {
            return;
        }

        this.sending = true;
        
        setTimeout(() => {
            this.sending = false;
            this.messageService.add({
                severity: 'success',
                summary: this.translate.instant('contact.messageSent'),
                detail: this.translate.instant('contact.thankYou')
            });
            this.formData = { name: '', email: '', subject: '', message: '' };
        }, 1500);
    }
}
