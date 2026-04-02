import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { LanguageService } from '../../../shared/services/language.service';
import { FaqApiService } from '../../../shared/services/faq-api.service';
import { FaqCategory, FaqItem } from '../../../shared/models/faq.model';

const FAQ_CATEGORY_KEYS: FaqCategory[] = [
    'Print3d',
    'Laser',
    'Cnc',
    'Software',
    'OrdersShipping',
    'Payments',
    'Support',
    'General'
];

const CATEGORY_ICONS: Record<FaqCategory, string> = {
    Print3d: 'pi pi-print',
    Laser: 'pi pi-bolt',
    Cnc: 'pi pi-cog',
    Software: 'pi pi-desktop',
    OrdersShipping: 'pi pi-truck',
    Payments: 'pi pi-credit-card',
    Support: 'pi pi-comments',
    General: 'pi pi-info-circle'
};

@Component({
    selector: 'app-help-center',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        TranslateModule,
        AccordionModule,
        InputTextModule,
        ButtonModule,
        IconFieldModule,
        InputIconModule,
        ProgressSpinnerModule
    ],
    template: `
        <div class="help-center-page" [dir]="languageService.direction">
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="container hero-container">
                    <span class="hero-badge">{{ 'helpCenter.badge' | translate }}</span>
                    <h1 class="hero-title">{{ 'helpCenter.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'helpCenter.subtitle' | translate }}</p>
                </div>
            </section>

            <section class="panel-section">
                <div class="container">
                    <div class="help-panel">
                        <div class="search-row">
                            <p-iconfield iconPosition="left" class="search-field">
                                <p-inputicon styleClass="pi pi-search" />
                                <input
                                    type="text"
                                    pInputText
                                    [(ngModel)]="searchModel"
                                    (ngModelChange)="onSearch($event)"
                                    [placeholder]="'helpCenter.searchPlaceholder' | translate"
                                    class="search-input"
                                />
                            </p-iconfield>
                        </div>

                        <p class="filter-label">{{ 'helpCenter.filterLabel' | translate }}</p>
                        <div class="category-cards">
                            @for (c of categoryCards; track c.category) {
                                <button
                                    type="button"
                                    class="category-card"
                                    [class.active]="selectedFilter() === c.category"
                                    (click)="setFilter(c.category)">
                                    <i [class]="c.icon + ' card-icon'"></i>
                                    <span class="card-title">{{ 'faq.categories.' + c.category | translate }}</span>
                                    <span class="card-hint">{{ 'helpCenter.categoryHints.' + c.category | translate }}</span>
                                </button>
                            }
                        </div>

                        <div class="chips-row">
                            <button
                                type="button"
                                class="chip"
                                [class.active]="selectedFilter() === 'all'"
                                (click)="setFilter('all')">
                                {{ 'common.all' | translate }}
                            </button>
                            @for (c of categoryCards; track c.category) {
                                <button
                                    type="button"
                                    class="chip"
                                    [class.active]="selectedFilter() === c.category"
                                    (click)="setFilter(c.category)">
                                    {{ 'faq.categories.' + c.category | translate }}
                                </button>
                            }
                        </div>

                        <div class="panel-divider"></div>

                        @if (loading()) {
                            <div class="loading-wrap">
                                <p-progressSpinner strokeWidth="3" styleClass="help-spinner" />
                                <p class="loading-text">{{ 'helpCenter.loading' | translate }}</p>
                            </div>
                        } @else if (visibleSections().length === 0) {
                            <p class="empty-text">{{ 'helpCenter.noResults' | translate }}</p>
                        } @else {
                            <div class="help-layout">
                                <aside class="help-sidebar">
                                    <nav class="side-nav">
                                        @for (s of visibleSections(); track s.category) {
                                            <a class="side-link" [href]="'#' + sectionAnchor(s.category)" (click)="scrollTo($event, s.category)">
                                                <i [class]="s.icon"></i>
                                                {{ 'faq.categories.' + s.category | translate }}
                                            </a>
                                        }
                                    </nav>
                                </aside>
                                <div class="help-main">
                                    @for (s of visibleSections(); track s.category) {
                                        <section class="help-section" [id]="sectionAnchor(s.category)">
                                            <div class="section-head">
                                                <h2 class="section-title">
                                                    <i [class]="s.icon + ' section-icon'"></i>
                                                    {{ 'faq.categories.' + s.category | translate }}
                                                </h2>
                                                <p class="section-desc">{{ 'helpCenter.sectionBlurb' | translate }}</p>
                                            </div>
                                            <p-accordion [multiple]="true" class="help-accordion">
                                                @for (item of s.items; track item.id) {
                                                    <p-accordion-panel [value]="item.id">
                                                        <p-accordion-header>
                                                            <span class="accordion-q">{{ question(item) }}</span>
                                                        </p-accordion-header>
                                                        <p-accordion-content>
                                                            <p class="accordion-a">{{ answer(item) }}</p>
                                                        </p-accordion-content>
                                                    </p-accordion-panel>
                                                }
                                            </p-accordion>
                                        </section>
                                    }
                                </div>
                            </div>
                        }
                    </div>
                </div>
            </section>

            <section class="cta-section">
                <div class="container">
                    <div class="cta-block">
                        <h2 class="cta-title">{{ 'helpCenter.cta.title' | translate }}</h2>
                        <p class="cta-description">{{ 'helpCenter.cta.description' | translate }}</p>
                        <div class="cta-actions">
                            <a routerLink="/contact" pButton [label]="'helpCenter.cta.contact' | translate" icon="pi pi-envelope" class="cta-btn"></a>
                            <a routerLink="/projects/request" pButton [label]="'helpCenter.cta.startProject' | translate" icon="pi pi-briefcase" [outlined]="true" class="cta-btn-secondary"></a>
                            <a routerLink="/account/orders" pButton [label]="'helpCenter.cta.myOrders' | translate" icon="pi pi-shopping-bag" [outlined]="true" class="cta-btn-secondary"></a>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
        }

        .help-center-page {
            width: 100%;
            min-height: 100%;
            background: #f5f6f8;
            overflow-x: hidden;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1.5rem;
        }

        .hero-section {
            position: relative;
            padding: 3rem 2rem 2.75rem;
            text-align: center;
            background: var(--gradient-dark);
        }

        .hero-bg {
            position: absolute;
            inset: 0;
            z-index: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
        }

        .hero-container {
            position: relative;
            z-index: 1;
            max-width: 720px;
            margin: 0 auto;
        }

        .hero-badge {
            display: inline-block;
            padding: 0.4rem 1rem;
            background: rgba(255,255,255,0.15);
            border-radius: 50px;
            font-size: 0.8rem;
            font-weight: 600;
            color: #fff;
            margin-bottom: 0.75rem;
        }

        .hero-title {
            font-size: clamp(1.5rem, 4vw, 2.1rem);
            font-weight: 700;
            color: #fff;
            margin: 0 0 0.5rem;
            line-height: 1.25;
        }

        .hero-subtitle {
            font-size: 1rem;
            color: rgba(255,255,255,0.88);
            margin: 0;
            line-height: 1.6;
        }

        .panel-section {
            padding-top: 1.75rem;
            padding-bottom: 2.5rem;
        }

        .help-panel {
            background: #fff;
            border-radius: 22px;
            border: 1px solid rgba(0, 0, 0, 0.06);
            box-shadow: 0 2px 16px rgba(0, 0, 0, 0.04);
            padding: 24px;
        }

        .search-row {
            margin-bottom: 1.25rem;
        }

        .search-field {
            display: block;
            max-width: 520px;
        }

        .search-input {
            width: 100%;
        }

        .filter-label {
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--text-color-secondary);
            margin: 0 0 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.04em;
        }

        .category-cards {
            display: grid;
            grid-template-columns: 1fr;
            gap: 12px;
            margin-bottom: 1.25rem;
        }

        @media (min-width: 640px) {
            .category-cards {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        @media (min-width: 1024px) {
            .category-cards {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        .category-card {
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            text-align: start;
            gap: 0.35rem;
            padding: 1rem 1.1rem;
            border-radius: 14px;
            border: 1px solid rgba(0, 0, 0, 0.08);
            background: var(--surface-50);
            cursor: pointer;
            transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
        }

        .category-card:hover {
            border-color: rgba(102, 126, 234, 0.35);
            background: #fff;
        }

        .category-card.active {
            border-color: var(--color-primary);
            box-shadow: 0 0 0 1px rgba(102, 126, 234, 0.25);
            background: #fff;
        }

        .card-icon {
            font-size: 1.35rem;
            color: var(--color-primary);
            margin-bottom: 0.15rem;
        }

        .card-title {
            font-weight: 700;
            font-size: 0.95rem;
            color: var(--text-color);
        }

        .card-hint {
            font-size: 0.8rem;
            color: var(--text-color-secondary);
            line-height: 1.4;
        }

        .chips-row {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            align-items: center;
            margin-bottom: 0.5rem;
        }

        .chip {
            border: 1px solid rgba(0, 0, 0, 0.1);
            background: #fff;
            border-radius: 999px;
            padding: 0.35rem 0.85rem;
            font-size: 0.85rem;
            cursor: pointer;
            transition: background 0.15s, border-color 0.15s;
        }

        .chip:hover {
            border-color: rgba(102, 126, 234, 0.4);
        }

        .chip.active {
            background: rgba(102, 126, 234, 0.12);
            border-color: var(--color-primary);
            color: var(--color-primary);
            font-weight: 600;
        }

        .panel-divider {
            height: 1px;
            background: rgba(0, 0, 0, 0.06);
            margin: 1.25rem 0 1.5rem;
        }

        .loading-wrap {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 2.5rem 1rem;
            gap: 1rem;
        }

        .loading-text {
            margin: 0;
            color: var(--text-color-secondary);
            font-size: 0.95rem;
        }

        :host ::ng-deep .help-spinner {
            width: 2.5rem;
            height: 2.5rem;
        }

        .empty-text {
            text-align: center;
            color: var(--text-color-secondary);
            padding: 2rem 1rem;
        }

        .help-layout {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1.5rem;
        }

        @media (min-width: 960px) {
            .help-layout {
                grid-template-columns: 220px 1fr;
                gap: 2rem;
                align-items: start;
            }

            .help-sidebar {
                position: sticky;
                top: 1rem;
            }
        }

        .side-nav {
            display: flex;
            flex-direction: column;
            gap: 0.35rem;
        }

        .side-link {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 0.65rem;
            border-radius: 10px;
            font-size: 0.9rem;
            color: var(--text-color);
            text-decoration: none;
            transition: background 0.15s;
        }

        .side-link:hover {
            background: var(--surface-100);
        }

        .side-link i {
            color: var(--color-primary);
            font-size: 0.95rem;
        }

        .help-section {
            margin-bottom: 2rem;
        }

        .help-section:last-child {
            margin-bottom: 0;
        }

        .section-head {
            margin-bottom: 1rem;
        }

        .section-title {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 1.2rem;
            font-weight: 700;
            margin: 0 0 0.35rem;
            color: var(--text-color);
        }

        .section-icon {
            color: var(--color-primary);
        }

        .section-desc {
            margin: 0;
            font-size: 0.9rem;
            color: var(--text-color-secondary);
            line-height: 1.55;
            max-width: 52rem;
        }

        .accordion-q {
            font-weight: 600;
            font-size: 0.95rem;
        }

        .accordion-a {
            margin: 0;
            font-size: 0.92rem;
            line-height: 1.65;
            color: var(--text-color);
            white-space: pre-wrap;
        }

        :host ::ng-deep .help-accordion .p-accordion-panel {
            border: 1px solid rgba(0, 0, 0, 0.06);
            border-radius: 12px !important;
            margin-bottom: 0.5rem;
            overflow: hidden;
        }

        :host ::ng-deep .help-accordion .p-accordion-header-link {
            padding: 0.85rem 1rem;
        }

        :host ::ng-deep .help-accordion .p-accordion-content {
            padding: 0 1rem 1rem;
        }

        .cta-section {
            padding-bottom: 3rem;
        }

        .cta-block {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%);
            border: 1px solid rgba(102, 126, 234, 0.2);
            border-radius: 20px;
            padding: 2rem 1.5rem;
            text-align: center;
        }

        .cta-title {
            font-size: 1.35rem;
            font-weight: 700;
            margin: 0 0 0.5rem;
        }

        .cta-description {
            margin: 0 auto 1.25rem;
            max-width: 520px;
            color: var(--text-color-secondary);
            font-size: 0.95rem;
            line-height: 1.55;
        }

        .cta-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            justify-content: center;
        }

        :host ::ng-deep .cta-btn.p-button {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
        }

        :host ::ng-deep .cta-btn-secondary.p-button.p-button-outlined {
            border-color: rgba(102, 126, 234, 0.45);
            color: var(--color-primary);
        }

        @media (max-width: 959px) {
            .help-sidebar {
                display: none;
            }
        }
    `]
})
export class HelpCenterComponent implements OnInit {
    languageService = inject(LanguageService);
    private faqApi = inject(FaqApiService);

