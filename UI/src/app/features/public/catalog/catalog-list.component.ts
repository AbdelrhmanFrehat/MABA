import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { AccordionModule } from 'primeng/accordion';
import { MultiSelectModule } from 'primeng/multiselect';
import { SliderModule } from 'primeng/slider';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { PaginatorModule } from 'primeng/paginator';
import { CatalogApiService } from '../../../shared/services/catalog-api.service';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { ProductCardComponent } from '../shared/product-card/product-card.component';
import { ProductQuickViewComponent } from '../shared/product-quick-view/product-quick-view.component';
import { Category, Brand, Tag } from '../../../shared/models/catalog.model';
import { Item } from '../../../shared/models/item.model';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { CartService } from '../../../shared/services/cart.service';
import { AuthService } from '../../../shared/services/auth.service';

interface CatalogFilters {
    categoryId?: string | null;
    brandId?: string | null;
    tagIds?: string[];
    minPrice?: number;
    maxPrice?: number;
    searchTerm?: string;
    inStock?: boolean;
    minRating?: number;
    isFeatured?: boolean;
}

type SortOption = {
    label: string;
    value: string;
    sortBy: string;
    sortOrder: 'asc' | 'desc';
};

@Component({
    selector: 'app-catalog-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        InputTextModule,
        ButtonModule,
        SelectModule,
        CheckboxModule,
        ProgressSpinnerModule,
        TooltipModule,
        AccordionModule,
        MultiSelectModule,
        SliderModule,
        TagModule,
        CardModule,
        PaginatorModule,
        ProductCardComponent,
        ProductQuickViewComponent,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="catalog-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-th-large"></i>
                        {{ languageService.language === 'ar' ? 'تصفح منتجاتنا' : 'Browse Products' }}
                    </span>
                    <h1 class="hero-title">{{ getCatalogHeading() }}</h1>
                    <p class="hero-description">
                        {{ languageService.language === 'ar' 
                            ? 'اكتشف مجموعتنا المتنوعة من المنتجات عالية الجودة'
                            : 'Discover our diverse collection of high-quality products' }}
                    </p>
                </div>
            </section>

            <!-- Breadcrumbs -->
            <nav class="breadcrumbs" aria-label="Breadcrumb">
                <div class="container">
                    <ol class="breadcrumb-list">
                        <li>
                            <a [routerLink]="['/']" class="breadcrumb-link">
                                <i class="pi pi-home"></i>
                                {{ 'common.home' | translate }}
                            </a>
                        </li>
                        <li class="breadcrumb-separator">/</li>
                        <li class="breadcrumb-current">{{ getCatalogHeading() }}</li>
                    </ol>
                </div>
            </nav>
            
            <div class="catalog-container">
                <!-- Filters Sidebar -->
                <aside class="filters-sidebar">
                    <div class="filters-header">
                        <h3>{{ 'catalog.filters' | translate }}</h3>
                        @if (activeFiltersCount > 0) {
                            <p-tag [value]="activeFiltersCount.toString()" severity="info" class="filter-count-badge"></p-tag>
                        }
                    </div>
                    
                    <!-- Mobile Filters Collapsible -->
                    <div class="mobile-filters">
                        <div class="mobile-filters-header" (click)="mobileFiltersExpanded = !mobileFiltersExpanded">
                            <h4>{{ 'catalog.filters' | translate }}</h4>
                            <i class="pi" [class.pi-chevron-down]="!mobileFiltersExpanded" [class.pi-chevron-up]="mobileFiltersExpanded"></i>
                        </div>
                        <div class="mobile-filters-content" [class.expanded]="mobileFiltersExpanded">
                            <ng-container *ngTemplateOutlet="filtersContent"></ng-container>
                        </div>
                    </div>

                    <!-- Desktop Filters -->
                    <div class="desktop-filters">
                        <ng-container *ngTemplateOutlet="filtersContent"></ng-container>
                    </div>
                </aside>

                <ng-template #filtersContent>
                    <!-- Search -->
                    <div class="filter-section">
                        <label>{{ 'catalog.search' | translate }}</label>
                        <input 
                            type="text" 
                            pInputText 
                            [(ngModel)]="filters.searchTerm"
                            [placeholder]="'catalog.searchPlaceholder' | translate"
                            (keyup.enter)="applyFilters()">
                    </div>

                    <!-- Categories -->
                    <div class="filter-section">
                        <label>{{ 'catalog.category' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.categoryId"
                            [options]="categoryOptions"
                            [placeholder]="'catalog.allCategories' | translate"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value">
                        </p-select>
                    </div>

                    <!-- Brands -->
                    <div class="filter-section">
                        <label>{{ 'catalog.brand' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.brandId"
                            [options]="brandOptions"
                            [placeholder]="'catalog.allBrands' | translate"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value">
                        </p-select>
                    </div>

                    <!-- Tags -->
                    <div class="filter-section">
                        <label>{{ 'catalog.tags' | translate }}</label>
                        <p-multiSelect
                            [(ngModel)]="filters.tagIds"
                            [options]="tagOptions"
                            [placeholder]="'catalog.selectTags' | translate"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            [display]="'chip'">
                        </p-multiSelect>
                    </div>

                    <!-- Price Range Slider -->
                    <div class="filter-section">
                        <label>{{ 'catalog.priceRange' | translate }}</label>
                        <div class="price-slider-container">
                            <p-slider
                                [(ngModel)]="priceRange"
                                [range]="true"
                                [min]="0"
                                [max]="maxPrice"
                                [step]="1">
                            </p-slider>
                            <div class="price-display">
                                <span>{{ priceRange[0] | number:'1.0-0' }} - {{ priceRange[1] | number:'1.0-0' }} {{ 'common.currency' | translate }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Stock Filter -->
                    <div class="filter-section">
                        <label>{{ 'catalog.availability' | translate }}</label>
                        <div class="checkbox-group">
                            <p-checkbox
                                [(ngModel)]="filters.inStock"
                                binary
                                inputId="inStock">
                            </p-checkbox>
                            <label for="inStock" class="checkbox-label">{{ 'catalog.inStock' | translate }}</label>
                        </div>
                    </div>

                    <!-- Rating Filter -->
                    <div class="filter-section">
                        <label>{{ 'catalog.minRating' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.minRating"
                            [options]="ratingOptions"
                            [placeholder]="'catalog.allRatings' | translate"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value">
                        </p-select>
                    </div>

                    <!-- Apply / Search button: apply all filters once -->
                    <p-button 
                        [label]="'catalog.applyFilters' | translate"
                        icon="pi pi-search"
                        iconPos="left"
                        styleClass="w-full mb-2"
                        (click)="applyFilters()">
                    </p-button>
                    <!-- Clear Filters -->
                    <p-button 
                        [label]="'catalog.clearFilters' | translate"
                        [outlined]="true"
                        styleClass="w-full"
                        [disabled]="activeFiltersCount === 0"
                        (click)="clearFilters()">
                    </p-button>
                </ng-template>

                <!-- Main Content -->
                <main class="catalog-main">
                    <div class="catalog-header">
                        <div>
                            <h1>{{ getCatalogHeading() }}</h1>
                            @if (activeFiltersCount > 0) {
                                <div class="active-filters">
                                    <span class="text-sm text-500">{{ 'catalog.activeFilters' | translate }}: </span>
                                    <p-tag 
                                        *ngFor="let filter of getActiveFilters()" 
                                        [value]="filter.label" 
                                        severity="info"
                                        [rounded]="true"
                                        styleClass="mr-2">
                                    </p-tag>
                                </div>
                            }
                        </div>
                        <div class="flex align-items-center gap-3">
                            <div class="results-count">
                                {{ totalRecords }} {{ 'catalog.productsFound' | translate }}
                            </div>
                            <div class="flex align-items-center gap-2">
                                <label class="text-sm">{{ 'catalog.sortBy' | translate }}:</label>
                                <p-select
                                    [(ngModel)]="selectedSort"
                                    [options]="sortOptions"
                                    optionLabel="label"
                                    (onChange)="applySort()"
                                    styleClass="w-12rem">
                                </p-select>
                            </div>
                            <div class="flex align-items-center gap-2">
                                <p-button 
                                    icon="pi pi-th-large"
                                    [outlined]="viewMode !== 'grid'"
                                    (click)="viewMode = 'grid'"
                                    [pTooltip]="'catalog.gridView' | translate">
                                </p-button>
                                <p-button 
                                    icon="pi pi-list"
                                    [outlined]="viewMode !== 'list'"
                                    (click)="viewMode = 'list'"
                                    [pTooltip]="'catalog.listView' | translate">
                                </p-button>
                            </div>
                        </div>
                    </div>

                    <div *ngIf="loading" class="loading-container">
                        <p-progressSpinner></p-progressSpinner>
                        <p>{{ 'catalog.loading' | translate }}</p>
                    </div>

                    <div *ngIf="!loading && items.length === 0" class="no-results">
                        <i class="pi pi-inbox text-6xl text-500 mb-4"></i>
                        <p>{{ 'catalog.noProductsFound' | translate }}</p>
                        <p-button 
                            [label]="'catalog.clearFilters' | translate"
                            [outlined]="true"
                            (click)="clearFilters()"
                            class="mt-3">
                        </p-button>
                    </div>

                    <div *ngIf="!loading && items.length > 0">
                        <div [class]="viewMode === 'grid' ? 'products-grid' : 'products-list'">
                            <app-product-card 
                                *ngFor="let item of items" 
                                [product]="item"
                                [viewMode]="viewMode"
                                (quickView)="openQuickView($event)"
                                (addToCart)="addToCart($event)">
                            </app-product-card>
                        </div>

                        <!-- Pagination -->
                        <div *ngIf="totalPages > 1" class="pagination-container mt-4">
                            <p-paginator
                                [first]="first"
                                [rows]="pageSize"
                                [totalRecords]="totalRecords"
                                [rowsPerPageOptions]="rowsPerPageOptions"
                                [showCurrentPageReport]="true"
                                [currentPageReportTemplate]="currentPageReportTemplate"
                                (onPageChange)="onPageChange($event)"
                                styleClass="custom-paginator">
                            </p-paginator>
                        </div>
                    </div>
                </main>
            </div>
        </div>

        <app-product-quick-view
            [(visible)]="quickViewVisible"
            [product]="selectedProduct"
            (addToCart)="onQuickViewAddToCart($event)">
        </app-product-quick-view>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .catalog-page {
            min-height: 60vh;
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
            max-width: 700px;
            margin: 0 auto;
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
            font-weight: 500;
            margin-bottom: 1.5rem;
        }

        .hero-title {
            font-size: clamp(2rem, 4vw, 3rem);
            font-weight: 800;
            color: white;
            margin-bottom: 0.75rem;
        }

        .hero-description {
            font-size: 1.1rem;
            color: rgba(255,255,255,0.8);
            line-height: 1.7;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .breadcrumbs {
            padding: 1.5rem 1rem;
            background: white;
            border-bottom: 1px solid #e9ecef;
        }

        .breadcrumb-list {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            list-style: none;
            padding: 0;
            margin: 0;
            flex-wrap: wrap;
            max-width: 1400px;
            margin: 0 auto;
        }

        .breadcrumb-link {
            color: #6c757d;
            text-decoration: none;
            display: flex;
            align-items: center;
            gap: 0.25rem;
            transition: color 0.2s;
        }

        .breadcrumb-link:hover {
            color: var(--color-primary);
        }

        .breadcrumb-separator {
            color: #6c757d;
        }

        .breadcrumb-current {
            color: #1a1a2e;
            font-weight: 600;
        }

        .catalog-container {
            max-width: 1400px;
            margin: 0 auto;
            display: grid;
            grid-template-columns: 300px 1fr;
            gap: 2rem;
            padding: 2rem 1rem;
        }

        .filters-sidebar {
            background: white;
            padding: 2rem;
            border-radius: 20px;
            height: fit-content;
            max-height: calc(100vh - 140px);
            overflow-y: auto;
            overflow-x: hidden;
            position: sticky;
            top: 100px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }
        .filters-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 1.5rem;
        }
        .filters-sidebar h3 {
            margin: 0;
        }
        .filter-count-badge {
            margin-left: 0.5rem;
        }
        .mobile-filters {
            display: none;
        }
        .mobile-filters-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1rem;
            background: var(--surface-100);
            border-radius: 8px;
            cursor: pointer;
            margin-bottom: 0.5rem;
        }
        .mobile-filters-header h4 {
            margin: 0;
            font-size: 1rem;
        }
        .mobile-filters-content {
            max-height: 0;
            overflow: hidden;
            transition: max-height 0.3s ease-out;
        }
        .mobile-filters-content.expanded {
            max-height: 2000px;
        }
        .desktop-filters {
            display: block;
        }
        .filter-section {
            margin-bottom: 1.5rem;
        }
        .filter-section label {
            display: block;
            margin-bottom: 0.5rem;
            font-weight: 600;
            font-size: 0.875rem;
        }
        /* Same width for all filter inputs and dropdowns */
        .filter-section input[type="text"],
        .filter-section p-select,
        .filter-section p-multiselect {
            display: block;
            width: 100%;
            box-sizing: border-box;
        }
        :host ::ng-deep .filter-section p-select .p-select,
        :host ::ng-deep .filter-section p-multiselect .p-multiselect {
            width: 100% !important;
        }
        /* Select root: flex so label and icons align in one row, icons on the right */
        :host ::ng-deep .filters-sidebar .p-select.p-inputwrapper,
        :host ::ng-deep .filters-sidebar .p-multiselect.p-inputwrapper {
            display: flex !important;
            align-items: center !important;
            flex-wrap: nowrap !important;
        }
        :host ::ng-deep .filters-sidebar .p-select .p-select-label,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-label,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-label-container {
            flex: 1 1 auto !important;
            min-width: 0 !important;
            overflow: visible;
            text-overflow: clip;
        }
        /* Clear (x) and dropdown arrow: keep on the right, theme color, aligned */
        :host ::ng-deep .filters-sidebar .p-select .p-select-clear-icon,
        :host ::ng-deep .filters-sidebar .p-select .p-select-dropdown-icon,
        :host ::ng-deep .filters-sidebar .p-select .p-select-dropdown,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-clear-icon,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-dropdown,
        :host ::ng-deep .filters-sidebar .p-multiselect .pi-chevron-down {
            flex-shrink: 0 !important;
            display: inline-flex !important;
            align-items: center !important;
            justify-content: center !important;
        }
        :host ::ng-deep .filters-sidebar .p-select .p-select-clear-icon,
        :host ::ng-deep .filters-sidebar .p-select .p-select-dropdown-icon svg,
        :host ::ng-deep .filters-sidebar .p-select .p-select-dropdown svg,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-clear-icon,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-dropdown .pi,
        :host ::ng-deep .filters-sidebar .p-multiselect .pi-chevron-down {
            color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-select .p-select-clear-icon svg,
        :host ::ng-deep .filters-sidebar .p-select .p-select-dropdown-icon svg,
        :host ::ng-deep .filters-sidebar .p-select [data-p-icon="times"],
        :host ::ng-deep .filters-sidebar .p-select [data-p-icon="chevron-down"] {
            width: 0.875rem !important;
            height: 0.875rem !important;
            color: var(--color-primary, #667eea) !important;
            fill: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-clear-icon svg,
        :host ::ng-deep .filters-sidebar .p-multiselect .p-multiselect-dropdown-icon svg,
        :host ::ng-deep .filters-sidebar .p-multiselect [data-p-icon="times"],
        :host ::ng-deep .filters-sidebar .p-multiselect [data-p-icon="chevron-down"] {
            width: 0.875rem !important;
            height: 0.875rem !important;
            color: var(--color-primary, #667eea) !important;
            fill: var(--color-primary, #667eea) !important;
        }
        /* Focus border: use theme color instead of green */
        :host ::ng-deep .filters-sidebar .p-select.p-inputwrapper-focus,
        :host ::ng-deep .filters-sidebar .p-select.p-focus,
        :host ::ng-deep .filters-sidebar .p-multiselect.p-inputwrapper-focus,
        :host ::ng-deep .filters-sidebar .p-multiselect.p-focus {
            border-color: var(--color-primary, #667eea) !important;
            box-shadow: 0 0 0 1px var(--color-primary, #667eea) !important;
        }
        .price-slider-container {
            padding: 0.5rem 0;
        }
        /* Price range slider: use page theme color instead of green */
        :host ::ng-deep .filters-sidebar .p-slider .p-slider-range {
            background: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-slider .p-slider-handle {
            background: var(--color-primary, #667eea) !important;
            border-color: var(--color-primary, #667eea) !important;
        }
        /* Search and Clear Filters buttons: theme purple */
        :host ::ng-deep .filters-sidebar .p-button:not(.p-button-outlined) {
            background: var(--color-primary, #667eea) !important;
            border-color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-button:not(.p-button-outlined):hover {
            background: var(--color-secondary, #764ba2) !important;
            border-color: var(--color-secondary, #764ba2) !important;
        }
        :host ::ng-deep .filters-sidebar .p-button.p-button-outlined {
            border-color: var(--color-primary, #667eea) !important;
            color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-button.p-button-outlined:hover {
            background: rgba(102, 126, 234, 0.08) !important;
        }
        .price-display {
            text-align: center;
            margin-top: 0.5rem;
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }
        .checkbox-group {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        .checkbox-label {
            cursor: pointer;
            margin: 0;
        }
        /* Availability checkbox: theme purple instead of green */
        :host ::ng-deep .filters-sidebar .p-checkbox .p-checkbox-box {
            border-color: var(--color-primary, #667eea);
        }
        :host ::ng-deep .filters-sidebar .p-checkbox.p-checkbox-checked .p-checkbox-box,
        :host ::ng-deep .filters-sidebar .p-checkbox .p-checkbox-box.p-highlight {
            background: var(--color-primary, #667eea) !important;
            border-color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .filters-sidebar .p-checkbox .p-checkbox-icon {
            color: white !important;
        }
        .active-filters {
            margin-top: 0.5rem;
            display: flex;
            align-items: center;
            flex-wrap: wrap;
            gap: 0.5rem;
        }
        .catalog-main {
            flex: 1;
        }
        .catalog-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 2rem;
            flex-wrap: wrap;
            gap: 1rem;
        }
        .catalog-header h1 {
            margin: 0;
        }
        /* Match font size and vertically center with dropdown/buttons */
        .results-count {
            color: var(--p-text-color, #495057);
            font-size: 1rem;
            font-weight: 500;
            display: flex;
            align-items: center;
            line-height: 1;
        }
        .catalog-header .flex.align-items-center label {
            color: var(--p-text-color, #495057);
            font-size: 1rem;
            font-weight: 600;
            display: flex;
            align-items: center;
            line-height: 1;
            margin: 0;
        }
        :host ::ng-deep .catalog-header .p-select.p-inputwrapper,
        :host ::ng-deep .catalog-main .catalog-header .p-select.p-inputwrapper {
            font-size: 1rem;
        }
        :host ::ng-deep .catalog-header .p-select .p-select-label,
        :host ::ng-deep .catalog-main .catalog-header .p-select .p-select-label {
            font-size: 1rem;
        }
        :host ::ng-deep .catalog-header .p-select.p-inputwrapper-focus,
        :host ::ng-deep .catalog-header .p-select .p-select-dropdown svg,
        :host ::ng-deep .catalog-header .p-select [data-p-icon="chevron-down"] {
            border-color: var(--color-primary, #667eea) !important;
            color: var(--color-primary, #667eea) !important;
            fill: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .catalog-header .p-select.p-inputwrapper-focus {
            box-shadow: 0 0 0 1px var(--color-primary, #667eea) !important;
        }
        /* Grid/List view buttons: theme purple instead of teal/green */
        :host ::ng-deep .catalog-header .p-button:not(.p-button-outlined) {
            background: var(--color-primary, #667eea) !important;
            border-color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .catalog-header .p-button:not(.p-button-outlined) .p-button-icon {
            color: white !important;
        }
        :host ::ng-deep .catalog-header .p-button.p-button-outlined {
            border-color: var(--color-primary, #667eea) !important;
            color: var(--color-primary, #667eea) !important;
        }
        :host ::ng-deep .catalog-header .p-button.p-button-outlined .p-button-icon {
            color: var(--color-primary, #667eea) !important;
        }
        .products-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 2rem;
            animation: fadeInGrid 0.4s ease-in;
        }

        :host ::ng-deep .products-grid app-product-card {
            --primary-color: #667eea;
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            transition: all 0.3s ease;
        }

        :host ::ng-deep .products-grid app-product-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
        }

        :host ::ng-deep .products-grid app-product-card .p-rating .p-rating-icon {
            color: #667eea;
        }
        :host ::ng-deep .products-grid app-product-card .p-rating .p-rating-icon.p-rating-icon-active {
            color: #667eea;
        }
        :host ::ng-deep .products-grid app-product-card .p-button.p-button-outlined {
            border-color: #667eea;
            color: #667eea;
        }
        :host ::ng-deep .products-grid app-product-card .p-button.p-button-outlined .p-button-icon {
            color: #667eea;
        }
        :host ::ng-deep .products-grid app-product-card .p-button:not(.p-button-outlined).p-button-primary,
        :host ::ng-deep .products-grid app-product-card .p-button:not(.p-button-text):not(.p-button-outlined) {
            background: #667eea;
            border-color: #667eea;
        }

        :host ::ng-deep .products-list app-product-card {
            --primary-color: #667eea;
        }
        :host ::ng-deep .products-list app-product-card .p-rating .p-rating-icon,
        :host ::ng-deep .products-list app-product-card .p-rating .p-rating-icon.p-rating-icon-active {
            color: #667eea;
        }
        :host ::ng-deep .products-list app-product-card .p-button.p-button-outlined {
            border-color: #667eea;
            color: #667eea;
        }
        :host ::ng-deep .products-list app-product-card .p-button.p-button-outlined .p-button-icon {
            color: #667eea;
        }
        :host ::ng-deep .products-list app-product-card .p-button:not(.p-button-outlined) {
            background: #667eea;
            border-color: #667eea;
        }

        @keyframes fadeInGrid {
            from {
                opacity: 0;
            }
            to {
                opacity: 1;
            }
        }
        .products-list {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            animation: fadeInList 0.4s ease-in;
        }
        @keyframes fadeInList {
            from {
                opacity: 0;
            }
            to {
                opacity: 1;
            }
        }
        .loading-container,
        .no-results {
            text-align: center;
            padding: 4rem 2rem;
        }
        .no-results p {
            font-size: 1.125rem;
            color: var(--text-color-secondary);
        }
        .pagination-container {
            display: flex;
            justify-content: center;
        }
        ::ng-deep .custom-paginator {
            background: transparent;
        }
        @media (max-width: 992px) {
            .catalog-page {
                padding: 1rem 0.5rem;
            }
            .catalog-container {
                grid-template-columns: 1fr;
                gap: 1rem;
            }
            .filters-sidebar {
                position: static;
                padding: 1rem;
                max-height: none;
                overflow: visible;
            }
            .mobile-filters {
                display: block;
            }
            .desktop-filters {
                display: none;
            }
            .products-grid {
                grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
                gap: 1rem;
            }
            .catalog-header {
                flex-direction: column;
                align-items: flex-start;
            }
            .catalog-header > div {
                width: 100%;
            }
        }
        @media (max-width: 576px) {
            .catalog-page {
                padding: 0.5rem;
            }
            .breadcrumbs {
                padding: 0 0.5rem;
                margin-bottom: 1rem;
            }
            .products-grid {
                grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
                gap: 0.75rem;
            }
        }
    `]
})
export class CatalogListComponent implements OnInit {
    items: Item[] = [];
    categories: Category[] = [];
    brands: Brand[] = [];
    tags: Tag[] = [];
    loading = false;
    filters: CatalogFilters = { categoryId: null, brandId: null };
    
    categoryOptions: any[] = [];
    brandOptions: any[] = [];
    tagOptions: any[] = [];
    ratingOptions: any[] = [
        { label: '4+ Stars', value: 4 },
        { label: '3+ Stars', value: 3 },
        { label: '2+ Stars', value: 2 },
        { label: '1+ Star', value: 1 }
    ];
    pageTitle = '';
    
    // Price Range
    priceRange: number[] = [0, 10000];
    maxPrice = 10000;
    
    // Pagination
    currentPage = 1;
    pageSize = 12;
    totalRecords = 0;
    
    // Mobile filters
    mobileFiltersExpanded = false;
    totalPages = 0;
    first = 0;
    rowsPerPageOptions = [12, 24, 48];
    currentPageReportTemplate = 'Showing {first} to {last} of {totalRecords} entries';
    
    // Sorting
    selectedSort: SortOption | null = null;
    sortOptions: SortOption[] = [
        { label: 'Newest', value: 'newest', sortBy: 'createdAt', sortOrder: 'desc' },
        { label: 'Price: Low to High', value: 'price-asc', sortBy: 'price', sortOrder: 'asc' },
        { label: 'Price: High to Low', value: 'price-desc', sortBy: 'price', sortOrder: 'desc' },
        { label: 'Best Selling', value: 'bestselling', sortBy: 'viewsCount', sortOrder: 'desc' },
        { label: 'Highest Rated', value: 'rating', sortBy: 'averageRating', sortOrder: 'desc' }
    ];
    
    // View mode
    viewMode: 'grid' | 'list' = 'grid';
    
    getCatalogHeading(): string {
        const fallback = this.languageService.language === 'ar' ? 'الفهرس' : 'Catalog';
        const translated = this.pageTitle || this.languageService.translate('catalog.title');
        return (translated && translated !== 'catalog.title') ? translated : fallback;
    }

    get activeFiltersCount(): number {
        let count = 0;
        if (this.filters.categoryId) count++;
        if (this.filters.brandId) count++;
        if (this.filters.tagIds && this.filters.tagIds.length > 0) count++;
        if (this.filters.minPrice || this.filters.maxPrice) count++;
        if (this.filters.inStock) count++;
        if (this.filters.minRating) count++;
        if (this.filters.searchTerm) count++;
        return count;
    }

    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private catalogApi = inject(CatalogApiService);
    private itemsApi = inject(ItemsApiService);
    private cartService = inject(CartService);
    private authService = inject(AuthService);
    private messageService = inject(MessageService);
    public languageService = inject(LanguageService);
    
    quickViewVisible = false;
    selectedProduct: Item | null = null;

    ngOnInit() {
        this.pageTitle = this.languageService.translate('catalog.title');
        this.selectedSort = this.sortOptions[0];
        this.loadFilters();
        this.setupRouteParams();
        this.loadItems();
    }

    setupRouteParams() {
        this.route.queryParams.subscribe(params => {
            if (params['category']) {
                this.filters.categoryId = params['category'];
            }
            if (params['brand']) {
                this.filters.brandId = params['brand'];
            }
            if (params['search']) {
                this.filters.searchTerm = params['search'];
            }
            if (params['page']) {
                this.currentPage = +params['page'];
            }
            if (params['isFeatured'] === 'true') {
                this.filters.isFeatured = true;
            }
        });

        this.route.params.subscribe(params => {
            const slug = params['slug'];
            const category = this.route.snapshot.url[0]?.path;
            
            if (category === 'category' && slug) {
                const foundCategory = this.categories.find(c => c.slug === slug);
                if (foundCategory) {
                    this.filters.categoryId = foundCategory.id;
                    this.pageTitle = this.languageService.getLocalizedName(foundCategory);
                }
            } else if (category === 'brand' && slug) {
                const foundBrand = this.brands.find(b => b.id === slug);
                if (foundBrand) {
                    this.filters.brandId = foundBrand.id;
                    this.pageTitle = this.languageService.getLocalizedName(foundBrand);
                } else {
                    this.pageTitle = this.languageService.translate('catalog.title');
                }
            } else {
                this.pageTitle = this.languageService.translate('catalog.title');
            }
        });
    }

    loadFilters() {
        this.catalogApi.getAllCategories(true, true).subscribe({
            next: (categories) => {
                this.categories = categories;
                const selectCategoryLabel = this.languageService.translate('catalog.allCategories');
                this.categoryOptions = [
                    { label: selectCategoryLabel, value: null },
                    ...categories.map(cat => ({
                        label: this.languageService.getLocalizedName(cat),
                        value: cat.id
                    }))
                ];
            }
        });

        this.catalogApi.getAllBrands(true).subscribe({
            next: (brands) => {
                this.brands = brands;
                const selectBrandLabel = this.languageService.translate('catalog.allBrands');
                this.brandOptions = [
                    { label: selectBrandLabel, value: null },
                    ...brands.map(brand => ({
                        label: this.languageService.getLocalizedName(brand),
                        value: brand.id
                    }))
                ];
            }
        });

        this.catalogApi.getAllTags(true).subscribe({
            next: (tags) => {
                this.tags = tags;
                const featuredOption = {
                    label: this.languageService.language === 'ar' ? '⭐ مميز' : '⭐ Featured',
                    value: '__featured__'
                };
                this.tagOptions = [
                    featuredOption,
                    ...tags.map(tag => ({
                        label: this.languageService.getLocalizedName(tag),
                        value: tag.id
                    }))
                ];
            }
        });
        
        // Load max price for slider from highest priced item in catalog.
        this.itemsApi.searchItems(undefined, {
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'price',
            sortDescending: true
        }).subscribe({
            next: (response) => {
                const highestPrice = response?.items?.[0]?.price;
                if (typeof highestPrice === 'number' && highestPrice > 0) {
                    this.maxPrice = Math.ceil(highestPrice / 100) * 100; // Round up to nearest 100
                    this.priceRange[1] = this.maxPrice;
                }
            }
        });
    }

    loadItems() {
        this.loading = true;
        const apiFilters: any = {
            pageNumber: this.currentPage,
            pageSize: this.pageSize
        };
        
        if (this.filters.categoryId != null && this.filters.categoryId !== '') apiFilters.categoryId = this.filters.categoryId;
        if (this.filters.brandId != null && this.filters.brandId !== '') apiFilters.brandId = this.filters.brandId;
        
        // Handle tags - check for special __featured__ tag
        if (this.filters.tagIds && this.filters.tagIds.length > 0) {
            const hasFeatured = this.filters.tagIds.includes('__featured__');
            const realTagIds = this.filters.tagIds.filter(id => id !== '__featured__');
            
            if (hasFeatured) {
                apiFilters.isFeatured = true;
            }
            if (realTagIds.length > 0) {
                apiFilters.tagId = realTagIds[0];
            }
        }
        
        if (this.filters.minPrice) apiFilters.minPrice = this.filters.minPrice;
        if (this.filters.maxPrice) apiFilters.maxPrice = this.filters.maxPrice;
        if (this.filters.inStock !== undefined && this.filters.inStock !== false) apiFilters.inStock = this.filters.inStock;
        if (this.filters.minRating) apiFilters.minRating = this.filters.minRating;
        if (this.filters.isFeatured !== undefined && this.filters.isFeatured !== false) apiFilters.isFeatured = this.filters.isFeatured;
        
        if (this.selectedSort) {
            apiFilters.sortBy = this.selectedSort.sortBy;
            apiFilters.sortDescending = this.selectedSort.sortOrder === 'desc';
        }

        // Always use search endpoint so we get proper server-side pagination (pageNumber + pageSize).
        // Empty searchTerm returns all items with filters applied.
        const searchTerm = this.filters.searchTerm?.trim() || undefined;
        this.itemsApi.searchItems(searchTerm, apiFilters).subscribe({
            next: (response) => {
                this.items = response.items ?? [];
                this.totalRecords = response.totalCount ?? 0;
                // If backend returned items but totalCount 0 (e.g. binding mismatch), show count from items
                if (this.totalRecords === 0 && this.items.length > 0) {
                    this.totalRecords = this.items.length;
                }
                this.totalPages = (response.totalPages ?? Math.ceil(this.totalRecords / this.pageSize)) || 1;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    applyFilters() {
        this.currentPage = 1;
        this.filters.minPrice = this.priceRange[0] > 0 ? this.priceRange[0] : undefined;
        this.filters.maxPrice = this.priceRange[1] < this.maxPrice ? this.priceRange[1] : undefined;
        this.updateUrl();
        this.loadItems();
    }

    applySort() {
        this.currentPage = 1;
        this.loadItems();
    }


    clearFilters() {
        this.filters = { categoryId: null, brandId: null, isFeatured: false, inStock: false };
        this.priceRange = [0, this.maxPrice];
        this.currentPage = 1;
        this.pageTitle = this.languageService.translate('catalog.title');
        this.router.navigate(['/catalog']);
        this.loadItems();
    }

    onPageChange(event: any) {
        this.first = event.first;
        this.pageSize = event.rows;
        this.currentPage = Math.floor(event.first / event.rows) + 1;
        this.updateUrl();
        this.loadItems();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.first = (this.currentPage - 1) * this.pageSize;
            this.updateUrl();
            this.loadItems();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    nextPage() {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.first = (this.currentPage - 1) * this.pageSize;
            this.updateUrl();
            this.loadItems();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    updateUrl() {
        const queryParams: any = {};
        if (this.filters.categoryId) queryParams.category = this.filters.categoryId;
        if (this.filters.brandId) queryParams.brand = this.filters.brandId;
        if (this.filters.searchTerm) queryParams.search = this.filters.searchTerm;
        if (this.currentPage > 1) queryParams.page = this.currentPage;
        if (this.filters.isFeatured) queryParams.isFeatured = 'true';
        
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams,
            queryParamsHandling: ''
        });
    }

    getLastRecordNumber(): number {
        return Math.min(this.currentPage * this.pageSize, this.totalRecords);
    }


    getActiveFilters(): Array<{ label: string }> {
        const activeFilters: Array<{ label: string }> = [];
        
        if (this.filters.categoryId) {
            const category = this.categories.find(c => c.id === this.filters.categoryId);
            if (category) {
                activeFilters.push({ label: this.languageService.getLocalizedName(category) });
            }
        }
        
        if (this.filters.brandId) {
            const brand = this.brands.find(b => b.id === this.filters.brandId);
            if (brand) {
                activeFilters.push({ label: this.languageService.getLocalizedName(brand) });
            }
        }
        
        if (this.filters.tagIds && this.filters.tagIds.length > 0) {
            const tagNames = this.filters.tagIds.map(tagId => {
                if (tagId === '__featured__') {
                    return this.languageService.language === 'ar' ? 'مميز' : 'Featured';
                }
                const tag = this.tags.find(t => t.id === tagId);
                return tag ? this.languageService.getLocalizedName(tag) : '';
            }).filter(name => name);
            if (tagNames.length > 0) {
                activeFilters.push({ label: tagNames.join(', ') });
            }
        }
        
        if (this.filters.minPrice || this.filters.maxPrice) {
            activeFilters.push({ 
                label: `${this.filters.minPrice || 0} - ${this.filters.maxPrice || this.maxPrice}` 
            });
        }
        
        if (this.filters.inStock) {
            activeFilters.push({ label: this.languageService.translate('catalog.inStock') });
        }
        
        if (this.filters.minRating) {
            activeFilters.push({ label: `${this.filters.minRating}+ ${this.languageService.translate('catalog.stars')}` });
        }

        return activeFilters;
    }

    openQuickView(item: Item) {
        this.selectedProduct = item;
        this.quickViewVisible = true;
    }

    onQuickViewAddToCart(event: { item: Item; quantity: number }) {
        this.addToCart(event.item, event.quantity);
    }

    addToCart(item: Item, quantity: number = 1) {
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
            return;
        }
        const request = {
            itemId: item.id,
            quantity: quantity
        };

        const itemDetails = {
            nameEn: item.nameEn || '',
            nameAr: item.nameAr || '',
            sku: item.sku || '',
            price: item.price,
            imageUrl: (item as any).primaryImageUrl || ''
        };

        this.cartService.addToCart(request, itemDetails).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.languageService.language === 'ar' ? 'تمت الإضافة' : 'Added',
                    detail: this.languageService.language === 'ar' 
                        ? `تمت إضافة "${item.nameAr || item.nameEn}" إلى السلة`
                        : `"${item.nameEn || item.nameAr}" added to cart`,
                    life: 3000
                });
            },
            error: (err) => {
                console.error('Error adding to cart:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: this.languageService.language === 'ar' ? 'خطأ' : 'Error',
                    detail: this.languageService.language === 'ar' 
                        ? 'حدث خطأ أثناء إضافة المنتج للسلة'
                        : 'Error adding item to cart',
                    life: 3000
                });
            }
        });
    }
}
