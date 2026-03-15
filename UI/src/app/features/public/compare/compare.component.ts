import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Item } from '../../../shared/models/item.model';

@Component({
    selector: 'app-compare',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        ButtonModule,
        TableModule,
        TranslateModule
    ],
    template: `
        <div class="compare-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-chart-bar"></i>
                        {{ languageService.language === 'ar' ? 'مقارنة المنتجات' : 'Product Comparison' }}
                    </span>
                    <h1 class="hero-title">{{ 'compare.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ languageService.language === 'ar' 
                        ? 'قارن بين المنتجات المختلفة لاتخاذ القرار الصحيح'
                        : 'Compare different products to make the right decision' }}</p>
                </div>
            </section>

            <!-- Main Content -->
            <section class="main-section">
                <div class="container">
                    <!-- Empty State -->
                    <div *ngIf="items.length === 0" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-chart-bar"></i>
                        </div>
                        <h2>{{ 'compare.empty' | translate }}</h2>
                        <p>{{ 'compare.emptyDescription' | translate }}</p>
                        <button class="cta-button" routerLink="/catalog">
                            <i class="pi pi-shopping-bag"></i>
                            {{ 'compare.startComparing' | translate }}
                            <i class="pi pi-arrow-right"></i>
                        </button>
                    </div>

                    <!-- Comparison Table -->
                    <div *ngIf="items.length > 0" class="comparison-container">
                        <div class="comparison-table">
                            <!-- Header Row with Products -->
                            <div class="table-row header-row">
                                <div class="table-cell feature-label">
                                    <span>{{ 'compare.feature' | translate }}</span>
                                </div>
                                <div class="table-cell product-cell" *ngFor="let item of items">
                                    <div class="product-header">
                                        <div class="product-image" [routerLink]="['/item', item.id]">
                                            <img [src]="getItemImage(item)" [alt]="languageService.getLocalizedName(item)" onerror="this.src='assets/img/defult.png'" />
                                        </div>
                                        <h3 class="product-name">{{ languageService.getLocalizedName(item) }}</h3>
                                        <button class="remove-btn" (click)="removeItem(item)">
                                            <i class="pi pi-times"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <!-- Data Rows -->
                            <div class="table-row" *ngFor="let row of comparisonData">
                                <div class="table-cell feature-label">
                                    <span>{{ row.label }}</span>
                                </div>
                                <div class="table-cell" *ngFor="let item of items">
                                    <span class="cell-value">{{ row.values[item.id] || '-' }}</span>
                                </div>
                            </div>

                            <!-- Action Row -->
                            <div class="table-row action-row">
                                <div class="table-cell feature-label">
                                    <span>{{ languageService.language === 'ar' ? 'الإجراءات' : 'Actions' }}</span>
                                </div>
                                <div class="table-cell" *ngFor="let item of items">
                                    <button class="add-cart-btn" [routerLink]="['/item', item.id]">
                                        <i class="pi pi-shopping-cart"></i>
                                        {{ languageService.language === 'ar' ? 'عرض المنتج' : 'View Product' }}
                                    </button>
                                </div>
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

        .compare-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 1400px;
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
            margin-bottom: 1rem;
        }

        .hero-subtitle {
            color: rgba(255,255,255,0.7);
            font-size: 1.1rem;
            max-width: 600px;
            margin: 0 auto;
        }

        /* ============ MAIN SECTION ============ */
        .main-section {
            padding: 3rem 1rem;
        }

        /* ============ EMPTY STATE ============ */
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

        /* ============ COMPARISON TABLE ============ */
        .comparison-container {
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            overflow: hidden;
        }

        .comparison-table {
            width: 100%;
        }

        .table-row {
            display: grid;
            grid-template-columns: 200px repeat(auto-fill, minmax(200px, 1fr));
            border-bottom: 1px solid #e9ecef;
        }

        .table-row:last-child {
            border-bottom: none;
        }

        .header-row {
            background: #f8f9fa;
        }

        .table-cell {
            padding: 1.5rem;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .feature-label {
            background: #f8f9fa;
            font-weight: 700;
            color: #1a1a2e;
            justify-content: flex-start;
        }

        .product-cell {
            padding: 2rem 1rem;
        }

        .product-header {
            text-align: center;
            position: relative;
        }

        .product-image {
            width: 120px;
            height: 120px;
            margin: 0 auto 1rem;
            border-radius: 16px;
            overflow: hidden;
            cursor: pointer;
            transition: transform 0.3s;
        }

        .product-image:hover {
            transform: scale(1.05);
        }

        .product-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .product-name {
            font-size: 1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .remove-btn {
            position: absolute;
            top: -0.5rem;
            right: 0;
            width: 32px;
            height: 32px;
            background: rgba(239, 68, 68, 0.1);
            border: none;
            border-radius: 50%;
            color: #ef4444;
            cursor: pointer;
            transition: all 0.3s;
        }

        .remove-btn:hover {
            background: #ef4444;
            color: white;
        }

        .cell-value {
            font-weight: 500;
            color: #1a1a2e;
        }

        .action-row .table-cell {
            padding: 1.5rem;
        }

        .add-cart-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 10px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .add-cart-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.3);
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .table-row {
                grid-template-columns: 120px repeat(auto-fill, minmax(150px, 1fr));
            }
            .product-image {
                width: 80px;
                height: 80px;
            }
            .table-cell {
                padding: 1rem;
            }
        }
    `]
})
export class CompareComponent implements OnInit {
    items: Item[] = [];
    comparisonData: any[] = [];

