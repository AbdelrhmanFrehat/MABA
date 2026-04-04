import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { LanguageService } from '../../../shared/services/language.service';


@Component({
    selector: 'app-public-footer',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, AccordionModule],
    template: `
        <footer class="public-footer" [dir]="languageService.direction">
            <div class="footer-top">
                <div class="footer-container">
                    <!-- Desktop layout (unchanged) -->
                    <div class="footer-desktop">
                        <div class="footer-grid">
                            <div class="footer-brand">
                                <div class="brand-logo">
                                    <img
                                        src="assets/images/maba-hero-logo.png"
                                        alt="MABA"
                                        class="footer-brand-logo-img"
                                        width="2048"
                                        height="1364" />
                                </div>
                                <p class="brand-description">{{ 'footer.aboutDescription' | translate }}</p>
                                <div class="social-links">
                                    <a href="https://www.facebook.com/MABA.Engineering.Solutions" target="_blank" class="social-link facebook" aria-label="Facebook"><i class="pi pi-facebook"></i></a>
                                    <a href="https://www.instagram.com/maba_tech" target="_blank" class="social-link instagram" aria-label="Instagram"><i class="pi pi-instagram"></i></a>
                                    <a href="https://wa.me/qr/DHGKCLYUCIOUD1" target="_blank" class="social-link whatsapp" aria-label="WhatsApp"><i class="pi pi-whatsapp"></i></a>
                                </div>
                            </div>
                            <div class="footer-column">
                                <h3>{{ 'footer.quickLinks' | translate }}</h3>
                                <ul>
                                    <li><a routerLink="/"><i class="pi pi-chevron-right"></i> {{ 'menu.home' | translate }}</a></li>
                                    <li><a routerLink="/catalog"><i class="pi pi-chevron-right"></i> {{ 'menu.catalog' | translate }}</a></li>
                                    <li><a routerLink="/3d-print"><i class="pi pi-chevron-right"></i> {{ 'menu.print3d' | translate }}</a></li>
                                    <li><a routerLink="/contact"><i class="pi pi-chevron-right"></i> {{ 'menu.contact' | translate }}</a></li>
                                </ul>
                            </div>
                            <div class="footer-column">
                                <h3>{{ 'footer.support' | translate }}</h3>
                                <ul>
                                    <li><a routerLink="/help"><i class="pi pi-chevron-right"></i> {{ 'footer.help' | translate }}</a></li>
                                    <li><a routerLink="/shipping"><i class="pi pi-chevron-right"></i> {{ 'footer.shipping' | translate }}</a></li>
                                    <li><a routerLink="/returns"><i class="pi pi-chevron-right"></i> {{ 'footer.returns' | translate }}</a></li>
                                    <li><a routerLink="/faq"><i class="pi pi-chevron-right"></i> {{ 'footer.faq' | translate }}</a></li>
                                </ul>
                            </div>
                            <div class="footer-column">
                                <h3>{{ 'footer.legal' | translate }}</h3>
                                <ul>
                                    <li><a routerLink="/privacy-policy"><i class="pi pi-chevron-right"></i> {{ 'footer.privacy' | translate }}</a></li>
                                    <li><a routerLink="/terms-of-service"><i class="pi pi-chevron-right"></i> {{ 'footer.termsOfService' | translate }}</a></li>
                                    <li><a routerLink="/project-terms"><i class="pi pi-chevron-right"></i> {{ 'footer.projectTerms' | translate }}</a></li>
                                    <li><a routerLink="/confidentiality"><i class="pi pi-chevron-right"></i> {{ 'footer.confidentialityNotice' | translate }}</a></li>
                                    <li><a routerLink="/service-sla"><i class="pi pi-chevron-right"></i> {{ 'footer.serviceSla' | translate }}</a></li>
                                </ul>
                            </div>
                            <div class="footer-column">
                                <h3>{{ 'footer.contact' | translate }}</h3>
                                <div class="contact-info">
                                    <div class="contact-item">
                                        <i class="pi pi-envelope"></i>
                                        <span>mabaengsol&#64;gmail.com</span>
                                    </div>
                                    <div class="contact-item">
                                        <i class="pi pi-phone"></i>
                                        <span>0598595969</span>
                                    </div>
                                    <div class="contact-item">
                                        <i class="pi pi-map-marker"></i>
                                        <span>{{ languageService.language === 'ar' ? 'عمارة الارسال، شارع الإذاعة، البيرة' : 'Al-Irsal Building, Al-Ithaa Street, Al-Bireh' }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Mobile layout: compact header + social + accordion -->
                    <div class="footer-mobile">
                        <div class="footer-mobile-header">
                            <img
                                src="assets/images/maba-hero-logo.png"
                                alt="MABA"
                                class="footer-mobile-logo"
                                width="2048"
                                height="1364" />
                            <div class="footer-mobile-brand">
                                <p class="footer-mobile-tagline">{{ 'footer.aboutDescription' | translate }}</p>
                            </div>
                        </div>
                        <div class="footer-mobile-social">
                            <a href="https://www.facebook.com/MABA.Engineering.Solutions" target="_blank" class="footer-mobile-social-btn" aria-label="Facebook"><i class="pi pi-facebook"></i></a>
                            <a href="https://www.instagram.com/maba_tech" target="_blank" class="footer-mobile-social-btn" aria-label="Instagram"><i class="pi pi-instagram"></i></a>
                            <a href="https://wa.me/qr/DHGKCLYUCIOUD1" target="_blank" class="footer-mobile-social-btn" aria-label="WhatsApp"><i class="pi pi-whatsapp"></i></a>
                        </div>
                        <p-accordion [multiple]="true" class="footer-mobile-accordion">
                            <p-accordion-panel value="quick">
                                <p-accordion-header>{{ 'footer.quickLinks' | translate }}</p-accordion-header>
                                <p-accordion-content>
                                    <nav class="footer-mobile-list">
                                        <a routerLink="/" class="footer-mobile-row"><span>{{ 'menu.home' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/catalog" class="footer-mobile-row"><span>{{ 'menu.catalog' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/3d-print" class="footer-mobile-row"><span>{{ 'menu.print3d' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/contact" class="footer-mobile-row"><span>{{ 'menu.contact' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                    </nav>
                                </p-accordion-content>
                            </p-accordion-panel>
                            <p-accordion-panel value="support">
                                <p-accordion-header>{{ 'footer.support' | translate }}</p-accordion-header>
                                <p-accordion-content>
                                    <nav class="footer-mobile-list">
                                        <a routerLink="/help" class="footer-mobile-row"><span>{{ 'footer.help' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/shipping" class="footer-mobile-row"><span>{{ 'footer.shipping' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/returns" class="footer-mobile-row"><span>{{ 'footer.returns' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/faq" class="footer-mobile-row"><span>{{ 'footer.faq' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                    </nav>
                                </p-accordion-content>
                            </p-accordion-panel>
                            <p-accordion-panel value="legal">
                                <p-accordion-header>{{ 'footer.legal' | translate }}</p-accordion-header>
                                <p-accordion-content>
                                    <nav class="footer-mobile-list">
                                        <a routerLink="/privacy-policy" class="footer-mobile-row"><span>{{ 'footer.privacy' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/terms-of-service" class="footer-mobile-row"><span>{{ 'footer.termsOfService' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/project-terms" class="footer-mobile-row"><span>{{ 'footer.projectTerms' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/confidentiality" class="footer-mobile-row"><span>{{ 'footer.confidentialityNotice' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                        <a routerLink="/service-sla" class="footer-mobile-row"><span>{{ 'footer.serviceSla' | translate }}</span><i class="pi pi-chevron-right"></i></a>
                                    </nav>
                                </p-accordion-content>
                            </p-accordion-panel>
                            <p-accordion-panel value="contact">
                                <p-accordion-header>{{ 'footer.contact' | translate }}</p-accordion-header>
                                <p-accordion-content>
                                    <div class="footer-mobile-contact">
                                        <div class="footer-mobile-contact-row">
                                            <i class="pi pi-envelope"></i>
                                            <a href="mailto:{{ footerEmail }}" class="footer-mobile-contact-text">{{ footerEmail }}</a>
                                            <button type="button" class="footer-mobile-copy" (click)="copyToClipboard(footerEmail)" aria-label="Copy email"><i class="pi pi-copy"></i></button>
                                        </div>
                                        <div class="footer-mobile-contact-row">
                                            <i class="pi pi-phone"></i>
                                            <a href="tel:{{ footerPhone }}" class="footer-mobile-contact-text">{{ footerPhone }}</a>
                                            <button type="button" class="footer-mobile-copy" (click)="copyToClipboard(footerPhone)" aria-label="Copy phone"><i class="pi pi-copy"></i></button>
                                        </div>
                                        <div class="footer-mobile-contact-row footer-mobile-contact-row-address">
                                            <i class="pi pi-map-marker"></i>
                                            <div class="footer-mobile-address-lines">
                                                @for (line of footerAddressLines; track line) {
                                                    <span class="footer-mobile-address-line">{{ line }}</span>
                                                }
                                            </div>
                                            <button type="button" class="footer-mobile-copy" (click)="copyToClipboard(footerAddress)" aria-label="Copy address"><i class="pi pi-copy"></i></button>
                                        </div>
                                    </div>
                                </p-accordion-content>
                            </p-accordion-panel>
                        </p-accordion>
                    </div>
                </div>
            </div>
            <div class="footer-back-to-top">
                <button type="button" class="footer-back-to-top-btn" (click)="scrollToTop()">
                    <i class="pi pi-arrow-up"></i> {{ 'footer.backToTop' | translate }}
                </button>
            </div>
            <div class="footer-bottom">
                <div class="footer-container">
                    <p class="footer-copyright">&copy; {{ currentYear }} MABA Engineering Solutions. {{ 'footer.allRightsReserved' | translate }}</p>
                    <p class="footer-legal" *ngIf="showFooterLegal">
                        <a routerLink="/privacy-policy" class="footer-legal-link">{{ 'footer.privacy' | translate }}</a>
                        <span class="footer-legal-sep">•</span>
                        <a routerLink="/terms-of-service" class="footer-legal-link">{{ 'footer.termsOfService' | translate }}</a>
                        <span class="footer-legal-sep">•</span>
                        <a routerLink="/project-terms" class="footer-legal-link">{{ 'footer.projectTerms' | translate }}</a>
                        <span class="footer-legal-sep">•</span>
                        <a routerLink="/confidentiality" class="footer-legal-link">{{ 'footer.confidentialityNotice' | translate }}</a>
                        <span class="footer-legal-sep">•</span>
                        <a routerLink="/service-sla" class="footer-legal-link">{{ 'footer.serviceSla' | translate }}</a>
                    </p>
                </div>
            </div>
        </footer>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            
            /* Social Brand Colors */
            --facebook-color: #1877F2;
            --instagram-gradient: linear-gradient(45deg, #f09433 0%,#e6683c 25%,#dc2743 50%,#cc2366 75%,#bc1888 100%);
            --whatsapp-color: #25D366;
            --twitter-color: #1DA1F2;
            --linkedin-color: #0A66C2;
            --youtube-color: #FF0000;
        }

        .public-footer {
            background: var(--gradient-dark);
            color: white;
            margin-top: auto;
        }

        .footer-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        .footer-top {
            padding: 4rem 0;
        }

        .footer-mobile {
            display: none;
        }

        .footer-desktop {
            display: block;
        }

        .footer-grid {
            display: grid;
            grid-template-columns: 2fr 1fr 1fr 1fr 1.5fr;
            gap: 3rem;
        }

        /* Brand Section */
        .footer-brand {
            padding-right: 2rem;
        }

        .brand-logo {
            display: flex;
            align-items: center;
            margin-bottom: 1.5rem;
            min-width: 0;
        }

        .footer-brand-logo-img {
            display: block;
            height: 130px;
            width: auto;
            max-height: 130px;
            max-width: min(650px, 100%);
            object-fit: contain;
            object-position: left center;
            flex-shrink: 0;
        }
        [dir="rtl"] .footer-brand-logo-img {
            object-position: right center;
        }

        .brand-description {
            color: rgba(255,255,255,0.7);
            line-height: 1.7;
            margin-bottom: 1.5rem;
        }

        /* Social Links */
        .social-links {
            display: flex;
            gap: 0.75rem;
        }

        .social-link {
            width: 44px;
            height: 44px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.25rem;
            transition: all 0.3s ease;
        }

        .social-link.facebook {
            background: var(--facebook-color);
        }

        .social-link.instagram {
            background: var(--instagram-gradient);
        }

        .social-link.whatsapp {
            background: var(--whatsapp-color);
        }

        .social-link.twitter {
            background: var(--twitter-color);
        }

        .social-link.linkedin {
            background: var(--linkedin-color);
        }

        .social-link.youtube {
            background: var(--youtube-color);
        }

        .social-link:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.3);
        }

        /* Footer Columns */
        .footer-column h3 {
            font-size: 1.1rem;
            font-weight: 700;
            margin-bottom: 1.5rem;
            color: white;
            position: relative;
            padding-bottom: 0.75rem;
        }

        .footer-column h3::after {
            content: '';
            position: absolute;
            bottom: 0;
            left: 0;
            width: 40px;
            height: 3px;
            background: var(--gradient-primary);
            border-radius: 3px;
        }

        [dir="rtl"] .footer-column h3::after {
            left: auto;
            right: 0;
        }

        .footer-column ul {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .footer-column ul li {
            margin-bottom: 0.75rem;
        }

        .footer-column a {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: rgba(255,255,255,0.7);
            text-decoration: none;
            transition: all 0.3s ease;
        }

        .footer-column a i {
            font-size: 0.75rem;
            transition: transform 0.3s ease;
        }

        .footer-column a:hover {
            color: var(--color-primary);
        }

        .footer-column a:hover i {
            transform: translateX(4px);
        }

        [dir="rtl"] .footer-column a:hover i {
            transform: translateX(-4px);
        }

        /* Contact Info */
        .contact-info {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .contact-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            color: rgba(255,255,255,0.7);
        }

        .contact-item i {
            flex-shrink: 0;
            align-self: flex-start;
            width: 36px;
            height: 36px;
            min-width: 36px;
            min-height: 36px;
            background: rgba(102, 126, 234, 0.2);
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--color-primary);
        }

        /* Footer Bottom */
        .footer-bottom {
            padding: 1.5rem 0;
            border-top: 1px solid rgba(255,255,255,0.1);
            text-align: center;
        }

        .footer-bottom p {
            color: rgba(255,255,255,0.5);
            font-size: 0.9rem;
            margin: 0;
        }

        .footer-back-to-top {
            text-align: center;
            padding: 0.75rem 0 0;
        }

        .footer-back-to-top-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.35rem;
            padding: 0.35rem 0.75rem;
            background: none;
            border: none;
            color: rgba(255,255,255,0.5);
            font-size: 0.8rem;
            cursor: pointer;
            transition: color 0.2s, opacity 0.2s;
        }

        .footer-back-to-top-btn:hover {
            color: rgba(255,255,255,0.85);
        }

        .footer-legal {
            margin: 0.5rem 0 0 0 !important;
        }

        .footer-legal-link {
            color: rgba(255,255,255,0.45);
            font-size: 0.85rem;
            text-decoration: none;
            transition: color 0.2s;
        }

        .footer-legal-link:hover {
            color: rgba(255,255,255,0.7);
        }

        .footer-legal-sep {
            margin: 0 0.5rem;
            color: rgba(255,255,255,0.35);
        }

        /* RTL Support */
        [dir="rtl"] .footer-brand {
            padding-right: 0;
            padding-left: 2rem;
        }

        /* Responsive – tablet */
        @media (max-width: 992px) {
            .footer-grid {
                grid-template-columns: 1fr 1fr;
                gap: 2rem;
            }
            .footer-brand {
                grid-column: 1 / -1;
                padding-right: 0;
            }
        }

        /* Mobile footer: compact accordion layout (< 768px) */
        @media (max-width: 768px) {
            .footer-desktop {
                display: none !important;
            }
            .footer-mobile {
                display: block;
            }
            .footer-top {
                padding: 1.5rem 0 2rem;
            }
        }

        /* Mobile-only styles (< 768px): no white card, sections on dark background */
        @media (max-width: 768px) {
            .footer-mobile-header {
                display: flex;
                align-items: center;
                gap: 0.75rem;
                margin-bottom: 0.75rem;
            }
            .footer-mobile-logo {
                width: auto;
                height: 110px;
                max-height: 110px;
                max-width: min(550px, 92vw);
                border-radius: 0;
                object-fit: contain;
                object-position: left center;
                flex-shrink: 0;
            }
            [dir="rtl"] .footer-mobile-logo {
                object-position: right center;
            }
            .footer-mobile-brand { min-width: 0; }
            .footer-mobile-tagline {
                font-size: 0.8rem;
                color: rgba(255,255,255,0.5);
                margin: 0;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }
            .footer-mobile-social {
                display: flex;
                gap: 0.5rem;
                margin-bottom: 1.5rem;
            }
            .footer-mobile-social-btn {
                width: 36px;
                height: 36px;
                border-radius: 50%;
                background: rgba(255,255,255,0.12);
                border: 1px solid rgba(255,255,255,0.15);
                color: rgba(255,255,255,0.85);
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 1rem;
                transition: background 0.2s, border-color 0.2s;
            }
            .footer-mobile-social-btn:hover {
                background: rgba(255,255,255,0.18);
                border-color: rgba(255,255,255,0.25);
                color: white;
            }
            /* Accordion on dark: no white container, subtle dividers */
            .footer-mobile-accordion ::ng-deep .p-accordion {
                background: transparent !important;
                border-radius: 0 !important;
                overflow: visible;
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-panel,
            .footer-mobile-accordion ::ng-deep .p-accordionpanel {
                border: none !important;
                border-radius: 0 !important;
                border-bottom: 1px solid rgba(255,255,255,0.1);
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-panel:last-child,
            .footer-mobile-accordion ::ng-deep .p-accordionpanel:last-child {
                border-bottom: none;
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-header,
            .footer-mobile-accordion ::ng-deep .p-accordionheader {
                background: transparent !important;
                border-radius: 0 !important;
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-header-link {
                padding: 0.65rem 0.5rem 0.65rem 0;
                background: transparent !important;
                border: none !important;
                color: rgba(255,255,255,0.95);
                font-weight: 600;
                font-size: 0.95rem;
                letter-spacing: 0.02em;
                position: relative;
                padding-bottom: 0.5rem;
                margin-bottom: 0.25rem;
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-header-link::after {
                content: '';
                position: absolute;
                bottom: 0;
                left: 0;
                width: 28px;
                height: 2px;
                background: var(--gradient-primary);
                border-radius: 2px;
            }
            [dir="rtl"] .footer-mobile-accordion ::ng-deep .p-accordion-header-link::after {
                left: auto;
                right: 0;
            }
            .footer-mobile-accordion ::ng-deep .p-accordion-content,
            .footer-mobile-accordion ::ng-deep .p-accordioncontent,
            .footer-mobile-accordion ::ng-deep .p-accordioncontent-content {
                padding: 0 0 1rem 0 !important;
                background: transparent !important;
                border: none !important;
                border-radius: 0 !important;
            }
            .footer-mobile-list {
                display: flex;
                flex-direction: column;
                gap: 0;
            }
            .footer-mobile-row {
                display: flex;
                align-items: center;
                justify-content: space-between;
                min-height: 42px;
                padding: 0 0.5rem;
                color: rgba(255,255,255,0.85);
                text-decoration: none;
                font-size: 0.9rem;
                transition: background 0.2s, color 0.2s;
                border-radius: 6px;
            }
            .footer-mobile-row:hover {
                background: rgba(255,255,255,0.06);
                color: white;
            }
            .footer-mobile-row i {
                font-size: 0.7rem;
                color: rgba(255,255,255,0.5);
            }
            [dir="rtl"] .footer-mobile-row i {
                transform: scaleX(-1);
            }
            [dir="rtl"] .footer-mobile-accordion ::ng-deep .p-accordion-toggle-icon {
                transform: scaleX(-1);
            }
            .footer-mobile-contact {
                display: flex;
                flex-direction: column;
                gap: 0.35rem;
            }
            .footer-mobile-contact-row {
                display: flex;
                align-items: flex-start;
                gap: 0.5rem;
                min-height: 42px;
                color: rgba(255,255,255,0.85);
                font-size: 0.875rem;
                padding: 0.25rem 0.5rem 0.25rem 0;
            }
            .footer-mobile-contact-row i.pi-envelope,
            .footer-mobile-contact-row i.pi-phone,
            .footer-mobile-contact-row i.pi-map-marker {
                flex-shrink: 0;
                width: 24px;
                text-align: center;
                color: rgba(255,255,255,0.6);
                margin-top: 2px;
            }
            .footer-mobile-contact-row a.footer-mobile-contact-text {
                color: rgba(255,255,255,0.9);
                text-decoration: none;
                flex: 1;
                min-width: 0;
            }
            .footer-mobile-address-lines {
                display: flex;
                flex-direction: column;
                gap: 0.15rem;
                flex: 1;
                min-width: 0;
            }
            .footer-mobile-address-line {
                color: rgba(255,255,255,0.85);
                line-height: 1.4;
                font-size: 0.875rem;
            }
            .footer-mobile-contact-row.footer-mobile-contact-row-address {
                align-items: flex-start;
            }
            .footer-mobile-contact-row.footer-mobile-contact-row-address .footer-mobile-copy {
                margin-top: 2px;
            }
            .footer-mobile-copy {
                margin-inline-start: auto;
                padding: 0.3rem 0.5rem;
                background: rgba(255,255,255,0.08);
                border: none;
                border-radius: 6px;
                color: rgba(255,255,255,0.7);
                cursor: pointer;
                display: flex;
                align-items: center;
                justify-content: center;
                transition: background 0.2s, color 0.2s;
                flex-shrink: 0;
            }
            .footer-mobile-copy:hover {
                background: rgba(255,255,255,0.14);
                color: white;
            }
            [dir="rtl"] .footer-mobile-copy {
                margin-inline-start: 0;
                margin-inline-end: auto;
            }
            .footer-top {
                padding: 1.25rem 0 1.5rem;
            }
        }

    `]
})
export class PublicFooterComponent {
    currentYear = new Date().getFullYear();
    footerEmail = 'mabaengsol@gmail.com';
    footerPhone = '0598595969';
    footerAddressAr = 'عمارة الارسال، شارع الإذاعة، البيرة';
    footerAddressEn = 'Al-Irsal Building, Al-Ithaa Street, Al-Bireh';
    showFooterLegal = true;

    get footerAddress(): string {
        return this.languageService.language === 'ar' ? this.footerAddressAr : this.footerAddressEn;
    }

    get footerAddressLines(): string[] {
        if (this.languageService.language === 'ar') {
            return ['البيرة', 'شارع الإذاعة', 'عمارة الارسال'];
        }
        return ['Al-Bireh', 'Al-Ithaa Street', 'Al-Irsal Building'];
    }

    constructor(public languageService: LanguageService) {}

    scrollToTop(): void {
        if (typeof window !== 'undefined') {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    copyToClipboard(text: string): void {
        if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
            navigator.clipboard.writeText(text);
        }
    }
}
