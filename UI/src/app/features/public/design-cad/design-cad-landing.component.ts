import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-design-cad-landing',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule],
    template: `
        <div class="design-cad-page" [dir]="languageService.direction">
            <!-- Hero: ONE full-width blueprint canvas, center fade, hidden on mobile -->
            <section class="hero-section relative overflow-hidden">
                <div class="hero-bg"></div>
                <div class="hero-circle hero-circle-1"></div>
                <div class="hero-circle hero-circle-2"></div>

                <!-- ONE blueprint canvas SVG: continuous grid, left gear/right cube, center fade mask -->
                <svg
                    class="pointer-events-none absolute inset-0 hidden h-full w-full lg:block hero-blueprint-svg"
                    viewBox="0 0 1600 600"
                    preserveAspectRatio="xMidYMid slice"
                    fill="none"
                    aria-hidden="true">
                    <defs>
                        <radialGradient id="centerFade" cx="50%" cy="50%" r="60%">
                            <stop offset="0%" stop-color="white" stop-opacity="0"/>
                            <stop offset="35%" stop-color="white" stop-opacity="0"/>
                            <stop offset="65%" stop-color="white" stop-opacity="1"/>
                            <stop offset="100%" stop-color="white" stop-opacity="1"/>
                        </radialGradient>
                        <mask id="maskCenter">
                            <rect width="1600" height="600" fill="url(#centerFade)"/>
                        </mask>
                        <filter id="softGlow" x="-50%" y="-50%" width="200%" height="200%">
                            <feGaussianBlur stdDeviation="0.6" result="blur"/>
                            <feMerge>
                                <feMergeNode in="blur"/>
                                <feMergeNode in="SourceGraphic"/>
                            </feMerge>
                        </filter>
                    </defs>
                    <g mask="url(#maskCenter)" stroke="rgba(140,210,255,0.85)" stroke-width="1.6" filter="url(#softGlow)">
                        <g opacity="0.22">
                            <path d="M80 0V600"/><path d="M160 0V600"/><path d="M240 0V600"/><path d="M320 0V600"/>
                            <path d="M400 0V600"/><path d="M480 0V600"/><path d="M560 0V600"/><path d="M640 0V600"/>
                            <path d="M720 0V600"/><path d="M800 0V600"/><path d="M880 0V600"/><path d="M960 0V600"/>
                            <path d="M1040 0V600"/><path d="M1120 0V600"/><path d="M1200 0V600"/><path d="M1280 0V600"/>
                            <path d="M1360 0V600"/><path d="M1440 0V600"/><path d="M1520 0V600"/>
                            <path d="M0 60H1600"/><path d="M0 120H1600"/><path d="M0 180H1600"/><path d="M0 240H1600"/>
                            <path d="M0 300H1600"/><path d="M0 360H1600"/><path d="M0 420H1600"/><path d="M0 480H1600"/><path d="M0 540H1600"/>
                        </g>
                        <g opacity="0.35">
                            <path d="M0 95H620"/><path d="M980 95H1600"/>
                            <path d="M0 505H640"/><path d="M960 505H1600"/>
                            <path d="M120 0V170"/><path d="M1480 430V600"/>
                        </g>
                        <g transform="translate(-40,0)">
                            <g opacity="0.75">
                                <circle cx="260" cy="230" r="120"/>
                                <circle cx="260" cy="230" r="88"/>
                                <circle cx="260" cy="230" r="18"/>
                                <path d="M260 90V370" opacity="0.35"/>
                                <path d="M120 230H400" opacity="0.35"/>
                                <path d="M170 320c35 25 75 40 120 40 45 0 85-15 120-40" opacity="0.35"/>
                            </g>
                            <g opacity="0.55">
                                <path d="M70 430H460"/>
                                <path d="M70 410V450"/><path d="M460 410V450"/>
                                <path d="M70 430l14-8v16l-14-8z" fill="rgba(140,210,255,0.85)" stroke="none"/>
                                <path d="M460 430l-14-8v16l14-8z" fill="rgba(140,210,255,0.85)" stroke="none"/>
                                <text x="245" y="418" font-size="14" fill="rgba(140,210,255,0.75)" stroke="none" text-anchor="middle">240 mm</text>
                            </g>
                        </g>
                        <g>
                            <g opacity="0.7">
                                <path d="M1320 190l140-80 140 80-140 80-140-80z"/>
                                <path d="M1320 190v170l140 80v-170"/>
                                <path d="M1600 190v170l-140 80"/>
                                <circle cx="1505" cy="420" r="52" opacity="0.35"/>
                                <path d="M1505 350v140M1435 420h140" opacity="0.25"/>
                            </g>
                            <g opacity="0.55">
                                <path d="M1180 520H1540"/>
                                <path d="M1180 500V540"/><path d="M1540 500V540"/>
                                <path d="M1180 520l14-8v16l-14-8z" fill="rgba(140,210,255,0.85)" stroke="none"/>
                                <path d="M1540 520l-14-8v16l14-8z" fill="rgba(140,210,255,0.85)" stroke="none"/>
                                <text x="1360" y="508" font-size="14" fill="rgba(140,210,255,0.75)" stroke="none" text-anchor="middle">360 mm</text>
                            </g>
                        </g>
                    </g>
                </svg>

                <!-- Hero content: centered, on top -->
                <div class="hero-inner relative z-10">
                    <div class="hero-content">
                        <span class="hero-badge">
                            <i class="pi pi-pencil"></i>
                            {{ 'designCad.hero.badge' | translate }}
                        </span>
                        <h1 class="hero-title">{{ 'designCad.hero.title' | translate }}</h1>
                        <p class="hero-description">{{ 'designCad.hero.subtitle' | translate }}</p>
                        <div class="hero-cta">
                            <p-button
                                [label]="'designCad.hero.ctaPrimary' | translate"
                                icon="pi pi-arrow-right"
                                iconPos="right"
                                routerLink="/design-cad/new"
                                severity="primary"
                                styleClass="hero-btn-primary">
                            </p-button>
                            <p-button
                                [label]="'designCad.hero.ctaSecondary' | translate"
                                icon="pi pi-briefcase"
                                [outlined]="true"
                                routerLink="/projects"
                                styleClass="hero-btn-secondary">
                            </p-button>
                        </div>
                    </div>
                </div>
                <button type="button" class="hero-scroll-indicator" (click)="scrollToSection('cards-section')" [attr.aria-label]="'designCad.aria.scrollDown' | translate">
                    <i class="pi pi-chevron-down"></i>
                </button>
            </section>

            <!-- What we offer: premium glass cards, consistent CTA -->
            <section id="cards-section" class="cards-section">
                <div class="container">
                    <header class="section-header">
                        <span class="section-label">{{ 'designCad.cards.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'designCad.cards.title' | translate }}</h2>
                    </header>
                    <div class="cards-grid">
                        @for (card of capabilityCards; track card.id) {
                            <article class="service-card">
                                <div class="card-icon-wrap">
                                    <i [class]="card.icon"></i>
                                </div>
                                <div class="card-body">
                                    <h3 class="card-title">{{ card.titleKey | translate }}</h3>
                                    <p class="card-desc">{{ card.descKey | translate }}</p>
                                    <p-button
                                        [label]="'designCad.cards.ctaLabel' | translate"
                                        icon="pi pi-arrow-right"
                                        iconPos="right"
                                        routerLink="/design-cad/new"
                                        [queryParams]="card.queryParams"
                                        severity="primary"
                                        styleClass="design-cad-card-btn">
                                    </p-button>
                                </div>
                            </article>
                        }
                    </div>
                </div>
            </section>

            <!-- How it works: flowchart (line + nodes + arrows), no cards -->
            <section class="how-section">
                <div class="container">
                    <header class="section-header">
                        <span class="section-label">{{ 'designCad.how.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'designCad.how.title' | translate }}</h2>
                    </header>
                    <div class="how-flow">
                        <div class="how-flow-line" aria-hidden="true"></div>
                        @for (step of howSteps; track step.titleKey; let i = $index) {
                            @if (i > 0) {
                                <span class="how-flow-arrow" aria-hidden="true"><i class="pi pi-chevron-right"></i></span>
                            }
                            <div class="how-flow-node-wrap">
                                <div class="how-flow-node">
                                    <span class="how-flow-num">{{ i + 1 }}</span>
                                    <i [class]="step.icon" class="how-flow-icon"></i>
                                </div>
                                <h4 class="how-flow-title">{{ step.titleKey | translate }}</h4>
                                <p class="how-flow-desc">{{ step.textKey | translate }}</p>
                            </div>
                        }
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --color-primary: #667eea;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }
        .design-cad-page { min-height: 100vh; }
        /* No green: all filled buttons use brand purple */
        :host ::ng-deep .design-cad-page button.p-button:not(.p-button-outlined),
        :host ::ng-deep .design-cad-page .p-button:not(.p-button-outlined),
        :host ::ng-deep .design-cad-page .service-card button.p-button,
        :host ::ng-deep .design-cad-page .service-card .p-button:not(.p-button-outlined) {
            background: #667eea !important;
            background-color: #667eea !important;
            color: white !important;
            border: none !important;
        }

        /* ---------- Hero: 60–70vh desktop, 70–80vh mobile; tight spacing; blueprint overlay ---------- */
        .hero-section {
            position: relative;
            min-height: 65vh;
            display: flex;
            align-items: center;
            justify-content: center;
            overflow: hidden;
        }
        .hero-bg {
            position: absolute;
            inset: 0;
            background: linear-gradient(145deg, #0c1445 0%, #1a1a2e 40%, #16213e 70%, #0f3460 100%);
            z-index: 0;
        }
        .hero-circle {
            position: absolute;
            border-radius: 50%;
            background: rgba(102, 126, 234, 0.18);
            filter: blur(70px);
            z-index: 1;
            pointer-events: none;
        }
        .hero-circle-1 { width: 380px; height: 380px; bottom: -100px; left: -80px; }
        .hero-circle-2 { width: 320px; height: 320px; top: -80px; right: -60px; }
        .hero-blueprint-svg {
            opacity: 0.2;
            z-index: 1;
        }
        .hero-inner {
            position: relative;
            z-index: 10;
            width: 100%;
            max-width: 800px;
            margin: 0 auto;
            padding: 1.5rem 1.25rem;
            text-align: center;
        }
        .hero-content { display: flex; flex-direction: column; align-items: center; gap: 0; }
        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.4rem;
            padding: 0.3rem 0.75rem;
            background: rgba(255,255,255,0.12);
            border-radius: 999px;
            font-size: 0.75rem;
            font-weight: 600;
            letter-spacing: 0.04em;
            text-transform: uppercase;
            color: rgba(255,255,255,0.9);
            margin-bottom: 0.6rem;
        }
        .hero-title {
            font-size: clamp(1.6rem, 3.5vw, 2.25rem);
            font-weight: 700;
            color: rgba(255,255,255,0.98);
            margin: 0 0 0.5rem 0;
            line-height: 1.2;
        }
        .hero-description {
            font-size: 0.95rem;
            color: rgba(255,255,255,0.85);
            margin: 0 0 1.25rem 0;
            line-height: 1.45;
            max-width: 520px;
        }
        .hero-cta {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            justify-content: center;
        }
        :host ::ng-deep .hero-btn-primary .p-button,
        :host ::ng-deep .hero-btn-primary.p-button .p-button {
            background: #667eea !important;
            background-color: #667eea !important;
            border: none !important;
            color: white !important;
            padding: 0.75rem 1.5rem !important;
            font-size: 0.95rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
            box-shadow: 0 4px 20px rgba(102, 126, 234, 0.35) !important;
            transition: all 0.25s ease !important;
        }
        :host ::ng-deep .hero-btn-primary .p-button:hover,
        :host ::ng-deep .hero-btn-primary.p-button .p-button:hover {
            transform: translateY(-1px) !important;
            box-shadow: 0 6px 28px rgba(102, 126, 234, 0.45) !important;
            background: #5a67d8 !important;
            background-color: #5a67d8 !important;
        }
        :host ::ng-deep .hero-btn-secondary .p-button,
        :host ::ng-deep .hero-btn-secondary.p-button .p-button {
            background: transparent !important;
            border: 2px solid rgba(255,255,255,0.45) !important;
            color: white !important;
            padding: 0.75rem 1.5rem !important;
            font-size: 0.95rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
            transition: all 0.25s ease !important;
        }
        :host ::ng-deep .hero-btn-secondary .p-button:hover,
        :host ::ng-deep .hero-btn-secondary.p-button .p-button:hover {
            background: rgba(255,255,255,0.08) !important;
            border-color: rgba(255,255,255,0.7) !important;
        }
        .hero-scroll-indicator {
            position: absolute;
            bottom: 0.75rem;
            left: 50%;
            transform: translateX(-50%);
            width: 36px;
            height: 36px;
            border-radius: 50%;
            border: 2px solid rgba(255,255,255,0.5);
            background: transparent;
            color: #fff;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s;
        }
        .hero-scroll-indicator:hover { background: rgba(255,255,255,0.12); }

        /* ---------- Typography: section label (small uppercase) + section title (larger) ---------- */
        .container { max-width: 1200px; margin: 0 auto; padding: 0 1.25rem; }
        .section-header {
            text-align: center;
            margin-bottom: 2rem;
        }
        .section-label {
            display: block;
            font-size: 0.7rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            text-transform: uppercase;
            color: #a5b4fc;
            margin-bottom: 0.4rem;
        }
        .section-title {
            font-size: 1.65rem;
            font-weight: 700;
            color: white;
            margin: 0;
            line-height: 1.25;
        }

        /* ---------- Cards: premium glass, smaller icon, line-clamp, consistent CTA ---------- */
        .cards-section {
            padding: 3rem 0;
            background: linear-gradient(180deg, #0c1445 0%, #1a1a2e 100%);
        }
        .cards-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 1.25rem;
        }
        .service-card {
            background: rgba(255,255,255,0.05);
            backdrop-filter: blur(14px);
            -webkit-backdrop-filter: blur(14px);
            border-radius: 16px;
            border: 1px solid rgba(255,255,255,0.08);
            box-shadow: 0 4px 24px rgba(0,0,0,0.12);
            display: flex;
            flex-direction: column;
            padding: 1.25rem;
            min-height: 220px;
            transition: all 0.3s ease;
        }
        .service-card:hover {
            background: rgba(255,255,255,0.08);
            border-color: rgba(102, 126, 234, 0.35);
            box-shadow: 0 8px 32px rgba(0,0,0,0.18), 0 0 24px rgba(102, 126, 234, 0.12);
            transform: translateY(-3px);
        }
        .card-icon-wrap {
            width: 44px;
            height: 44px;
            border-radius: 12px;
            background: var(--gradient-primary);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1rem;
            flex-shrink: 0;
        }
        .card-icon-wrap i {
            font-size: 1.25rem;
            color: rgba(255,255,255,0.98);
        }
        .card-body {
            flex: 1;
            display: flex;
            flex-direction: column;
            min-height: 0;
        }
        .card-title {
            font-size: 1rem;
            font-weight: 700;
            color: white;
            margin: 0 0 0.4rem 0;
            line-height: 1.3;
        }
        .card-desc {
            font-size: 0.8rem;
            color: rgba(255,255,255,0.6);
            line-height: 1.45;
            margin: 0 0 1rem 0;
            flex: 1;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }
        :host ::ng-deep .design-cad-card-btn.p-button .p-button,
        :host ::ng-deep .design-cad-card-btn .p-button,
        :host ::ng-deep .service-card .p-button:not(.p-button-outlined),
        :host ::ng-deep .service-card button.p-button {
            background: #667eea !important;
            background-color: #667eea !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            font-size: 0.875rem !important;
            border-radius: 10px !important;
            padding: 0.5rem 1rem !important;
            transition: all 0.2s ease !important;
        }
        :host ::ng-deep .design-cad-card-btn .p-button:hover,
        :host ::ng-deep .service-card .p-button:hover {
            background: #5a67d8 !important;
            background-color: #5a67d8 !important;
            box-shadow: 0 4px 14px rgba(102, 126, 234, 0.35) !important;
        }

        /* ---------- How it works: flowchart (line + circular nodes + arrows), no boxes ---------- */
        .how-section {
            padding: 3rem 0;
            background: linear-gradient(180deg, #1a1a2e 0%, #0c1445 100%);
        }
        .how-section .section-label { color: #a5b4fc; }
        .how-section .section-title { color: white; }
        .how-flow {
            display: flex;
            align-items: flex-start;
            justify-content: center;
            flex-wrap: wrap;
            gap: 0;
            margin-top: 2rem;
            max-width: 880px;
            margin-left: auto;
            margin-right: auto;
            position: relative;
        }
        .how-flow-line {
            position: absolute;
            top: 28px;
            left: 12%;
            right: 12%;
            height: 2px;
            background: rgba(109, 124, 255, 0.4);
            z-index: 0;
            pointer-events: none;
        }
        [dir="rtl"] .how-flow-line { left: 12%; right: 12%; }
        .how-flow-arrow {
            flex-shrink: 0;
            width: 24px;
            height: 24px;
            margin-top: 16px;
            border-radius: 50%;
            background: rgba(109, 124, 255, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1;
        }
        .how-flow-arrow i {
            font-size: 0.65rem;
            color: white;
            font-weight: bold;
        }
        [dir="rtl"] .how-flow-arrow i { transform: scaleX(-1); }
        .how-flow-node-wrap {
            flex: 1 1 0;
            min-width: 0;
            max-width: 240px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            position: relative;
            z-index: 1;
        }
        .how-flow-node {
            width: 56px;
            height: 56px;
            border-radius: 50%;
            background: #6d7cff;
            border: 3px solid #0a0a0a;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            flex-shrink: 0;
        }
        .how-flow-num {
            position: absolute;
            top: -4px;
            right: -4px;
            font-size: 0.6rem;
            font-weight: 700;
            color: #fff;
            background: #0a0a0a;
            border: 1px solid #1a1a2e;
            width: 16px;
            height: 16px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 0 0 1px rgba(0,0,0,0.5);
        }
        .how-flow-icon {
            font-size: 1.25rem;
            color: white;
        }
        .how-flow-title {
            font-size: 0.95rem;
            font-weight: 700;
            color: white;
            margin: 0.75rem 0 0.35rem 0;
            line-height: 1.3;
        }
        .how-flow-desc {
            font-size: 0.8rem;
            color: rgba(255,255,255,0.55);
            line-height: 1.5;
            margin: 0;
            max-width: 100%;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }

        @media (min-width: 1024px) {
            .how-flow-node-wrap { max-width: 260px; }
        }

        /* ---------- Responsive: 1 col mobile, 2 tablet, 4 desktop; no huge gaps ---------- */
        @media (max-width: 1024px) {
            .cards-grid { grid-template-columns: repeat(2, 1fr); }
            .how-flow-node-wrap { max-width: 220px; }
        }
        @media (max-width: 768px) {
            .hero-section { min-height: 72vh; }
            .hero-inner { padding: 1.25rem 1rem; }
            .hero-description { margin-bottom: 1rem; }
            .cards-section { padding: 2rem 0; }
            .cards-grid { grid-template-columns: 1fr; gap: 1rem; }
            .service-card { min-height: 200px; padding: 1.25rem; }
            .section-header { margin-bottom: 1.5rem; }
            .section-title { font-size: 1.4rem; }
            .how-section { padding: 2rem 0; }
            .how-flow { flex-direction: column; align-items: center; gap: 1.25rem; margin-top: 1.5rem; max-width: 100%; }
            .how-flow-line { display: none; }
            .how-flow-arrow { margin-top: 0; transform: rotate(90deg); }
            [dir="rtl"] .how-flow-arrow { transform: rotate(-90deg) scaleX(-1); }
            .how-flow-node-wrap { max-width: 100%; }
        }
        @media (max-width: 480px) {
            .hero-section { min-height: 75vh; }
        }
        @media (max-width: 390px) {
            .how-section .container { padding: 0 1rem; }
            .how-flow-node { width: 48px; height: 48px; border-width: 2px; }
            .how-flow-icon { font-size: 1.1rem; }
            .how-flow-title { font-size: 0.9rem; }
            .how-flow-desc { font-size: 0.78rem; }
        }
    `]
})
export class DesignCadLandingComponent {
    languageService = inject(LanguageService);

