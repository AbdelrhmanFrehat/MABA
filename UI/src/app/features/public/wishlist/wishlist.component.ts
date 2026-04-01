import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { WishlistApiService } from '../../../shared/services/wishlist-api.service';
import { CartApiService } from '../../../shared/services/cart-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import { Wishlist, WishlistItem } from '../../../shared/models/wishlist.model';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-wishlist',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        ButtonModule,
        ToastModule,
        ConfirmDialogModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <div class="wishlist-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-heart"></i>
                        {{ languageService.language === 'ar' ? 'المفضلة' : 'Wishlist' }}
                    </span>
                    <h1 class="hero-title">{{ 'wishlist.title' | translate }}</h1>
                    <p class="hero-description" *ngIf="wishlist && wishlist.items.length > 0">
                        {{ wishlist.items.length }} {{ languageService.language === 'ar' ? 'منتج في المفضلة' : 'items saved' }}
                    </p>
                </div>
            </section>

            <!-- Main Content -->
            <section class="content-section">
                <div class="container">
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                        </div>
                    </div>

                    <!-- Empty State -->
                    <div *ngIf="!loading && (!wishlist || wishlist.items.length === 0)" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-heart"></i>
                        </div>
                        <h2>{{ 'wishlist.empty' | translate }}</h2>
                        <p>{{ 'wishlist.emptyDescription' | translate }}</p>
                        <p-button 
                            [label]="'wishlist.startShopping' | translate" 
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            [routerLink]="['/catalog']"
                            styleClass="cta-button">
                        </p-button>
                    </div>

                    <!-- Wishlist Items -->
                    <div *ngIf="!loading && wishlist && wishlist.items.length > 0">
                        <div class="wishlist-header">
                            <p-button 
                                [label]="'wishlist.clearAll' | translate" 
                                icon="pi pi-trash"
                                [text]="true"
                                severity="danger"
                                (click)="clearWishlist()">
                            </p-button>
                        </div>

                        <div class="wishlist-grid">
                            <div *ngFor="let item of wishlist.items; let i = index" 
                                 class="wishlist-card" 
                                 [style.animation-delay]="(i * 0.05) + 's'">
                                <a [routerLink]="['/item', item.item.id]" class="card-link">
                                    <div class="card-image">
                                        <img 
                                            [src]="getItemImage(item)" 
                                            [alt]="getItemName(item)"
                                            onerror="this.src='assets/img/defult.png'"
                                        />
                                        <div class="image-overlay">
                                            <p-button 
                                                [label]="languageService.language === 'ar' ? 'عرض التفاصيل' : 'View Details'"
                                                [outlined]="true"
                                                styleClass="overlay-btn">
                                            </p-button>
                                        </div>
                                    </div>
                                    <div class="card-content">
                                        <h3>{{ getItemName(item) }}</h3>
                                        <div class="card-rating" *ngIf="item.item.averageRating > 0">
                                            <i class="pi pi-star-fill"></i>
                                            <span>{{ item.item.averageRating | number:'1.1-1' }}</span>
                                            <span class="reviews">({{ item.item.reviewsCount || 0 }})</span>
                                        </div>
                                        <div class="card-price">{{ formatCurrency(item.item.price) }}</div>
                                    </div>
                                </a>
                                <div class="card-actions">
                                    <p-button 
                                        [label]="'wishlist.addToCart' | translate" 
                                        icon="pi pi-shopping-cart"
                                        [loading]="addingToCart === item.item.id"
                                        (click)="addToCart(item)"
                                        styleClass="cart-btn">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-trash"
                                        [rounded]="true"
                                        severity="danger"
                                        [outlined]="true"
                                        [pTooltip]="'wishlist.remove' | translate"
                                        [loading]="removing === item.item.id"
                                        (click)="removeItem(item)">
                                    </p-button>
                                </div>
                                <div class="card-date">
                                    <i class="pi pi-calendar"></i>
                                    {{ formatDate(item.addedAt) }}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
        <p-toast></p-toast>
        <p-confirmdialog></p-confirmdialog>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .wishlist-page {
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
            margin-bottom: 0.5rem;
        }

        .hero-description {
            font-size: 1.1rem;
            color: rgba(255,255,255,0.8);
        }

        /* ============ CONTENT SECTION ============ */
        .content-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
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

        /* ============ EMPTY STATE ============ */
        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.1);
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

        /* ============ WISHLIST HEADER ============ */
        .wishlist-header {
            display: flex;
            justify-content: flex-end;
            margin-bottom: 2rem;
        }

        /* ============ WISHLIST GRID ============ */
        .wishlist-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 2rem;
        }

        .wishlist-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            animation: fadeIn 0.5s ease-out backwards;
            transition: all 0.3s ease;
        }

        .wishlist-card:hover {
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

        .card-link {
            text-decoration: none;
            color: inherit;
            display: block;
        }

        .card-image {
            position: relative;
            height: 250px;
            overflow: hidden;
            background: #f8f9fa;
        }

        .card-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.5s ease;
        }

        .wishlist-card:hover .card-image img {
            transform: scale(1.1);
        }

        .image-overlay {
            position: absolute;
            inset: 0;
            background: rgba(26, 26, 46, 0.7);
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: 0;
            transition: opacity 0.3s ease;
        }

        .wishlist-card:hover .image-overlay {
            opacity: 1;
        }

        :host ::ng-deep .overlay-btn {
            color: white !important;
            border-color: white !important;
            border-radius: 50px !important;
        }

        .card-content {
            padding: 1.5rem;
        }

        .card-content h3 {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
            line-height: 1.4;
        }

        .card-rating {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 0.75rem;
            color: #FFB800;
            font-size: 0.875rem;
        }

        .card-rating .reviews {
            color: #6c757d;
        }

        .card-price {
            font-size: 1.5rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .card-actions {
            display: flex;
            gap: 0.75rem;
            padding: 0 1.5rem 1.5rem;
        }

        :host ::ng-deep .cart-btn {
            flex: 1;
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
        }

        .card-date {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 1.5rem;
            border-top: 1px solid #e9ecef;
            font-size: 0.75rem;
            color: #6c757d;
        }

        .card-date i {
            color: var(--color-primary);
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .wishlist-grid {
                grid-template-columns: 1fr;
            }
            .card-image {
                height: 200px;
            }
        }
    `]
})
export class WishlistComponent implements OnInit {
    wishlist: Wishlist | null = null;
    loading = false;
    addingToCart: string | null = null;
    removing: string | null = null;

    private wishlistApiService = inject(WishlistApiService);
    private cartApiService = inject(CartApiService);
    private authService = inject(AuthService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.loadWishlist();
    }

    loadWishlist() {
        this.loading = true;
        this.wishlistApiService.getWishlist().subscribe({
            next: (wishlist) => {
                this.wishlist = wishlist;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    addToCart(item: WishlistItem) {
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
            return;
        }
        this.addingToCart = item.item.id;
        this.cartApiService.addToCart({
            itemId: item.item.id,
            quantity: 1
        }).subscribe({
            next: () => {
                this.addingToCart = null;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('wishlist.addedToCart')
                });
            },
            error: () => {
                this.addingToCart = null;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    removeItem(item: WishlistItem) {
        this.confirmationService.confirm({
            message: this.translateService.instant('wishlist.confirmRemove'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.removing = item.item.id;
                this.wishlistApiService.removeFromWishlist(item.item.id).subscribe({
                    next: () => {
                        this.removing = null;
                        this.loadWishlist();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('wishlist.itemRemoved')
                        });
                    },
                    error: () => {
                        this.removing = null;
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    clearWishlist() {
        this.confirmationService.confirm({
            message: this.translateService.instant('wishlist.confirmClear'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.wishlistApiService.clearWishlist().subscribe({
                    next: () => {
                        this.wishlist = null;
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('wishlist.cleared')
                        });
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    getItemName(item: WishlistItem): string {
        return this.languageService.getLocalizedName(item.item);
    }

    getItemImage(item: WishlistItem): string {
        if (item.item.mediaAssets && item.item.mediaAssets.length > 0) {
            const primaryImage = item.item.mediaAssets.find(m => m.isPrimary);
            if (primaryImage) {
                return primaryImage.fileUrl;
            }
            return item.item.mediaAssets[0].fileUrl;
        }
        return 'assets/img/defult.png';
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }
}