    private itemsApiService = inject(ItemsApiService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.loadComparisonItems();
    }

    loadComparisonItems() {
        const storedIds = localStorage.getItem('compare_items');
        if (storedIds) {
            const ids = JSON.parse(storedIds);
            ids.forEach((id: string) => {
                this.itemsApiService.getItemById(id).subscribe({
                    next: (item) => {
                        this.items.push(item);
                        this.buildComparisonData();
                    }
                });
            });
        }
    }

    buildComparisonData() {
        const lang = this.languageService.language;
        this.comparisonData = [
            {
                label: this.languageService.translate('compare.price'),
                values: this.items.reduce((acc, item) => {
                    acc[item.id] = `${item.price} ${item.currency}`;
                    return acc;
                }, {} as any)
            },
            {
                label: this.languageService.translate('compare.brand'),
                values: this.items.reduce((acc, item) => {
                    acc[item.id] = item.brandNameEn || '-';
                    return acc;
                }, {} as any)
            },
            {
                label: this.languageService.translate('compare.rating'),
                values: this.items.reduce((acc, item) => {
                    acc[item.id] = item.averageRating.toFixed(1);
                    return acc;
                }, {} as any)
            }
        ];
    }

    removeItem(item: Item) {
        this.items = this.items.filter(i => i.id !== item.id);
        const ids = this.items.map(i => i.id);
        localStorage.setItem('compare_items', JSON.stringify(ids));
        this.buildComparisonData();
    }

    getItemImage(item: Item): string {
        if (item.mediaAssets && item.mediaAssets.length > 0) {
            const primaryImage = item.mediaAssets.find((m: any) => m.isPrimary || m.mediaType === 'Image') as { fileUrl?: string; mediaAssetUrl?: string } | undefined;
            if (primaryImage) {
                const url = primaryImage.fileUrl ?? primaryImage.mediaAssetUrl;
                if (url) return url;
            }
            const firstMedia = item.mediaAssets[0] as { fileUrl?: string; mediaAssetUrl?: string } | undefined;
            if (firstMedia) {
                const url = firstMedia.fileUrl ?? firstMedia.mediaAssetUrl;
                if (url) return url;
            }
        }
        return 'assets/img/defult.png';
    }
}
