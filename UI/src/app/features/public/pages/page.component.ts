import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { CmsApiService } from '../../../shared/services/cms-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Page } from '../../../shared/models/cms.model';

@Component({
    selector: 'app-page',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        ProgressSpinnerModule,
        ButtonModule,
        TranslateModule
    ],
    template: `
        <div class="cms-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge" *ngIf="page">
                        <i class="pi pi-file"></i>
                        {{ languageService.language === 'ar' ? 'صفحة المحتوى' : 'Content Page' }}
                    </span>
                    <h1 class="hero-title">{{ getTitle() || (languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...') }}</h1>
                </div>
            </section>

            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
                <div class="loading-spinner">
                    <i class="pi pi-spin pi-spinner"></i>
                    <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                </div>
            </div>

            <!-- Content Section -->
            <section *ngIf="!loading && page" class="content-section">
                <div class="container">
                    <article class="content-card">
                        <div class="page-content" [innerHTML]="getContent()"></div>
                    </article>
                </div>
            </section>

            <!-- Not Found State -->
            <section *ngIf="!loading && !page" class="not-found-section">
                <div class="container">
                    <div class="not-found-card">
                        <div class="not-found-icon">
                            <i class="pi pi-file-excel"></i>
                        </div>
                        <h2>{{ 'page.notFound' | translate }}</h2>
                        <p>{{ 'page.notFoundDescription' | translate }}</p>
                        <button class="cta-button" routerLink="/">
                            <i class="pi pi-home"></i>
                            {{ languageService.language === 'ar' ? 'العودة للرئيسية' : 'Back to Home' }}
                        </button>
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

        .cms-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 900px;
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

        /* ============ LOADING ============ */
        .loading-container {
            display: flex;
            justify-content: center;
            padding: 6rem 2rem;
        }

        .loading-spinner {
            text-align: center;
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .loading-spinner p {
            margin-top: 1rem;
            color: #6c757d;
        }

        /* ============ CONTENT SECTION ============ */
        .content-section {
            padding: 3rem 1rem;
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

        .page-content {
            line-height: 1.9;
            color: #1a1a2e;
            font-size: 1.05rem;
        }

        :host ::ng-deep .page-content h2 {
            font-size: 1.75rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-top: 2.5rem;
            margin-bottom: 1rem;
            position: relative;
            padding-bottom: 0.75rem;
        }

        :host ::ng-deep .page-content h2::after {
            content: '';
            position: absolute;
            bottom: 0;
            left: 0;
            width: 60px;
            height: 3px;
            background: var(--gradient-primary);
            border-radius: 3px;
        }

        :host ::ng-deep .page-content h3 {
            font-size: 1.35rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-top: 2rem;
            margin-bottom: 0.75rem;
        }

        :host ::ng-deep .page-content p {
            margin-bottom: 1.25rem;
            color: #4a5568;
        }

        :host ::ng-deep .page-content ul,
        :host ::ng-deep .page-content ol {
            padding-left: 1.5rem;
            margin-bottom: 1.25rem;
        }

        :host ::ng-deep .page-content li {
            margin-bottom: 0.5rem;
            color: #4a5568;
        }

        :host ::ng-deep .page-content a {
            color: var(--color-primary);
            text-decoration: none;
            transition: color 0.3s;
        }

        :host ::ng-deep .page-content a:hover {
            text-decoration: underline;
        }

        :host ::ng-deep .page-content blockquote {
            padding: 1.5rem 2rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            border-left: 4px solid var(--color-primary);
            border-radius: 0 12px 12px 0;
            margin: 2rem 0;
            font-style: italic;
            color: #4a5568;
        }

        :host ::ng-deep .page-content img {
            max-width: 100%;
            height: auto;
            border-radius: 12px;
            margin: 1.5rem 0;
        }

        :host ::ng-deep .page-content table {
            width: 100%;
            border-collapse: collapse;
            margin: 1.5rem 0;
        }

        :host ::ng-deep .page-content th,
        :host ::ng-deep .page-content td {
            padding: 1rem;
            border: 1px solid #e9ecef;
            text-align: left;
        }

        :host ::ng-deep .page-content th {
            background: #f8f9fa;
            font-weight: 700;
        }

        /* ============ NOT FOUND SECTION ============ */
        .not-found-section {
            padding: 3rem 1rem;
        }

        .not-found-card {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .not-found-icon {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, rgba(239, 68, 68, 0.1) 0%, rgba(220, 38, 38, 0.1) 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 2rem;
        }

        .not-found-icon i {
            font-size: 3rem;
            color: #ef4444;
        }

        .not-found-card h2 {
            font-size: 1.75rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .not-found-card p {
            color: #6c757d;
            margin-bottom: 2rem;
        }

        .cta-button {
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 50px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .cta-button:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .content-card {
                padding: 2rem 1.5rem;
            }
        }
    `]
})
export class PageComponent implements OnInit {
    page: Page | null = null;
    loading = false;
    slug = '';

    private route = inject(ActivatedRoute);
    private cmsApiService = inject(CmsApiService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.route.data.subscribe(data => {
            this.slug = data['slug'] || this.route.snapshot.url[0]?.path || '';
            if (this.slug) {
                this.loadPage();
            }
        });
    }

    loadPage() {
        this.loading = true;
        this.cmsApiService.getPageBySlug(this.slug).subscribe({
            next: (page) => {
                this.page = page;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    getTitle(): string {
        if (!this.page) return '';
        return this.languageService.language === 'ar' ? this.page.titleAr : this.page.titleEn;
    }

    getContent(): string {
        if (!this.page) return '';
        return this.languageService.language === 'ar' ? (this.page.contentAr || '') : (this.page.contentEn || '');
    }
}
