import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { SoftwareApiService } from '../../../core/services/software-api.service';
import { SoftwareProduct } from '../../../shared/models/software.model';

@Component({
    selector: 'app-software-library',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        ButtonModule,
        InputTextModule,
        SelectModule,
        CardModule,
        TagModule,
        SkeletonModule,
        BadgeModule,
        TooltipModule
    ],
    template: `
        <div class="software-library">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-content">
                    <span class="hero-badge">{{ 'software.badge' | translate }}</span>
                    <h1>{{ 'software.title' | translate }}</h1>
                    <p>{{ 'software.subtitle' | translate }}</p>
                </div>
            </section>

            <!-- Filters Section -->
            <section class="filters-section">
                <div class="filters-container">
                    <div class="search-box">
                        <i class="pi pi-search"></i>
                        <input
                            type="text"
                            pInputText
                            [(ngModel)]="searchTerm"
                            (ngModelChange)="onSearchChange()"
                            [placeholder]="'software.searchPlaceholder' | translate"
                        />
                    </div>
                    <div class="filter-group">
                        <p-select
                            [(ngModel)]="selectedCategory"
                            [options]="categoryOptions"
                            (onChange)="applyFilters()"
                            [placeholder]="'software.allCategories' | translate"
                            [showClear]="true"
                            styleClass="filter-select"
                        ></p-select>
                        <p-select
                            [(ngModel)]="selectedOs"
                            [options]="osOptions"
                            (onChange)="applyFilters()"
                            [placeholder]="'software.allPlatforms' | translate"
                            [showClear]="true"
                            styleClass="filter-select"
                        ></p-select>
                    </div>
                </div>
            </section>

            <!-- Products Grid -->
            <section class="products-section">
                <div class="products-grid" *ngIf="!loading">
                    <div class="product-card" *ngFor="let product of products">
                        <div class="card-header">
                            <div class="product-icon">
                                <i [class]="product.iconKey || 'pi pi-box'"></i>
                            </div>
                            <div class="product-meta">
                                <span class="category-badge" *ngIf="product.category">{{ product.category }}</span>
                                <p-tag
                                    *ngIf="product.latestReleaseStatus"
                                    [value]="product.latestReleaseStatus"
                                    [severity]="getStatusSeverity(product.latestReleaseStatus)"
                                    [rounded]="true"
                                ></p-tag>
                            </div>
                        </div>
                        <div class="card-body">
                            <h3>{{ isRtl ? product.nameAr : product.nameEn }}</h3>
                            <p class="summary">{{ isRtl ? product.summaryAr : product.summaryEn }}</p>
                            <div class="version-info" *ngIf="product.latestVersion">
                                <span class="version-label">{{ 'software.latestVersion' | translate }}:</span>
                                <span class="version-number">v{{ product.latestVersion }}</span>
                            </div>
                        </div>
                        <div class="card-footer">
                            <div class="download-stats">
                                <i class="pi pi-download"></i>
                                <span>{{ formatDownloadCount(product.downloadCount || 0) }}</span>
                            </div>
                            <a [routerLink]="['/software', product.slug]" class="view-btn">
                                {{ 'software.viewDetails' | translate }}
                                <i class="pi pi-arrow-right"></i>
                            </a>
                        </div>
                    </div>
                </div>

                <!-- Loading Skeleton -->
                <div class="products-grid" *ngIf="loading">
                    <div class="product-card skeleton-card" *ngFor="let i of [1,2,3,4]">
                        <p-skeleton width="100%" height="200px"></p-skeleton>
                    </div>
                </div>

                <!-- Empty State -->
                <div class="empty-state" *ngIf="!loading && products.length === 0">
                    <i class="pi pi-inbox"></i>
                    <h3>{{ 'software.noProducts' | translate }}</h3>
                    <p>{{ 'software.noProductsDesc' | translate }}</p>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-hero: linear-gradient(145deg, #0c1445 0%, #1a1a2e 40%, #16213e 70%, #0f3460 100%);
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
        }

        .software-library {
            min-height: 100vh;
            background: #f8fafc;
        }

        .hero-section {
            background: var(--gradient-hero);
            padding: 5rem 2rem;
            text-align: center;
            color: white;
            position: relative;
            overflow: hidden;
        }

        .hero-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: 
                radial-gradient(circle at 20% 80%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 80% 20%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            pointer-events: none;
        }

        .hero-content {
            max-width: 800px;
            margin: 0 auto;
            position: relative;
            z-index: 1;
        }

        .hero-badge {
            display: inline-block;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.3) 0%, rgba(118, 75, 162, 0.3) 100%);
            border: 1px solid rgba(102, 126, 234, 0.4);
            padding: 0.5rem 1.25rem;
            border-radius: 20px;
            font-size: 0.85rem;
            margin-bottom: 1.5rem;
            color: #a5b4fc;
            font-weight: 500;
            letter-spacing: 0.05em;
        }

        .hero-content h1 {
            font-size: 2.75rem;
            font-weight: 800;
            margin-bottom: 1rem;
            background: linear-gradient(135deg, #ffffff 0%, #a5b4fc 50%, #667eea 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .hero-content p {
            font-size: 1.15rem;
            color: rgba(255, 255, 255, 0.8);
            max-width: 600px;
            margin: 0 auto;
            line-height: 1.6;
        }

        .filters-section {
            background: white;
            border-bottom: 1px solid #e5e7eb;
            padding: 1.5rem 2rem;
            position: sticky;
            top: 0;
            z-index: 100;
        }

        .filters-container {
            max-width: 1200px;
            margin: 0 auto;
            display: flex;
            gap: 1rem;
            flex-wrap: wrap;
            align-items: center;
        }

        .search-box {
            flex: 1;
            min-width: 250px;
            position: relative;
        }

        .search-box i {
            position: absolute;
            left: 1rem;
            top: 50%;
            transform: translateY(-50%);
            color: #9ca3af;
        }

        [dir="rtl"] .search-box i {
            left: auto;
            right: 1rem;
        }

        .search-box input {
            width: 100%;
            padding: 0.75rem 1rem 0.75rem 2.5rem;
            border: 1px solid #e5e7eb;
            border-radius: 8px;
            font-size: 0.95rem;
        }

        [dir="rtl"] .search-box input {
            padding: 0.75rem 2.5rem 0.75rem 1rem;
        }

        .filter-group {
            display: flex;
            gap: 0.75rem;
        }

        :host ::ng-deep .filter-select {
            min-width: 160px;
        }

        .products-section {
            max-width: 1200px;
            margin: 0 auto;
            padding: 2rem;
        }

        .products-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 1.5rem;
        }

        .product-card {
            background: white;
            border-radius: 12px;
            border: 1px solid #e5e7eb;
            overflow: hidden;
            transition: all 0.2s ease;
            display: flex;
            flex-direction: column;
        }

        .product-card:hover {
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
            transform: translateY(-4px);
        }

        .card-header {
            padding: 1.25rem;
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 1px solid #f3f4f6;
        }

        .product-icon {
            width: 48px;
            height: 48px;
            background: linear-gradient(135deg, #667eea11 0%, #764ba211 100%);
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .product-icon i {
            font-size: 1.5rem;
            color: #667eea;
        }

        .product-meta {
            display: flex;
            flex-direction: column;
            align-items: flex-end;
            gap: 0.5rem;
        }

        [dir="rtl"] .product-meta {
            align-items: flex-start;
        }

        .category-badge {
            font-size: 0.75rem;
            color: #6b7280;
            background: #f3f4f6;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
        }

        /* Status tag (Stable, Beta, etc.) – theme purple instead of green */
        :host ::ng-deep .p-tag.p-tag-success,
        :host ::ng-deep .p-tag[data-pc-severity="success"],
        :host ::ng-deep [data-p-severity="success"].p-tag {
            background: #667eea !important;
            background-color: #667eea !important;
            color: white !important;
        }

        .card-body {
            padding: 1.25rem;
            flex: 1;
        }

        .card-body h3 {
            font-size: 1.15rem;
            font-weight: 600;
            color: #1f2937;
            margin: 0 0 0.5rem 0;
        }

        .summary {
            font-size: 0.9rem;
            color: #6b7280;
            margin: 0 0 1rem 0;
            line-height: 1.5;
        }

        .version-info {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 0.85rem;
        }

        .version-label {
            color: #9ca3af;
        }

        .version-number {
            font-weight: 600;
            color: #667eea;
            background: #667eea11;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
        }

        .card-footer {
            padding: 1rem 1.25rem;
            border-top: 1px solid #f3f4f6;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .download-stats {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #9ca3af;
            font-size: 0.85rem;
        }

        .download-stats i {
            font-size: 0.9rem;
        }

        .view-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #667eea;
            text-decoration: none;
            font-size: 0.9rem;
            font-weight: 500;
            transition: all 0.2s ease;
        }

        .view-btn:hover {
            color: #764ba2;
        }

        .view-btn i {
            font-size: 0.8rem;
        }

        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            color: #6b7280;
        }

        .empty-state i {
            font-size: 3rem;
            color: #d1d5db;
            margin-bottom: 1rem;
        }

        .empty-state h3 {
            margin: 0 0 0.5rem 0;
            color: #374151;
        }

        .skeleton-card {
            padding: 0;
        }

        @media (max-width: 768px) {
            .hero-content h1 {
                font-size: 1.75rem;
            }

            .filters-container {
                flex-direction: column;
            }

            .search-box {
                width: 100%;
            }

            .filter-group {
                width: 100%;
                flex-wrap: wrap;
            }

            :host ::ng-deep .filter-select {
                flex: 1;
                min-width: 120px;
            }
        }
    `]
})
export class SoftwareLibraryComponent implements OnInit {
    private readonly softwareApi = inject(SoftwareApiService);
    private readonly translate = inject(TranslateService);