    searchModel = '';
    searchTerm = signal('');
    selectedFilter = signal<'all' | FaqCategory>('all');
    allFaqs = signal<FaqItem[]>([]);
    loading = signal(true);

    readonly categoryCards = FAQ_CATEGORY_KEYS.map((category) => ({
        category,
        icon: CATEGORY_ICONS[category]
    }));

    visibleSections = computed(() => {
        const q = this.searchTerm().trim().toLowerCase();
        const filter = this.selectedFilter();
        let list = this.allFaqs();

        if (filter !== 'all') {
            list = list.filter((item) => this.toCategoryKey(item) === filter);
        }

        if (q) {
            list = list.filter((item) => this.itemMatchesSearch(item, q));
        }

        const byCat = new Map<FaqCategory, FaqItem[]>();
        for (const cat of FAQ_CATEGORY_KEYS) {
            byCat.set(cat, []);
        }

        for (const item of list) {
            const cat = this.toCategoryKey(item);
            const bucket = byCat.get(cat);
            if (bucket) {
                bucket.push(item);
            }
        }

        return FAQ_CATEGORY_KEYS.filter((cat) => (byCat.get(cat)?.length ?? 0) > 0).map((category) => ({
            category,
            icon: CATEGORY_ICONS[category],
            items: (byCat.get(category) ?? []).sort(
                (a, b) =>
                    (a.sortOrder ?? 0) - (b.sortOrder ?? 0) ||
                    a.questionEn.localeCompare(b.questionEn, undefined, { sensitivity: 'base' })
            )
        }));
    });

