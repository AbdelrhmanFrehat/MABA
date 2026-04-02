import { Component, OnInit, AfterViewInit, OnDestroy, inject, ElementRef, effect, ViewChild } from '@angular/core';
import { Carousel } from 'primeng/carousel';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CarouselModule } from 'primeng/carousel';
import { CardModule } from 'primeng/card';
import { CatalogApiService } from '../../../shared/services/catalog-api.service';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { CmsApiService } from '../../../shared/services/cms-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Category } from '../../../shared/models/catalog.model';
import { Item } from '../../../shared/models/item.model';
import { HomePageContent, Banner, FeaturedCategory, FeaturedItem, Testimonial } from '../../../shared/models/cms.model';
import { LearnMoreDialogComponent } from '../shared/learn-more-dialog/learn-more-dialog.component';
import { HeroTickerComponent, HeroTickerItem } from './hero-ticker/hero-ticker.component';
import { HeroTickerApiService } from '../../../shared/services/hero-ticker-api.service';
import { SystemSettingsApiService } from '../../../shared/services/system-settings-api.service';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, CarouselModule, CardModule, LearnMoreDialogComponent, HeroTickerComponent],
    template: `
        <div class="home-page" [dir]="languageService.direction">
            <!-- Hero Section - Industrial Authority Focus -->
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="hero-circle hero-circle-1"></div>
                <div class="hero-circle hero-circle-2"></div>
                <div class="hero-circle hero-circle-3"></div>
                <app-hero-ticker [items]="heroTickerItems"></app-hero-ticker>
                <div class="hero-content">
                    <div class="hero-brand">MABA</div>
                    <h1 class="hero-main-title">
                        <span class="hero-title-line1">{{ languageService.language === 'ar' ? 'هندسة أنظمة عالية الأداء' : 'Engineering High-Performance Systems' }}</span>
                    </h1>
                    <p class="hero-tagline">{{ languageService.language === 'ar' ? 'روبوتات مخصصة، منصات CNC، أنظمة مدمجة، وحلول صناعية ذكية.' : 'Custom robotics, CNC platforms, embedded systems, and intelligent industrial solutions.' }}</p>
                    <div class="hero-actions">
                        <p-button
                            [label]="languageService.language === 'ar' ? 'ابدأ مشروعك' : 'Start a Project'"
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            routerLink="/projects"
                            styleClass="hero-btn-primary">
                        </p-button>
                        <p-button
                            [label]="languageService.language === 'ar' ? 'استكشف القدرات' : 'Explore Capabilities'"
                            icon="pi pi-cog"
                            iconPos="left"
                            [outlined]="true"
                            (onClick)="scrollToSection('capabilities')"
                            styleClass="hero-btn-secondary">
                        </p-button>
                    </div>
                </div>
            </section>

            <!-- Statistics Section -->
            <section id="stats" class="statistics-section home-section">
                <div class="container">
                    <div class="stats-grid">
                        <div class="stat-card" *ngFor="let stat of statistics; let i = index" [style.animation-delay]="(i * 0.1) + 's'">
                            <div class="stat-icon-wrapper">
                                <i [class]="stat.icon"></i>
                            </div>
                            <div class="stat-info">
                                <div class="stat-value">{{ stat.value }}+</div>
                                <div class="stat-label">{{ stat.label }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Engineering Divisions Section -->
            <section id="divisions" class="divisions-section home-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'أقسامنا' : 'Divisions' }}</span>
                        <h2 class="section-title">{{ languageService.language === 'ar' ? 'أقسام الهندسة' : 'Engineering Divisions' }}</h2>
                    </div>
                    <div class="divisions-grid">
                        <div class="division-card" *ngFor="let division of engineeringDivisions">
                            <div class="division-icon">
                                <i [class]="division.icon"></i>
                            </div>
                            <h3 class="division-title">{{ division.title }}</h3>
                            <ul class="division-list">
                                <li *ngFor="let item of division.items">{{ item }}</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Engineering Infrastructure Section -->
            <section id="capabilities" class="infrastructure-section home-section">
                <div class="container">
                    <div class="section-header section-header-light">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'البنية التحتية' : 'Capabilities' }}</span>
                        <h2 class="section-title section-title-light">{{ languageService.language === 'ar' ? 'البنية التحتية الهندسية' : 'Engineering Infrastructure' }}</h2>
                    </div>
                    <div class="parallelogram-strip">
                        <ng-container *ngFor="let cap of infrastructureCapabilities; let i = index; let first = first; let last = last">
                            <a *ngIf="cap.link" [routerLink]="cap.link" class="parallelogram-segment link-segment" [class.first]="first" [class.last]="last">
                                <div class="segment-content">
                                    <div class="segment-icon">
                                        <i [class]="cap.icon"></i>
                                    </div>
                                    <div class="segment-text">
                                        <span class="segment-title">{{ cap.label }}</span>
                                        <span class="segment-subtitle">{{ cap.subtitle }}</span>
                                    </div>
                                </div>
                            </a>
                            <div *ngIf="!cap.link" class="parallelogram-segment" [class.first]="first" [class.last]="last">
                                <div class="segment-content">
                                    <div class="segment-icon">
                                        <i [class]="cap.icon"></i>
                                    </div>
                                    <div class="segment-text">
                                        <span class="segment-title">{{ cap.label }}</span>
                                        <span class="segment-subtitle">{{ cap.subtitle }}</span>
                                    </div>
                                </div>
                            </div>
                        </ng-container>
                    </div>
                    <!-- Mobile fallback -->
                    <div class="infrastructure-mobile">
                        <ng-container *ngFor="let cap of infrastructureCapabilities">
                            <a *ngIf="cap.link" [routerLink]="cap.link" class="mobile-card link-card">
                                <div class="mobile-icon">
                                    <i [class]="cap.icon"></i>
                                </div>
                                <div class="mobile-text">
                                    <span class="mobile-title">{{ cap.label }}</span>
                                    <span class="mobile-subtitle">{{ cap.subtitle }}</span>
                                </div>
                            </a>
                            <div *ngIf="!cap.link" class="mobile-card">
                                <div class="mobile-icon">
                                    <i [class]="cap.icon"></i>
                                </div>
                                <div class="mobile-text">
                                    <span class="mobile-title">{{ cap.label }}</span>
                                    <span class="mobile-subtitle">{{ cap.subtitle }}</span>
                                </div>
                            </div>
                        </ng-container>
                    </div>
                </div>
            </section>

            <!-- Rapid Prototyping & Manufacturing Services -->
            <section class="manufacturing-section home-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'خدمات التصنيع' : 'Manufacturing' }}</span>
                        <h2 class="section-title">{{ languageService.language === 'ar' ? 'النماذج الأولية والتصنيع السريع' : 'Rapid Prototyping & Manufacturing Services' }}</h2>
                    </div>
                    <div class="manufacturing-grid">
                        <!-- Additive Manufacturing Card -->
                        <div class="manufacturing-card">
                            <div class="manufacturing-visual">
                                <div class="cube-animation">
                                    <div class="cube">
                                        <div class="face front"></div>
                                        <div class="face back"></div>
                                        <div class="face right"></div>
                                        <div class="face left"></div>
                                        <div class="face top"></div>
                                        <div class="face bottom"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="manufacturing-content">
                                <h3>{{ languageService.language === 'ar' ? 'الطباعة ثلاثية الأبعاد الصناعية' : 'Industrial 3D Printing' }}</h3>
                                <p>{{ languageService.language === 'ar' ? 'تصنيع إضافي عالي الدقة للمكونات الهيكلية والحاويات والتجميعات الميكانيكية.' : 'High-precision additive manufacturing for structural components, enclosures, and mechanical assemblies.' }}</p>
                                <p-button 
                                    [label]="languageService.language === 'ar' ? 'استكشف الطباعة ثلاثية الأبعاد' : 'Explore 3D Printing'"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/3d-print']"
                                    styleClass="manufacturing-button">
                                </p-button>
                            </div>
                        </div>
                        <!-- Laser Fabrication Card -->
                        <div class="manufacturing-card">
                            <div class="manufacturing-visual laser-visual">
                                <div class="laser-animation">
                                    <div class="laser-head">
                                        <div class="laser-nozzle"></div>
                                        <div class="laser-beam"></div>
                                    </div>
                                    <div class="laser-material"></div>
                                    <div class="laser-sparks">
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                    </div>
                                </div>
                            </div>
                            <div class="manufacturing-content">
                                <h3>{{ languageService.language === 'ar' ? 'التصنيع بالليزر الدقيق' : 'Precision Laser Fabrication' }}</h3>
                                <p>{{ languageService.language === 'ar' ? 'خدمات القطع والنقش بالليزر عالية الدقة للمواد المتنوعة.' : 'High-precision laser cutting and engraving services for various materials.' }}</p>
                                <p-button 
                                    [label]="languageService.language === 'ar' ? 'استكشف الخدمة' : 'Explore Service'"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/laser-engraving']"
                                    styleClass="manufacturing-button">
                                </p-button>
                            </div>
                        </div>
                        <!-- CNC Machining Card -->
                        <div class="manufacturing-card">
                            <div class="manufacturing-visual cnc-visual">
                                <div class="cnc-animation">
                                    <div class="cnc-gantry">
                                        <div class="cnc-spindle">
                                            <div class="cnc-bit"></div>
                                        </div>
                                    </div>
                                    <div class="cnc-workpiece">
                                        <div class="cnc-cut-path"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="manufacturing-content">
                                <h3>{{ languageService.language === 'ar' ? 'تشغيل CNC الدقيق' : 'Precision CNC Machining' }}</h3>
                                <p>{{ languageService.language === 'ar' ? 'خدمات التصنيع باستخدام الحاسب الآلي للمعادن والبلاستيك والمواد المركبة.' : 'Computer-controlled machining services for metals, plastics, and composite materials.' }}</p>
                                <p-button 
                                    [label]="languageService.language === 'ar' ? 'استكشف CNC' : 'Explore CNC'"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/cnc']"
                                    styleClass="manufacturing-button">
                                </p-button>
                            </div>
                        </div>
                        <!-- Design & CAD Card -->
                        <div class="manufacturing-card">
                            <div class="manufacturing-visual cad-visual">
                                <div class="cad-animation">
                                    <div class="cad-drawing">
                                        <div class="cad-outline"></div>
                                        <div class="cad-dim-h"></div>
                                        <div class="cad-dim-v"></div>
                                        <div class="cad-pencil">
                                            <i class="pi pi-pencil"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="manufacturing-content">
                                <h3>{{ languageService.language === 'ar' ? 'التصميم و CAD' : 'Design & CAD' }}</h3>
                                <p>{{ languageService.language === 'ar' ? 'تصميم ونمذجة CAD احترافية، من الفكرة إلى الملفات الجاهزة للتصنيع.' : 'Professional CAD design and modeling, from idea to manufacturing-ready files.' }}</p>
                                <p-button 
                                    [label]="languageService.language === 'ar' ? 'استكشف CAD' : 'Explore CAD'"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/design-cad']"
                                    styleClass="manufacturing-button">
                                </p-button>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Banners Carousel -->
            <section *ngIf="banners.length > 0" class="banners-section home-section">
                <div class="container">
                    <p-carousel 
                        [value]="banners" 
                        [numVisible]="1" 
                        [numScroll]="1" 
                        [circular]="true"
                        [autoplayInterval]="5000"
                        [responsiveOptions]="carouselResponsiveOptions"
                        styleClass="banner-carousel"
                    >
                        <ng-template #item let-banner>
                            <a [href]="banner.linkUrl || '#'" [routerLink]="banner.linkUrl ? null : []" class="banner-item">
                                <img [src]="banner.imageUrl" [alt]="getBannerTitle(banner)" class="banner-image" onerror="this.src='assets/img/defult.png'" />
                                <div *ngIf="getBannerTitle(banner)" class="banner-overlay">
                                    <h2>{{ getBannerTitle(banner) }}</h2>
                                </div>
                            </a>
                        </ng-template>
                    </p-carousel>
                </div>
            </section>

            <!-- Testimonials -->
            <section *ngIf="testimonials.length > 0" class="testimonials-section home-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'آراء العملاء' : 'Testimonials' }}</span>
                        <h2 class="section-title">{{ 'home.testimonials.title' | translate }}</h2>
                    </div>
                    <p-carousel 
                        [value]="testimonials" 
                        [numVisible]="3" 
                        [numScroll]="1" 
                        [circular]="true"
                        [responsiveOptions]="testimonialsResponsiveOptions"
                    >
                        <ng-template #item let-testimonial>
                            <div class="testimonial-card">
                                <div class="testimonial-rating">
                                    <i *ngFor="let i of [1,2,3,4,5]" 
                                       class="pi" 
                                       [class.pi-star-fill]="i <= testimonial.rating"
                                       [class.pi-star]="i > testimonial.rating">
                                    </i>
                                </div>
                                <p class="testimonial-text">"{{ getTestimonialText(testimonial) }}"</p>
                                <div class="testimonial-author">
                                    <img *ngIf="testimonial.imageUrl" [src]="testimonial.imageUrl" [alt]="getTestimonialName(testimonial)" class="testimonial-avatar" onerror="this.src='assets/img/defult.png'" />
                                    <div class="testimonial-info">
                                        <div class="testimonial-name">{{ getTestimonialName(testimonial) }}</div>
                                        <div *ngIf="testimonial.company" class="testimonial-company">{{ testimonial.company }}</div>
                                    </div>
                                </div>
                            </div>
                        </ng-template>
                    </p-carousel>
                </div>
            </section>

            <!-- Featured Products Section -->
            <section class="products-section home-section">
                <div class="container">
                    <div class="featured-section-wrapper">
                        <!-- Header -->
                        <div class="featured-header">
                            <span class="section-badge">{{ languageService.language === 'ar' ? 'منتجات مميزة' : 'Featured' }}</span>
                            <h2 class="section-title">{{ 'home.featured.title' | translate }}</h2>
                        </div>

                        <!-- Loading State -->
                        <div class="products-grid" *ngIf="productsLoading">
                            <div class="product-card skeleton-card" *ngFor="let i of [1,2,3,4]">
                                <div class="skeleton-image"></div>
                                <div class="skeleton-content">
                                    <div class="skeleton-line skeleton-title"></div>
                                    <div class="skeleton-line skeleton-price"></div>
                                    <div class="skeleton-line skeleton-button"></div>
                                </div>
                            </div>
                        </div>

                        <!-- Empty State -->
                        <div class="products-empty-state" *ngIf="!productsLoading && displayProducts.length === 0">
                            <i class="pi pi-box"></i>
                            <h3>{{ languageService.language === 'ar' ? 'لا توجد منتجات مميزة بعد' : 'No featured products yet' }}</h3>
                            <p-button
                                [label]="languageService.language === 'ar' ? 'تصفح الكتالوج' : 'Browse Catalog'"
                                routerLink="/catalog"
                                styleClass="empty-state-btn">
                            </p-button>
                        </div>

                        <!-- Desktop Carousel with Arrows (≥768px) -->
                        <div class="products-carousel-desktop" *ngIf="!productsLoading && displayProducts.length > 0">
                            <button type="button" class="custom-nav-btn prev-btn" (click)="scrollFeaturedPrev()">
                                <i class="pi pi-chevron-left"></i>
                            </button>
                            <p-carousel
                                #featuredCarousel
                                [value]="displayProducts"
                                [numVisible]="4"
                                [numScroll]="4"
                                [circular]="false"
                                [showNavigators]="false"
                                [showIndicators]="false"
                                [responsiveOptions]="featuredDesktopCarouselOptions"
                                styleClass="featured-desktop-carousel">
                                <ng-template #item let-product let-i="index">
                                    <div class="product-card" [style.animation-delay]="(i * 0.05) + 's'">
                                        <a [routerLink]="['/item', getProductId(product)]" class="product-link">
                                            <div class="product-image-wrapper">
                                                <img
                                                    [src]="getProductImageResolved(product)"
                                                    [alt]="getProductName(product)"
                                                    onerror="this.src='assets/img/defult.png'"
                                                />
                                                <span class="product-badge">
                                                    {{ languageService.language === 'ar' ? 'مميز' : 'Featured' }}
                                                </span>
                                            </div>
                                            <div class="product-content">
                                                <h3>{{ getProductName(product) }}</h3>
                                                <div *ngIf="getProductRating(product)" class="product-rating">
                                                    <i class="pi pi-star-fill"></i>
                                                    <span>{{ getProductRating(product) | number:'1.1-1' }}</span>
                                                    <span class="reviews-count" *ngIf="getProductReviewsCount(product)">({{ getProductReviewsCount(product) }})</span>
                                                </div>
                                                <div class="product-price">{{ formatCurrency(getProductPrice(product)) }}</div>
                                                <p-button
                                                    [label]="'home.featured.viewDetails' | translate"
                                                    styleClass="product-button w-full">
                                                </p-button>
                                            </div>
                                        </a>
                                    </div>
                                </ng-template>
                            </p-carousel>
                            <button type="button" class="custom-nav-btn next-btn" (click)="scrollFeaturedNext()">
                                <i class="pi pi-chevron-right"></i>
                            </button>
                        </div>

                        <!-- Mobile Carousel (<768px) -->
                        <div class="products-carousel-mobile" *ngIf="!productsLoading && displayProducts.length > 0">
                            <p-carousel
                                [value]="displayProducts"
                                [numVisible]="1"
                                [numScroll]="1"
                                [circular]="true"
                                [showNavigators]="true"
                                [showIndicators]="true"
                                [autoplayInterval]="5000"
                                styleClass="featured-mobile-carousel">
                                <ng-template #item let-product let-i="index">
                                    <div class="featured-mobile-card">
                                        <a [routerLink]="['/item', getProductId(product)]" class="featured-mobile-link">
                                            <!-- Image area: full width, single block, no inner frame -->
                                            <div class="featured-mobile-image">
                                                <img
                                                    [src]="getProductImageResolved(product)"
                                                    [alt]="getProductName(product)"
                                                    (error)="onProductImageError($event)"
                                                />
                                                <div class="featured-mobile-placeholder">
                                                    <i class="pi pi-image" aria-hidden="true"></i>
                                                    <span class="featured-mobile-placeholder-text">{{ languageService.language === 'ar' ? 'لا توجد صورة' : 'No image available' }}</span>
                                                </div>
                                                <span class="featured-mobile-badge">{{ languageService.language === 'ar' ? 'مميز' : 'Featured' }}</span>
                                            </div>
                                            <div class="featured-mobile-content">
                                                <h3>{{ getProductName(product) }}</h3>
                                                <div *ngIf="getProductRating(product)" class="featured-mobile-rating">
                                                    <i class="pi pi-star-fill"></i>
                                                    <span>{{ getProductRating(product) | number:'1.1-1' }}</span>
                                                    <span class="reviews-count" *ngIf="getProductReviewsCount(product)">({{ getProductReviewsCount(product) }})</span>
                                                </div>
                                                <div class="featured-mobile-price">{{ formatCurrency(getProductPrice(product)) }}</div>
                                                <p-button
                                                    [label]="'home.featured.viewDetails' | translate"
                                                    styleClass="featured-mobile-view-btn">
                                                </p-button>
                                            </div>
                                        </a>
                                    </div>
                                </ng-template>
                            </p-carousel>
                            <div class="carousel-swipe-hint" *ngIf="displayProducts.length > 1">
                                {{ languageService.language === 'ar' ? 'اسحب لمشاهدة المزيد' : 'Swipe to see more' }}
                            </div>
                        </div>

                        <!-- Browse Products Button -->
                        <div class="browse-products-cta" *ngIf="!productsLoading && displayProducts.length > 0">
                            <p-button
                                [label]="languageService.language === 'ar' ? 'تصفح المنتجات' : 'Browse Products'"
                                icon="pi pi-shopping-bag"
                                iconPos="left"
                                routerLink="/catalog"
                                styleClass="browse-products-btn">
                            </p-button>
                        </div>
                    </div>
                </div>
            </section>

            <!-- AI Assistant CTA -->
            <section class="cta-section home-section">
                <div class="cta-bg"></div>
                <div class="cta-particles">
                    <div class="particle" *ngFor="let i of [1,2,3,4,5,6,7,8]"></div>
                </div>
                <div class="container">
                    <div class="cta-content">
                        <h2 class="cta-title">{{ 'home.ai.title' | translate }}</h2>
                        <p class="cta-description">{{ 'home.ai.description' | translate }}</p>
                        <div class="cta-buttons">
                            <p-button
                                [label]="'home.ai.cta' | translate"
                                icon="pi pi-comments"
                                routerLink="/chat"
                                styleClass="cta-btn-primary">
                            </p-button>
                        </div>
                    </div>
                </div>
            </section>
            <app-learn-more-dialog [(visible)]="showLearnMoreDialog"></app-learn-more-dialog>
        </div>
    `,
    styles: [`
        :host {
            display: block;
            margin-top: -70px;
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-secondary: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-accent: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --color-accent: #00f2fe;
            --shadow-lg: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .home-page {
            width: 100%;
            overflow-x: hidden;
            background: #fafbfc;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION - Welcoming style ============ */
        .hero-section {
            position: relative;
            min-height: 70vh;
            max-height: 820px;
            box-sizing: border-box;
            display: flex;
            align-items: center;
            justify-content: center;
            /* Ticker sits just below header (moved up); content starts below ticker */
            padding: calc(var(--public-header-offset, 70px) + 108px + 1.5rem) 2rem 2.75rem;
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
        .hero-circle-1 {
            width: 450px;
            height: 450px;
            bottom: -140px;
            left: -120px;
        }
        .hero-circle-2 {
            width: 400px;
            height: 400px;
            top: -120px;
            right: -100px;
        }
        .hero-circle-3 {
            width: 320px;
            height: 320px;
            left: -60px;
            top: 45%;
            transform: translateY(-50%);
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
            max-width: 720px;
            margin: 0 auto;
        }

        .hero-brand {
            font-size: clamp(2.5rem, 8vw, 5rem);
            font-weight: 900;
            letter-spacing: 0.06em;
            background: linear-gradient(135deg, #ffffff 0%, #a5b4fc 50%, #667eea 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 0.4rem;
            text-shadow: 0 0 60px rgba(102, 126, 234, 0.5);
            line-height: 1;
            filter: drop-shadow(0 4px 20px rgba(102, 126, 234, 0.3));
        }

        .hero-main-title {
            margin-bottom: 1rem;
            line-height: 1.2;
        }
        .hero-title-line1 {
            display: block;
            font-size: clamp(1.35rem, 3.5vw, 2.25rem);
            font-weight: 700;
            color: rgba(255,255,255,0.95);
            letter-spacing: -0.01em;
            line-height: 1.3;
        }

        .hero-tagline {
            font-size: clamp(0.95rem, 1.8vw, 1.15rem);
            color: rgba(255,255,255,0.9);
            line-height: 1.6;
            margin-bottom: 1.75rem;
        }

        .hero-actions {
            display: flex;
            gap: 1.25rem;
            flex-wrap: wrap;
            justify-content: center;
        }

        :host ::ng-deep .hero-btn-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 1.75rem !important;
            font-size: 1rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
            color: white !important;
            box-shadow: var(--shadow-glow) !important;
            transition: all 0.3s ease !important;
        }
        :host ::ng-deep .hero-btn-primary:hover {
            transform: translateY(-1px) !important;
            box-shadow: 0 0 40px rgba(102, 126, 234, 0.5) !important;
        }

        :host ::ng-deep .hero-btn-secondary {
            background: transparent !important;
            border: 2px solid rgba(255,255,255,0.4) !important;
            color: white !important;
            padding: 1rem 1.75rem !important;
            font-size: 1rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
            transition: all 0.3s ease !important;
        }
        :host ::ng-deep .hero-btn-secondary:hover {
            background: rgba(255,255,255,0.1) !important;
            border-color: rgba(255,255,255,0.7) !important;
        }

        /* ============ SECTIONS (visible by default - no scroll reveal) ============ */
        .home-section {
            opacity: 1;
            transform: translateY(0);
        }
        .home-section .section-header {
            opacity: 1;
            transform: translateY(0);
        }
        .home-section .stat-card,
        .home-section .category-card,
        .home-section .product-card,
        .home-section .feature-card {
            opacity: 1;
            transform: translateY(0);
        }
        .home-section .print3d-card,
        .home-section .cta-content {
            opacity: 1;
            transform: translateY(0);
        }
        .home-section .banner-item,
        .home-section .testimonial-card {
            opacity: 1;
            transform: translateY(0);
        }

        /* ============ STATISTICS SECTION ============ */
        .statistics-section {
            padding: 5rem 2rem;
            background: linear-gradient(180deg, #0c1445 0%, #1a1a2e 100%);
            margin-top: -1px;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
        }

        .stat-card {
            display: flex;
            align-items: center;
            gap: 1.25rem;
            background: rgba(255,255,255,0.05);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.1);
            padding: 1.75rem;
            border-radius: 20px;
            animation: fadeIn 0.6s ease-out backwards;
            transition: all 0.3s ease;
        }

        .stat-card:hover {
            transform: translateY(-5px);
            background: rgba(255,255,255,0.08);
            border-color: rgba(102, 126, 234, 0.5);
        }

        .stat-icon-wrapper {
            width: 60px;
            height: 60px;
            background: var(--gradient-primary);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
            flex-shrink: 0;
        }

        .stat-info {
            flex: 1;
        }

        .stat-value {
            font-size: 2rem;
            font-weight: 800;
            color: white;
            line-height: 1;
            margin-bottom: 0.5rem;
        }

        .stat-label {
            font-size: 0.9rem;
            color: rgba(255,255,255,0.6);
        }

        /* ============ SECTION HEADER ============ */
        .section-header {
            text-align: center;
            margin-bottom: 4rem;
        }

        .section-badge {
            display: inline-block;
            padding: 0.5rem 1.25rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.875rem;
            font-weight: 600;
            letter-spacing: 0.02em;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 1rem;
        }

        .section-title {
            font-size: clamp(1.75rem, 4vw, 2.75rem);
            font-weight: 700;
            color: #1a1a2e;
            line-height: 1.25;
            letter-spacing: -0.01em;
        }

        /* ============ BANNERS SECTION ============ */
        .banners-section {
            padding: 4rem 2rem;
            background: white;
        }

        .banner-item {
            display: block;
            position: relative;
            width: 100%;
            height: 400px;
            overflow: hidden;
            border-radius: 24px;
        }

        .banner-image {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.5s ease;
        }

        .banner-item:hover .banner-image {
            transform: scale(1.05);
        }

        .banner-overlay {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            background: linear-gradient(to top, rgba(12, 20, 69, 0.9), rgba(26, 26, 46, 0.5), transparent);
            padding: 3rem 2rem 2rem;
            color: white;
        }

        .banner-overlay h2 {
            font-size: 2rem;
            font-weight: 700;
        }

        /* ============ PRODUCTS SECTION ============ */
        .products-section {
            padding: 4rem 2rem;
            background: linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%);
        }

        /* Featured Section Wrapper - Unified Container */
        .featured-section-wrapper {
            max-width: 1200px;
            margin: 0 auto;
            padding: 3rem 2.5rem;
            border-radius: 24px;
            background: linear-gradient(135deg, #ffffff 0%, #fafbfc 100%);
            border: 1px solid rgba(102, 126, 234, 0.1);
            box-shadow: 
                0 4px 24px rgba(102, 126, 234, 0.06),
                0 1px 3px rgba(0, 0, 0, 0.04);
            position: relative;
        }

        .featured-header {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.75rem;
            margin-bottom: 2rem;
            text-align: center;
        }

        .featured-header .section-badge {
            margin: 0 auto;
        }

        .featured-header .section-title {
            margin: 0;
        }

        .section-header-row {
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-bottom: 2rem;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .section-header-left {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .section-header-row .section-badge {
            align-self: flex-start;
        }

        .section-header-row .section-title {
            margin: 0;
        }

        .view-all-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            color: var(--color-primary);
            text-decoration: none;
            font-weight: 600;
            font-size: 0.95rem;
            padding: 0.75rem 1.25rem;
            border: 2px solid var(--color-primary);
            border-radius: 50px;
            transition: all 0.3s ease;
        }

        .view-all-btn:hover {
            background: var(--gradient-primary);
            color: white;
            transform: translateX(4px);
        }

        [dir="rtl"] .view-all-btn:hover {
            transform: translateX(-4px);
        }

        .view-all-btn i {
            font-size: 0.85rem;
            transition: transform 0.3s ease;
        }

        [dir="rtl"] .view-all-btn i {
            transform: rotate(180deg);
        }

        .view-all-btn:hover i {
            transform: translateX(4px);
        }

        [dir="rtl"] .view-all-btn:hover i {
            transform: translateX(-4px) rotate(180deg);
        }

        /* Products Grid (for loading skeleton) */
        .products-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 1.5rem;
        }

        /* Desktop Carousel */
        .products-carousel-desktop {
            display: block;
            position: relative;
            padding: 0 3.5rem;
        }

        /* Hide desktop carousel on mobile, show mobile carousel */
        .products-carousel-mobile {
            display: none;
        }

        /* Desktop Carousel Styling */
        :host ::ng-deep .featured-desktop-carousel {
            padding: 0;
            position: relative;
        }

        :host ::ng-deep .featured-desktop-carousel .p-carousel-item {
            padding: 0.5rem 0.75rem;
        }

        :host ::ng-deep .featured-desktop-carousel .p-carousel-content {
            padding: 0.5rem 0;
        }

        :host ::ng-deep .featured-desktop-carousel .p-carousel-items-container {
            padding: 0.5rem 0;
        }

        /* Custom Navigation Buttons */
        .custom-nav-btn {
            position: absolute;
            top: 50%;
            transform: translateY(-50%);
            width: 48px;
            height: 48px;
            background: white;
            border: 2px solid #e0e0e0;
            border-radius: 50%;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            z-index: 10;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
        }

        .custom-nav-btn i {
            font-size: 1.25rem;
            color: #667eea;
            transition: color 0.3s ease;
        }

        .custom-nav-btn.prev-btn {
            left: 0;
        }

        .custom-nav-btn.next-btn {
            right: 0;
        }

        .custom-nav-btn:hover {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-color: transparent;
            transform: translateY(-50%) scale(1.08);
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
        }

        .custom-nav-btn:hover i {
            color: white;
        }

        /* RTL support */
        [dir="rtl"] .custom-nav-btn.prev-btn {
            left: auto;
            right: 0;
        }

        [dir="rtl"] .custom-nav-btn.next-btn {
            right: auto;
            left: 0;
        }

        /* Browse Products CTA Button */
        .browse-products-cta {
            text-align: center;
            margin-top: 3rem;
            padding-top: 1rem;
        }

        :host ::ng-deep .browse-products-btn {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            border: none !important;
            padding: 1rem 3rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: 0 8px 30px rgba(102, 126, 234, 0.35) !important;
            transition: all 0.3s ease !important;
            letter-spacing: 0.02em !important;
        }

        :host ::ng-deep .browse-products-btn:hover {
            transform: translateY(-4px) !important;
            box-shadow: 0 15px 40px rgba(102, 126, 234, 0.5) !important;
        }

        :host ::ng-deep .browse-products-btn .p-button-icon {
            margin-right: 0.5rem !important;
        }

        [dir="rtl"] :host ::ng-deep .browse-products-btn .p-button-icon {
            margin-right: 0 !important;
            margin-left: 0.5rem !important;
        }

        @media (max-width: 1024px) {
            .products-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        @media (max-width: 768px) {
            .products-carousel-desktop {
                display: none;
            }
            .products-carousel-mobile {
                display: block;
            }
            .section-header-row {
                flex-direction: column;
                align-items: flex-start;
            }
            /* Featured section container – one clean card, radius 20px */
            .featured-section-wrapper {
                padding: 1.5rem 1rem;
                border-radius: 20px;
                border: 1px solid rgba(102, 126, 234, 0.08);
                box-shadow: 0 2px 16px rgba(102, 126, 234, 0.06);
            }
            .featured-header {
                margin-bottom: 1.25rem;
            }
            /* Mobile carousel – padding for arrows */
            :host ::ng-deep .featured-mobile-carousel {
                padding: 0 2.5rem 0.5rem;
            }
            :host ::ng-deep .featured-mobile-carousel .p-carousel-item {
                padding: 0 0.5rem;
            }
            :host ::ng-deep .featured-mobile-carousel .p-carousel-content {
                padding: 0.35rem 0;
            }
            /* Theme-consistent arrows: small, subtle, ~40px, no heavy shadow */
            :host ::ng-deep .featured-mobile-carousel .p-carousel-prev,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-next,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-prev-button,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-next-button {
                width: 40px !important;
                height: 40px !important;
                min-width: 40px !important;
                min-height: 40px !important;
                background: rgba(255, 255, 255, 0.95) !important;
                border: 1px solid rgba(102, 126, 234, 0.2) !important;
                border-radius: 50% !important;
                box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06) !important;
                color: var(--color-primary) !important;
            }
            :host ::ng-deep .featured-mobile-carousel .p-carousel-prev:hover,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-next:hover,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-prev-button:hover,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-next-button:hover {
                background: rgba(102, 126, 234, 0.08) !important;
                border-color: rgba(102, 126, 234, 0.35) !important;
                color: var(--color-primary) !important;
                box-shadow: 0 2px 8px rgba(102, 126, 234, 0.12) !important;
            }
            [dir="rtl"] :host ::ng-deep .featured-mobile-carousel .p-carousel-prev i,
            [dir="rtl"] :host ::ng-deep .featured-mobile-carousel .p-carousel-prev-button i {
                transform: scaleX(-1);
            }
            [dir="rtl"] :host ::ng-deep .featured-mobile-carousel .p-carousel-next i,
            [dir="rtl"] :host ::ng-deep .featured-mobile-carousel .p-carousel-next-button i {
                transform: scaleX(-1);
            }
            /* Dots – smaller, less spacing */
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator-list {
                gap: 0.35rem;
                margin-top: 0.75rem;
            }
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator button,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator-button {
                width: 8px !important;
                height: 8px !important;
                border-radius: 50%;
                background: #d1d5db !important;
                transition: all 0.2s ease;
            }
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator.p-highlight button,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator.p-highlight .p-carousel-indicator-button,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator.p-carousel-indicator-active button,
            :host ::ng-deep .featured-mobile-carousel .p-carousel-indicator.p-carousel-indicator-active .p-carousel-indicator-button {
                background: var(--gradient-primary) !important;
                width: 14px !important;
                border-radius: 4px;
            }
            /* Swipe hint – very subtle, only when multiple items (handled in template) */
            .carousel-swipe-hint {
                text-align: center;
                margin-top: 0.5rem;
                font-size: 0.7rem;
                color: #9ca3af;
                opacity: 0.5;
            }
            /* Featured mobile card – flat structure: [ Image full width ] Title Rating Price Button */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-card {
                background: #fff;
                border-radius: 16px;
                overflow: hidden;
                border: 1px solid rgba(0, 0, 0, 0.06);
                box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-link {
                display: block;
                text-decoration: none;
                color: inherit;
            }
            /* Image area: 100% card width, single block – tall so image is main focus */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image {
                position: relative;
                width: 100%;
                height: 220px;
                min-height: 220px;
                margin: 0;
                padding: 0;
                display: flex;
                align-items: center;
                justify-content: center;
                background: linear-gradient(180deg, #f8f9fc 0%, #eef1f7 100%);
                border-radius: 16px 16px 0 0;
                border: 1px solid rgba(0, 0, 0, 0.06);
                border-bottom: none;
                box-sizing: border-box;
                overflow: hidden;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image img {
                width: 100%;
                height: 100%;
                max-width: 100%;
                max-height: 100%;
                object-fit: contain;
                object-position: center;
                padding: 10px;
                box-sizing: border-box;
            }
            /* Placeholder: hidden by default; shown when .product-image-placeholder set by onProductImageError */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-placeholder {
                display: none;
                position: absolute;
                inset: 0;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                gap: 0.5rem;
                pointer-events: none;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image.product-image-placeholder .featured-mobile-placeholder {
                display: flex;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image.product-image-placeholder .featured-mobile-placeholder i {
                font-size: 56px;
                color: rgba(102, 126, 234, 0.25);
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image.product-image-placeholder .featured-mobile-placeholder-text {
                font-size: 0.75rem;
                color: #9ca3af;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-image.product-image-placeholder img {
                display: none;
            }
            /* Featured badge – top-right of image area */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-badge {
                position: absolute;
                top: 10px;
                right: 10px;
                padding: 0.35rem 0.75rem;
                background: var(--gradient-primary);
                color: white;
                border-radius: 50px;
                font-size: 0.7rem;
                font-weight: 600;
            }
            [dir="rtl"] :host ::ng-deep .products-carousel-mobile .featured-mobile-badge {
                right: auto;
                left: 10px;
            }
            /* Content: title, rating, price, button */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-content {
                padding: 0.875rem 1rem 1rem;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-content h3 {
                font-size: 0.95rem;
                font-weight: 700;
                color: #1a1a2e;
                margin: 0 0 0.35rem 0;
                line-height: 1.35;
                display: -webkit-box;
                -webkit-line-clamp: 2;
                -webkit-box-orient: vertical;
                overflow: hidden;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-rating {
                display: flex;
                align-items: center;
                gap: 0.35rem;
                margin-bottom: 0.35rem;
                font-size: 0.8rem;
                color: #FFB800;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-rating .reviews-count {
                color: #6c757d;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-price {
                margin-bottom: 0.5rem;
                font-size: 1.1rem;
                font-weight: 600;
                color: #667eea !important;
            }
            /* View Details – small button, theme purple (override any green/success from PrimeNG) */
            :host ::ng-deep .products-carousel-mobile .featured-mobile-view-btn,
            :host ::ng-deep .products-carousel-mobile .featured-mobile-view-btn .p-button {
                padding: 0.45rem 0.85rem !important;
                font-size: 0.85rem !important;
                font-weight: 600 !important;
                border-radius: 10px !important;
                width: 100%;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
                background-color: #667eea !important;
                border: none !important;
                color: #fff !important;
            }
            :host ::ng-deep .products-carousel-mobile .featured-mobile-view-btn .p-button:hover,
            :host ::ng-deep .products-carousel-mobile .featured-mobile-view-btn:hover .p-button {
                background: linear-gradient(135deg, #5a67d8 0%, #6b46c1 100%) !important;
                background-color: #5a67d8 !important;
                color: #fff !important;
            }
            /* Browse Products CTA – closer, slightly smaller */
            .browse-products-cta {
                margin-top: 1rem;
                padding-top: 0.25rem;
            }
            :host ::ng-deep .browse-products-btn,
            :host ::ng-deep .browse-products-btn .p-button {
                width: 100%;
                padding: 0.65rem 1.5rem !important;
                font-size: 0.9rem !important;
                border-radius: 12px !important;
            }
        }

        /* Loading Skeleton */
        .skeleton-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
        }

        .skeleton-image {
            height: 200px;
            background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
            background-size: 200% 100%;
            animation: shimmer 1.5s infinite;
        }

        .skeleton-content {
            padding: 1.5rem;
        }

        .skeleton-line {
            background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
            background-size: 200% 100%;
            animation: shimmer 1.5s infinite;
            border-radius: 4px;
            margin-bottom: 0.75rem;
        }

        .skeleton-title {
            height: 1.25rem;
            width: 80%;
        }

        .skeleton-price {
            height: 1.5rem;
            width: 40%;
        }

        .skeleton-button {
            height: 2.5rem;
            width: 100%;
            margin-bottom: 0;
        }

        @keyframes shimmer {
            0% { background-position: -200% 0; }
            100% { background-position: 200% 0; }
        }

        /* Empty State */
        .products-empty-state {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 20px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.05);
        }

        .products-empty-state i {
            font-size: 4rem;
            color: #d1d5db;
            margin-bottom: 1rem;
        }

        .products-empty-state h3 {
            font-size: 1.25rem;
            color: #374151;
            margin-bottom: 1.5rem;
        }

        :host ::ng-deep .empty-state-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
        }

        /* Mobile Carousel – base (desktop may use .mobile-card in responsive too; mobile-only overrides below in @media) */
        .mobile-card {
            max-width: 100%;
        }

        .product-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            animation: fadeIn 0.6s ease-out backwards;
            transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            flex-direction: column;
            height: 100%;
            border: 1px solid rgba(0,0,0,0.04);
        }

        .product-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 25px 50px rgba(102, 126, 234, 0.18);
            border-color: rgba(102, 126, 234, 0.15);
        }

        .product-link {
            text-decoration: none;
            color: inherit;
            display: flex;
            flex-direction: column;
            height: 100%;
        }

        .product-image-wrapper {
            position: relative;
            height: 200px;
            overflow: hidden;
            flex-shrink: 0;
        }

        .product-image-wrapper img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.5s ease;
        }

        .product-card:hover .product-image-wrapper img {
            transform: scale(1.1);
        }

        .product-badge {
            position: absolute;
            top: 1rem;
            right: 1rem;
            padding: 0.35rem 0.85rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        [dir="rtl"] .product-badge {
            right: auto;
            left: 1rem;
        }

        .product-content {
            padding: 1.25rem;
            flex: 1;
            display: flex;
            flex-direction: column;
        }

        .product-content h3 {
            font-size: 1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
            line-height: 1.4;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            min-height: 2.8em;
        }

        .product-rating {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 0.75rem;
            color: #FFB800;
            font-size: 0.875rem;
        }

        .reviews-count {
            color: #6c757d;
        }

        .product-price {
            font-size: 1.25rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 0.75rem;
            margin-top: auto;
        }

        :host ::ng-deep .product-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
            font-weight: 600 !important;
        }

        /* ============ TESTIMONIALS SECTION ============ */
        .testimonials-section {
            padding: 6rem 2rem;
            background: white;
        }

        .testimonial-card {
            background: #f8f9fa;
            padding: 2.5rem;
            border-radius: 20px;
            margin: 0 1rem;
            text-align: center;
            border: 2px solid transparent;
            transition: all 0.3s ease;
            position: relative;
        }

        .testimonial-card::before {
            content: '"';
            position: absolute;
            top: 1rem;
            left: 1.5rem;
            font-size: 4rem;
            color: var(--color-primary);
            opacity: 0.2;
            font-family: serif;
            line-height: 1;
        }

        .testimonial-card:hover {
            transform: translateY(-5px);
            background: white;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            border-color: var(--color-primary);
        }

        .testimonial-rating {
            color: #FFB800;
            margin-bottom: 1.5rem;
            font-size: 1.25rem;
        }

        .testimonial-text {
            font-style: italic;
            margin-bottom: 2rem;
            color: #495057;
            font-size: 1.1rem;
            line-height: 1.8;
            position: relative;
            z-index: 1;
        }

        .testimonial-author {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 1rem;
        }

        .testimonial-avatar {
            width: 60px;
            height: 60px;
            border-radius: 50%;
            object-fit: cover;
            border: 3px solid var(--color-primary);
        }

        .testimonial-info {
            text-align: start;
        }

        .testimonial-name {
            font-weight: 700;
            color: #1a1a2e;
        }

        .testimonial-company {
            font-size: 0.875rem;
            color: #6c757d;
        }

        /* ============ ENGINEERING DIVISIONS SECTION ============ */
        .divisions-section {
            padding: 6rem 2rem;
            background: white;
        }

        .divisions-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 2rem;
        }

        .division-card {
            background: #f8f9fa;
            padding: 2.5rem;
            border-radius: 20px;
            border: 2px solid transparent;
            transition: all 0.3s ease;
        }

        .division-card:hover {
            background: white;
            transform: translateY(-8px);
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            border-color: var(--color-primary);
        }

        .division-icon {
            width: 64px;
            height: 64px;
            background: var(--gradient-primary);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.75rem;
            color: white;
            margin-bottom: 1.5rem;
            box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
        }

        .division-title {
            font-size: 1.35rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.25rem;
        }

        .division-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .division-list li {
            position: relative;
            padding-left: 1.5rem;
            margin-bottom: 0.75rem;
            color: #495057;
            font-size: 0.95rem;
            line-height: 1.5;
        }

        .division-list li::before {
            content: '';
            position: absolute;
            left: 0;
            top: 0.5rem;
            width: 6px;
            height: 6px;
            background: var(--color-primary);
            border-radius: 50%;
        }

        [dir="rtl"] .division-list li {
            padding-left: 0;
            padding-right: 1.5rem;
        }

        [dir="rtl"] .division-list li::before {
            left: auto;
            right: 0;
        }

        /* ============ INFRASTRUCTURE SECTION ============ */
        .infrastructure-section {
            padding: 6rem 2rem;
            background: var(--gradient-dark);
        }

        .section-header-light .section-badge {
            background: rgba(255,255,255,0.15);
        }

        .section-title-light {
            color: white !important;
        }

        /* Parallelogram Strip Design */
        .parallelogram-strip {
            display: flex;
            align-items: stretch;
            gap: 4px;
            padding: 0 2rem;
        }

        .parallelogram-segment {
            flex: 1;
            position: relative;
            min-height: 140px;
            cursor: default;
        }

        a.parallelogram-segment.link-segment {
            text-decoration: none;
            color: inherit;
            cursor: pointer;
        }

        a.parallelogram-segment.link-segment:hover::before {
            background: rgba(255, 255, 255, 0.1);
        }

        .parallelogram-segment::before {
            content: '';
            position: absolute;
            inset: 0;
            background: rgba(255, 255, 255, 0.04);
            backdrop-filter: blur(16px);
            -webkit-backdrop-filter: blur(16px);
            border: 1px solid rgba(255, 255, 255, 0.08);
            transform: skewX(-8deg);
            transition: all 0.3s ease;
        }

        .parallelogram-segment.first::before {
            border-radius: 12px 0 0 12px;
        }

        .parallelogram-segment.last::before {
            border-radius: 0 12px 12px 0;
        }

        .parallelogram-segment:hover::before {
            background: rgba(255, 255, 255, 0.07);
            border-color: rgba(102, 126, 234, 0.3);
            box-shadow: 0 0 20px rgba(102, 126, 234, 0.15);
        }

        /* RTL: flip skew direction */
        [dir="rtl"] .parallelogram-segment::before {
            transform: skewX(8deg);
        }

        [dir="rtl"] .parallelogram-segment.first::before {
            border-radius: 0 12px 12px 0;
        }

        [dir="rtl"] .parallelogram-segment.last::before {
            border-radius: 12px 0 0 12px;
        }

        .segment-content {
            position: relative;
            z-index: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            padding: 1.5rem 1rem;
            height: 100%;
        }

        .segment-icon {
            width: 48px;
            height: 48px;
            background: var(--gradient-primary);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            color: white;
            margin-bottom: 1rem;
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
            border: 2px solid rgba(255, 255, 255, 0.1);
        }

        .segment-text {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.25rem;
        }

        .segment-title {
            font-size: 0.9rem;
            font-weight: 600;
            color: rgba(255, 255, 255, 0.95);
            line-height: 1.3;
            max-width: 140px;
        }

        .segment-subtitle {
            font-size: 0.75rem;
            font-weight: 400;
            color: rgba(255, 255, 255, 0.5);
            letter-spacing: 0.02em;
        }

        /* Mobile fallback - hidden on desktop */
        .infrastructure-mobile {
            display: none;
        }

        .mobile-card {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem 1.25rem;
            background: rgba(255, 255, 255, 0.04);
            backdrop-filter: blur(16px);
            -webkit-backdrop-filter: blur(16px);
            border: 1px solid rgba(255, 255, 255, 0.08);
            border-radius: 12px;
            margin-bottom: 0.75rem;
        }

        a.mobile-card.link-card {
            text-decoration: none;
            color: inherit;
            cursor: pointer;
        }

        a.mobile-card.link-card:hover {
            background: rgba(255, 255, 255, 0.08);
            border-color: rgba(102, 126, 234, 0.25);
        }

        .mobile-card:last-child {
            margin-bottom: 0;
        }

        .mobile-icon {
            width: 44px;
            height: 44px;
            background: var(--gradient-primary);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.1rem;
            color: white;
            flex-shrink: 0;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
        }

        .mobile-text {
            display: flex;
            flex-direction: column;
            gap: 0.125rem;
        }

        .mobile-title {
            font-size: 0.9rem;
            font-weight: 600;
            color: rgba(255, 255, 255, 0.95);
        }

        .mobile-subtitle {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.5);
        }

        .infrastructure-label {
            color: rgba(255,255,255,0.9);
            font-size: 0.9rem;
            font-weight: 500;
            line-height: 1.4;
        }

        /* ============ MANUFACTURING SECTION ============ */
        .manufacturing-section {
            padding: 6rem 2rem;
            background: linear-gradient(180deg, #f8f9fa 0%, white 100%);
        }

        .manufacturing-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
        }

        .manufacturing-card {
            display: flex;
            flex-direction: column;
            background: white;
            border-radius: 24px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            border: 1px solid #e9ecef;
            transition: all 0.3s ease;
        }

        .manufacturing-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 20px 50px rgba(0,0,0,0.12);
            border-color: var(--color-primary);
        }

        .manufacturing-visual {
            height: 180px;
            background: var(--gradient-dark);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .laser-visual {
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
        }

        .laser-animation {
            position: relative;
            width: 120px;
            height: 120px;
        }

        .laser-head {
            position: absolute;
            top: 0;
            left: 20%;
            width: 40px;
            height: 30px;
            background: linear-gradient(180deg, #4a4a5a 0%, #2d2d3a 100%);
            border-radius: 4px 4px 0 0;
            z-index: 3;
            animation: laserMove 2.5s ease-in-out infinite;
        }

        @keyframes laserMove {
            0%, 100% { left: 15%; }
            50% { left: 55%; }
        }

        .laser-nozzle {
            position: absolute;
            bottom: -8px;
            left: 50%;
            transform: translateX(-50%);
            width: 12px;
            height: 12px;
            background: #ff4444;
            border-radius: 50%;
            box-shadow: 0 0 10px #ff4444, 0 0 20px #ff4444, 0 0 30px #ff0000;
            animation: laserGlow 0.5s ease-in-out infinite alternate;
        }

        .laser-beam {
            position: absolute;
            top: 30px;
            left: 50%;
            transform: translateX(-50%);
            width: 3px;
            height: 58px;
            background: linear-gradient(180deg, #ff4444 0%, #ff0000 30%, #ff6600 60%, transparent 100%);
            box-shadow: 0 0 8px #ff4444, 0 0 16px #ff0000;
            animation: laserPulse 0.3s ease-in-out infinite;
        }

        .laser-material {
            position: absolute;
            bottom: 18px;
            left: 50%;
            transform: translateX(-50%);
            width: 80px;
            height: 12px;
            background: linear-gradient(90deg, #8B4513 0%, #A0522D 50%, #8B4513 100%);
            border-radius: 2px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.3);
        }

        .laser-sparks {
            position: absolute;
            bottom: 0;
            left: 15%;
            width: 40px;
            height: 18px;
            animation: laserMove 2.5s ease-in-out infinite;
        }

        .laser-sparks span {
            position: absolute;
            width: 3px;
            height: 3px;
            background: #ffaa00;
            border-radius: 50%;
            box-shadow: 0 0 4px #ffaa00;
            animation: sparkFlyDown 0.6s ease-out infinite;
        }

        .laser-sparks span:nth-child(1) { left: 5px; animation-delay: 0s; }
        .laser-sparks span:nth-child(2) { left: 15px; animation-delay: 0.15s; }
        .laser-sparks span:nth-child(3) { left: 25px; animation-delay: 0.3s; }
        .laser-sparks span:nth-child(4) { left: 10px; animation-delay: 0.45s; }

        @keyframes sparkFlyDown {
            0% { transform: translateY(0) scale(1); opacity: 1; }
            100% { transform: translateY(15px) translateX(5px) scale(0.3); opacity: 0; }
        }

        @keyframes laserGlow {
            0% { box-shadow: 0 0 10px #ff4444, 0 0 20px #ff4444; }
            100% { box-shadow: 0 0 15px #ff6666, 0 0 30px #ff4444, 0 0 40px #ff0000; }
        }

        @keyframes laserPulse {
            0%, 100% { opacity: 1; height: 60px; }
            50% { opacity: 0.8; height: 58px; }
        }


        .cnc-visual {
            background: linear-gradient(135deg, #1a2a1a 0%, #0d2d1d 100%);
        }

        .cnc-animation {
            position: relative;
            width: 120px;
            height: 120px;
        }

        .cnc-gantry {
            position: absolute;
            top: 15px;
            left: 10px;
            width: 50px;
            height: 14px;
            background: linear-gradient(180deg, #5a5a6a 0%, #3d3d4a 100%);
            border-radius: 4px;
            animation: cncMoveCenter 2.5s ease-in-out infinite;
        }

        .cnc-spindle {
            position: absolute;
            top: 14px;
            left: 50%;
            transform: translateX(-50%);
            width: 16px;
            height: 42px;
            background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
            border-radius: 4px;
        }

        .cnc-bit {
            position: absolute;
            bottom: -16px;
            left: 50%;
            transform: translateX(-50%);
            width: 3px;
            height: 18px;
            background: linear-gradient(180deg, #d0d0d0 0%, #a0a0a0 100%);
            border-radius: 0 0 1px 1px;
        }

        .cnc-bit::after {
            content: '';
            position: absolute;
            bottom: -4px;
            left: 50%;
            transform: translateX(-50%);
            width: 0;
            height: 0;
            border-left: 3px solid transparent;
            border-right: 3px solid transparent;
            border-top: 6px solid #a0a0a0;
        }

        .cnc-chips {
            position: absolute;
            bottom: 24px;
            left: 10px;
            width: 20px;
            height: 12px;
            animation: cncChipsMove 2.5s ease-in-out infinite;
        }

        .cnc-chips span {
            position: absolute;
            width: 3px;
            height: 2px;
            background: #c9a86c;
            border-radius: 1px;
            animation: chipFly 0.4s ease-out infinite;
        }

        .cnc-chips span:nth-child(1) { left: 8px; animation-delay: 0s; }
        .cnc-chips span:nth-child(2) { left: 10px; animation-delay: 0.1s; }
        .cnc-chips span:nth-child(3) { left: 12px; animation-delay: 0.2s; }

        .cnc-workpiece {
            position: absolute;
            bottom: 10px;
            left: 50%;
            transform: translateX(-50%);
            width: 100px;
            height: 16px;
            background: linear-gradient(90deg, #deb887 0%, #d2b48c 50%, #deb887 100%);
            border-radius: 3px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.3);
            overflow: hidden;
        }

        .cnc-cut-path {
            position: absolute;
            top: 3px;
            left: 8px;
            width: 0%;
            height: 3px;
            background: #8b7355;
            border-radius: 2px;
            animation: cncCutPath 2.5s ease-in-out infinite;
        }

        @keyframes cncMoveCenter {
            0%, 100% { left: 10px; }
            50% { left: 60px; }
        }

        @keyframes cncChipsMove {
            0%, 100% { left: 30px; }
            50% { left: 80px; }
        }

        @keyframes chipFly {
            0% { transform: translateY(0) rotate(0deg) scale(1); opacity: 1; }
            100% { transform: translateY(-15px) translateX(5px) rotate(90deg) scale(0.3); opacity: 0; }
        }

        @keyframes cncCutPath {
            0% { width: 0%; }
            50% { width: 40%; }
            100% { width: 70%; }
        }

        .cad-visual {
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
        }

        .cad-animation {
            position: relative;
            width: 120px;
            height: 120px;
        }

        .cad-drawing {
            position: absolute;
            inset: 10px;
            border: 2px solid rgba(109, 124, 255, 0.5);
            border-radius: 4px;
            background: rgba(109, 124, 255, 0.06);
        }

        .cad-outline {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 36px;
            height: 28px;
            border: 1px solid rgba(109, 124, 255, 0.6);
            border-radius: 2px;
            animation: cadPulse 2.5s ease-in-out infinite;
        }

        .cad-dim-h {
            position: absolute;
            bottom: 8px;
            left: 50%;
            transform: translateX(-50%);
            width: 50px;
            height: 1px;
            background: rgba(109, 124, 255, 0.4);
            border-left: 4px solid rgba(109, 124, 255, 0.7);
            border-right: 4px solid rgba(109, 124, 255, 0.7);
        }

        .cad-dim-v {
            position: absolute;
            top: 50%;
            right: 8px;
            transform: translateY(-50%);
            width: 1px;
            height: 30px;
            background: rgba(109, 124, 255, 0.4);
            border-top: 4px solid rgba(109, 124, 255, 0.7);
            border-bottom: 4px solid rgba(109, 124, 255, 0.7);
        }

        .cad-pencil {
            position: absolute;
            top: 12px;
            right: 12px;
            width: 28px;
            height: 28px;
            border-radius: 6px;
            background: rgba(109, 124, 255, 0.35);
            display: flex;
            align-items: center;
            justify-content: center;
            color: rgba(255, 255, 255, 0.9);
            font-size: 0.9rem;
            animation: cadPulse 2.5s ease-in-out infinite 0.3s;
        }

        @keyframes cadPulse {
            0%, 100% { opacity: 0.85; }
            50% { opacity: 1; }
        }

        .manufacturing-content {
            padding: 2rem;
            display: flex;
            flex-direction: column;
            flex: 1;
            min-height: 0;
        }

        .manufacturing-content h3 {
            font-size: 1.35rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .manufacturing-content p {
            flex: 1;
            min-height: 0;
            color: #6c757d;
            font-size: 0.95rem;
            line-height: 1.6;
            margin-bottom: 1.5rem;
        }

        :host ::ng-deep .manufacturing-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 0.75rem 1.5rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
        }

        .cube-animation {
            perspective: 800px;
        }

        .cube {
            width: 100px;
            height: 100px;
            position: relative;
            transform-style: preserve-3d;
            animation: rotateCube3D 10s linear infinite;
        }

        @keyframes rotateCube3D {
            0% { transform: rotateX(-20deg) rotateY(0deg); }
            100% { transform: rotateX(-20deg) rotateY(360deg); }
        }

        .face {
            position: absolute;
            width: 100px;
            height: 100px;
            border: 2px solid rgba(79, 172, 254, 0.5);
            background: rgba(79, 172, 254, 0.1);
            backdrop-filter: blur(5px);
        }

        .front { transform: translateZ(50px); }
        .back { transform: rotateY(180deg) translateZ(50px); }
        .right { transform: rotateY(90deg) translateZ(50px); }
        .left { transform: rotateY(-90deg) translateZ(50px); }
        .top { transform: rotateX(90deg) translateZ(50px); }
        .bottom { transform: rotateX(-90deg) translateZ(50px); }

        /* ============ CTA SECTION ============ */
        .cta-section {
            position: relative;
            padding: 8rem 2rem;
            overflow: hidden;
        }

        .cta-bg {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .cta-particles {
            position: absolute;
            inset: 0;
            z-index: 1;
        }

        .particle {
            position: absolute;
            width: 10px;
            height: 10px;
            background: rgba(255,255,255,0.1);
            border-radius: 50%;
            animation: particleFloat 15s infinite;
        }

        .particle:nth-child(1) { left: 10%; top: 20%; animation-delay: 0s; }
        .particle:nth-child(2) { left: 20%; top: 80%; animation-delay: 2s; }
        .particle:nth-child(3) { left: 60%; top: 10%; animation-delay: 4s; }
        .particle:nth-child(4) { left: 80%; top: 50%; animation-delay: 6s; }
        .particle:nth-child(5) { left: 40%; top: 60%; animation-delay: 8s; }
        .particle:nth-child(6) { left: 90%; top: 30%; animation-delay: 10s; }
        .particle:nth-child(7) { left: 30%; top: 40%; animation-delay: 12s; }
        .particle:nth-child(8) { left: 70%; top: 90%; animation-delay: 14s; }

        @keyframes particleFloat {
            0%, 100% { transform: translateY(0) scale(1); opacity: 0.3; }
            50% { transform: translateY(-100px) scale(1.5); opacity: 0.8; }
        }

        .cta-content {
            position: relative;
            z-index: 10;
            text-align: center;
            max-width: 700px;
            margin: 0 auto;
        }

        .cta-title {
            font-size: clamp(2rem, 5vw, 3.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1.5rem;
            line-height: 1.2;
        }

        .cta-description {
            font-size: 1.25rem;
            color: rgba(255,255,255,0.8);
            margin-bottom: 2.5rem;
            line-height: 1.7;
        }

        .cta-buttons {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }

        :host ::ng-deep .cta-btn-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1.25rem 2.5rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: var(--shadow-glow) !important;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 1200px) {
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .divisions-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .parallelogram-strip {
                flex-wrap: wrap;
                gap: 0.75rem;
                padding: 0;
            }
            .parallelogram-segment {
                flex: 1 1 30%;
                min-width: 30%;
            }
            .parallelogram-segment::before {
                transform: skewX(-5deg);
                border-radius: 10px;
            }
            [dir="rtl"] .parallelogram-segment::before {
                transform: skewX(5deg);
            }
            .parallelogram-segment.first::before,
            .parallelogram-segment.last::before,
            [dir="rtl"] .parallelogram-segment.first::before,
            [dir="rtl"] .parallelogram-segment.last::before {
                border-radius: 10px;
            }
            .manufacturing-grid {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        @media (max-width: 1200px) {
            .manufacturing-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 992px) {
            .hero-section {
                min-height: 60vh;
                max-height: none;
                padding: calc(var(--public-header-offset, 70px) + 88px + 1rem) 1.5rem 2.5rem;
            }
            .manufacturing-grid {
                grid-template-columns: 1fr;
            }
        }

        @media (max-width: 768px) {
            .hero-section {
                min-height: unset;
                max-height: none;
                /* Ticker hidden on mobile; standard top padding */
                padding: 4.5rem 1rem 5rem;
            }
            .hero-content {
                max-width: 100%;
            }
            .hero-actions {
                flex-direction: column;
            }
            .stats-grid,
            .divisions-grid {
                grid-template-columns: 1fr;
            }
            .parallelogram-strip {
                display: none;
            }
            .infrastructure-mobile {
                display: block;
            }
            .featured-section-wrapper {
                padding: 2rem 1.5rem;
                border-radius: 20px;
                margin: 0 -0.5rem;
            }
            .featured-header {
                margin-bottom: 1.5rem;
            }
            .products-grid {
                grid-template-columns: 1fr;
            }
            .stat-card {
                flex-direction: column;
                text-align: center;
            }
            .section-header {
                margin-bottom: 3rem;
            }
            .cta-buttons {
                flex-direction: column;
            }
            .division-card {
                padding: 1.5rem;
            }
            .manufacturing-visual {
                height: 140px;
            }
            .cube {
                width: 70px;
                height: 70px;
            }
            .face {
                width: 70px;
                height: 70px;
            }
            .front { transform: translateZ(35px); }
            .back { transform: rotateY(180deg) translateZ(35px); }
            .right { transform: rotateY(90deg) translateZ(35px); }
            .left { transform: rotateY(-90deg) translateZ(35px); }
            .top { transform: rotateX(90deg) translateZ(35px); }
            .bottom { transform: rotateX(-90deg) translateZ(35px); }
        }

        @media (max-width: 480px) {
            .mobile-card {
                padding: 0.875rem 1rem;
            }
            .mobile-icon {
                width: 40px;
                height: 40px;
                font-size: 1rem;
            }
            .mobile-title {
                font-size: 0.85rem;
            }
            .mobile-subtitle {
                font-size: 0.75rem;
            }
            .featured-section-wrapper {
                padding: 1.5rem 1rem;
                border-radius: 16px;
            }
        }
    `]
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild('featuredCarousel') featuredCarousel!: Carousel;
    
    homeContent: HomePageContent | null = null;
    banners: Banner[] = [];
    featuredCategories: FeaturedCategory[] = [];
    featuredItems: FeaturedItem[] = [];
    testimonials: Testimonial[] = [];
    categories: Category[] = [];
    featuredProducts: Item[] = [];
    /** Resolved list for the carousel: API featured first, then CMS-only items. Updated when API or CMS loads. */
    displayProducts: (FeaturedItem | Item)[] = [];
    /** Hero ticker images (from API or empty; component falls back to placeholders when empty). */
    heroTickerItems: HeroTickerItem[] = [];
    /** Cache for item primary image when list API doesn't return it — filled by loading detail. */
    itemImageUrlCache: Record<string, string> = {};
    private itemImageFetchRequested = new Set<string>();
    categoriesLoading = true;
    productsLoading = true;
    cmsLoading = true;
    featuredPage = 0;

    private statisticsConfig: { icon: string; value: number; labelEn: string; labelAr: string }[] = [];
    statistics: { icon: string; value: number; label: string }[] = [];
    engineeringDivisions: { icon: string; title: string; items: string[] }[] = [];
    infrastructureCapabilities: { icon: string; label: string; subtitle: string; link?: string }[] = [];

    carouselResponsiveOptions: any[] = [
        { breakpoint: '1024px', numVisible: 1, numScroll: 1 },
        { breakpoint: '768px', numVisible: 1, numScroll: 1 },
        { breakpoint: '560px', numVisible: 1, numScroll: 1 }
    ];

    testimonialsResponsiveOptions: any[] = [
        { breakpoint: '1024px', numVisible: 2, numScroll: 1 },
        { breakpoint: '768px', numVisible: 1, numScroll: 1 },
        { breakpoint: '560px', numVisible: 1, numScroll: 1 }
    ];

    featuredCarouselResponsiveOptions: any[] = [
        { breakpoint: '1400px', numVisible: 4, numScroll: 2 },
        { breakpoint: '1200px', numVisible: 3, numScroll: 1 },
        { breakpoint: '992px', numVisible: 3, numScroll: 1 },
        { breakpoint: '768px', numVisible: 2, numScroll: 1 },
        { breakpoint: '576px', numVisible: 2, numScroll: 1 }
    ];

    featuredDesktopCarouselOptions: any[] = [
        { breakpoint: '1400px', numVisible: 4, numScroll: 4 },
        { breakpoint: '1200px', numVisible: 3, numScroll: 3 },
        { breakpoint: '992px', numVisible: 3, numScroll: 3 },
        { breakpoint: '768px', numVisible: 2, numScroll: 2 }
    ];

    private catalogApi = inject(CatalogApiService);
    private itemsApi = inject(ItemsApiService);
    private cmsApi = inject(CmsApiService);
    private heroTickerApi = inject(HeroTickerApiService);
    private systemSettingsApi = inject(SystemSettingsApiService);
    public languageService = inject(LanguageService);
    showLearnMoreDialog = false;
    private elementRef = inject(ElementRef<HTMLElement>);
    private intersectionObserver: IntersectionObserver | null = null;

    constructor() {
        // Update labels when language changes
        effect(() => {
            // Access the signal to create dependency
            this.languageService.languageSignal();
            this.updateLabels();
        });
    }

    ngOnInit() {
        this.loadStatisticsConfig();
        this.loadHomeContent();
        this.loadCategories();
        this.loadFeaturedProducts();
        this.loadHeroTicker();
    }

    ngAfterViewInit() {
        this.setupScrollReveal();
        setTimeout(() => this.setupScrollReveal(), 400);
    }

    ngOnDestroy() {
        this.intersectionObserver?.disconnect();
        this.intersectionObserver = null;
    }

    private setupScrollReveal() {
        const host = this.elementRef.nativeElement;
        const sections = host.querySelectorAll('.home-section') as NodeListOf<HTMLElement>;
        if (!sections.length) return;
        if (this.intersectionObserver) {
            sections.forEach((el: HTMLElement) => {
                if (el.dataset['observed'] !== 'true') {
                    el.dataset['observed'] = 'true';
                    this.intersectionObserver!.observe(el);
                }
            });
            return;
        }
        this.intersectionObserver = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('in-view');
                    }
                });
            },
            { threshold: 0.12, rootMargin: '0px 0px -40px 0px' }
        );
        sections.forEach((el: HTMLElement) => {
            el.dataset['observed'] = 'true';
            this.intersectionObserver!.observe(el);
        });
    }

    updateLabels() {
        const isAr = this.languageService.language === 'ar';

        if (!this.statisticsConfig.length) {
            this.statisticsConfig = [
                { icon: 'pi pi-cog', value: 150, labelAr: 'مشروع مُنجز', labelEn: 'Projects Completed' },
                { icon: 'pi pi-building', value: 50, labelAr: 'شريك صناعي', labelEn: 'Industrial Partners' },
                { icon: 'pi pi-users', value: 200, labelAr: 'عميل نشط', labelEn: 'Active Clients' },
                { icon: 'pi pi-clock', value: 10, labelAr: 'سنوات خبرة', labelEn: 'Years Experience' }
            ];
        }

        this.statistics = this.statisticsConfig.map((x) => ({
            icon: x.icon,
            value: x.value,
            label: isAr ? x.labelAr : x.labelEn
        }));

        this.engineeringDivisions = [
            {
                icon: 'pi pi-microchip',
                title: isAr ? 'الأنظمة الهندسية' : 'Engineering Systems',
                items: isAr
                    ? ['منصات مدمجة مخصصة', 'الروبوتات والأتمتة', 'أنظمة المراقبة الذكية']
                    : ['Custom embedded platforms', 'Robotics & automation', 'Intelligent monitoring systems']
            },
            {
                icon: 'pi pi-wrench',
                title: isAr ? 'التصنيع الدقيق' : 'Precision Manufacturing',
                items: isAr
                    ? ['تشغيل CNC', 'الطباعة ثلاثية الأبعاد', 'أنظمة الليزر']
                    : ['CNC machining', '3D printing', 'Laser systems']
            },
            {
                icon: 'pi pi-box',
                title: isAr ? 'تطوير المنتجات' : 'Product Development',
                items: isAr
                    ? ['من النموذج إلى الإنتاج', 'تكامل الأجهزة والبرمجيات', 'التحقق والاختبار']
                    : ['Prototype to production', 'Hardware-software integration', 'Validation & testing']
            }
        ];

        this.infrastructureCapabilities = [
            { 
                icon: 'pi pi-pencil', 
                label: isAr ? 'التصميم و CAD' : 'Design & CAD', 
                subtitle: isAr ? 'التصميم والرسم بمساعدة الحاسوب' : 'CAD design & drawing services',
                link: '/design'
            },
            { 
                icon: 'pi pi-sitemap', 
                label: isAr ? 'تصميم الأجهزة والدوائر' : 'Hardware Design & PCB', 
                subtitle: isAr ? 'تطوير النماذج الأولية' : 'Prototype Development' 
            },
            { 
                icon: 'pi pi-code', 
                label: isAr ? 'هندسة البرمجيات المدمجة' : 'Embedded Firmware', 
                subtitle: isAr ? 'أنظمة الوقت الفعلي' : 'Real-time Systems' 
            },
            { 
                icon: 'pi pi-th-large', 
                label: isAr ? 'التصميم الميكانيكي' : 'Mechanical CAD', 
                subtitle: isAr ? 'المحاكاة والتحليل' : 'Simulation & Analysis' 
            },
            { 
                icon: 'pi pi-sync', 
                label: isAr ? 'تكامل الأنظمة' : 'System Integration', 
                subtitle: isAr ? 'الاختبار والتحقق' : 'Testing & Validation' 
            },
            { 
                icon: 'pi pi-cog', 
                label: isAr ? 'التصنيع الصناعي' : 'Industrial Fabrication', 
                subtitle: isAr ? 'الإنتاج المتقدم' : 'Advanced Manufacturing' 
            }
        ];
    }

    private loadStatisticsConfig() {
        this.systemSettingsApi.getSettings(undefined, true).subscribe({
            next: (settings) => {
                try {
                    const setting = (settings || []).find(s => s.key === 'home.statistics');
                    if (!setting?.value) {
                        this.updateLabels();
                        return;
                    }
                    const parsed = JSON.parse(setting.value) as { icon: string; value: number; labelEn: string; labelAr: string }[];
                    if (Array.isArray(parsed) && parsed.length > 0) {
                        this.statisticsConfig = parsed.map((x) => ({
                            icon: x.icon || 'pi pi-circle',
                            value: Number(x.value ?? 0),
                            labelEn: x.labelEn || '',
                            labelAr: x.labelAr || ''
                        }));
                    }
                } catch {
                    // Keep defaults when setting value is malformed.
                }
                this.updateLabels();
            },
            error: () => {
                // Keep defaults when setting does not exist yet.
                this.updateLabels();
            }
        });
    }

    scrollToSection(sectionId: string) {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    loadHomeContent() {
        this.cmsLoading = true;
        this.cmsApi.getHomePage().subscribe({
            next: (content) => {
                this.homeContent = content;
                this.banners = content.banners || [];
                this.featuredCategories = content.featuredCategories || [];
                this.featuredItems = content.featuredItems || [];
                this.testimonials = content.testimonials || [];
                this.cmsLoading = false;
                this.updateDisplayProducts();
                setTimeout(() => this.setupScrollReveal(), 100);
            },
            error: () => {
                this.cmsLoading = false;
            }
        });
    }

    loadCategories() {
        this.categoriesLoading = true;
        this.catalogApi.getAllCategories(true, false).subscribe({
            next: (categories) => {
                this.categories = categories.filter(cat => !cat.parentId).slice(0, 6);
                this.categoriesLoading = false;
                setTimeout(() => this.setupScrollReveal(), 100);
            },
            error: () => {
                this.categoriesLoading = false;
            }
        });
    }

    loadHeroTicker() {
        this.heroTickerApi.getPublic().subscribe({
            next: (items) => {
                this.heroTickerItems = items.map(dto => ({
                    id: String(dto.id),
                    title: dto.title ?? undefined,
                    imageUrl: this.getFullImageUrl(dto.imageUrl),
                    sortOrder: dto.sortOrder
                }));
            },
            error: () => {
                this.heroTickerItems = [];
            }
        });
    }

    loadFeaturedProducts() {
        this.productsLoading = true;
        this.itemsApi.getFeaturedItems().subscribe({
            next: (items) => {
                const featured = items || [];
                if (featured.length === 0) {
                    // No featured items: try "new" items as fallback
                    this.itemsApi.getNewItems().subscribe({
                        next: (newItems) => {
                            this.featuredProducts = newItems || [];
                            this.productsLoading = false;
                            this.updateDisplayProducts();
                            setTimeout(() => this.setupScrollReveal(), 100);
                        },
                        error: () => {
                            this.featuredProducts = [];
                            this.productsLoading = false;
                            this.updateDisplayProducts();
                            setTimeout(() => this.setupScrollReveal(), 100);
                        }
                    });
                } else {
                    this.featuredProducts = featured;
                    this.productsLoading = false;
                    this.updateDisplayProducts();
                    setTimeout(() => this.setupScrollReveal(), 100);
                }
            },
            error: () => {
                // API failed: try new items so we still show something
                this.itemsApi.getNewItems().subscribe({
                    next: (newItems) => {
                        this.featuredProducts = newItems || [];
                        this.productsLoading = false;
                        this.updateDisplayProducts();
                        setTimeout(() => this.setupScrollReveal(), 100);
                    },
                    error: () => {
                        this.featuredProducts = [];
                        this.productsLoading = false;
                        this.updateDisplayProducts();
                    }
                });
            }
        });
    }

    /** Merge two item lists by id, no duplicates, up to maxLength. */
    private mergeById(primary: Item[], secondary: Item[], maxLength: number): Item[] {
        const ids = new Set<string>();
        const result: Item[] = [];
        for (const p of primary) {
            if (p?.id && !ids.has(p.id)) {
                result.push(p);
                ids.add(p.id);
            }
        }
        for (const s of secondary) {
            if (result.length >= maxLength) break;
            if (s?.id && !ids.has(s.id)) {
                result.push(s);
                ids.add(s.id);
            }
        }
        return result;
    }

    getBannerTitle(banner: Banner): string {
        return this.languageService.language === 'ar' ? (banner.titleAr || '') : (banner.titleEn || '');
    }

    getDisplayCategories(): (FeaturedCategory | Category)[] {
        if (this.featuredCategories.length > 0) {
            return this.featuredCategories;
        }
        return this.categories;
    }

    getCategoryName(category: FeaturedCategory | Category): string {
        if ('categoryNameEn' in category) {
            return this.languageService.language === 'ar' 
                ? category.categoryNameAr 
                : category.categoryNameEn;
        }
        return this.languageService.getLocalizedName(category);
    }

    getCategoryId(category: FeaturedCategory | Category): string {
        if ('categoryId' in category) {
            return category.categoryId;
        }
        return (category as Category).id || '';
    }

    getCategoryImage(category: FeaturedCategory | Category): string | undefined {
        if ('imageUrl' in category) {
            return category.imageUrl;
        }
        return (category as Category).imageUrl;
    }

    /** Icon class for category when no image – distinct logo per category. */
    getCategoryIcon(category: FeaturedCategory | Category): string {
        const name = this.getCategoryName(category).toLowerCase();
        const slug = 'slug' in category ? (category as Category).slug?.toLowerCase() ?? '' : '';
        const key = name + ' ' + slug;
        if (/electronic|elektronik/i.test(key)) return 'pi-microchip';
        if (/medical|medikal|طب|طبي|معدات/i.test(key)) return 'pi-heart-fill';
        if (/tool|أدوات|معدات/i.test(key) && !/medical/i.test(key)) return 'pi-wrench';
        if (/3d|print|طابعة|طباعة/i.test(key)) return 'pi-box';
        if (/component|مكون|قطع/i.test(key)) return 'pi-th-large';
        if (/fashion|أزياء|ملابس/i.test(key)) return 'pi-tag';
        if (/home|منزل|بيت/i.test(key)) return 'pi-home';
        if (/sport|رياض/i.test(key)) return 'pi-star-fill';
        return 'pi-folder';
    }

    getProductId(product: FeaturedItem | Item): string {
        if ('itemId' in product) {
            return product.itemId;
        }
        return (product as Item).id || '';
    }

    /** Updates displayProducts for the carousel: API list first, then CMS items not already in the list. */
    updateDisplayProducts(): void {
        const ids = new Set<string>();
        const result: (FeaturedItem | Item)[] = [];
        for (const p of this.featuredProducts) {
            result.push(p);
            ids.add(this.getProductId(p));
        }
        for (const p of this.featuredItems) {
            const id = this.getProductId(p);
            if (!ids.has(id)) {
                result.push(p);
                ids.add(id);
            }
        }
        this.displayProducts = result;
        this.loadMissingFeaturedImages();
    }

    /** When list API doesn't return primaryImageUrl, fetch item detail and cache first image. */
    private loadMissingFeaturedImages(): void {
        for (const p of this.displayProducts) {
            const id = this.getProductId(p);
            if (!id || this.getProductImage(p) !== 'assets/img/defult.png' || this.itemImageFetchRequested.has(id)) continue;
            this.itemImageFetchRequested.add(id);
            this.itemsApi.getItemDetail(id).subscribe({
                next: (detail: any) => {
                    const media = detail?.mediaAssets || detail?.MediaAssets;
                    if (!Array.isArray(media) || media.length === 0) return;
                    const firstImage = media.find((m: any) => (m.mediaType || m.MediaType) === 'Image') || media[0];
                    const raw = firstImage?.fileUrl ?? firstImage?.FileUrl ?? firstImage?.mediaAssetUrl ?? firstImage?.MediaAssetUrl;
                    if (raw) {
                        this.itemImageUrlCache = { ...this.itemImageUrlCache, [id]: this.getFullImageUrl(raw) };
                    }
                }
            });
        }
    }

    getProductImageResolved(product: FeaturedItem | Item): string {
        const id = this.getProductId(product);
        return (id && this.itemImageUrlCache[id]) || this.getProductImage(product);
    }

    getProductName(product: FeaturedItem | Item): string {
        if ('itemNameEn' in product) {
            return this.languageService.language === 'ar' 
                ? product.itemNameAr 
                : product.itemNameEn;
        }
        return this.languageService.getLocalizedName(product);
    }

    getProductPrice(product: FeaturedItem | Item): number {
        if ('price' in product) {
            return product.price;
        }
        return 0;
    }

    getProductRating(product: FeaturedItem | Item): number {
        if ('averageRating' in product) {
            return product.averageRating;
        }
        return 0;
    }

    getProductReviewsCount(product: FeaturedItem | Item): number {
        if ('reviewsCount' in product) {
            return product.reviewsCount;
        }
        return 0;
    }

    getProductImage(product: FeaturedItem | Item): string {
        // Check for CMS imageUrl (FeaturedItem)
        if ('imageUrl' in product && product.imageUrl) {
            return this.getFullImageUrl(product.imageUrl);
        }
        // Check for primaryImageUrl from API (Item) — support both camelCase and PascalCase
        const primaryUrl = (product as any).primaryImageUrl ?? (product as any).PrimaryImageUrl;
        if (primaryUrl) {
            return this.getFullImageUrl(primaryUrl);
        }
        // Check mediaAssets array
        if ('mediaAssets' in product && product.mediaAssets && Array.isArray(product.mediaAssets) && product.mediaAssets.length > 0) {
            const mediaArray = product.mediaAssets as any[];
            const primaryImage = mediaArray.find((m: any) => m.isPrimary || m.mediaType === 'Image');
            if (primaryImage) {
                if ('fileUrl' in primaryImage) {
                    return this.getFullImageUrl(primaryImage.fileUrl);
                } else if ('mediaAssetUrl' in primaryImage) {
                    return this.getFullImageUrl(primaryImage.mediaAssetUrl);
                }
            }
            const firstMedia = mediaArray[0];
            if (firstMedia) {
                if ('fileUrl' in firstMedia) {
                    return this.getFullImageUrl(firstMedia.fileUrl);
                } else if ('mediaAssetUrl' in firstMedia) {
                    return this.getFullImageUrl(firstMedia.mediaAssetUrl);
                }
            }
        }
        return 'assets/img/defult.png';
    }

    /** Mark image area as placeholder on error (mobile Featured carousel – show icon + "No image available"). */
    onProductImageError(event: Event): void {
        const el = event.target as HTMLImageElement;
        if (!el?.parentElement) return;
        el.onerror = null;
        el.parentElement.classList.add('product-image-placeholder');
    }

    private getFullImageUrl(url: string): string {
        if (!url) return 'assets/img/defult.png';
        // Legacy seed paths often point to sample-* files that don't exist in production.
        // Remap them to the default image to avoid repeated 404s in console.
        if (/^(https?:\/\/[^/]+)?\/uploads\/images\/sample-\d+\.(jpg|jpeg|png|webp)$/i.test(url)) {
            return 'assets/img/defult.png';
        }
        if (url.startsWith('http')) return url;
        const apiUrl = environment.apiUrl || 'http://localhost:5000/api/v1';
        const baseUrl = apiUrl.replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '') || '';
        const path = url.startsWith('/') ? url : '/' + url;
        return baseUrl ? `${baseUrl}${path}` : url;
    }

    getTestimonialName(testimonial: Testimonial): string {
        return this.languageService.language === 'ar' 
            ? testimonial.nameAr 
            : testimonial.nameEn;
    }

    getTestimonialText(testimonial: Testimonial): string {
        return this.languageService.language === 'ar' 
            ? testimonial.textAr 
            : testimonial.textEn;
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    scrollFeaturedPrev(): void {
        if (this.featuredCarousel) {
            this.featuredCarousel.navBackward(new MouseEvent('click'));
        }
    }

    scrollFeaturedNext(): void {
        if (this.featuredCarousel) {
            this.featuredCarousel.navForward(new MouseEvent('click'));
        }
    }
}
