import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { LanguageService } from '../../../../shared/services/language.service';
import { Item } from '../../../../shared/models/item.model';
import { environment } from '../../../../../environments/environment';

@Component({
    selector: 'app-product-card',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, TranslateModule, CardModule, ButtonModule, RatingModule, TagModule, TooltipModule],
    template: `
        <div class="product-card" [class.list-view]="viewMode === 'list'">
            <a [routerLink]="['/item', product.id]" class="product-link">
                <div class="product-image-container">
                    <img 
                        [src]="productImage" 
                        [alt]="productName"
                        class="product-image"
                        loading="lazy"
                        (error)="onImageError($event)">
                    @if (isOutOfStock) {
                        <div class="out-of-stock-badge">
                            {{ 'catalog.outOfStock' | translate }}
                        </div>
                    }
                    @if (isNewProduct) {
                        <div class="new-badge">
                            {{ 'catalog.new' | translate }}
                        </div>
                    }
                    @if (isFeatured) {
                        <div class="featured-badge">
                            <i class="pi pi-star-fill"></i>
                            {{ 'catalog.featured' | translate }}
                        </div>
                    }
                    <div class="quick-actions-overlay">
                        <button 
                            pButton 
                            type="button"
                            icon="pi pi-eye"
                            class="p-button-rounded p-button-text p-button-plain"
                            [pTooltip]="'catalog.quickView' | translate"
                            tooltipPosition="top"
                            (click)="onQuickView($event)">
                        </button>
                        <button 
                            pButton 
                            type="button"
                            icon="pi pi-shopping-cart"
                            class="p-button-rounded p-button-text p-button-plain"
                            [pTooltip]="'catalog.addToCart' | translate"
                            tooltipPosition="top"
                            [disabled]="isOutOfStock"
                            (click)="onAddToCart($event)">
                        </button>
                    </div>
                </div>
                <div class="product-info">
                    <h3 class="product-name">{{ productName }}</h3>
                    @if (product.brandNameEn || product.brandNameAr) {
                        <div class="product-brand">
                            {{ languageService.language === 'ar' ? (product.brandNameAr || '') : (product.brandNameEn || '') }}
                        </div>
                    }
                    <div class="product-price-container">
                        <div class="product-price">
                            {{ product.price | number:'1.2-2' }} ILS
                        </div>
                        @if (hasDiscount) {
                            <div class="product-old-price">
                                {{ originalPrice | number:'1.2-2' }} ILS
                            </div>
                            <div class="discount-badge">
                                -{{ discountPercentage }}%
                            </div>
                        }
                    </div>
                    @if (product.averageRating && product.averageRating > 0) {
                        <div class="product-rating">
                            <p-rating 
                                [ngModel]="product.averageRating" 
                                [readonly]="true"
                                [stars]="5">
                            </p-rating>
                            <span class="rating-value">{{ product.averageRating | number:'1.1-1' }}</span>
                            @if (product.reviewsCount && product.reviewsCount > 0) {
                                <span class="reviews-count">({{ product.reviewsCount }})</span>
                            }
                        </div>
                    }
                    @if (product.generalDescriptionEn || product.generalDescriptionAr) {
                        <p class="product-description">
                            {{ productDescription }}
                        </p>
                    }
                </div>
            </a>
            <div class="product-actions">
                <button 
                    pButton 
                    type="button"
                    [label]="'catalog.viewDetails' | translate"
                    icon="pi pi-eye"
                    class="p-button-sm p-button-outlined"
                    [routerLink]="['/item', product.id]">
                </button>
                <button 
                    pButton 
                    type="button"
                    [label]="'catalog.addToCart' | translate"
                    icon="pi pi-shopping-cart"
                    class="p-button-sm add-to-cart-btn"
                    [disabled]="isOutOfStock"
                    (click)="onAddToCart($event)">
                </button>
            </div>
        </div>
    `,
    styles: [`
        :host {
            --primary-color: #667eea;
            --primary-color-dark: #764ba2;
        }
        .product-card {
            background: var(--surface-card);
            border-radius: 12px;
            overflow: hidden;
            border: 1px solid var(--surface-border);
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            flex-direction: column;
            height: 100%;
            position: relative;
        }
        .product-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
            border-color: #667eea;
        }
        .product-card.list-view {
            flex-direction: row;
            max-width: 100%;
        }
        .product-card.list-view .product-image-container {
            width: 300px;
            min-width: 300px;
            height: 200px;
        }
        .product-card.list-view .product-info {
            flex: 1;
        }
        .product-link {
            text-decoration: none;
            color: inherit;
            flex: 1;
            display: flex;
            flex-direction: column;
        }
        .product-image-container {
            width: 100%;
            height: 280px;
            overflow: hidden;
            background: var(--surface-ground);
            position: relative;
        }
        .product-image {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.4s ease;
        }
        .product-card:hover .product-image {
            transform: scale(1.05);
        }
        .out-of-stock-badge,
        .new-badge,
        .featured-badge {
            position: absolute;
            top: 0.75rem;
            padding: 0.375rem 0.75rem;
            border-radius: 6px;
            font-size: 0.75rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            z-index: 2;
        }
        .out-of-stock-badge {
            right: 0.75rem;
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3);
        }
        .new-badge {
            left: 0.75rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
        }
        .featured-badge {
            left: 0.75rem;
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
            color: white;
            box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
            display: flex;
            align-items: center;
            gap: 0.25rem;
        }
        .featured-badge i {
            font-size: 0.875rem;
        }
        .quick-actions-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
            opacity: 0;
            transition: opacity 0.3s ease;
            z-index: 3;
        }
        .product-card:hover .quick-actions-overlay {
            opacity: 1;
        }
        .quick-actions-overlay button {
            background: white;
            color: var(--text-color);
            border: none;
        }
        .quick-actions-overlay button:hover {
            background: #667eea;
            color: white;
        }
        .product-info {
            padding: 1.25rem;
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        .product-name {
            font-size: 1.125rem;
            font-weight: 600;
            margin: 0;
            color: var(--text-color);
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            line-height: 1.4;
        }
        .product-brand {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            font-weight: 500;
        }
        .product-price-container {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            flex-wrap: wrap;
        }
        .product-price {
            font-size: 1.5rem;
            font-weight: 700;
            color: #667eea;
            margin: 0;
        }
        .product-old-price {
            font-size: 1rem;
            color: var(--text-color-secondary);
            text-decoration: line-through;
        }
        .discount-badge {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
            font-size: 0.75rem;
            font-weight: 700;
        }
        .product-rating {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin: 0.25rem 0;
        }
        .rating-value {
            font-weight: 600;
            color: var(--text-color);
            font-size: 0.875rem;
        }
        .reviews-count {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
        }
        .product-description {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            margin-top: auto;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            line-height: 1.5;
        }
        .product-actions {
            padding: 0 1.25rem 1.25rem;
            display: flex;
            gap: 0.5rem;
        }
        .product-actions button {
            flex: 1;
        }
        :host ::ng-deep .p-rating .p-rating-icon.p-rating-icon-active,
        :host ::ng-deep .p-rating .p-rating-icon:hover {
            color: #667eea !important;
        }
        :host ::ng-deep .p-button.p-button-outlined {
            background: transparent !important;
            background-color: transparent !important;
            border: 2px solid #667eea !important;
            border-color: #667eea !important;
            color: #667eea !important;
        }
        :host ::ng-deep .p-button.p-button-outlined:hover {
            background: rgba(102, 126, 234, 0.1) !important;
            border-color: #5a72d9 !important;
            color: #5a72d9 !important;
        }
        :host ::ng-deep .p-button.p-button-outlined .p-button-icon {
            color: #667eea !important;
        }
        :host ::ng-deep .product-actions .p-button.p-button-outlined {
            background: transparent !important;
            border: 2px solid #667eea !important;
            color: #667eea !important;
        }
        :host ::ng-deep .p-button:not(.p-button-outlined):not(.p-button-text) {
            background: #667eea !important;
            border-color: #667eea !important;
        }
        :host ::ng-deep .add-to-cart-btn {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            background-color: #667eea !important;
            border-color: #667eea !important;
            color: white !important;
        }
        :host ::ng-deep .add-to-cart-btn:hover {
            background: linear-gradient(135deg, #5a72d9 0%, #6a4190 100%) !important;
            background-color: #5a72d9 !important;
            border-color: #5a72d9 !important;
        }
        @media (max-width: 768px) {
            .product-image-container {
                height: 220px;
            }
            .product-card.list-view {
                flex-direction: column;
            }
            .product-card.list-view .product-image-container {
                width: 100%;
                min-width: unset;
            }
        }
    `]
})
export class ProductCardComponent {
    @Input() product!: Item;
    @Input() viewMode: 'grid' | 'list' = 'grid';
    @Output() quickView = new EventEmitter<Item>();
    @Output() addToCart = new EventEmitter<Item>();

