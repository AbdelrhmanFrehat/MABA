import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { LanguageService } from '../../../shared/services/language.service';
import { DESIGN_REQUEST_TYPES, type DesignRequestType } from '../../../shared/models/design-request.model';

const STARTING_POINT_ICONS: Record<DesignRequestType, string> = {
    IdeaOnly: 'pi-lightbulb',
    BrokenDesign: 'pi-wrench',
    PhysicalObject: 'pi-box',
    ExistingCAD: 'pi-file-edit',
    TechnicalDrawings: 'pi-list',
    ImproveExistingProduct: 'pi-arrow-right-arrow-left'
};

@Component({
    selector: 'app-design-landing',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, CardModule],
    template: `
        <div class="design-page" [dir]="languageService.direction">
            <!-- Small Hero (not full height) -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-pencil"></i>
                        {{ 'design.hero.badge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'design.hero.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'design.hero.subtitle' | translate }}</p>
                    <div class="hero-cta">
                        <p-button
                            [label]="'design.hero.ctaStart' | translate"
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            routerLink="/design/new"
                            styleClass="hero-btn-primary">
                        </p-button>
                        <p-button
                            [label]="'design.hero.ctaExplore' | translate"
                            icon="pi pi-folder-open"
                            iconPos="right"
                            routerLink="/projects"
                            [outlined]="true"
                            styleClass="hero-btn-secondary">
                        </p-button>
                    </div>
                </div>
            </section>

            <!-- Choose your starting point - 6 cards -->
            <section class="starting-points-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'design.startingPoints.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'design.startingPoints.title' | translate }}</h2>
                    </div>
                    <div class="cards-grid">
                        @for (type of startingPointTypes; track type) {
                            <div class="start-card">
                                <div class="start-card-icon">
                                    <i class="pi" [ngClass]="getIcon(type)"></i>
                                </div>
                                <h3 class="start-card-title">{{ 'design.startingPoints.' + type + '.title' | translate }}</h3>
                                <p class="start-card-desc">{{ 'design.startingPoints.' + type + '.desc' | translate }}</p>
                                <p-button
                                    [label]="'design.startingPoints.start' | translate"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/design/new']"
                                    [queryParams]="{ type: type }"
                                    styleClass="start-card-btn">
                                </p-button>
                            </div>
                        }
                    </div>
                </div>
            </section>

            <!-- What we deliver strip -->
            <section class="deliver-strip">
                <div class="container">
                    <div class="strip-content">
                        <span class="strip-item">{{ 'design.deliver.cadModel' | translate }}</span>
                        <span class="strip-dot">•</span>
                        <span class="strip-item">{{ 'design.deliver.manufacturingFiles' | translate }}</span>
                        <span class="strip-dot">•</span>
                        <span class="strip-item">{{ 'design.deliver.drawings' | translate }}</span>
                        <span class="strip-dot">•</span>
                        <span class="strip-item">{{ 'design.deliver.dfm' | translate }}</span>
                        <span class="strip-dot">•</span>
                        <span class="strip-item">{{ 'design.deliver.prototypeSupport' | translate }}</span>
                    </div>
                </div>
            </section>

            <!-- Workflow 4 steps -->
            <section class="workflow-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'design.workflow.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'design.workflow.title' | translate }}</h2>
                    </div>
                    <div class="workflow-steps">
                        <div class="workflow-step">
                            <div class="step-num">01</div>
                            <div class="step-icon"><i class="pi pi-send"></i></div>
                            <h3>{{ 'design.workflow.submit' | translate }}</h3>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-num">02</div>
                            <div class="step-icon"><i class="pi pi-search"></i></div>
                            <h3>{{ 'design.workflow.review' | translate }}</h3>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-num">03</div>
                            <div class="step-icon"><i class="pi pi-dollar"></i></div>
                            <h3>{{ 'design.workflow.quote' | translate }}</h3>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-num">04</div>
                            <div class="step-icon"><i class="pi pi-check-circle"></i></div>
                            <h3>{{ 'design.workflow.deliver' | translate }}</h3>
                        </div>
                    </div>
                </div>
            </section>

            <!-- CTA footer block -->
            <section class="cta-section">
                <div class="cta-bg"></div>
                <div class="container">
                    <div class="cta-content">
                        <h2 class="cta-title">{{ 'design.cta.title' | translate }}</h2>
                        <p-button
                            [label]="'design.hero.ctaStart' | translate"
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            routerLink="/design/new"
                            styleClass="cta-btn">
                        </p-button>
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
        }
        .design-page { width: 100%; background: #fafbfc; }
        .container { max-width: 1200px; margin: 0 auto; padding: 0 1.5rem; }

        /* Small hero - not full height */
        .hero-section {
            position: relative;
            padding: 4rem 2rem;
            overflow: hidden;
        }
        .hero-bg-gradient { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .hero-pattern {
            position: absolute; inset: 0;
            background-image: radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            z-index: 1;
        }
        .hero-content { position: relative; z-index: 10; text-align: center; max-width: 680px; margin: 0 auto; }
        .hero-badge {
            display: inline-flex; align-items: center; gap: 0.5rem;
            padding: 0.5rem 1rem; background: rgba(255,255,255,0.15); border-radius: 50px;
            font-size: 0.875rem; font-weight: 600; color: white; margin-bottom: 1rem;
        }
        .hero-title { font-size: clamp(1.75rem, 4vw, 2.75rem); font-weight: 800; color: white; margin-bottom: 0.5rem; line-height: 1.2; }
        .hero-subtitle { font-size: clamp(1rem, 1.2vw, 1.1rem); color: rgba(255,255,255,0.9); margin-bottom: 1.5rem; line-height: 1.6; }
        .hero-cta { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
        :host ::ng-deep .hero-btn-primary { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.75rem 1.25rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .hero-btn-secondary { background: transparent !important; border: 2px solid rgba(255,255,255,0.6) !important; color: white !important; padding: 0.75rem 1.25rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .hero-btn-secondary:hover { background: rgba(255,255,255,0.1) !important; border-color: white !important; }

        /* Starting points cards */
        .starting-points-section { padding: 4rem 0; }
        .section-header { text-align: center; margin-bottom: 2.5rem; }
        .section-badge { display: inline-block; padding: 0.5rem 1.25rem; background: var(--gradient-primary); color: white; border-radius: 50px; font-size: 0.875rem; font-weight: 600; margin-bottom: 0.5rem; }
        .section-title { font-size: clamp(1.5rem, 3vw, 2rem); font-weight: 700; color: #1a1a2e; margin: 0; }
        .cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; }
        .start-card {
            background: white; border-radius: 16px; padding: 1.5rem; box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            transition: transform 0.2s, box-shadow 0.2s; border: 1px solid #e5e7eb;
        }
        .start-card:hover { transform: translateY(-4px); box-shadow: 0 12px 32px rgba(102, 126, 234, 0.12); }
        .start-card-icon { width: 48px; height: 48px; background: var(--gradient-primary); border-radius: 12px; display: flex; align-items: center; justify-content: center; margin-bottom: 1rem; }
        .start-card-icon i { font-size: 1.25rem; color: white; }
        .start-card-title { font-size: 1.1rem; font-weight: 700; color: #1a1a2e; margin: 0 0 0.5rem 0; }
        .start-card-desc { font-size: 0.9rem; color: #6b7280; line-height: 1.5; margin: 0 0 1rem 0; }
        :host ::ng-deep .start-card-btn { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.5rem 1rem !important; font-size: 0.875rem !important; border-radius: 10px !important; }

        /* Deliver strip */
        .deliver-strip { padding: 2rem; background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%); border-top: 1px solid #e5e7eb; border-bottom: 1px solid #e5e7eb; }
        .strip-content { display: flex; flex-wrap: wrap; align-items: center; justify-content: center; gap: 0.5rem 1rem; font-size: 0.95rem; font-weight: 500; color: #4b5563; }
        .strip-dot { color: var(--color-primary); font-weight: 700; }
        [dir="rtl"] .strip-dot { margin: 0 0.25rem; }

        /* Workflow */
        .workflow-section { padding: 4rem 0; background: #fff; }
        .workflow-steps { display: flex; align-items: flex-start; justify-content: center; gap: 0; flex-wrap: wrap; margin-top: 2rem; }
        .workflow-step { flex: 1; min-width: 160px; max-width: 220px; text-align: center; padding: 1rem; }
        .step-num { font-size: 0.7rem; font-weight: 700; color: var(--color-primary); margin-bottom: 0.25rem; }
        .step-icon { width: 56px; height: 56px; margin: 0 auto 0.75rem; background: var(--gradient-primary); border-radius: 14px; display: flex; align-items: center; justify-content: center; }
        .step-icon i { font-size: 1.35rem; color: white; }
        .workflow-step h3 { font-size: 1rem; font-weight: 700; color: #1a1a2e; margin: 0; }
        .workflow-connector { width: 40px; height: 2px; background: linear-gradient(90deg, var(--color-primary), var(--color-secondary)); margin-top: 3.5rem; flex-shrink: 0; }
        @media (max-width: 900px) { .workflow-connector { display: none; } }

        /* CTA footer */
        .cta-section { position: relative; padding: 4rem 2rem; overflow: hidden; }
        .cta-bg { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .cta-content { position: relative; z-index: 10; text-align: center; }
        .cta-title { font-size: clamp(1.25rem, 2.5vw, 1.75rem); font-weight: 700; color: white; margin: 0 0 1.5rem 0; }
        :host ::ng-deep .cta-btn { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.75rem 1.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }

        @media (max-width: 768px) {
            .cards-grid { grid-template-columns: 1fr; }
            .strip-content { font-size: 0.85rem; }
        }
    `]
})
export class DesignLandingComponent {
    languageService = inject(LanguageService);
    startingPointTypes = DESIGN_REQUEST_TYPES;

    getIcon(type: DesignRequestType): string {
        return STARTING_POINT_ICONS[type] || 'pi-file';
    }
}
