import { Component, Input, Output, EventEmitter, OnInit, OnChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { GalleriaModule } from 'primeng/galleria';
import { LanguageService } from '../../../../shared/services/language.service';
import { Item } from '../../../../shared/models/item.model';
import { MediaUrlService } from '../../../../shared/services/media-url.service';

@Component({
    selector: 'app-product-quick-view',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        DialogModule,
        ButtonModule,
        RatingModule,
        InputNumberModule,
        TagModule,
        GalleriaModule
    ],
    template: `
        <p-dialog
            [(visible)]="visible"
            [modal]="true"
            [closable]="true"
            [dismissableMask]="true"
            [style]="{ width: '90vw', maxWidth: '900px' }"
            [draggable]="false"
            [resizable]="false"
            (onHide)="onClose()">
            <ng-template pTemplate="header">
                <h2 class="m-0">{{ productName }}</h2>
            </ng-template>
            
            <div *ngIf="product" class="quick-view-content">
                <div class="grid">
                    <!-- Images -->
                    <div class="col-12 md:col-6">
                        <p-galleria
                            *ngIf="images.length > 0"
                            [value]="images"
                            [responsiveOptions]="responsiveOptions"
                            [numVisible]="4"
                            [circular]="true"
                            [showItemNavigators]="true"
                            [showThumbnails]="true"
                            [thumbnailsPosition]="'bottom'"
                            styleClass="w-full">
                            <ng-template #item let-item>
                                <img [src]="item.url" [alt]="item.alt" class="w-full" style="max-height: 400px; object-fit: contain;" onerror="this.src='assets/img/defult.png'" />
                            </ng-template>
                            <ng-template #thumbnail let-item>
                                <img [src]="item.url" [alt]="item.alt" class="w-full" style="height: 80px; object-fit: cover;" onerror="this.src='assets/img/defult.png'" />
                            </ng-template>
                        </p-galleria>
                        <div *ngIf="images.length === 0" class="flex align-items-center justify-content-center" style="height: 400px; background: var(--surface-ground);">
                            <img [src]="productImage" [alt]="productName" class="w-full" style="max-height: 400px; object-fit: contain;" onerror="this.src='assets/img/defult.png'" />
                        </div>
                    </div>

                    <!-- Product Info -->
                    <div class="col-12 md:col-6">
                        <div class="product-details">
                            <!-- Brand -->
                            @if (product.brandNameEn || product.brandNameAr) {
                                <div class="product-brand mb-2">
                                    {{ languageService.language === 'ar' ? (product.brandNameAr || '') : (product.brandNameEn || '') }}
                                </div>
                            }

                            <!-- Price -->
                            <div class="product-price mb-3">
                                <span class="price-value">{{ product.price | number:'1.2-2' }} ILS</span>
                            </div>

                            <!-- Rating -->
                            @if (product.averageRating && product.averageRating > 0) {
                                <div class="product-rating mb-3">
                                    <p-rating 
                                        [ngModel]="product.averageRating" 
                                        [readonly]="true"
                                        [stars]="5">
                                    </p-rating>
                                    <span class="rating-value ml-2">{{ product.averageRating | number:'1.1-1' }}</span>
                                    @if (product.reviewsCount && product.reviewsCount > 0) {
                                        <span class="reviews-count ml-2">({{ product.reviewsCount }})</span>
                                    }
                                </div>
                            }

                            <!-- Stock Status -->
                            <div class="stock-status mb-3">
                                <p-tag 
                                    [value]="isInStock ? ('catalog.inStock' | translate) : ('catalog.outOfStock' | translate)" 
                                    [severity]="isInStock ? 'success' : 'danger'">
                                </p-tag>
                            </div>

                            <!-- SKU -->
                            <div class="product-sku mb-3">
                                <span class="label">{{ 'item.sku' | translate }}: </span>
                                <span>{{ product.sku }}</span>
                            </div>

                            <!-- Description -->
                            @if (productDescription) {
                                <div class="product-description mb-3">
                                    <p [innerHTML]="productDescription"></p>
                                </div>
                            }

                            <!-- Quantity & Add to Cart -->
                            <div class="add-to-cart-section">
                                <div class="flex align-items-center gap-3 mb-3">
                                    <label class="font-medium">{{ 'item.quantity' | translate }}:</label>
                                    <p-inputNumber 
                                        [(ngModel)]="quantity"
                                        [min]="1"
                                        [max]="maxQuantity"
                                        [showButtons]="true"
                                        styleClass="w-8rem">
                                    </p-inputNumber>
                                </div>
                                <div class="flex gap-2">
                                    <p-button 
                                        [label]="'item.addToCart' | translate" 
                                        icon="pi pi-shopping-cart"
                                        [disabled]="!isInStock"
                                        (click)="onAddToCart()"
                                        styleClass="flex-1">
                                    </p-button>
                                    <p-button 
                                        [label]="'catalog.viewDetails' | translate" 
                                        icon="pi pi-eye"
                                        [outlined]="true"
                                        [routerLink]="['/item', product.id]"
                                        (click)="onClose()">
                                    </p-button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </p-dialog>
    `,
    styles: [`
        .quick-view-content {
            padding: 0.5rem;
        }
        .product-details {
            padding: 1rem;
        }
        .product-brand {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            font-weight: 500;
        }
        .product-price {
            font-size: 2rem;
            font-weight: 700;
            color: var(--primary-color);
        }
        .product-rating {
            display: flex;
            align-items: center;
        }
        .rating-value {
            font-weight: 600;
            color: var(--text-color);
        }
        .reviews-count {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
        }
        .product-sku {
            font-size: 0.875rem;
        }
        .product-sku .label {
            font-weight: 600;
        }
        .product-description {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            line-height: 1.6;
        }
        .add-to-cart-section {
            border-top: 1px solid var(--surface-border);
            padding-top: 1rem;
            margin-top: 1rem;
        }
        ::ng-deep .p-dialog-header {
            padding: 1.5rem;
        }
        ::ng-deep .p-dialog-content {
            padding: 1.5rem;
        }
    `]
})
export class ProductQuickViewComponent implements OnInit, OnChanges {
    @Input() product: Item | null = null;
    @Input() visible: boolean = false;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() addToCart = new EventEmitter<{ item: Item; quantity: number }>();