    constructor(public languageService: LanguageService) {}

    get productName(): string {
        return this.languageService.getLocalizedName(this.product);
    }

    get productDescription(): string {
        if (this.languageService.language === 'ar' && this.product.generalDescriptionAr) {
            return this.product.generalDescriptionAr;
        }
        return this.product.generalDescriptionEn || '';
    }

    get productImage(): string {
        // First check for primaryImageUrl (from backend ItemDto)
        if ((this.product as any).primaryImageUrl) {
            return this.getFullImageUrl((this.product as any).primaryImageUrl);
        }
        
        // Use actual mediaAssets if available
        if (this.product.mediaAssets && this.product.mediaAssets.length > 0) {
            const primaryImage = this.product.mediaAssets.find((m: any) => m.isPrimary || m.mediaType === 'Image') as { fileUrl?: string; mediaAssetUrl?: string } | undefined;
            if (primaryImage) {
                const url = primaryImage.fileUrl ?? primaryImage.mediaAssetUrl;
                if (url) return this.getFullImageUrl(url);
            }
            const firstMedia = this.product.mediaAssets[0] as { fileUrl?: string; mediaAssetUrl?: string } | undefined;
            if (firstMedia) {
                const url = firstMedia.fileUrl ?? firstMedia.mediaAssetUrl;
                if (url) return this.getFullImageUrl(url);
            }
        }
        return 'assets/img/defult.png';
    }

