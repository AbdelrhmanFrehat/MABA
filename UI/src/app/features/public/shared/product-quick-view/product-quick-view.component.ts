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
import { LanguageService } from '../../../../shared/services/language.service';
import { Item } from '../../../../shared/models/item.model';
import { environment } from '../../../../../environments/environment';

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
        TagModule
    ],
    template: `
        <p-dialog
            [(visible)]="visible"
            [modal]="true"
            [closable]="false"
            [dismissableMask]="true"
            [style]="{ width: '95vw', maxWidth: '1020px' }"
            [draggable]="false"
            [resizable]="false"
            styleClass="qv-dialog"
            (onHide)="onClose()">

            <!-- Header -->
            <ng-template pTemplate="header">
                <div class="qv-header">
                    <div class="qv-header-left">
                        <span class="qv-header-eyebrow">{{ 'catalog.quickView' | translate }}</span>
                        <h2 class="qv-header-title">{{ productName }}</h2>
                    </div>
                    <button type="button" class="qv-close-btn" (click)="onClose()" [attr.aria-label]="'common.close' | translate">
                        <i class="pi pi-times"></i>
                    </button>
                </div>
            </ng-template>

            <!-- Body -->
            <div *ngIf="product" class="qv-body">

                <!-- LEFT: Image panel -->
                <div class="qv-image-panel">
                    <div class="qv-main-image-wrap">
                        <img
                            [src]="activeImage"
                            [alt]="productName"
                            class="qv-main-image"
                            onerror="this.src='assets/img/defult.png'" />
                        @if (product.isNew) {
                            <span class="qv-badge qv-badge-new">NEW</span>
                        }
                        @if (product.isOnSale && product.discountPrice) {
                            <span class="qv-badge qv-badge-sale">SALE</span>
                        }
                    </div>

                    <!-- Thumbnails -->
                    @if (images.length > 1) {
                        <div class="qv-thumbnails">
                            @for (img of images; track img.url; let i = $index) {
                                <button
                                    type="button"
                                    class="qv-thumb-btn"
                                    [class.active]="i === activeImageIndex"
                                    (click)="selectImage(i)">
                                    <img [src]="img.url" [alt]="img.alt" onerror="this.src='assets/img/defult.png'" />
                                </button>
                            }
                        </div>
                    }
                </div>

                <!-- RIGHT: Info panel -->
                <div class="qv-info-panel">

                    <!-- Brand -->
                    @if (product.brandNameEn || product.brandNameAr) {
                        <div class="qv-brand">
                            <i class="pi pi-building"></i>
                            {{ languageService.language === 'ar' ? (product.brandNameAr || '') : (product.brandNameEn || '') }}
                        </div>
                    }

                    <!-- Price -->
                    <div class="qv-price-block">
                        @if (product.isOnSale && product.discountPrice) {
                            <span class="qv-price-original">₪{{ product.price | number:'1.2-2' }}</span>
                            <span class="qv-price-main">₪{{ product.discountPrice | number:'1.2-2' }}</span>
                        } @else {
                            <span class="qv-price-main">₪{{ product.price | number:'1.2-2' }}</span>
                        }
                    </div>

                    <!-- Rating -->
                    @if (product.averageRating && product.averageRating > 0) {
                        <div class="qv-rating">
                            <p-rating [ngModel]="product.averageRating" [readonly]="true" [stars]="5"></p-rating>
                            <span class="qv-rating-num">{{ product.averageRating | number:'1.1-1' }}</span>
                            @if (product.reviewsCount && product.reviewsCount > 0) {
                                <span class="qv-rating-count">({{ product.reviewsCount }})</span>
                            }
                        </div>
                    }

                    <!-- Stock + SKU row -->
                    <div class="qv-meta-row">
                        <span class="qv-stock-pill" [class.in-stock]="isInStock" [class.out-stock]="!isInStock">
                            <i [class]="isInStock ? 'pi pi-check-circle' : 'pi pi-times-circle'"></i>
                            {{ isInStock ? ('catalog.inStock' | translate) : ('catalog.outOfStock' | translate) }}
                        </span>
                        <span class="qv-sku">
                            <span class="qv-sku-label">SKU</span> {{ product.sku }}
                        </span>
                    </div>

                    <!-- Short description -->
                    @if (productDescription) {
                        <p class="qv-description">{{ productDescription }}</p>
                    }

                    <!-- Quantity + Add to Cart -->
                    <div class="qv-actions">
                        <div class="qv-qty-row">
                            <label class="qv-qty-label">{{ 'item.quantity' | translate }}</label>
                            <p-inputNumber
                                [(ngModel)]="quantity"
                                [min]="1"
                                [max]="maxQuantity"
                                [showButtons]="true"
                                styleClass="qv-qty-input">
                            </p-inputNumber>
                        </div>

                        <button
                            type="button"
                            class="qv-add-btn"
                            [disabled]="!isInStock"
                            (click)="onAddToCart()">
                            <i class="pi pi-shopping-cart"></i>
                            {{ 'item.addToCart' | translate }}
                        </button>

                        <a
                            class="qv-details-link"
                            [routerLink]="['/item', product.id]"
                            (click)="onClose()">
                            <i class="pi pi-arrow-right"></i>
                            {{ 'catalog.viewDetails' | translate }}
                        </a>
                    </div>

                </div>
            </div>
        </p-dialog>
    `,
    styles: [`
        /* ── Dialog shell overrides ──────────────────────── */
        :host ::ng-deep .qv-dialog .p-dialog-header {
            padding: 0 !important;
            background: #0f172a !important;
            border-bottom: 1px solid #1e293b !important;
        }
        :host ::ng-deep .qv-dialog .p-dialog-header-actions { display: none !important; }
        :host ::ng-deep .qv-dialog .p-dialog-content {
            padding: 0 !important;
            overflow: hidden !important;
            border-radius: 0 0 12px 12px !important;
        }
        :host ::ng-deep .qv-dialog { border-radius: 12px !important; overflow: hidden !important; }

        /* ── Header ─────────────────────────────────────── */
        .qv-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            padding: 0.875rem 1.25rem;
            width: 100%;
        }
        .qv-header-left { display: flex; flex-direction: column; gap: 0.1rem; min-width: 0; }
        .qv-header-eyebrow {
            font-size: 0.65rem;
            font-weight: 600;
            letter-spacing: 0.1em;
            text-transform: uppercase;
            color: #94a3b8;
        }
        .qv-header-title {
            margin: 0;
            font-size: 1.1rem;
            font-weight: 700;
            color: #f1f5f9;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 700px;
        }
        .qv-close-btn {
            flex-shrink: 0;
            width: 2rem;
            height: 2rem;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            background: rgba(255,255,255,0.06);
            border: 1px solid rgba(255,255,255,0.1);
            color: #94a3b8;
            border-radius: 6px;
            cursor: pointer;
            transition: background 0.15s, color 0.15s;
            font-size: 0.8rem;
        }
        .qv-close-btn:hover { background: rgba(255,255,255,0.14); color: #f1f5f9; }

        /* ── Body layout (2-column) ──────────────────────── */
        .qv-body {
            display: flex;
            min-height: 460px;
        }
        .qv-image-panel {
            flex: 0 0 58%;
            background: #f8fafc;
            border-right: 1px solid #e2e8f0;
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            padding: 1.5rem;
        }
        .qv-info-panel {
            flex: 1;
            padding: 1.75rem 1.5rem 1.5rem;
            display: flex;
            flex-direction: column;
            gap: 0.875rem;
            overflow-y: auto;
            background: #fff;
        }

        /* ── Main image ──────────────────────────────────── */
        .qv-main-image-wrap {
            position: relative;
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            overflow: hidden;
            min-height: 340px;
        }
        .qv-main-image {
            max-height: 400px;
            max-width: 100%;
            width: 100%;
            object-fit: contain;
            padding: 0.5rem;
            display: block;
        }
        .qv-badge {
            position: absolute;
            top: 0.75rem;
            left: 0.75rem;
            font-size: 0.65rem;
            font-weight: 700;
            letter-spacing: 0.06em;
            padding: 0.25rem 0.55rem;
            border-radius: 4px;
        }
        .qv-badge-new { background: #3b82f6; color: #fff; }
        .qv-badge-sale { background: #ef4444; color: #fff; }

        /* ── Thumbnails ──────────────────────────────────── */
        .qv-thumbnails {
            display: flex;
            gap: 0.5rem;
            overflow-x: auto;
            padding-bottom: 2px;
        }
        .qv-thumb-btn {
            flex-shrink: 0;
            width: 64px;
            height: 64px;
            border: 2px solid #e2e8f0;
            border-radius: 7px;
            overflow: hidden;
            background: #fff;
            cursor: pointer;
            padding: 0;
            transition: border-color 0.15s;
        }
        .qv-thumb-btn img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }
        .qv-thumb-btn.active { border-color: #667eea; }
        .qv-thumb-btn:hover:not(.active) { border-color: #94a3b8; }

        /* ── Brand ───────────────────────────────────────── */
        .qv-brand {
            display: flex;
            align-items: center;
            gap: 0.35rem;
            font-size: 0.78rem;
            font-weight: 600;
            color: #94a3b8;
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }
        .qv-brand .pi { font-size: 0.75rem; }

        /* ── Price ───────────────────────────────────────── */
        .qv-price-block {
            display: flex;
            align-items: baseline;
            gap: 0.75rem;
        }
        .qv-price-main {
            font-size: 2.25rem;
            font-weight: 800;
            color: #0f172a;
            letter-spacing: -0.02em;
            line-height: 1;
        }
        .qv-price-original {
            font-size: 1.1rem;
            font-weight: 500;
            color: #94a3b8;
            text-decoration: line-through;
        }

        /* ── Rating ──────────────────────────────────────── */
        .qv-rating {
            display: flex;
            align-items: center;
            gap: 0.4rem;
        }
        .qv-rating-num { font-size: 0.875rem; font-weight: 600; color: #334155; }
        .qv-rating-count { font-size: 0.8rem; color: #94a3b8; }
        :host ::ng-deep .qv-rating .p-rating-icon,
        :host ::ng-deep .qv-rating .p-rating-option-active .p-rating-icon {
            color: #f59e0b !important;
            fill: #f59e0b !important;
        }

        /* ── Stock pill + SKU ────────────────────────────── */
        .qv-meta-row {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            flex-wrap: wrap;
        }
        .qv-stock-pill {
            display: inline-flex;
            align-items: center;
            gap: 0.3rem;
            font-size: 0.75rem;
            font-weight: 600;
            padding: 0.25rem 0.65rem;
            border-radius: 20px;
        }
        .qv-stock-pill.in-stock { background: #dcfce7; color: #166534; }
        .qv-stock-pill.out-stock { background: #fee2e2; color: #991b1b; }
        .qv-stock-pill .pi { font-size: 0.7rem; }
        .qv-sku {
            font-size: 0.78rem;
            color: #64748b;
        }
        .qv-sku-label {
            font-weight: 600;
            color: #334155;
            margin-right: 0.15rem;
        }

        /* ── Description ─────────────────────────────────── */
        .qv-description {
            margin: 0;
            font-size: 0.84rem;
            color: #475569;
            line-height: 1.65;
            display: -webkit-box;
            -webkit-line-clamp: 4;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }

        /* ── Actions ─────────────────────────────────────── */
        .qv-actions {
            display: flex;
            flex-direction: column;
            gap: 0.625rem;
            margin-top: auto;
            padding-top: 1rem;
            border-top: 1px solid #f1f5f9;
        }
        .qv-qty-row {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }
        .qv-qty-label {
            font-size: 0.8rem;
            font-weight: 600;
            color: #334155;
            white-space: nowrap;
        }
        :host ::ng-deep .qv-qty-input { width: 8rem !important; }

        .qv-add-btn {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            width: 100%;
            padding: 0.8rem 1.5rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #fff;
            border: none;
            border-radius: 8px;
            font-size: 0.925rem;
            font-weight: 700;
            cursor: pointer;
            transition: opacity 0.15s, transform 0.1s;
            letter-spacing: 0.01em;
        }
        .qv-add-btn:hover:not(:disabled) { opacity: 0.9; transform: translateY(-1px); }
        .qv-add-btn:disabled { opacity: 0.45; cursor: not-allowed; transform: none; }
        .qv-add-btn .pi { font-size: 0.875rem; }

        .qv-details-link {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.4rem;
            width: 100%;
            padding: 0.65rem;
            border: 1.5px solid #e2e8f0;
            border-radius: 8px;
            color: #475569;
            font-size: 0.84rem;
            font-weight: 600;
            text-decoration: none;
            transition: border-color 0.15s, color 0.15s, background 0.15s;
            text-align: center;
        }
        .qv-details-link:hover { border-color: #667eea; color: #667eea; background: #f5f8ff; }
        .qv-details-link .pi { font-size: 0.75rem; }

        /* ── Responsive: stack on narrow screens ─────────── */
        @media (max-width: 700px) {
            .qv-body { flex-direction: column; }
            .qv-image-panel {
                flex: none;
                border-right: none;
                border-bottom: 1px solid #e2e8f0;
                padding: 1rem;
            }
            .qv-main-image-wrap { min-height: 220px; }
            .qv-info-panel { padding: 1.25rem; }
            .qv-price-main { font-size: 1.75rem; }
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
    activeImageIndex = 0;

    languageService = inject(LanguageService);

    ngOnInit() {
        if (this.product) this.loadImages();
    }

    ngOnChanges() {
        if (this.product) {
            this.loadImages();
            this.quantity = 1;
            this.activeImageIndex = 0;
        }
    }

    get activeImage(): string {
        return this.images[this.activeImageIndex]?.url ?? 'assets/img/defult.png';
    }

    selectImage(index: number) {
        this.activeImageIndex = index;
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
        if (!url?.trim()) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        const apiUrl = environment.apiUrl || '';
        const baseUrl = apiUrl.replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '') || '';
        return baseUrl ? `${baseUrl}${url.startsWith('/') ? url : '/' + url}` : url;
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
                    // ItemMediaLink uses `mediaType`, MediaAsset uses `mediaTypeKey`
                    // Accept entries where type is 'Image', or where type is unset
                    // (catalog list items often omit mediaType on image-only arrays)
                    const type = m.mediaType ?? m.mediaTypeKey;
                    return !type || type === 'Image';
                })
                .map((m: any) => {
                    const raw = m.fileUrl ?? m.mediaAssetUrl ?? '';
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
