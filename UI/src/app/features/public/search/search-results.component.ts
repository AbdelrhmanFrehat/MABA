import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule } from '@ngx-translate/core';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { ProductCardComponent } from '../shared/product-card/product-card.component';
import { Item } from '../../../shared/models/item.model';

@Component({
    selector: 'app-search-results',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        InputTextModule,
        ButtonModule,
        ProgressSpinnerModule,
        TranslateModule,
        ProductCardComponent
    ],
    template: `
        <div class="search-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-search"></i>
                        {{ languageService.language === 'ar' ? 'نتائج البحث' : 'Search Results' }}
                    </span>
                    <h1 class="hero-title">{{ 'search.title' | translate }}</h1>
                </div>
            </section>

            <!-- Search Section -->
            <section class="search-section">
                <div class="container">
                    <div class="search-box">
                        <div class="search-input-wrapper">
                            <i class="pi pi-search"></i>
                            <input 
                                type="text" 
                                pInputText
                                [(ngModel)]="searchQuery"
                                [placeholder]="languageService.language === 'ar' ? 'ابحث عن منتجات...' : 'Search for products...'"
                                (keyup.enter)="performSearch()"
                            />
                        </div>
                        <p-button 
                            [label]="'search.search' | translate" 
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            (click)="performSearch()"
                            styleClass="search-btn">
                        </p-button>
                    </div>
                </div>
            </section>

            <!-- Results Section -->
            <section class="results-section">
                <div class="container">
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري البحث...' : 'Searching...' }}</p>
                        </div>
                    </div>

                    <div *ngIf="!loading && searchQuery && results.length === 0" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-search"></i>
                        </div>
                        <h2>{{ 'search.noResults' | translate }}</h2>
                        <p>{{ languageService.language === 'ar' 
                            ? 'لم نجد نتائج لـ "' + searchQuery + '"'
                            : 'No results found for "' + searchQuery + '"' }}</p>
                        <p-button 
                            [label]="languageService.language === 'ar' ? 'تصفح المنتجات' : 'Browse Products'"
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            routerLink="/catalog"
                            styleClass="cta-button">
                        </p-button>
                    </div>

                    <div *ngIf="!loading && results.length > 0">
                        <div class="results-header">
                            <span class="results-count">
                                {{ totalResults }} {{ languageService.language === 'ar' ? 'نتيجة لـ' : 'results for' }} 
                                "<strong>{{ searchQuery }}</strong>"
                            </span>
                        </div>
                        <div class="results-grid">
                            <div *ngFor="let item of results; let i = index" class="result-card" [style.animation-delay]="(i * 0.05) + 's'">
                                <app-product-card [product]="item"></app-product-card>
                            </div>
                        </div>
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

        .search-page {
            width: 100%;
            min-height: 100vh;
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

        /* ============ SEARCH SECTION ============ */
        .search-section {
            padding: 0 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .search-box {
            display: flex;
            gap: 1rem;
            background: white;
            padding: 1.5rem;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
            max-width: 800px;
            margin: 0 auto;
        }

        .search-input-wrapper {
            flex: 1;
            display: flex;
            align-items: center;
            gap: 1rem;
            background: #f8f9fa;
            padding: 0.75rem 1.5rem;
            border-radius: 12px;
        }

        .search-input-wrapper i {
            color: #6c757d;
            font-size: 1.25rem;
        }

        .search-input-wrapper input {
            flex: 1;
            border: none !important;
            background: transparent !important;
            box-shadow: none !important;
            font-size: 1.1rem;
        }

        :host ::ng-deep .search-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 0.875rem 2rem !important;
            border-radius: 12px !important;
            font-weight: 600 !important;
        }

        /* ============ RESULTS SECTION ============ */
        .results-section {
            padding: 3rem 1rem;
        }

        .loading-container {
            text-align: center;
            padding: 4rem;
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .loading-spinner p {
            margin-top: 1rem;
            color: #6c757d;
        }

        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .empty-icon {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 2rem;
        }

        .empty-icon i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .empty-state h2 {
            font-size: 1.75rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .empty-state p {
            color: #6c757d;
            margin-bottom: 2rem;
        }

        :host ::ng-deep .cta-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 2rem !important;
            border-radius: 50px !important;
        }

        .results-header {
            margin-bottom: 2rem;
        }

        .results-count {
            font-size: 1.1rem;
            color: #6c757d;
        }

        .results-count strong {
            color: var(--color-primary);
        }

        .results-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 2rem;
        }

        .result-card {
            animation: fadeIn 0.5s ease-out backwards;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        :host ::ng-deep .result-card app-product-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            transition: all 0.3s ease;
        }

        :host ::ng-deep .result-card app-product-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
        }

        @media (max-width: 768px) {
            .search-box {
                flex-direction: column;
            }
            .results-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class SearchResultsComponent implements OnInit {
    searchQuery = '';
    results: Item[] = [];
    loading = false;
    totalResults = 0;

    private route = inject(ActivatedRoute);
    private itemsApiService = inject(ItemsApiService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            if (params['q']) {
                this.searchQuery = params['q'];
                this.performSearch();
            }
        });
    }

    performSearch() {
        if (!this.searchQuery.trim()) return;

        this.loading = true;
        this.itemsApiService.searchItems(this.searchQuery, {
            page: 1,
            pageSize: 50
        }).subscribe({
            next: (response) => {
                this.results = response.items;
                this.totalResults = response.totalCount;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }
}