    quantity = 1;
    images: Array<{ url: string; alt: string }> = [];
    
    responsiveOptions: any[] = [
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

    languageService = inject(LanguageService);
    private mediaUrl = inject(MediaUrlService);

    ngOnInit() {
        if (this.product) {
            this.loadImages();
        }
    }

    ngOnChanges() {
        if (this.product) {
            this.loadImages();
            this.quantity = 1;
        }
    }

    get productName(): string {
        if (!this.product) return '';
        return this.languageService.getLocalizedName(this.product);
    }

    get productDescription(): string {
        if (!this.product) return '';
        if (this.languageService.language === 'ar' && this.product.generalDescriptionAr) {
            return this.product.generalDescriptionAr;
        }
        return this.product.generalDescriptionEn || '';
    }

    get productImage(): string {
        if (!this.product) return 'assets/img/defult.png';
        const primaryUrl = (this.product as { primaryImageUrl?: string }).primaryImageUrl;
        if (primaryUrl) return this.getFullImageUrl(primaryUrl);
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
        return this.mediaUrl.getUrl(url);
    }

    get isInStock(): boolean {
        if (!this.product) return false;
        return (this.product.inventory?.quantityOnHand || 0) > 0;
    }

    get maxQuantity(): number {
        if (!this.product) return 1;
        return this.product.inventory?.quantityOnHand || 1;
    }

    loadImages() {
        if (!this.product) return;
        this.images = [];
        
        if (this.product.mediaAssets && this.product.mediaAssets.length > 0) {
            this.images = this.product.mediaAssets
                .filter((m: any) => {
                    const mediaType = 'mediaType' in m ? m.mediaType : (m.mediaType || 'Image');
                    return mediaType === 'Image';
                })
                .map((m: any) => {
                    const raw = (m as { fileUrl?: string; mediaAssetUrl?: string }).fileUrl ?? (m as { fileUrl?: string; mediaAssetUrl?: string }).mediaAssetUrl ?? '';
                    return { url: this.getFullImageUrl(raw), alt: this.productName };
                })
                .filter(img => img.url);
        }
        
        if (this.images.length === 0) {
            this.images = [{ url: this.productImage, alt: this.productName }];
        }
    }

    onClose() {
        this.visible = false;
        this.visibleChange.emit(false);
    }

    onAddToCart() {
        if (this.product && this.isInStock) {
            this.addToCart.emit({ item: this.product, quantity: this.quantity });
            this.onClose();
        }
    }
}
