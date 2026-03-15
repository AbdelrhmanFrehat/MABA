import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../shared/services/language.service';

export type LegalPageType = 'privacy-policy' | 'terms-of-service' | 'project-terms' | 'confidentiality' | 'service-sla';

const LEGAL_PAGE_CONFIG: Record<LegalPageType, { legalKey: string; sectionKeys: string[] }> = {
    'privacy-policy': {
        legalKey: 'privacyPolicy',
        sectionKeys: [
            'intro',
            'informationWeCollect',
            'howWeUse',
            'fileConfidentiality',
            'dataProtection',
            'thirdPartyServices',
            'cookies',
            'changesToPolicy',
            'contact'
        ]
    },
    'terms-of-service': {
        legalKey: 'termsOfService',
        sectionKeys: [
            'acceptance',
            'useOfWebsite',
            'services',
            'customerFilesMaterials',
            'intellectualProperty',
            'limitationOfLiability',
            'projectAgreements',
            'modifications',
            'contact'
        ]
    },
    'project-terms': {
        legalKey: 'projectTerms',
        sectionKeys: [
            'scopeOfServices',
            'projectRequests',
            'customerResponsibilities',
            'manufacturingTolerances',
            'intellectualProperty',
            'confidentiality',
            'liability',
            'acceptance'
        ]
    },
    'confidentiality': {
        legalKey: 'confidentiality',
        sectionKeys: [
            'intro',
            'confidentialTreatment',
            'restrictedAccess',
            'noPublicDisclosure',
            'intellectualProperty',
            'optionalNda'
        ]
    },
    'service-sla': {
        legalKey: 'serviceSla',
        sectionKeys: [
            'intro',
            'projectRequestReview',
            'quotationProcess',
            'productionTimeline',
            'communication',
            'technicalConsultation',
            'serviceAvailability',
            'support'
        ]
    }
};

@Component({
    selector: 'app-static-legal-page',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="legal-page" [dir]="languageService.direction">
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <h1 class="hero-title">{{ 'legal.' + legalKey + '.title' | translate }}</h1>
                </div>
            </section>
            <section class="content-section">
                <div class="container">
                    <article class="content-card">
                        <p class="subtitle">{{ 'legal.lastUpdated' | translate }}: {{ 'legal.' + legalKey + '.lastUpdated' | translate }}</p>
                        @for (section of sectionKeys; track section) {
                            <div class="legal-section">
                                <h2 class="section-heading">{{ 'legal.' + legalKey + '.sections.' + section + '.title' | translate }}</h2>
                                <p class="section-body">{{ 'legal.' + legalKey + '.sections.' + section + '.body' | translate }}</p>
                            </div>
                        }
                    </article>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
        }
        .legal-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }
        .container {
            max-width: 900px;
            margin: 0 auto;
            padding: 0 1rem;
        }
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
        .hero-title {
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 700;
            color: white;
            margin: 0;
        }
        .content-section {
            padding: 3rem 1rem 4rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }
        .content-card {
            background: white;
            border-radius: 24px;
            padding: 3rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }
        .subtitle {
            font-size: 0.95rem;
            color: #6b7280;
            margin: 0 0 2rem 0;
            padding-bottom: 1.5rem;
            border-bottom: 1px solid #e5e7eb;
        }
        .legal-section {
            margin-bottom: 2.5rem;
        }
        .legal-section:last-child {
            margin-bottom: 0;
        }
        .section-heading {
            font-size: 1.35rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0 0 0.75rem 0;
            position: relative;
            padding-bottom: 0.5rem;
        }
        .section-heading::after {
            content: '';
            position: absolute;
            bottom: 0;
            left: 0;
            width: 48px;
            height: 3px;
            background: var(--gradient-primary);
            border-radius: 3px;
        }
        [dir="rtl"] .section-heading::after {
            left: auto;
            right: 0;
        }
        .section-body {
            font-size: 1.05rem;
            line-height: 1.85;
            color: #4a5568;
            margin: 0;
            white-space: pre-line;
        }
        @media (max-width: 768px) {
            .content-card {
                padding: 1.5rem 1.25rem;
            }
            .content-section {
                padding: 2rem 1rem 3rem;
            }
        }
    `]
})
export class StaticLegalPageComponent {
    languageService = inject(LanguageService);
    private route = inject(ActivatedRoute);

    pageType: LegalPageType = 'privacy-policy';
    legalKey = 'privacyPolicy';
    sectionKeys: string[] = [];

    constructor() {
        this.route.data.subscribe(data => {
            this.pageType = (data['pageType'] || 'privacy-policy') as LegalPageType;
            const config = LEGAL_PAGE_CONFIG[this.pageType];
            this.legalKey = config.legalKey;
            this.sectionKeys = config.sectionKeys;
        });
    }
}
