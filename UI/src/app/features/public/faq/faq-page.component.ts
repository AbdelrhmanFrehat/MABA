import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { FaqApiService } from '../../../shared/services/faq-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { FaqItem } from '../../../shared/models/faq.model';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
    selector: 'app-faq-page',
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
        InputIconModule
    ],
    template: `
        <div class="faq-page" [dir]="languageService.direction">
            <!-- 1) Hero (compact) -->
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="container hero-container">
                    <span class="hero-badge">{{ 'faq.badge' | translate }}</span>
                    <h1 class="hero-title">{{ 'faq.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'faq.subtitle' | translate }}</p>
                </div>
            </section>

            <!-- 2) ONE FAQ Panel (search + chips + divider + accordion) -->
            <section class="panel-section">
                <div class="container">
                    <div class="faq-panel">
                        <div class="search-row">
                            <p-iconfield iconPosition="left" class="search-field">
                                <p-inputicon styleClass="pi pi-search" />
                                <input
                                    type="text"
                                    pInputText
                                    [(ngModel)]="searchTerm"
                                    (ngModelChange)="onSearchChange()"
                                    [placeholder]="'faq.searchPlaceholder' | translate"
                                    class="search-input"
                                />
                            </p-iconfield>
                        </div>
                        <div class="chips-row">
                            <span class="chips-label-inline">{{ 'faq.filters.categoryLabel' | translate }}</span>
                            <button
                                type="button"
                                class="chip"
                                [class.active]="selectedCategory() === null"
                                (click)="setCategory(null)">
                                {{ 'faq.categories.all' | translate }}
                            </button>
                            @for (cat of categoryOptions; track cat.value) {
                                <button
                                    type="button"
                                    class="chip"
                                    [class.active]="selectedCategory() === cat.value"
                                    (click)="setCategory(cat.value)">
                                    {{ 'faq.categories.' + cat.name | translate }}
                                </button>
                            }
                        </div>
                        <div class="panel-divider"></div>
                        @if (loading()) {
                            <p class="loading-text">{{ 'common.loading' | translate }}</p>
                        } @else if (filteredItems().length === 0) {
                            <p class="empty-text">{{ 'faq.noResults' | translate }}</p>
                        } @else {
                            <p-accordion [value]="['0']" [multiple]="true" class="faq-accordion">
                                @for (item of filteredItems(); track item.id; let i = $index) {
                                    <p-accordion-panel [value]="i.toString()">
                                        <p-accordion-header>
                                            <span class="accordion-question">{{ getQuestion(item) }}</span>
                                        </p-accordion-header>
                                        <p-accordion-content>
                                            <div class="accordion-answer" [innerHTML]="getAnswerHtml(item)"></div>
                                        </p-accordion-content>
                                    </p-accordion-panel>
                                }
                            </p-accordion>
                        }
                    </div>
                </div>
            </section>

            <!-- 4) Help CTA -->
            <section class="cta-section">
                <div class="container">
                    <div class="cta-block">
                        <h2 class="cta-title">{{ 'faq.cta.title' | translate }}</h2>
                        <p class="cta-description">{{ 'faq.cta.description' | translate }}</p>
                        <a routerLink="/contact" pButton [label]="'faq.cta.button' | translate" icon="pi pi-envelope" iconPos="left" class="cta-btn"></a>
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
        }

        .faq-page {
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

        /* 1) Hero – reduced height, compact */
        .hero-section {
            position: relative;
            padding: 3.5rem 2rem 3rem;
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
            max-width: 680px;
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
            font-size: clamp(1.5rem, 4vw, 2.25rem);
            font-weight: 700;
            color: #fff;
            margin: 0 0 0.5rem;
            line-height: 1.25;
        }

        .hero-subtitle {
            font-size: 1rem;
            color: rgba(255,255,255,0.9);
            margin: 0;
            line-height: 1.6;
        }

        /* 2) ONE FAQ Panel – search + chips + divider + accordion */
        .panel-section {
            padding-top: 28px;
            padding-bottom: 2.5rem;
        }

        .faq-panel {
            max-width: 1200px;
            margin: 0 auto;
            background: #fff;
            border-radius: 22px;
            border: 1px solid rgba(0, 0, 0, 0.06);
            box-shadow: 0 2px 16px rgba(0, 0, 0, 0.04);
            padding: 24px;
        }

        .search-row {
            margin: 0 0 16px 0;
        }

        .search-field {
            width: 100%;
            display: block;
        }

        .search-field ::ng-deep input {
            width: 100% !important;
            height: 44px !important;
            min-height: 44px !important;
            padding-inline-start: 2.75rem !important;
            padding-inline-end: 1rem !important;
            border-radius: 10px !important;
            border: 1px solid #e5e7eb !important;
            font-size: 0.95rem !important;
        }

        .search-field ::ng-deep input:focus {
            outline: none !important;
            border-color: var(--color-primary) !important;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1) !important;
        }

        .chips-row {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 0.5rem 0.75rem;
        }

        .chips-label-inline {
            font-size: 0.8rem;
            font-weight: 600;
            color: #6b7280;
            margin-inline-end: 0.25rem;
        }

        .chip {
            height: 34px;
            padding: 0 0.875rem;
            border-radius: 50px;
            border: 1px solid #e5e7eb;
            background: #f8f9fb;
            font-size: 0.875rem;
            font-weight: 500;
            color: #4b5563;
            cursor: pointer;
            transition: all 0.2s;
        }

        .chip:hover {
            background: #f1f3f5;
            border-color: #d1d5db;
        }

        .chip.active {
            background: var(--gradient-primary);
            border-color: var(--color-primary);
            color: #fff;
        }

        .panel-divider {
            height: 0;
            border: none;
            border-top: 1px solid #eee;
            margin: 20px 0 0 0;
        }

        /* Accordion inside panel – no outer box, only internal separators */
        .faq-panel :host ::ng-deep .p-accordion {
            border: none;
            background: transparent;
            border-radius: 0;
        }

        .faq-panel :host ::ng-deep .p-accordion .p-accordion-panel {
            border: none;
            border-bottom: 1px solid #eee;
            border-radius: 0;
            box-shadow: none;
        }

        .faq-panel :host ::ng-deep .p-accordion .p-accordion-panel:last-child {
            border-bottom: none;
        }

        .faq-panel :host ::ng-deep .p-accordion-header-link {
            padding: 1.125rem 0 1.125rem 0;
            font-weight: 700;
            font-size: 1rem;
            color: #1a1a2e;
            transition: background 0.2s;
            border: none !important;
            background: transparent !important;
        }

        .faq-panel :host ::ng-deep .p-accordion-header-link:hover {
            background: rgba(102, 126, 234, 0.06) !important;
        }

        .faq-panel :host ::ng-deep .p-accordion-content {
            padding: 0 0 1.25rem 0;
            color: #4b5563;
            line-height: 1.9;
            border: none;
            background: transparent !important;
        }

        .accordion-answer p { margin: 0 0 0.75rem; }
        .accordion-answer p:last-child { margin-bottom: 0; }
        .accordion-answer ul, .accordion-answer ol { margin: 0.5rem 0; padding-inline-start: 1.5rem; }

        .loading-text, .empty-text {
            text-align: center;
            color: #6b7280;
            padding: 2.5rem 1rem;
            font-size: 0.95rem;
        }

        /* 4) Help CTA */
        .cta-section {
            padding: 2rem 0 3.5rem;
        }

        .cta-block {
            background: var(--gradient-dark);
            border-radius: 20px;
            padding: 2rem 2.5rem;
            text-align: center;
        }

        .cta-title {
            font-size: clamp(1.25rem, 2.5vw, 1.5rem);
            font-weight: 700;
            color: #fff;
            margin: 0 0 0.5rem;
        }

        .cta-description {
            font-size: 1rem;
            color: rgba(255,255,255,0.9);
            margin: 0 0 1.25rem;
            line-height: 1.6;
        }

        :host ::ng-deep .cta-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            padding: 0.75rem 1.5rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
        }

        :host ::ng-deep .cta-btn:hover {
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.35);
        }

        /* RTL: accordion icon */
        [dir="rtl"] .faq-panel :host ::ng-deep .p-accordion-header-link .p-accordion-toggle-icon {
            transform: scaleX(-1);
        }

        /* Responsive */
        @media (max-width: 768px) {
            .hero-section {
                padding: 2.5rem 1rem 2rem;
            }

            .panel-section {
                padding-top: 24px;
            }

            .faq-panel {
                padding: 20px 1rem;
                border-radius: 20px;
            }

            .chips-row {
                gap: 0.5rem;
            }

            .chip {
                flex: 0 1 auto;
                min-width: fit-content;
            }

            .cta-block {
                padding: 1.5rem 1.25rem;
            }
        }
    `]
})
export class FaqPageComponent implements OnInit {
    private faqApi = inject(FaqApiService);
    private sanitizer = inject(DomSanitizer);