    products: SoftwareProduct[] = [];
    loading = true;
    searchTerm = '';
    selectedCategory: string | null = null;
    selectedOs: string | null = null;

    categoryOptions: { label: string; value: string }[] = [];
    osOptions = [
        { label: 'Windows', value: 'Windows' },
        { label: 'macOS', value: 'MacOS' },
        { label: 'Linux', value: 'Linux' }
    ];

    private searchTimeout: any;

    get isRtl(): boolean {
        return this.translate.currentLang === 'ar';
    }

    ngOnInit() {
        this.loadCategories();
        this.loadProducts();
    }

    loadCategories() {
        this.softwareApi.getCategories().subscribe({
            next: (categories) => {
                this.categoryOptions = categories.map(c => ({ label: c, value: c }));
            }
        });
    }

    loadProducts() {
        this.loading = true;
        this.softwareApi.getProducts(
            this.searchTerm || undefined,
            this.selectedCategory || undefined,
            this.selectedOs || undefined
        ).subscribe({
            next: (products) => {
                this.products = products;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onSearchChange() {
        clearTimeout(this.searchTimeout);
        this.searchTimeout = setTimeout(() => {
            this.loadProducts();
        }, 300);
    }

    applyFilters() {
        this.loadProducts();
    }

    getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
        switch (status?.toLowerCase()) {
            case 'stable': return 'success';
            case 'beta': return 'warn';
            case 'alpha': return 'info';
            case 'deprecated': return 'danger';
            default: return 'secondary';
        }
    }

    formatDownloadCount(count: number): string {
        if (count >= 1000000) {
            return (count / 1000000).toFixed(1) + 'M';
        }
        if (count >= 1000) {
            return (count / 1000).toFixed(1) + 'K';
        }
        return count.toString();
    }
}
