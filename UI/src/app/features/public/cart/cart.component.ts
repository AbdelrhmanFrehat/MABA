import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CartService } from '../../../shared/services/cart.service';
import { Cart, CartItem } from '../../../shared/models/cart.model';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-cart',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        InputNumberModule,
        InputTextModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <div class="cart-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-shopping-cart"></i>
                        {{ languageService.language === 'ar' ? 'سلة التسوق' : 'Shopping Cart' }}
                    </span>
                    <h1 class="hero-title">{{ 'cart.title' | translate }}</h1>
                    <p class="hero-description" *ngIf="cart && cart.items.length > 0">
                        {{ cart.items.length }} {{ languageService.language === 'ar' ? 'منتج في السلة' : 'items in your cart' }}
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

                    <!-- Empty Cart -->
                    <div *ngIf="!loading && (!cart || cart.items.length === 0)" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-shopping-cart"></i>
                        </div>
                        <h2>{{ 'cart.empty' | translate }}</h2>
                        <p>{{ 'cart.emptyDescription' | translate }}</p>
                        <p-button 
                            [label]="'cart.continueShopping' | translate" 
                            icon="pi pi-arrow-left"
                            [routerLink]="['/catalog']"
                            styleClass="cta-button">
                        </p-button>
                    </div>

                    <!-- Cart with Items -->
                    <div *ngIf="!loading && cart && cart.items.length > 0" class="cart-grid">
                        <div class="cart-items-section">
                            <div class="section-card">
                                <div class="card-header">
                                    <h2>{{ 'cart.items' | translate }} ({{ cart.items.length }})</h2>
                                    <p-button 
                                        [label]="'cart.clearCart' | translate" 
                                        icon="pi pi-trash"
                                        [text]="true"
                                        severity="danger"
                                        (click)="clearCart()">
                                    </p-button>
                                </div>

                                <div class="cart-items">
                                    <div *ngFor="let item of cart.items; let i = index" 
                                         class="cart-item" 
                                         [style.animation-delay]="(i * 0.05) + 's'">
                                        <div class="item-image">
                                            <img 
                                                [src]="item.mediaAssetUrl || 'assets/img/defult.png'" 
                                                [alt]="getItemName(item)"
                                                onerror="this.src='assets/img/defult.png'"
                                            />
                                        </div>
                                        <div class="item-details">
                                            <h3>{{ getItemName(item) }}</h3>
                                            <p class="item-sku">{{ 'cart.sku' | translate }}: {{ item.itemSku }}</p>
                                            <div class="item-price-mobile">{{ formatCurrency(item.unitPrice) }}</div>
                                        </div>
                                        <div class="item-quantity">
                                            <label>{{ 'cart.quantity' | translate }}</label>
                                            <p-inputNumber 
                                                [(ngModel)]="item.quantity"
                                                [min]="1"
                                                [max]="999"
                                                [showButtons]="true"
                                                (onInput)="updateQuantity(item)"
                                                styleClass="quantity-input">
                                            </p-inputNumber>
                                        </div>
                                        <div class="item-price">
                                            <div class="unit-price">{{ formatCurrency(item.unitPrice) }}</div>
                                            <div class="subtotal">{{ formatCurrency(item.subtotal) }}</div>
                                        </div>
                                        <p-button 
                                            icon="pi pi-trash"
                                            [rounded]="true"
                                            [text]="true"
                                            severity="danger"
                                            (click)="removeItem(item)"
                                            styleClass="remove-btn">
                                        </p-button>
                                    </div>
                                </div>
                            </div>

                            <!-- Coupon Section -->
                            <div class="section-card coupon-card">
                                <h3>{{ 'cart.couponCode' | translate }}</h3>
                                <div class="coupon-form">
                                    <input 
                                        type="text" 
                                        pInputText
                                        [(ngModel)]="couponCode"
                                        [placeholder]="'cart.enterCouponCode' | translate"
                                        class="coupon-input"
                                    />
                                    <p-button 
                                        [label]="'cart.applyCoupon' | translate" 
                                        icon="pi pi-check"
                                        (click)="applyCoupon()"
                                        [disabled]="!couponCode || applyingCoupon"
                                        [loading]="applyingCoupon"
                                        styleClass="apply-btn">
                                    </p-button>
                                </div>
                                <div *ngIf="cart.couponCode" class="coupon-applied">
                                    <i class="pi pi-check-circle"></i>
                                    <span>{{ 'cart.couponApplied' | translate }}: <strong>{{ cart.couponCode }}</strong></span>
                                    <p-button 
                                        icon="pi pi-times"
                                        [text]="true"
                                        size="small"
                                        (click)="removeCoupon()">
                                    </p-button>
                                </div>
                            </div>
                        </div>

                        <!-- Order Summary -->
                        <div class="summary-section">
                            <div class="summary-card">
                                <h2>{{ 'cart.orderSummary' | translate }}</h2>
                                
                                <div class="summary-rows">
                                    <div class="summary-row">
                                        <span>{{ 'cart.subtotal' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.subtotal) }}</span>
                                    </div>
                                    
                                    <div *ngIf="cart.discountAmount > 0" class="summary-row discount">
                                        <span>{{ 'cart.discount' | translate }}</span>
                                        <span class="value">-{{ formatCurrency(cart.discountAmount) }}</span>
                                    </div>
                                    
                                    <div class="summary-row">
                                        <span>{{ 'cart.tax' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.taxAmount) }}</span>
                                    </div>
                                    
                                    <div class="summary-row">
                                        <span>{{ 'cart.shipping' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.shippingAmount) }}</span>
                                    </div>
                                </div>
                                
                                <div class="summary-total">
                                    <span>{{ 'cart.total' | translate }}</span>
                                    <span class="total-value">{{ formatCurrency(cart.total) }}</span>
                                </div>
                                
                                <p-button 
                                    [label]="'cart.proceedToCheckout' | translate" 
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    [routerLink]="['/checkout']"
                                    styleClass="checkout-btn w-full">
                                </p-button>
                                
                                <p-button 
                                    [label]="'cart.continueShopping' | translate" 
                                    [outlined]="true"
                                    [routerLink]="['/catalog']"
                                    styleClass="continue-btn w-full">
                                </p-button>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
        <p-toast></p-toast>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-accent: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .cart-page {
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
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%),
                linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px),
                linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px);
            background-size: 100% 100%, 100% 100%, 60px 60px, 60px 60px;
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
            display: flex;
            justify-content: center;
            padding: 4rem;
        }

        .loading-spinner {
            text-align: center;
            color: #6c757d;
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .loading-spinner p {
            margin-top: 1rem;
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
            font-size: 1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
        }

        /* ============ CART GRID ============ */
        .cart-grid {
            display: grid;
            grid-template-columns: 1fr 380px;
            gap: 2rem;
            align-items: start;
        }

        .section-card {
            background: white;
            border-radius: 24px;
            padding: 2rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            margin-bottom: 1.5rem;
        }

        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1.5rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .card-header h2 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0;
        }

        /* ============ CART ITEMS ============ */
        .cart-item {
            display: grid;
            grid-template-columns: 100px 1fr 140px 120px 40px;
            gap: 1.5rem;
            align-items: center;
            padding: 1.5rem 0;
            border-bottom: 1px solid #e9ecef;
            animation: fadeIn 0.4s ease-out backwards;
        }

        .cart-item:last-child {
            border-bottom: none;
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

        .item-image {
            width: 100px;
            height: 100px;
            border-radius: 16px;
            overflow: hidden;
            background: #f8f9fa;
        }

        .item-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .item-details h3 {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .item-sku {
            font-size: 0.875rem;
            color: #6c757d;
            margin: 0;
        }

        .item-price-mobile {
            display: none;
            font-size: 1.1rem;
            font-weight: 700;
            color: var(--color-primary);
            margin-top: 0.5rem;
        }

        .item-quantity label {
            display: block;
            font-size: 0.75rem;
            color: #6c757d;
            margin-bottom: 0.5rem;
        }

        :host ::ng-deep .quantity-input {
            width: 120px;
        }

        :host ::ng-deep .quantity-input input {
            border-radius: 8px !important;
        }

        .item-price {
            text-align: right;
        }

        .unit-price {
            font-size: 0.875rem;
            color: #6c757d;
            margin-bottom: 0.25rem;
        }

        .subtotal {
            font-size: 1.25rem;
            font-weight: 700;
            color: var(--color-primary);
        }

        /* ============ COUPON SECTION ============ */
        .coupon-card h3 {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a1a2e;
            margin-bottom: 1rem;
        }

        .coupon-form {
            display: flex;
            gap: 1rem;
        }

        .coupon-input {
            flex: 1;
            border-radius: 12px !important;
        }

        :host ::ng-deep .apply-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 12px !important;
        }

        .coupon-applied {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-top: 1rem;
            padding: 1rem;
            background: rgba(102, 126, 234, 0.12);
            border-radius: 12px;
            color: #667eea;
        }

        .coupon-applied i {
            font-size: 1.25rem;
        }

        /* ============ SUMMARY SECTION ============ */
        .summary-card {
            background: white;
            border-radius: 24px;
            padding: 2rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            position: sticky;
            top: 100px;
        }

        .summary-card h2 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.5rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .summary-rows {
            margin-bottom: 1.5rem;
        }

        .summary-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0.75rem 0;
            color: #6c757d;
        }

        .summary-row .value {
            font-weight: 600;
            color: #1a1a2e;
        }

        .summary-row.discount .value {
            color: var(--p-primary-color, #667eea);
        }

        .summary-total {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1.5rem 0;
            border-top: 2px solid #e9ecef;
            margin-bottom: 1.5rem;
        }

        .summary-total span:first-child {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a2e;
        }

        .total-value {
            font-size: 1.75rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        :host ::ng-deep .checkout-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            margin-bottom: 0.75rem;
        }

        :host ::ng-deep .continue-btn {
            border-color: var(--color-primary) !important;
            color: var(--color-primary) !important;
            border-radius: 50px !important;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 992px) {
            .cart-grid {
                grid-template-columns: 1fr;
            }
            .summary-card {
                position: static;
            }
        }

        @media (max-width: 768px) {
            .cart-item {
                grid-template-columns: 80px 1fr 40px;
                gap: 1rem;
            }
            .item-quantity,
            .item-price {
                display: none;
            }
            .item-image {
                width: 80px;
                height: 80px;
            }
            .item-price-mobile {
                display: block;
            }
            .coupon-form {
                flex-direction: column;
            }
        }
    `]
})
export class CartComponent implements OnInit {
    cart: Cart | null = null;
    loading = false;
    couponCode = '';
    applyingCoupon = false;

    private cartService = inject(CartService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.loadCart();
    }

    loadCart() {
        this.loading = true;
        this.cartService.getCart().subscribe({
            next: (cart) => {
                this.cart = cart;
                this.loading = false;
            },
            error: (error) => {
                console.error('Error loading cart:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
                this.loading = false;
            }
        });
    }

    updateQuantity(item: CartItem) {
        if (item.quantity < 1) {
            item.quantity = 1;
            return;
        }
        
        this.cartService.updateCartItem(item.id, { quantity: item.quantity }).subscribe({
            next: (cart) => {
                this.cart = cart;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('cart.quantityUpdated')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
                this.loadCart();
            }
        });
    }

    removeItem(item: CartItem) {
        this.cartService.removeCartItem(item.id).subscribe({
            next: (cart) => {
                this.cart = cart;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('cart.itemRemoved')
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

    clearCart() {
        if (confirm(this.translateService.instant('cart.confirmClearCart'))) {
            this.cartService.clearCart().subscribe({
                next: () => {
                    this.cart = null;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('cart.cartCleared')
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
    }

    applyCoupon() {
        // TODO: Implement coupon functionality when backend supports it
        this.messageService.add({
            severity: 'info',
            summary: this.translateService.instant('messages.info'),
            detail: 'Coupon functionality coming soon'
        });
    }

    removeCoupon() {
        // TODO: Implement coupon functionality when backend supports it
    }

    getItemName(item: CartItem): string {
        const lang = this.languageService.language;
        return lang === 'ar' ? item.itemNameAr : item.itemNameEn;
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }
}
