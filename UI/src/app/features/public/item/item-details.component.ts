import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { RatingModule } from 'primeng/rating';
import { GalleriaModule } from 'primeng/galleria';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { DownloadableFilesSectionComponent } from '../../../shared/components/downloadable-files-section/downloadable-files-section.component';
import { ReviewsApiService } from '../../../shared/services/reviews-api.service';
import { CartService } from '../../../shared/services/cart.service';
import { WishlistApiService } from '../../../shared/services/wishlist-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { AuthService } from '../../../shared/services/auth.service';
import { Item, ItemDetail, ItemSection, ItemDocument, CompatibleMachine } from '../../../shared/models/item.model';
import { environment } from '../../../../environments/environment';
import { Review, ReviewListResponse, CreateReviewRequest } from '../../../shared/models/review.model';
import { MediaAsset } from '../../../shared/models/media.model';

@Component({
    selector: 'app-item-details',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputNumberModule,
        InputTextModule,
        RatingModule,
        GalleriaModule,
        TagModule,
        TextareaModule,
        TooltipModule,
        ToastModule,
        TranslateModule,
        DownloadableFilesSectionComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="item-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
            <!-- Breadcrumbs -->
                    <nav class="breadcrumbs">
                        <a [routerLink]="['/']">{{ 'common.home' | translate }}</a>
                        <i class="pi pi-angle-right"></i>
                        <a *ngIf="item?.categoryNameEn" [routerLink]="['/catalog']" [queryParams]="{category: item?.categoryId || ''}">
                            {{ languageService.language === 'ar' ? (item?.categoryNameAr || '') : (item?.categoryNameEn || '') }}
                        </a>
                        <i class="pi pi-angle-right" *ngIf="item?.categoryNameEn"></i>
                        <span class="current">{{ languageService.getLocalizedName(item) }}</span>
            </nav>
                </div>
            </section>

            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
                <div class="loading-spinner">
                    <i class="pi pi-spin pi-spinner"></i>
                    <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                </div>
            </div>

            <!-- Main Content -->
            <section *ngIf="!loading && itemDetail" class="main-section">
                <div class="container">
                    <div class="product-grid">
                <!-- Left Column: Images & Media -->
                        <div class="product-gallery">
                            <div class="gallery-card">
                        <p-galleria
                            *ngIf="images.length > 0"
                            [value]="images"
                            [responsiveOptions]="responsiveOptions"
                            [numVisible]="5"
                            [circular]="true"
                            [showItemNavigators]="true"
                            [showThumbnails]="true"
                            [thumbnailsPosition]="'bottom'"
                            [autoPlay]="false"
                            [transitionInterval]="0"
                                    styleClass="custom-galleria"
                        >
                            <ng-template #item let-item>
                                        <img [src]="item.url" [alt]="item.alt" class="gallery-main-image" onerror="this.src='assets/img/defult.png'" />
                            </ng-template>
                            <ng-template #thumbnail let-item>
                                        <img [src]="item.url" [alt]="item.alt" class="gallery-thumb" onerror="this.src='assets/img/defult.png'" />
                            </ng-template>
                        </p-galleria>
                                <div *ngIf="images.length === 0" class="no-image">
                                    <img src="assets/img/defult.png" [alt]="languageService.getLocalizedName(item)" />
                        </div>

                        <!-- Video -->
                                <div *ngIf="videoUrl" class="video-container">
                                    <video [src]="videoUrl" controls></video>
                        </div>
                            </div>
                </div>

                <!-- Right Column: Product Info -->
                        <div class="product-info">
                            <div class="info-card">
                                <h1 class="product-title">{{ languageService.getLocalizedName(itemDetail.item) }}</h1>
                        
                        <!-- Rating -->
                                <div class="rating-row">
                            <p-rating [ngModel]="itemDetail.item.averageRating || 0" [readonly]="true"></p-rating>
                                    <span class="reviews-count">({{ itemDetail.item.reviewsCount }} {{ 'item.reviews' | translate }})</span>
                        </div>

                        <!-- Price -->
                                <div class="price-display">
                            {{ formatCurrency(itemDetail.item.price) }}
                        </div>

                        <!-- SKU & Stock -->
                                <div class="product-meta">
                                    <div class="meta-item">
                                        <span class="meta-label">{{ 'item.sku' | translate }}:</span>
                                        <span class="meta-value">{{ itemDetail.item.sku }}</span>
                            </div>
                                    <div class="meta-item">
                                        <span class="meta-label">{{ 'item.availability' | translate }}:</span>
                                        <span class="stock-badge" [class.in-stock]="isInStock()" [class.out-of-stock]="!isInStock()">
                                            <i [class]="isInStock() ? 'pi pi-check-circle' : 'pi pi-times-circle'"></i>
                                            {{ getStockStatus() }}
                                        </span>
                            </div>
                                    <div *ngIf="itemDetail.item.brandNameEn" class="meta-item">
                                        <span class="meta-label">{{ 'item.brand' | translate }}:</span>
                                        <span class="meta-value brand">{{ languageService.language === 'ar' ? itemDetail.item.brandNameAr : itemDetail.item.brandNameEn }}</span>
                            </div>
                        </div>

                        <!-- Description -->
                                <div class="product-description" [innerHTML]="getDescription()"></div>

                        <!-- Add to Cart -->
                                <div class="cart-actions">
                                    <div class="quantity-selector">
                                        <label>{{ 'item.quantity' | translate }}:</label>
                                        <div class="quantity-input-wrap">
                                <p-inputNumber 
                                    [(ngModel)]="quantity"
                                    [min]="1"
                                    [max]="getMaxQuantity()"
                                    [showButtons]="true"
                                    styleClass="quantity-input"
                                ></p-inputNumber>
                                        </div>
                                    </div>
                                    <button 
                                        class="add-to-cart-btn"
                                [disabled]="!isInStock()"
                                        (click)="addToCart()">
                                        <i class="pi pi-shopping-cart"></i>
                                        {{ 'item.addToCart' | translate }}
                                    </button>
                        </div>

                        <!-- Actions -->
                                <div class="secondary-actions">
                                    <button 
                                        class="action-btn wishlist-btn"
                                        [class.active]="isInWishlist"
                                (click)="toggleWishlist()"
                                        [pTooltip]="isInWishlist ? ('item.removeFromWishlist' | translate) : ('item.addToWishlist' | translate)">
                                        <i [class]="isInWishlist ? 'pi pi-heart-fill' : 'pi pi-heart'"></i>
                                    </button>
                                    <button 
                                        class="action-btn share-btn"
                                        (click)="shareItem()">
                                        <i class="pi pi-share-alt"></i>
                                        {{ 'item.share' | translate }}
                                    </button>
                        </div>
                            </div>
                        </div>
                </div>

                    <!-- Tabs Section -->
                    <div class="tabs-section">
                        <div class="tabs-card">
                            <div class="tabs-nav">
                                <button 
                                    class="tab-btn" 
                                    [class.active]="activeTab === 'details'"
                                    (click)="activeTab = 'details'">
                                    <i class="pi pi-info-circle"></i>
                                    {{ 'item.details' | translate }}
                                </button>
                                <button 
                                    class="tab-btn" 
                                    [class.active]="activeTab === 'reviews'"
                                    (click)="activeTab = 'reviews'">
                                    <i class="pi pi-star"></i>
                                    {{ 'item.reviews' | translate }}
                                    <span class="tab-badge">{{ reviewsSummary?.totalReviews || 0 }}</span>
                                </button>
                        </div>

                            <div class="tab-content">
                        <!-- Details Tab -->
                                <div *ngIf="activeTab === 'details'" class="details-content">
                                    <div *ngFor="let section of itemDetail.sections" class="detail-section">
                                        <h3 class="section-title">{{ languageService.language === 'ar' ? section.titleAr : section.titleEn }}</h3>
                                        <p *ngIf="section.contentEn || section.contentAr" class="section-text">
                                    {{ languageService.language === 'ar' ? section.contentAr : section.contentEn }}
                                </p>
                                        <div *ngIf="section.features.length > 0" class="features-grid">
                                            <div *ngFor="let feature of section.features" class="feature-item">
                                                <span class="feature-label">{{ languageService.language === 'ar' ? feature.labelAr : feature.labelEn }}</span>
                                                <span class="feature-value">{{ languageService.language === 'ar' ? feature.valueAr : feature.valueEn }}</span>
                                    </div>
                                </div>
                            </div>

                            <!-- Documents -->
                                    <div *ngIf="itemDetail.documents.length > 0" class="documents-section">
                                        <h3 class="section-title">{{ 'item.documents' | translate }}</h3>
                                        <div class="documents-list">
                                    <a *ngFor="let doc of itemDetail.documents" 
                                       [href]="doc.fileUrl" 
                                       target="_blank"
                                               class="document-item">
                                                <i class="pi pi-file-pdf"></i>
                                                <span class="doc-name">{{ languageService.language === 'ar' ? doc.nameAr : doc.nameEn }}</span>
                                                <span class="doc-size">{{ formatFileSize(doc.fileSize) }}</span>
                                    </a>
                                </div>
                            </div>
                        </div>

                        <!-- Reviews Tab -->
                                <div *ngIf="activeTab === 'reviews'" class="reviews-content">
                                    <div class="reviews-header">
                                        <div class="reviews-summary">
                                            <div class="average-rating">{{ reviewsSummary?.averageRating || 0 }}</div>
                                            <div class="rating-details">
                                                <p-rating [ngModel]="reviewsSummary?.averageRating || 0" [readonly]="true"></p-rating>
                                                <span>{{ reviewsSummary?.totalReviews || 0 }} {{ 'item.reviews' | translate }}</span>
                                            </div>
                                        </div>
                                        <button 
                                        *ngIf="isLoggedIn"
                                            class="write-review-btn"
                                            (click)="showReviewForm = true">
                                            <i class="pi pi-pencil"></i>
                                            {{ 'item.writeReview' | translate }}
                                        </button>
                                </div>

                                <!-- Review Form -->
                                    <div *ngIf="showReviewForm" class="review-form-card">
                                        <h4>{{ 'item.writeReview' | translate }}</h4>
                                        <form [formGroup]="reviewForm" class="review-form">
                                            <div class="form-group">
                                                <label>{{ 'item.rating' | translate }} *</label>
                                            <p-rating [(ngModel)]="reviewRating" formControlName="rating"></p-rating>
                                        </div>
                                            <div class="form-group">
                                                <label>{{ 'item.reviewTitle' | translate }}</label>
                                                <input type="text" pInputText formControlName="title" />
                                        </div>
                                            <div class="form-group">
                                                <label>{{ 'item.reviewComment' | translate }}</label>
                                                <textarea pTextarea formControlName="comment" rows="5"></textarea>
                                        </div>
                                            <div class="form-actions">
                                                <button class="submit-btn" (click)="submitReview()" [disabled]="submittingReview">
                                                    <i *ngIf="submittingReview" class="pi pi-spin pi-spinner"></i>
                                                    {{ 'common.submit' | translate }}
                                                </button>
                                                <button class="cancel-btn" (click)="showReviewForm = false">
                                                    {{ 'common.cancel' | translate }}
                                                </button>
                                        </div>
                                    </form>
                                    </div>

                                <!-- Reviews List -->
                                    <div *ngIf="reviews.length === 0" class="no-reviews">
                                        <i class="pi pi-comments"></i>
                                        <p>{{ 'item.noReviews' | translate }}</p>
                                </div>
                                    <div *ngFor="let review of reviews" class="review-item">
                                        <div class="review-header">
                                            <div class="reviewer-info">
                                                <span class="reviewer-name">{{ review.userName }}</span>
                                                <span class="review-date">{{ formatDate(review.createdAt) }}</span>
                                            </div>
                                        <p-rating [ngModel]="review.rating" [readonly]="true"></p-rating>
                                    </div>
                                        <h4 *ngIf="review.title" class="review-title">{{ review.title }}</h4>
                                        <p class="review-comment">{{ review.comment }}</p>
                            </div>
                        </div>

                            </div>
                        </div>
                </div>

                <!-- Downloadable Files Section -->
                @if (itemDetail) {
                    <div class="item-downloads-wrapper">
                        <app-downloadable-files-section
                            entityType="Item"
                            [entityId]="itemDetail.item.id">
                        </app-downloadable-files-section>
                    </div>
                }

                <!-- Related Products -->
                    <div class="related-section" *ngIf="relatedItems.length > 0">
                        <div class="section-header">
                            <span class="section-badge">
                                <i class="pi pi-sparkles"></i>
                                {{ languageService.language === 'ar' ? 'منتجات مشابهة' : 'You May Also Like' }}
                            </span>
                            <h2>{{ 'item.relatedProducts' | translate }}</h2>
                        </div>
                        <div class="related-grid">
                            <div *ngFor="let relatedItem of relatedItems; let i = index" class="related-card" [style.animation-delay]="(i * 0.1) + 's'">
                                <div class="related-image" [routerLink]="['/item', relatedItem.id]">
                                    <img [src]="getItemImage(relatedItem)" [alt]="languageService.getLocalizedName(relatedItem)" onerror="this.src='assets/img/defult.png'" />
                                </div>
                                <div class="related-info">
                                    <h4>
                                        <a [routerLink]="['/item', relatedItem.id]">
                                        {{ languageService.getLocalizedName(relatedItem) }}
                                    </a>
                                </h4>
                                    <div class="related-price">{{ formatCurrency(relatedItem.price) }}</div>
                                    <button class="related-cart-btn" (click)="addRelatedToCart(relatedItem)">
                                        <i class="pi pi-shopping-cart"></i>
                                        {{ 'item.addToCart' | translate }}
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

        .item-page {
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
            padding: 2rem;
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
            max-width: 1400px;
            margin: 0 auto;
        }

        .breadcrumbs {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            flex-wrap: wrap;
        }

        .breadcrumbs a {
            color: rgba(255,255,255,0.7);
            text-decoration: none;
            transition: color 0.3s;
        }

        .breadcrumbs a:hover {
            color: white;
        }

        .breadcrumbs i {
            color: rgba(255,255,255,0.4);
            font-size: 0.75rem;
        }

        .breadcrumbs .current {
            color: white;
            font-weight: 600;
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

        /* ============ MAIN SECTION ============ */
        .main-section {
            padding: 3rem 1rem;
        }

        .product-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 3rem;
        }

        /* ============ GALLERY ============ */
        .gallery-card {
            background: white;
            border-radius: 24px;
            padding: 2rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        :host ::ng-deep .custom-galleria .p-galleria-content {
            background: transparent;
        }

        .gallery-main-image {
            width: 100%;
            max-height: 500px;
            object-fit: contain;
            border-radius: 16px;
        }

        .gallery-thumb {
            width: 100%;
            height: 70px;
            object-fit: cover;
            border-radius: 8px;
        }

        .no-image {
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 400px;
        }

        .no-image img {
            max-width: 100%;
            max-height: 400px;
            object-fit: contain;
        }

        .video-container {
            margin-top: 2rem;
        }

        .video-container video {
            width: 100%;
            max-height: 400px;
            border-radius: 16px;
        }

        /* ============ PRODUCT INFO ============ */
        .info-card {
            background: white;
            border-radius: 24px;
            padding: 2.5rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .product-title {
            font-size: 2rem;
            font-weight: 800;
            color: #1a1a2e;
            margin-bottom: 1rem;
            line-height: 1.3;
        }

        .rating-row {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        /* Rating stars: gold/amber (no green) */
        :host ::ng-deep .p-rating .p-rating-on-icon,
        :host ::ng-deep .p-rating-icon.p-rating-on-icon,
        :host ::ng-deep .p-rating .p-rating-on-icon .pi,
        :host ::ng-deep .p-rating .p-rating-on-icon i {
            color: #FFB800 !important;
        }
        :host ::ng-deep .p-rating .p-rating-on-icon svg,
        :host ::ng-deep .p-rating-on-icon svg {
            fill: #FFB800 !important;
            stroke: #FFB800 !important;
        }
        :host ::ng-deep .p-rating .p-rating-off-icon,
        :host ::ng-deep .p-rating-icon.p-rating-off-icon {
            color: #e5e7eb !important;
        }
        :host ::ng-deep .p-rating .p-rating-off-icon svg {
            fill: #e5e7eb !important;
            stroke: #e5e7eb !important;
        }

        .reviews-count {
            color: #6c757d;
            font-size: 0.9rem;
        }

        .price-display {
            font-size: 2.5rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 1.5rem;
        }

        .product-meta {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            padding: 1.5rem;
            background: #f8f9fa;
            border-radius: 16px;
            margin-bottom: 1.5rem;
        }

        .meta-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .meta-label {
            font-weight: 600;
            color: #6c757d;
            min-width: 100px;
        }

        .meta-value {
            color: #1a1a2e;
        }

        .meta-value.brand {
            color: var(--color-primary);
            font-weight: 600;
        }

        .stock-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.375rem 1rem;
            border-radius: 50px;
            font-weight: 600;
            font-size: 0.85rem;
        }

        .stock-badge.in-stock {
            background: rgba(102, 126, 234, 0.12);
            color: #667eea;
        }

        .stock-badge.out-of-stock {
            background: rgba(239, 68, 68, 0.1);
            color: #ef4444;
        }

        .product-description {
            color: #6c757d;
            line-height: 1.7;
            margin-bottom: 2rem;
        }

        .cart-actions {
            display: flex;
            align-items: stretch;
            gap: 1rem;
            margin-bottom: 1.5rem;
            flex-wrap: nowrap;
        }

        .quantity-selector {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            flex: 0 0 auto;
            width: 220px;
            min-width: 220px;
            overflow: hidden;
        }

        .quantity-selector label {
            font-weight: 600;
            color: #1a1a2e;
            white-space: nowrap;
            flex-shrink: 0;
        }

        .quantity-input-wrap {
            width: 120px;
            min-width: 120px;
            flex-shrink: 0;
        }

        :host ::ng-deep .quantity-input-wrap .quantity-input,
        :host ::ng-deep .quantity-input-wrap .p-inputnumber {
            width: 120px !important;
            max-width: 120px !important;
        }

        :host ::ng-deep .quantity-input-wrap .p-inputnumber .p-inputnumber-input {
            width: 100% !important;
        }

        .add-to-cart-btn {
            flex: 1 1 auto;
            min-width: 180px;
            height: 48px;
            box-sizing: border-box;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
            padding: 0 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 700;
            font-size: 1rem;
            cursor: pointer;
            transition: all 0.3s;
        }

        .add-to-cart-btn:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
        }

        .add-to-cart-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .secondary-actions {
            display: flex;
            gap: 1rem;
        }

        .action-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .action-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .wishlist-btn.active {
            border-color: #ef4444;
            color: #ef4444;
            background: rgba(239, 68, 68, 0.1);
        }

        /* ============ TABS SECTION ============ */
        .tabs-section {
            margin-top: 3rem;
        }

        .tabs-card {
            background: white;
            border-radius: 24px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .tabs-nav {
            display: flex;
            background: #f8f9fa;
            padding: 0.5rem;
            gap: 0.5rem;
        }

        .tab-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 2rem;
            background: transparent;
            border: none;
            border-radius: 12px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .tab-btn:hover {
            color: var(--color-primary);
        }

        .tab-btn.active {
            background: white;
            color: var(--color-primary);
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }

        .tab-badge {
            background: var(--gradient-primary);
            color: white;
            padding: 0.125rem 0.5rem;
            border-radius: 50px;
            font-size: 0.75rem;
        }

        .tab-content {
            padding: 2rem;
        }

        /* Details Tab */
        .detail-section {
            margin-bottom: 2rem;
        }

        .section-title {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1rem;
        }

        .section-text {
            color: #6c757d;
            line-height: 1.7;
            margin-bottom: 1.5rem;
        }

        .features-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 1rem;
        }

        .feature-item {
            display: flex;
            justify-content: space-between;
            padding: 1rem;
            background: #f8f9fa;
            border-radius: 12px;
        }

        .feature-label {
            font-weight: 600;
            color: #6c757d;
        }

        .feature-value {
            color: #1a1a2e;
        }

        .documents-section {
            margin-top: 2rem;
        }

        .documents-list {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .document-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            background: #f8f9fa;
            border-radius: 12px;
            text-decoration: none;
            color: #1a1a2e;
            transition: all 0.3s;
        }

        .document-item:hover {
            background: rgba(102, 126, 234, 0.1);
            color: var(--color-primary);
        }

        .document-item i {
            font-size: 1.5rem;
            color: var(--color-primary);
        }

        .doc-name {
            flex: 1;
            font-weight: 600;
        }

        .doc-size {
            color: #6c757d;
            font-size: 0.85rem;
        }

        /* Reviews Tab */
        .reviews-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 2rem;
            padding-bottom: 2rem;
            border-bottom: 1px solid #e9ecef;
        }

        .reviews-summary {
            display: flex;
            align-items: center;
            gap: 1.5rem;
        }

        .average-rating {
            font-size: 3rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .rating-details span {
            display: block;
            color: #6c757d;
            margin-top: 0.25rem;
        }

        .write-review-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .write-review-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
        }

        .review-form-card {
            background: #f8f9fa;
            border-radius: 16px;
            padding: 2rem;
            margin-bottom: 2rem;
        }

        .review-form-card h4 {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.5rem;
        }

        .review-form {
            display: flex;
            flex-direction: column;
            gap: 1.25rem;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-group label {
            font-weight: 600;
            color: #1a1a2e;
        }

        .form-group input,
        .form-group textarea {
            width: 100%;
            padding: 0.875rem 1rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            font-size: 1rem;
            transition: border-color 0.3s;
        }

        .form-group input:focus,
        .form-group textarea:focus {
            outline: none;
            border-color: var(--color-primary);
        }

        .form-actions {
            display: flex;
            gap: 1rem;
        }

        .submit-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.875rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
        }

        .submit-btn:disabled {
            opacity: 0.7;
        }

        .cancel-btn {
            padding: 0.875rem 2rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
        }

        .no-reviews,
        .no-machines {
            text-align: center;
            padding: 3rem;
            color: #6c757d;
        }

        .no-reviews i,
        .no-machines i {
            font-size: 3rem;
            margin-bottom: 1rem;
            opacity: 0.5;
        }

        .review-item {
            padding: 1.5rem 0;
            border-bottom: 1px solid #e9ecef;
        }

        .review-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 0.75rem;
        }

        .reviewer-info {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .reviewer-name {
            font-weight: 700;
            color: #1a1a2e;
        }

        .review-date {
            color: #6c757d;
            font-size: 0.85rem;
        }

        .review-title {
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .review-comment {
            color: #6c757d;
            line-height: 1.6;
        }

        /* Machines Tab */
        .machine-item {
            padding: 1.25rem;
            background: #f8f9fa;
            border-radius: 12px;
            margin-bottom: 1rem;
        }

        .machine-name {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            font-weight: 700;
            color: var(--color-primary);
            text-decoration: none;
            font-size: 1.1rem;
        }

        .machine-name:hover {
            text-decoration: underline;
        }

        .machine-notes {
            margin-top: 0.5rem;
            color: #6c757d;
            padding-left: 2rem;
        }

        /* ============ RELATED PRODUCTS ============ */
        .related-section {
            margin-top: 4rem;
        }

        .section-header {
            text-align: center;
            margin-bottom: 3rem;
        }

        .section-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1.25rem;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 50px;
            color: var(--color-primary);
            font-size: 0.875rem;
            font-weight: 600;
            margin-bottom: 1rem;
        }

        .section-header h2 {
            font-size: 2rem;
            font-weight: 800;
            color: #1a1a2e;
        }

        .related-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
        }

        .related-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            transition: all 0.3s;
            animation: fadeIn 0.5s ease-out backwards;
        }

        .related-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
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

        .related-image {
            position: relative;
            padding-top: 100%;
            cursor: pointer;
            overflow: hidden;
        }

        .related-image img {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.3s;
        }

        .related-card:hover .related-image img {
            transform: scale(1.05);
        }

        .related-info {
            padding: 1.5rem;
        }

        .related-info h4 {
            margin-bottom: 0.5rem;
        }

        .related-info h4 a {
            color: #1a1a2e;
            text-decoration: none;
            font-weight: 700;
            transition: color 0.3s;
        }

        .related-info h4 a:hover {
            color: var(--color-primary);
        }

        .related-price {
            font-size: 1.25rem;
            font-weight: 800;
            color: var(--color-primary);
            margin-bottom: 1rem;
        }

        .related-cart-btn {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 0.75rem 1rem;
            background: transparent;
            border: 2px solid var(--color-primary);
            border-radius: 10px;
            color: var(--color-primary);
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .related-cart-btn:hover {
            background: var(--gradient-primary);
            border-color: transparent;
            color: white;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 1024px) {
            .product-grid {
                grid-template-columns: 1fr;
            }
            .related-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .main-section {
                padding: 1.5rem 1rem;
            }
            .gallery-card {
                padding: 1rem;
            }
            .gallery-main-image {
                max-height: 260px;
            }
            :host ::ng-deep .custom-galleria .p-galleria-item-wrapper,
            :host ::ng-deep .custom-galleria .p-galleria-item-container {
                max-height: 260px;
            }
            .no-image {
                min-height: 240px;
            }
            .no-image img {
                max-height: 240px;
            }
            .video-container video {
                max-height: 220px;
            }
            .product-title {
                font-size: 1.35rem;
            }
            .price-display {
                font-size: 1.5rem;
            }
            .info-card {
                padding: 1.25rem 1rem;
            }
            .tabs-nav {
                flex-wrap: wrap;
            }
            .tab-btn {
                flex: 1;
                justify-content: center;
                padding: 0.75rem 1rem;
                font-size: 0.85rem;
            }
            .reviews-header {
                flex-direction: column;
                gap: 1.5rem;
            }
            .cart-actions {
                flex-direction: column;
            }
            .quantity-selector {
                width: 100%;
                justify-content: center;
            }
            .add-to-cart-btn {
                width: 100%;
            }
            .related-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class ItemDetailsComponent implements OnInit {
    itemDetail: ItemDetail | null = null;
    loading = true;
    quantity = 1;
    isInWishlist = false;
    isLoggedIn = false;
    showReviewForm = false;
    submittingReview = false;
    reviewRating = 0;
    reviews: Review[] = [];
    reviewsSummary: any = null;
    relatedItems: Item[] = [];
    images: any[] = [];
    videoUrl: string | null = null;
    responsiveOptions: any[] = [];
    activeTab: 'details' | 'reviews' = 'details';

    reviewForm: FormGroup;

    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private fb = inject(FormBuilder);
    private itemsApiService = inject(ItemsApiService);
    private reviewsApiService = inject(ReviewsApiService);
    private cartService = inject(CartService);
    private wishlistApiService = inject(WishlistApiService);
    private authService = inject(AuthService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.reviewForm = this.fb.group({
            rating: [0, Validators.required],
            title: [''],
            comment: ['']
        });
    }

    get item(): Item | null {
        return this.itemDetail?.item || null;
    }

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadItemDetail(id);
            this.checkWishlist(id);
            this.checkAuth();
        }

        this.responsiveOptions = [
            {
                breakpoint: '1024px',
                numVisible: 4
            },
            {
                breakpoint: '768px',
                numVisible: 3
            },
            {
                breakpoint: '560px',
                numVisible: 1
            }
        ];
    }

    loadItemDetail(id: string) {
        this.loading = true;
        this.itemsApiService.getItemDetail(id).subscribe({
            next: (detail: any) => {
                this.itemDetail = detail?.item != null
                    ? detail
                    : {
                        item: detail,
                        sections: detail?.sections ?? [],
                        documents: detail?.documents ?? [],
                        mediaAssets: detail?.mediaAssets ?? [],
                        compatibleMachines: detail?.compatibleMachines ?? [],
                        reviews: detail?.reviews ?? [],
                        relatedItems: detail?.relatedItems ?? []
                    };
                this.loadImages();
                this.loadVideo();
                this.loadReviews(id);
                this.loadRelatedItems(id);
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    loadImages() {
        if (!this.itemDetail) return;
        this.images = (this.itemDetail.mediaAssets || [])
            .filter(media => media.mediaType === 'Image')
            .map(media => {
                const raw = (media as { fileUrl?: string; mediaAssetUrl?: string }).fileUrl ?? (media as { fileUrl?: string; mediaAssetUrl?: string }).mediaAssetUrl ?? '';
                return {
                    url: this.getFullMediaUrl(raw),
                    alt: this.languageService.language === 'ar' ? media.altTextAr : media.altTextEn || this.languageService.getLocalizedName(this.itemDetail!.item)
                };
            });
    }

    loadVideo() {
        if (!this.itemDetail) return;
        const videoMedia = this.itemDetail.mediaAssets?.find(media => media.mediaType === 'Video') as { fileUrl?: string; mediaAssetUrl?: string } | undefined;
        const raw = videoMedia?.fileUrl ?? videoMedia?.mediaAssetUrl ?? null;
        this.videoUrl = raw ? this.getFullMediaUrl(raw) : null;
    }

    private getFullMediaUrl(url: string | null | undefined): string {
        if (!url?.trim()) return '';
        if (url.startsWith('http')) return url;
        const apiUrl = environment.apiUrl || '';
        const baseUrl = apiUrl.replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '') || '';
        return baseUrl ? `${baseUrl}${url.startsWith('/') ? url : '/' + url}` : url;
    }

    loadReviews(itemId: string) {
        this.reviewsApiService.getReviews(itemId).subscribe({
            next: (response) => {
                this.reviews = response.items || [];
                this.reviewsSummary = {
                    averageRating: response.averageRating,
                    totalReviews: response.totalCount
                };
            }
        });
    }

    loadRelatedItems(itemId: string) {
        this.itemsApiService.getRelatedItems(itemId, 4).subscribe({
            next: (items) => {
                this.relatedItems = items;
            }
        });
    }

    checkWishlist(itemId: string) {
        this.wishlistApiService.isInWishlist(itemId).subscribe({
            next: (inWishlist) => { this.isInWishlist = inWishlist; },
            error: () => { this.isInWishlist = false; }
        });
    }

    checkAuth() {
        if (!this.authService.token) {
            this.isLoggedIn = false;
            return;
        }
        this.authService.getCurrentUser().subscribe({
            next: (user) => { this.isLoggedIn = !!user; },
            error: () => { this.isLoggedIn = false; }
        });
    }

    addToCart() {
        if (!this.item) return;
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
            return;
        }
        const request = {
            itemId: this.item.id,
            quantity: this.quantity
        };
        
        const itemDetails = {
            nameEn: this.item.nameEn || '',
            nameAr: this.item.nameAr || '',
            sku: this.item.sku || '',
            price: this.item.price,
            imageUrl: (this.item as any).primaryImageUrl || ''
        };
        
        this.cartService.addToCart(request, itemDetails).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('item.addedToCart')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    addRelatedToCart(relatedItem: Item) {
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
            return;
        }
        const request = {
            itemId: relatedItem.id,
            quantity: 1
        };
        
        const itemDetails = {
            nameEn: relatedItem.nameEn || '',
            nameAr: relatedItem.nameAr || '',
            sku: relatedItem.sku || '',
            price: relatedItem.price,
            imageUrl: (relatedItem as any).primaryImageUrl || ''
        };
        
        this.cartService.addToCart(request, itemDetails).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('item.addedToCart')
                });
            }
        });
    }

    toggleWishlist() {
        if (!this.item) return;
        if (this.isInWishlist) {
            this.wishlistApiService.removeFromWishlist(this.item.id).subscribe({
                next: () => {
                    this.isInWishlist = false;
                }
            });
        } else {
            this.wishlistApiService.addToWishlist({ itemId: this.item.id }).subscribe({
                next: () => {
                    this.isInWishlist = true;
                }
            });
        }
    }

    shareItem() {
        if (navigator.share && this.item) {
            navigator.share({
                title: this.languageService.getLocalizedName(this.item),
                text: this.getDescription(),
                url: window.location.href
            }).catch(() => {});
        } else {
            navigator.clipboard.writeText(window.location.href).then(() => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('item.linkCopied')
                });
            });
        }
    }

    submitReview() {
        if (!this.item || !this.reviewForm.valid) return;
        this.submittingReview = true;
        const formValue = this.reviewForm.value;
        const request: CreateReviewRequest = {
            itemId: this.item.id,
            rating: formValue.rating,
            title: formValue.title,
            comment: formValue.comment
        };
        this.reviewsApiService.createReview(request).subscribe({
            next: () => {
                this.submittingReview = false;
                this.showReviewForm = false;
                this.loadReviews(this.item!.id);
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('item.reviewSubmitted')
                });
            },
            error: () => {
                this.submittingReview = false;
            }
        });
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

    getDescription(): string {
        if (!this.item) return '';
        const desc = this.languageService.language === 'ar' 
            ? this.item.generalDescriptionAr 
            : this.item.generalDescriptionEn;
        return desc || '';
    }

    isInStock(): boolean {
        return (this.item?.inventory?.quantityOnHand || 0) > 0;
    }

    getStockStatus(): string {
        return this.isInStock() 
            ? this.translateService.instant('item.inStock')
            : this.translateService.instant('item.outOfStock');
    }

    getMaxQuantity(): number {
        return this.item?.inventory?.quantityOnHand || 0;
    }

    formatCurrency(value: number): string {
        const locale = this.languageService?.language === 'ar' ? 'ar-IL' : 'en-IL';
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString();
    }

    formatFileSize(bytes: number): string {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }
}