    ngOnInit(): void {
        this.loading.set(true);
        this.faqApi.getPublicFaq().subscribe({
            next: (data) => {
                this.allFaqs.set(Array.isArray(data) ? data : []);
                this.loading.set(false);
            },
            error: () => {
                this.allFaqs.set([]);
                this.loading.set(false);
            }
        });
    }

    onSearch(value: string): void {
        this.searchTerm.set(value ?? '');
    }

    setFilter(v: 'all' | FaqCategory): void {
        this.selectedFilter.set(v);
    }

    sectionAnchor(category: FaqCategory): string {
        return `help-cat-${category}`;
    }

    scrollTo(ev: Event, category: FaqCategory): void {
        ev.preventDefault();
        const el = document.getElementById(this.sectionAnchor(category));
        el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    question(item: FaqItem): string {
        return this.languageService.language === 'ar' && item.questionAr ? item.questionAr : item.questionEn;
    }

    answer(item: FaqItem): string {
        return this.languageService.language === 'ar' && item.answerAr ? item.answerAr : item.answerEn;
    }

    private toCategoryKey(item: FaqItem): FaqCategory {
        const c = item.category as FaqCategory | number | string;
        if (typeof c === 'number' && c >= 0 && c < FAQ_CATEGORY_KEYS.length) {
            return FAQ_CATEGORY_KEYS[c];
        }
        if (typeof c === 'string' && /^\d+$/.test(c)) {
            const n = parseInt(c, 10);
            if (n >= 0 && n < FAQ_CATEGORY_KEYS.length) {
                return FAQ_CATEGORY_KEYS[n];
            }
        }
        if (typeof c === 'string' && (FAQ_CATEGORY_KEYS as string[]).includes(c)) {
            return c as FaqCategory;
        }
        return 'General';
    }

    private itemMatchesSearch(item: FaqItem, q: string): boolean {
        const fields = [item.questionEn, item.questionAr, item.answerEn, item.answerAr];
        return fields.some((f) => (f ?? '').toLowerCase().includes(q));
    }
}