    capabilityCards = [
        { id: '1', icon: 'pi pi-pencil', titleKey: 'designCad.cards.customCad.title', descKey: 'designCad.cards.customCad.desc', queryParams: { type: 'custom' } },
        { id: '2', icon: 'pi pi-refresh', titleKey: 'designCad.cards.reverse.title', descKey: 'designCad.cards.reverse.desc', queryParams: { type: 'reverse' } },
        { id: '3', icon: 'pi pi-wrench', titleKey: 'designCad.cards.optimization.title', descKey: 'designCad.cards.optimization.desc', queryParams: { type: 'fix' } },
        { id: '4', icon: 'pi pi-box', titleKey: 'designCad.cards.manufacturing.title', descKey: 'designCad.cards.manufacturing.desc', queryParams: { type: 'outputs' } },
        { id: '5', icon: 'pi pi-wrench', titleKey: 'designCad.cards.modifyProduct.title', descKey: 'designCad.cards.modifyProduct.desc', queryParams: { type: 'modify' } },
        { id: '6', icon: 'pi pi-cog', titleKey: 'designCad.cards.mechanicalAssembly.title', descKey: 'designCad.cards.mechanicalAssembly.desc', queryParams: { type: 'assembly' } }
    ];

    howSteps = [
        { titleKey: 'designCad.how.step1Title', textKey: 'designCad.how.step1Text', icon: 'pi pi-list' },
        { titleKey: 'designCad.how.step2Title', textKey: 'designCad.how.step2Text', icon: 'pi pi-upload' },
        { titleKey: 'designCad.how.step3Title', textKey: 'designCad.how.step3Text', icon: 'pi pi-check-circle' }
    ];

    scrollToSection(id: string) {
        const el = document.getElementById(id);
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}