    languageService = inject(LanguageService);

    items = signal<FaqItem[]>([]);
    loading = signal(true);
    searchTerm = '';
    selectedCategory = signal<number | null>(null);
    categoryOptions: { value: number; name: string }[] = [];

    filteredItems = computed(() => {
        const list = this.items();
        let filtered = list;
        if (this.searchTerm.trim()) {
            const term = this.searchTerm.toLowerCase();
            filtered = filtered.filter(
                i =>
                    i.questionEn.toLowerCase().includes(term) ||
                    (i.questionAr?.toLowerCase().includes(term)) ||
                    i.answerEn.toLowerCase().includes(term) ||
                    (i.answerAr?.toLowerCase().includes(term))
            );
        }
        const cat = this.selectedCategory();
        if (cat !== null) {
            filtered = filtered.filter(i => (typeof i.category === 'number' ? i.category : parseInt(String(i.category), 10)) === cat);
        }
        return filtered;
    });

    ngOnInit() {
        this.loadFaq();
        this.faqApi.getCategories().subscribe(cats => {
            this.categoryOptions = cats;
        });
    }

    loadFaq() {
        this.loading.set(true);
        this.faqApi
            .getPublicFaq()
            .subscribe({
                next: data => {
                    this.items.set(Array.isArray(data) ? data : []);
                    this.loading.set(false);
                },
                error: () => this.loading.set(false)
            });
    }

    onSearchChange() {
        // Filter is done in computed, no API call needed
    }

    setCategory(cat: number | null) {
        this.selectedCategory.set(cat);
    }

    getQuestion(item: FaqItem): string {
        return this.languageService.language === 'ar' && item.questionAr
            ? item.questionAr
            : item.questionEn;
    }

    getAnswerHtml(item: FaqItem): SafeHtml {
        const raw = this.languageService.language === 'ar' && item.answerAr
            ? item.answerAr
            : item.answerEn;
        if (!raw) return '';
        const escaped = raw
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
        const withBreaks = escaped.replace(/\n/g, '<br>');
        return this.sanitizer.bypassSecurityTrustHtml(withBreaks);
    }
}
