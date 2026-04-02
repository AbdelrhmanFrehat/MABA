import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrdersApiService } from '../../../../shared/services/orders-api.service';
import { CartService } from '../../../../shared/services/cart.service';
import { Order } from '../../../../shared/models/order.model';
import { LanguageService } from '../../../../shared/services/language.service';

@Component({
    selector: 'app-order-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        ButtonModule,
        TagModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="order-detail-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge" [class.success-badge]="isSuccess">
                        <i [class]="isSuccess ? 'pi pi-check-circle' : 'pi pi-shopping-bag'"></i>
                        {{ isSuccess 
                            ? (languageService.language === 'ar' ? 'تم بنجاح!' : 'Success!') 
                            : (languageService.language === 'ar' ? 'تفاصيل الطلب' : 'Order Details') }}
                    </span>
                    <h1 class="hero-title">
                        {{ isSuccess 
                            ? (languageService.language === 'ar' ? 'شكراً لطلبك!' : 'Thank You for Your Order!') 
                            : (languageService.language === 'ar' ? 'طلب رقم' : 'Order') + ' #' + (order?.orderNumber || '') }}
                    </h1>
                    <p class="hero-subtitle" *ngIf="isSuccess">
                        {{ languageService.language === 'ar' 
                            ? 'تم استلام طلبك وسنقوم بمعالجته في أقرب وقت' 
                            : 'Your order has been received and will be processed shortly' }}
                    </p>
                </div>
            </section>

            <!-- Main Section -->
            <section class="main-section">
                <div class="container">
                    <!-- Back Button -->
                    <div class="back-nav">
                        <a routerLink="/account/orders" class="back-link">
                            <i [class]="languageService.direction === 'rtl' ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"></i>
                            {{ languageService.language === 'ar' ? 'العودة للطلبات' : 'Back to Orders' }}
                        </a>
                    </div>

                    <!-- Loading -->
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                        </div>
                    </div>

                    <!-- Order Content -->
                    <div *ngIf="!loading && order" class="order-content">
                        <div class="order-grid">
                            <!-- Left Column -->
                            <div class="order-main">
                                <!-- Order Status -->
                                <div class="status-card">
                                    <div class="status-header">
                                        <h2>
                                            <i class="pi pi-info-circle"></i>
                                            {{ languageService.language === 'ar' ? 'حالة الطلب' : 'Order Status' }}
                                        </h2>
                                        <p-tag 
                                            [value]="getStatusName(order.status)" 
                                            [severity]="getStatusSeverity(order.status.key || '')"
                                            [style]="{ fontSize: '1rem', padding: '0.5rem 1rem' }"
                                        ></p-tag>
                                    </div>
                                    <div class="status-info">
                                        <div class="info-item">
                                            <span class="label">{{ languageService.language === 'ar' ? 'رقم الطلب' : 'Order Number' }}</span>
                                            <span class="value">{{ order.orderNumber }}</span>
                                        </div>
                                        <div class="info-item">
                                            <span class="label">{{ languageService.language === 'ar' ? 'تاريخ الطلب' : 'Order Date' }}</span>
                                            <span class="value">{{ formatDate(order.createdAt) }}</span>
                                        </div>
                                        <div class="info-item">
                                            <span class="label">{{ languageService.language === 'ar' ? 'حالة الدفع' : 'Payment Status' }}</span>
                                            <span class="value">{{ order.paymentStatus }}</span>
                                        </div>
                                    </div>
                                </div>

                                <!-- Order Items -->
                                <div class="items-card">
                                    <h2>
                                        <i class="pi pi-box"></i>
                                        {{ languageService.language === 'ar' ? 'المنتجات' : 'Order Items' }}
                                        <span class="items-count">({{ order.items.length || 0 }})</span>
                                    </h2>
                                    <div class="items-list">
                                        <div *ngFor="let item of order.items" class="item-row">
                                            <div class="item-image">
                                                <img 
                                                    [src]="item.mediaAssetUrl || 'assets/img/defult.png'" 
                                                    [alt]="getItemName(item)"
                                                    (error)="onImageError($event)"
                                                />
                                            </div>
                                            <div class="item-details">
                                                <h4>{{ getItemName(item) }}</h4>
                                                <p class="sku">{{ languageService.language === 'ar' ? 'الكود' : 'SKU' }}: {{ item.itemSku }}</p>
                                                <p class="quantity">{{ languageService.language === 'ar' ? 'الكمية' : 'Qty' }}: {{ item.quantity }}</p>
                                            </div>
                                            <div class="item-price">
                                                <span class="total">{{ formatCurrency(item.total) }}</span>
                                                <span class="unit-price">{{ formatCurrency(item.unitPrice) }} {{ languageService.language === 'ar' ? 'للوحدة' : 'each' }}</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Addresses -->
                                <div class="addresses-grid">
                                    <div class="address-card" *ngIf="order.shippingAddress">
                                        <h3>
                                            <i class="pi pi-truck"></i>
                                            {{ languageService.language === 'ar' ? 'عنوان الشحن' : 'Shipping Address' }}
                                        </h3>
                                        <div class="address-content">
                                            <p class="name">{{ order.shippingAddress.fullName }}</p>
                                            <p>{{ order.shippingAddress.addressLine1 }}</p>
                                            <p *ngIf="order.shippingAddress.addressLine2">{{ order.shippingAddress.addressLine2 }}</p>
                                            <p>{{ order.shippingAddress.city }}, {{ order.shippingAddress.state }} {{ order.shippingAddress.postalCode }}</p>
                                            <p>{{ order.shippingAddress.country }}</p>
                                            <p class="phone"><i class="pi pi-phone"></i> {{ order.shippingAddress.phone }}</p>
                                        </div>
                                    </div>
                                    <div class="address-card" *ngIf="order.billingAddress">
                                        <h3>
                                            <i class="pi pi-credit-card"></i>
                                            {{ languageService.language === 'ar' ? 'عنوان الفاتورة' : 'Billing Address' }}
                                        </h3>
                                        <div class="address-content">
                                            <p class="name">{{ order.billingAddress.fullName }}</p>
                                            <p>{{ order.billingAddress.addressLine1 }}</p>
                                            <p *ngIf="order.billingAddress.addressLine2">{{ order.billingAddress.addressLine2 }}</p>
                                            <p>{{ order.billingAddress.city }}, {{ order.billingAddress.state }} {{ order.billingAddress.postalCode }}</p>
                                            <p>{{ order.billingAddress.country }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Right Column - Summary -->
                            <div class="order-sidebar">
                                <div class="summary-card">
                                    <h2>
                                        <i class="pi pi-calculator"></i>
                                        {{ languageService.language === 'ar' ? 'ملخص الطلب' : 'Order Summary' }}
                                    </h2>
                                    <div class="summary-rows">
                                        <div class="summary-row">
                                            <span>{{ languageService.language === 'ar' ? 'المجموع الفرعي' : 'Subtotal' }}</span>
                                            <span>{{ formatCurrency(order.subtotal) }}</span>
                                        </div>
                                        <div class="summary-row" *ngIf="order.discountAmount > 0">
                                            <span>{{ languageService.language === 'ar' ? 'الخصم' : 'Discount' }}</span>
                                            <span class="discount">-{{ formatCurrency(order.discountAmount) }}</span>
                                        </div>
                                        <div class="summary-row">
                                            <span>{{ languageService.language === 'ar' ? 'الضريبة' : 'Tax' }}</span>
                                            <span>{{ formatCurrency(order.taxAmount) }}</span>
                                        </div>
                                        <div class="summary-row">
                                            <span>{{ languageService.language === 'ar' ? 'الشحن' : 'Shipping' }}</span>
                                            <span>{{ formatCurrency(order.shippingAmount) }}</span>
                                        </div>
                                        <div class="summary-total">
                                            <span>{{ languageService.language === 'ar' ? 'الإجمالي' : 'Total' }}</span>
                                            <span class="total-amount">{{ formatCurrency(order.totalAmount) }}</span>
                                        </div>
                                    </div>
                                </div>

                                <!-- Actions -->
                                <div class="actions-card">
                                    <h3>{{ languageService.language === 'ar' ? 'إجراءات' : 'Actions' }}</h3>
                                    <button class="action-btn primary" (click)="downloadInvoice()">
                                        <i class="pi pi-download"></i>
                                        {{ languageService.language === 'ar' ? 'تحميل الفاتورة' : 'Download Invoice' }}
                                    </button>
                                    <button *ngIf="canReorder(order)" class="action-btn secondary" (click)="reorder()">
                                        <i class="pi pi-refresh"></i>
                                        {{ languageService.language === 'ar' ? 'إعادة الطلب' : 'Reorder' }}
                                    </button>
                                    <a routerLink="/catalog" class="action-btn outline">
                                        <i class="pi pi-shopping-cart"></i>
                                        {{ languageService.language === 'ar' ? 'متابعة التسوق' : 'Continue Shopping' }}
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Error State -->
                    <div *ngIf="!loading && !order" class="error-state">
                        <div class="error-icon">
                            <i class="pi pi-exclamation-triangle"></i>
                        </div>
                        <h2>{{ languageService.language === 'ar' ? 'لم يتم العثور على الطلب' : 'Order Not Found' }}</h2>
                        <p>{{ languageService.language === 'ar' ? 'الطلب المطلوب غير موجود أو تم حذفه' : 'The requested order could not be found' }}</p>
                        <a routerLink="/account/orders" class="back-btn">
                            <i class="pi pi-arrow-left"></i>
                            {{ languageService.language === 'ar' ? 'العودة للطلبات' : 'Back to Orders' }}
                        </a>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            /* MABA purple accents — do not rely on global --primary-* aliases (avoids teal/green mismatches) */
            --primary-color: #667eea;
            --primary-700: #764ba2;
            --primary-color-rgb: 102, 126, 234;
        }

        .order-detail-page {
            min-height: 100vh;
            background: var(--surface-ground);
        }

        :host ::ng-deep .p-tag.p-tag-success,
        :host ::ng-deep .p-tag[data-pc-severity="success"],
        :host ::ng-deep [data-p-severity="success"].p-tag {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            color: #fff !important;
            border: none !important;
        }

        /* Hero Section */
        .hero-section {
            position: relative;
            padding: 4rem 2rem 3rem;
            overflow: hidden;
            background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-700) 100%);
        }

        .hero-bg-gradient {
            position: absolute;
            inset: 0;
            background: radial-gradient(circle at 30% 70%, rgba(255,255,255,0.15) 0%, transparent 50%),
                        radial-gradient(circle at 70% 30%, rgba(255,255,255,0.1) 0%, transparent 50%);
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
            opacity: 0.5;
        }

        .hero-content {
            position: relative;
            max-width: 1200px;
            margin: 0 auto;
            text-align: center;
            z-index: 1;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1.25rem;
            background: rgba(255,255,255,0.2);
            border-radius: 50px;
            color: white;
            font-size: 0.875rem;
            font-weight: 600;
            margin-bottom: 1rem;
            backdrop-filter: blur(10px);
        }

        .hero-badge.success-badge {
            background: rgba(102, 126, 234, 0.25);
            border: 1px solid rgba(118, 75, 162, 0.45);
        }

        .hero-title {
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 800;
            color: white;
            margin: 0 0 0.5rem;
            text-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .hero-subtitle {
            font-size: 1.125rem;
            color: rgba(255,255,255,0.9);
            margin: 0;
        }

        /* Main Section */
        .main-section {
            padding: 2rem;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        /* Back Navigation */
        .back-nav {
            margin-bottom: 1.5rem;
        }

        .back-link {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            color: var(--text-color-secondary);
            text-decoration: none;
            font-weight: 500;
            transition: color 0.2s;
        }

        .back-link:hover {
            color: var(--primary-color);
        }

        /* Loading */
        .loading-container {
            display: flex;
            justify-content: center;
            padding: 4rem;
        }

        .loading-spinner {
            text-align: center;
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--primary-color);
        }

        .loading-spinner p {
            margin-top: 1rem;
            color: var(--text-color-secondary);
        }

        /* Order Grid */
        .order-grid {
            display: grid;
            grid-template-columns: 1fr 380px;
            gap: 2rem;
        }

        @media (max-width: 992px) {
            .order-grid {
                grid-template-columns: 1fr;
            }
        }

        /* Cards Base */
        .status-card, .items-card, .address-card, .summary-card, .actions-card {
            background: var(--surface-card);
            border-radius: 16px;
            padding: 1.5rem;
            margin-bottom: 1.5rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.05);
            border: 1px solid var(--surface-border);
        }

        .status-card h2, .items-card h2, .summary-card h2 {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            font-size: 1.25rem;
            font-weight: 700;
            color: var(--text-color);
            margin: 0 0 1.25rem;
        }

        .status-card h2 i, .items-card h2 i, .summary-card h2 i {
            color: var(--primary-color);
        }

        /* Status Card */
        .status-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .status-info {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
            margin-top: 1rem;
            padding-top: 1rem;
            border-top: 1px solid var(--surface-border);
        }

        .info-item {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }

        .info-item .label {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }

        .info-item .value {
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-color);
        }

        /* Items Card */
        .items-count {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            font-weight: 400;
        }

        .items-list {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .item-row {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            background: var(--surface-ground);
            border-radius: 12px;
            transition: transform 0.2s;
        }

        .item-row:hover {
            transform: translateX(4px);
        }

        [dir="rtl"] .item-row:hover {
            transform: translateX(-4px);
        }

        .item-image {
            width: 80px;
            height: 80px;
            border-radius: 10px;
            overflow: hidden;
            flex-shrink: 0;
        }

        .item-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .item-details {
            flex: 1;
            min-width: 0;
        }

        .item-details h4 {
            margin: 0 0 0.25rem;
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .item-details .sku, .item-details .quantity {
            margin: 0;
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }

        .item-price {
            text-align: end;
            flex-shrink: 0;
        }

        .item-price .total {
            display: block;
            font-size: 1.125rem;
            font-weight: 700;
            color: var(--primary-color);
        }

        .item-price .unit-price {
            font-size: 0.75rem;
            color: var(--text-color-secondary);
        }

        /* Addresses */
        .addresses-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 1.5rem;
        }

        .address-card h3 {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-color);
            margin: 0 0 1rem;
        }

        .address-card h3 i {
            color: var(--primary-color);
        }

        .address-content p {
            margin: 0 0 0.375rem;
            font-size: 0.9375rem;
            color: var(--text-color-secondary);
            line-height: 1.5;
        }

        .address-content .name {
            font-weight: 600;
            color: var(--text-color);
        }

        .address-content .phone {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-top: 0.75rem;
            color: var(--primary-color);
        }

        /* Summary Card */
        .summary-rows {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .summary-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 0.9375rem;
            color: var(--text-color-secondary);
        }

        .summary-row .discount {
            color: #667eea;
            font-weight: 500;
        }

        .summary-total {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-top: 1rem;
            margin-top: 0.5rem;
            border-top: 2px solid var(--surface-border);
        }

        .summary-total span:first-child {
            font-size: 1.125rem;
            font-weight: 700;
            color: var(--text-color);
        }

        .total-amount {
            font-size: 1.5rem;
            font-weight: 800;
            color: var(--primary-color);
        }

        /* Actions Card */
        .actions-card h3 {
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-color);
            margin: 0 0 1rem;
        }

        .action-btn {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            width: 100%;
            padding: 0.875rem 1.5rem;
            border-radius: 10px;
            font-size: 0.9375rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            text-decoration: none;
            margin-bottom: 0.75rem;
            border: none;
        }

        .action-btn.primary {
            background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-700) 100%);
            color: white;
        }

        .action-btn.primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(var(--primary-color-rgb), 0.3);
        }

        .action-btn.secondary {
            background: var(--surface-ground);
            color: var(--text-color);
            border: 1px solid var(--surface-border);
        }

        .action-btn.secondary:hover {
            background: var(--surface-hover);
        }

        .action-btn.outline {
            background: transparent;
            color: var(--primary-color);
            border: 2px solid var(--primary-color);
        }

        .action-btn.outline:hover {
            background: var(--primary-color);
            color: white;
        }

        /* Error State */
        .error-state {
            text-align: center;
            padding: 4rem 2rem;
        }

        .error-icon {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: linear-gradient(135deg, #fecaca 0%, #fee2e2 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .error-icon i {
            font-size: 2.5rem;
            color: #dc2626;
        }

        .error-state h2 {
            font-size: 1.5rem;
            color: var(--text-color);
            margin: 0 0 0.5rem;
        }

        .error-state p {
            color: var(--text-color-secondary);
            margin: 0 0 1.5rem;
        }

        .back-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: var(--primary-color);
            color: white;
            border-radius: 10px;
            text-decoration: none;
            font-weight: 600;
            transition: all 0.2s;
        }

        .back-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(var(--primary-color-rgb), 0.3);
        }

        /* Responsive */
        @media (max-width: 768px) {
            .hero-section {
                padding: 3rem 1rem 2rem;
            }

            .main-section {
                padding: 1rem;
            }

            .item-row {
                flex-direction: column;
                text-align: center;
            }

            .item-image {
                width: 100px;
                height: 100px;
            }

            .item-price {
                text-align: center;
            }

            .addresses-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class OrderDetailComponent implements OnInit {
    order: Order | null = null;
    loading = false;
    isSuccess = false;

    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private ordersApiService = inject(OrdersApiService);
    private cartService = inject(CartService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        // Check for success query param
        this.isSuccess = this.route.snapshot.queryParamMap.get('success') === 'true';
        
        const orderId = this.route.snapshot.paramMap.get('id');
        if (orderId) {
            this.loadOrder(orderId);
        }

        // Show success message if redirected from checkout
        if (this.isSuccess) {
            setTimeout(() => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.languageService.language === 'ar' ? 'تم بنجاح!' : 'Success!',
                    detail: this.languageService.language === 'ar' 
                        ? 'تم تقديم طلبك بنجاح. سنقوم بإرسال تأكيد على بريدك الإلكتروني.'
                        : 'Your order has been placed successfully. We will send a confirmation to your email.',
                    life: 5000
                });
            }, 500);
        }
    }

    loadOrder(id: string) {
        this.loading = true;
        this.ordersApiService.getOrderById(id).subscribe({
            next: (order) => {
                this.order = order;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onImageError(event: Event) {
        const img = event.target as HTMLImageElement;
        img.src = 'assets/img/defult.png';
    }

    downloadInvoice() {
        if (!this.order) return;
        this.ordersApiService.downloadInvoice(this.order.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `invoice-${this.order!.orderNumber}.pdf`;
                link.click();
                window.URL.revokeObjectURL(url);
                this.messageService.add({
                    severity: 'success',
                    summary: this.languageService.language === 'ar' ? 'تم!' : 'Done!',
                    detail: this.languageService.language === 'ar' ? 'تم تحميل الفاتورة' : 'Invoice downloaded'
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.languageService.language === 'ar' ? 'خطأ' : 'Error',
                    detail: this.languageService.language === 'ar' ? 'فشل تحميل الفاتورة' : 'Failed to download invoice'
                });
            }
        });
    }

    reorder() {
        if (!this.order) return;
        
        let itemsAdded = 0;
        const totalItems = this.order.items.length;

        this.order.items.forEach(item => {
            const itemDetails = {
                nameEn: item.itemNameEn,
                nameAr: item.itemNameAr,
                sku: item.itemSku,
                price: item.unitPrice,
                imageUrl: item.mediaAssetUrl
            };

            this.cartService.addToCart({
                itemId: item.itemId,
                quantity: item.quantity
            }, itemDetails).subscribe({
                next: () => {
                    itemsAdded++;
                    if (itemsAdded === totalItems) {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.languageService.language === 'ar' ? 'تم!' : 'Done!',
                            detail: this.languageService.language === 'ar' 
                                ? 'تمت إضافة المنتجات للسلة'
                                : 'Items added to cart'
                        });
                        this.router.navigate(['/cart']);
                    }
                }
            });
        });
    }

    canReorder(order: Order): boolean {
        return order.status?.key === 'Completed' || order.status?.key === 'Delivered';
    }

    getStatusName(status: any): string {
        if (!status) return '';
        return this.languageService.language === 'ar' ? status.nameAr : status.nameEn;
    }

    getStatusSeverity(statusKey: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | null | undefined {
        const severityMap: Record<string, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            'Pending': 'warn',
            'Processing': 'info',
            'Shipped': 'info',
            'Delivered': 'success',
            'Cancelled': 'danger',
            'Completed': 'success'
        };
        return severityMap[statusKey] || 'secondary';
    }

    getItemName(item: any): string {
        return this.languageService.language === 'ar' ? item.itemNameAr : item.itemNameEn;
    }

    formatCurrency(value: number): string {
        if (value === undefined || value === null) return '0.00 ₪';
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    formatDate(date: string): string {
        if (!date) return '';
        return new Date(date).toLocaleDateString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}