    private getFullImageUrl(url: string): string {
        if (!url) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        // Get base URL from environment
        const apiUrl = environment.apiUrl || 'http://localhost:5153/api/v1';
        const baseUrl = apiUrl.replace('/api/v1', '').replace('/api', '');
        return `${baseUrl}${url}`;
    }

    get isOutOfStock(): boolean {
        // Check inventory if available
        return (this.product.inventory?.quantityOnHand || 0) <= 0;
    }

    get isNewProduct(): boolean {
        if (!this.product.createdAt) return false;
        const createdDate = new Date(this.product.createdAt);
        const daysSinceCreation = (Date.now() - createdDate.getTime()) / (1000 * 60 * 60 * 24);
        return daysSinceCreation <= 30; // New if created within last 30 days
    }

    get isFeatured(): boolean {
        // Check if product has high rating or views
        return (this.product.averageRating >= 4.5) || (this.product.viewsCount > 1000);
    }

    get hasDiscount(): boolean {
        // For now, we'll assume no discount unless there's a discount field
        // This can be extended when discount functionality is added
        return false;
    }

    get originalPrice(): number {
        return this.product.price;
    }

    get discountPercentage(): number {
        return 0;
    }

    onImageError(event: Event) {
        const img = event.target as HTMLImageElement;
        img.src = 'assets/img/defult.png';
    }

    onQuickView(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.quickView.emit(this.product);
    }

    onAddToCart(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        if (!this.isOutOfStock) {
            this.addToCart.emit(this.product);
        }
    }
}

