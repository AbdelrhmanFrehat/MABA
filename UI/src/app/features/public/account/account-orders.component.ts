import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrdersApiService } from '../../../shared/services/orders-api.service';
import { CartApiService } from '../../../shared/services/cart-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import { Router } from '@angular/router';
import { Order, OrderListResponse, OrderStatus } from '../../../shared/models/order.model';
import { LanguageService } from '../../../shared/services/language.service';
import { MabaInvoicePdfService } from '../../../shared/services/maba-invoice-pdf.service';
import { mapOrderToMabaInvoice } from '../../../shared/utils/map-order-to-maba-invoice';

@Component({
    selector: 'app-account-orders',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        SelectModule,
        TagModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="orders-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-shopping-bag"></i>
                        {{ languageService.language === 'ar' ? 'طلباتي' : 'My Orders' }}
                    </span>
                    <h1 class="hero-title">{{ 'account.orders.title' | translate }}</h1>
                </div>
            </section>

            <!-- Main Section -->
            <section class="main-section">
                <div class="container">
                    <!-- Filters -->
                    <div class="filters-card">
                        <div class="filter-group">
                            <label>{{ 'account.orders.filterByStatus' | translate }}</label>
                            <p-select 
                                [options]="statusOptions"
                                [(ngModel)]="selectedStatusId"
                                optionLabel="name"
                                optionValue="id"
                                [placeholder]="'account.orders.allStatuses' | translate"
                                styleClass="filter-select"
                                (onChange)="loadOrders()"
                            ></p-select>
                        </div>
                    </div>

                    <!-- Loading -->
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                        </div>
                    </div>

                    <!-- Empty State -->
                    <div *ngIf="!loading && orders.length === 0" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-shopping-bag"></i>
                        </div>
                        <h2>{{ 'account.orders.noOrders' | translate }}</h2>
                        <p>{{ 'account.orders.noOrdersDescription' | translate }}</p>
                        <button class="cta-button" routerLink="/catalog">
                            <i class="pi pi-shopping-cart"></i>
                            {{ 'account.orders.startShopping' | translate }}
                            <i class="pi pi-arrow-right"></i>
                        </button>
                    </div>

                    <!-- Orders List -->
                    <div *ngIf="!loading && orders.length > 0" class="orders-list">
                        <div *ngFor="let order of orders; let i = index" class="order-card" [style.animation-delay]="(i * 0.1) + 's'">
                            <div class="order-header">
                                <div class="order-info">
                                    <h3>{{ 'account.orders.orderNumber' | translate }}: {{ order.orderNumber }}</h3>
                                    <span class="order-date">
                                        <i class="pi pi-calendar"></i>
                                        {{ formatDate(order.createdAt) }}
                                    </span>
                                </div>
                                <div class="order-status" [class]="'status-' + order.status.key.toLowerCase()">
                                    {{ getStatusName(order.status) }}
                                </div>
                            </div>

                            <div class="order-items">
                                <div *ngFor="let item of order.items.slice(0, 3)" class="order-item">
                                    <img 
                                        [src]="item.mediaAssetUrl || 'assets/img/defult.png'" 
                                        [alt]="getItemName(item)"
                                        onerror="this.src='assets/img/defult.png'"
                                    />
                                    <div class="item-info">
                                        <span class="item-name">{{ getItemName(item) }}</span>
                                        <span class="item-qty">{{ 'account.orders.quantity' | translate }}: {{ item.quantity }}</span>
                                    </div>
                                    <span class="item-price">{{ formatCurrency(item.total) }}</span>
                                </div>
                                <div *ngIf="order.items.length > 3" class="more-items">
                                    + {{ order.items.length - 3 }} {{ 'account.orders.moreItems' | translate }}
                                </div>
                            </div>

                            <div class="order-footer">
                                <div class="order-total">
                                    <span class="total-label">{{ 'account.orders.total' | translate }}:</span>
                                    <span class="total-value">{{ formatCurrency(order.totalAmount) }}</span>
                                </div>
                                <div class="order-actions">
                                    <button class="action-btn view-btn" [routerLink]="['/account/orders', order.id]">
                                        <i class="pi pi-eye"></i>
                                        {{ 'account.orders.viewDetails' | translate }}
                                    </button>
                                    <button class="action-btn" (click)="downloadInvoice(order.id)">
                                        <i class="pi pi-download"></i>
                                    </button>
                                    <button *ngIf="canReorder(order)" class="action-btn" (click)="reorder(order)">
                                        <i class="pi pi-refresh"></i>
                                    </button>
                                </div>
                            </div>
                        </div>

                        <!-- Pagination -->
                        <div *ngIf="totalRecords > pageSize" class="pagination">
                            <button 
                                class="page-btn"
                                [disabled]="currentPage === 1"
                                (click)="previousPage()">
                                <i class="pi pi-chevron-left"></i>
                                {{ 'common.previous' | translate }}
                            </button>
                            <span class="page-info">
                                {{ ((currentPage - 1) * pageSize) + 1 }} - {{ getLastRecordNumber() }} 
                                {{ languageService.language === 'ar' ? 'من' : 'of' }} {{ totalRecords }}
                            </span>
                            <button 
                                class="page-btn"
                                [disabled]="currentPage * pageSize >= totalRecords"
                                (click)="nextPage()">
                                {{ 'common.next' | translate }}
                                <i class="pi pi-chevron-right"></i>
                            </button>
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

        .orders-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 1000px;
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
        }

        /* ============ MAIN SECTION ============ */
        .main-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        /* ============ FILTERS ============ */
        .filters-card {
            background: white;
            border-radius: 16px;
            padding: 1.5rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            margin-bottom: 2rem;
        }

        .filter-group {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .filter-group label {
            font-weight: 600;
            color: #1a1a2e;
        }

        :host ::ng-deep .filter-select {
            min-width: 200px;
        }

        /* ============ LOADING ============ */
        .loading-container {
            text-align: center;
            padding: 4rem 2rem;
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
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
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

        .cta-button {
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 50px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .cta-button:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        /* ============ ORDERS LIST ============ */
        .orders-list {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .order-card {
            background: white;
            border-radius: 20px;
            padding: 1.5rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            animation: fadeIn 0.5s ease-out backwards;
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

        .order-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 1.5rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .order-info h3 {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.25rem;
        }

        .order-date {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #6c757d;
            font-size: 0.9rem;
        }

        .order-status {
            padding: 0.5rem 1rem;
            border-radius: 50px;
            font-weight: 600;
            font-size: 0.85rem;
        }

        .status-pending { background: rgba(234, 179, 8, 0.1); color: #ca8a04; }
        .status-processing { background: rgba(59, 130, 246, 0.1); color: #2563eb; }
        .status-shipped { background: rgba(99, 102, 241, 0.1); color: #4f46e5; }
        .status-delivered { background: rgba(102, 126, 234, 0.1); color: #667eea; }
        .status-completed { background: rgba(102, 126, 234, 0.1); color: #667eea; }
        .status-cancelled { background: rgba(239, 68, 68, 0.1); color: #dc2626; }

        .order-items {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            margin-bottom: 1.5rem;
        }

        .order-item {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .order-item img {
            width: 60px;
            height: 60px;
            border-radius: 10px;
            object-fit: cover;
        }

        .item-info {
            flex: 1;
            display: flex;
            flex-direction: column;
        }

        .item-name {
            font-weight: 600;
            color: #1a1a2e;
        }

        .item-qty {
            font-size: 0.85rem;
            color: #6c757d;
        }

        .item-price {
            font-weight: 700;
            color: var(--color-primary);
        }

        .more-items {
            color: #6c757d;
            font-size: 0.9rem;
            padding-left: 76px;
        }

        .order-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-top: 1rem;
            border-top: 1px solid #e9ecef;
        }

        .order-total {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .total-label {
            color: #6c757d;
        }

        .total-value {
            font-size: 1.25rem;
            font-weight: 800;
            color: #1a1a2e;
        }

        .order-actions {
            display: flex;
            gap: 0.75rem;
        }

        .action-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.625rem 1rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 10px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .action-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .action-btn.view-btn {
            background: var(--gradient-primary);
            border-color: transparent;
            color: white;
        }

        .action-btn.view-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
        }

        /* ============ PAGINATION ============ */
        .pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 1rem;
            margin-top: 2rem;
        }

        .page-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 10px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .page-btn:hover:not(:disabled) {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .page-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .page-info {
            color: #6c757d;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .order-header {
                flex-direction: column;
                gap: 1rem;
            }
            .order-footer {
                flex-direction: column;
                gap: 1rem;
            }
            .order-actions {
                width: 100%;
                flex-wrap: wrap;
            }
            .action-btn.view-btn {
                flex: 1;
            }
        }
    `]
})
export class AccountOrdersComponent implements OnInit {
    orders: Order[] = [];
    loading = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedStatusId: string = '';
    orderStatuses: OrderStatus[] = [];
    statusOptions: any[] = [];
    userId: string | null = null;

    private ordersApiService = inject(OrdersApiService);
    private cartApiService = inject(CartApiService);
    private authService = inject(AuthService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);
    private mabaInvoicePdf = inject(MabaInvoicePdfService);

    ngOnInit() {
        this.loadUser();
        this.loadOrderStatuses();
    }

    loadUser() {
        this.authService.getCurrentUser().subscribe({
            next: (user) => {
                this.userId = user.id;
                this.loadOrders();
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.sessionExpired')
                });
            }
        });
    }

    loadOrderStatuses() {
        this.ordersApiService.getOrderStatuses().subscribe({
            next: (statuses) => {
                this.orderStatuses = statuses;
                const lang = this.languageService.language;
                this.statusOptions = [
                    { id: '', name: this.translateService.instant('account.orders.allStatuses') },
                    ...statuses.map((s: OrderStatus) => ({
                        id: s.id,
                        name: lang === 'ar' ? s.nameAr : s.nameEn
                    }))
                ];
            }
        });
    }

    loadOrders() {
        if (!this.userId) return;

        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedStatusId) {
            params.statusId = this.selectedStatusId;
        }

        this.ordersApiService.getUserOrders(this.userId, params).subscribe({
            next: (response) => {
                this.orders = response.items || [];
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.loadOrders();
        }
    }

    nextPage() {
        if (this.currentPage * this.pageSize < this.totalRecords) {
            this.currentPage++;
            this.loadOrders();
        }
    }

    downloadInvoice(orderId: string) {
        this.ordersApiService.getOrderById(orderId).subscribe({
            next: async (order) => {
                try {
                    const lang = this.languageService.language === 'ar' ? 'ar' : 'en';
                    const doc = mapOrderToMabaInvoice(order, lang);
                    const blob = await this.mabaInvoicePdf.generatePdf(doc);
                    const safeNum = (order.orderNumber || orderId).replace(/[^\w.-]/g, '_');
                    this.mabaInvoicePdf.downloadBlob(blob, `MABA-invoice-${safeNum}.pdf`);
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('account.orders.invoiceDownloaded')
                    });
                } catch {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('messages.loadError')
                    });
                }
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    reorder(order: Order) {
        const addToCartPromises = order.items.map(item => 
            this.cartApiService.addToCart({
                itemId: item.itemId,
                quantity: item.quantity
            }).toPromise()
        );

        Promise.all(addToCartPromises).then(() => {
            this.messageService.add({
                severity: 'success',
                summary: this.translateService.instant('messages.success'),
                detail: this.translateService.instant('account.orders.reordered')
            });
            this.router.navigate(['/cart']);
        }).catch(() => {
            this.messageService.add({
                severity: 'error',
                summary: this.translateService.instant('messages.error'),
                detail: this.translateService.instant('messages.saveError')
            });
        });
    }

    canReorder(order: Order): boolean {
        return order.status.key === 'Completed' || order.status.key === 'Delivered';
    }

    getStatusName(status: OrderStatus): string {
        const lang = this.languageService.language;
        return lang === 'ar' ? status.nameAr : status.nameEn;
    }

    getLastRecordNumber(): number {
        return Math.min(this.currentPage * this.pageSize, this.totalRecords);
    }

    getItemName(item: any): string {
        const lang = this.languageService.language;
        return lang === 'ar' ? item.itemNameAr : item.itemNameEn;
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(value);
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString();
    }
}
