import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrdersApiService } from '../../../../shared/services/orders-api.service';
import { Order } from '../../../../shared/models/order.model';
import { LanguageService } from '../../../../shared/services/language.service';

@Component({
    selector: 'app-order-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        SelectModule,
        TextareaModule,
        ToastModule,
        TagModule,
        TranslateModule,
        ConfirmDialogModule
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />
        <div class="order-detail-container">
            <!-- Header -->
            <div class="order-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.orders.orderDetails' | translate }}</h1>
                        <p class="header-subtitle" *ngIf="order">
                            {{ 'admin.orders.orderNumber' | translate }}: {{ displayOrderNumber }}
                        </p>
                    </div>
                    <p-button 
                        [label]="'common.back' | translate" 
                        icon="pi pi-arrow-left"
                        [outlined]="true"
                        (onClick)="goBack()"
                    ></p-button>
                </div>
            </div>

            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
                <i class="pi pi-spin pi-spinner text-4xl"></i>
                <p>{{ 'common.loading' | translate }}...</p>
            </div>

            <!-- Order Content -->
            <div *ngIf="!loading && order" class="order-content">
                <div class="order-main-grid">
                    <!-- Left Column: Order Info & Items -->
                    <div class="order-main-column">
                        <!-- Order Information Card -->
                        <p-card [styleClass]="'order-info-card'">
                            <ng-template #header>
                                <div class="card-header">
                                    <i class="pi pi-info-circle"></i>
                                    <span>{{ 'admin.orders.orderInformation' | translate }}</span>
                                </div>
                            </ng-template>
                            <div class="order-info-grid">
                                <div class="info-item">
                                    <label>{{ 'admin.orders.orderNumber' | translate }}</label>
                                    <span class="info-value">{{ displayOrderNumber }}</span>
                                </div>
                                <div class="info-item">
                                    <label>{{ 'admin.orders.status' | translate }}</label>
                                    <p-tag 
                                        [value]="displayStatusName" 
                                        [severity]="getStatusSeverity(displayStatusKey)"
                                    ></p-tag>
                                </div>
                                <div class="info-item">
                                    <label>{{ 'admin.orders.customer' | translate }}</label>
                                    <span class="info-value">{{ displayCustomerName }}</span>
                                </div>
                                <div class="info-item">
                                    <label>{{ 'admin.orders.paymentStatus' | translate }}</label>
                                    <p-tag 
                                        [value]="displayPaymentStatus" 
                                        [severity]="getPaymentStatusSeverity(displayPaymentStatus)"
                                    ></p-tag>
                                </div>
                                <div class="info-item">
                                    <label>{{ 'admin.orders.createdDate' | translate }}</label>
                                    <span class="info-value">{{ formatDate(order.createdAt) }}</span>
                                </div>
                                <div class="info-item" *ngIf="order.updatedAt">
                                    <label>{{ 'admin.orders.updatedDate' | translate }}</label>
                                    <span class="info-value">{{ formatDate(order.updatedAt) }}</span>
                                </div>
                            </div>
                        </p-card>

                        <!-- Order Items Card -->
                        <p-card [styleClass]="'order-items-card'">
                            <ng-template #header>
                                <div class="card-header">
                                    <i class="pi pi-shopping-bag"></i>
                                    <span>{{ 'admin.orders.orderItems' | translate }}</span>
                                    <span class="items-count">({{ orderItemsCount }})</span>
                                </div>
                            </ng-template>
                            <div class="order-items-list">
                                <div *ngFor="let item of displayOrderItems" class="order-item">
                                    <div class="item-image-wrapper">
                                        <img 
                                            [src]="item.mediaAssetUrl || 'assets/img/defult.png'" 
                                            [alt]="item.itemNameEn"
                                            class="item-image"
                                            onerror="this.src='assets/img/defult.png'"
                                        />
                                    </div>
                                    <div class="item-details">
                                        <div class="item-name">{{ getItemName(item) }}</div>
                                        <div class="item-meta">
                                            <span *ngIf="item.itemSku">{{ 'admin.orders.sku' | translate }}: {{ item.itemSku }}</span>
                                            <span class="item-quantity">{{ 'admin.orders.quantity' | translate }}: {{ item.quantity }}</span>
                                        </div>
                                        <div class="item-price-per-unit">
                                            {{ formatCurrency(item.unitPrice) }} {{ 'admin.orders.each' | translate }}
                                        </div>
                                    </div>
                                    <div class="item-total">
                                        <div class="item-total-amount">{{ formatCurrency(item.total || item.lineTotal || 0) }}</div>
                                    </div>
                                </div>
                            </div>
                            <div class="order-items-subtotal">
                                <span class="subtotal-label">{{ 'admin.orders.subtotal' | translate }}</span>
                                <span class="subtotal-value">{{ formatCurrency(displaySubtotal) }}</span>
                            </div>
                        </p-card>

                        <!-- Addresses -->
                        <div class="addresses-grid">
                            <p-card [styleClass]="'address-card'" *ngIf="displayShippingAddress">
                                <ng-template #header>
                                    <div class="card-header">
                                        <i class="pi pi-truck"></i>
                                        <span>{{ 'admin.orders.shippingAddress' | translate }}</span>
                                    </div>
                                </ng-template>
                                <div class="address-content">
                                    <p><strong>{{ displayShippingAddress.fullName }}</strong></p>
                                    <p>{{ displayShippingAddress.addressLine1 }}</p>
                                    <p *ngIf="displayShippingAddress.addressLine2">{{ displayShippingAddress.addressLine2 }}</p>
                                    <p>{{ displayShippingAddress.city }}<span *ngIf="displayShippingAddress.state">, {{ displayShippingAddress.state }}</span> <span *ngIf="displayShippingAddress.postalCode">{{ displayShippingAddress.postalCode }}</span></p>
                                    <p>{{ displayShippingAddress.country }}</p>
                                    <p *ngIf="displayShippingAddress.phone">
                                        <i class="pi pi-phone"></i> {{ displayShippingAddress.phone }}
                                    </p>
                                </div>
                            </p-card>

                            <p-card [styleClass]="'address-card'" *ngIf="displayBillingAddress">
                                <ng-template #header>
                                    <div class="card-header">
                                        <i class="pi pi-credit-card"></i>
                                        <span>{{ 'admin.orders.billingAddress' | translate }}</span>
                                    </div>
                                </ng-template>
                                <div class="address-content">
                                    <p><strong>{{ displayBillingAddress.fullName }}</strong></p>
                                    <p>{{ displayBillingAddress.addressLine1 }}</p>
                                    <p *ngIf="displayBillingAddress.addressLine2">{{ displayBillingAddress.addressLine2 }}</p>
                                    <p>{{ displayBillingAddress.city }}<span *ngIf="displayBillingAddress.state">, {{ displayBillingAddress.state }}</span> <span *ngIf="displayBillingAddress.postalCode">{{ displayBillingAddress.postalCode }}</span></p>
                                    <p>{{ displayBillingAddress.country }}</p>
                                    <p *ngIf="displayBillingAddress.phone">
                                        <i class="pi pi-phone"></i> {{ displayBillingAddress.phone }}
                                    </p>
                                </div>
                            </p-card>
                        </div>
                    </div>

                    <!-- Right Column: Summary & Actions -->
                    <div class="order-sidebar">
                        <!-- Order Summary Card -->
                        <p-card [styleClass]="'order-summary-card'">
                            <ng-template #header>
                                <div class="card-header">
                                    <i class="pi pi-calculator"></i>
                                    <span>{{ 'admin.orders.orderSummary' | translate }}</span>
                                </div>
                            </ng-template>
                            <div class="summary-list">
                                <div class="summary-item">
                                    <span>{{ 'admin.orders.subtotal' | translate }}</span>
                                    <span>{{ formatCurrency(displaySubtotal) }}</span>
                                </div>
                                <div class="summary-item" *ngIf="displayDiscountAmount > 0">
                                    <span>{{ 'admin.orders.discount' | translate }}</span>
                                    <span class="discount-amount">-{{ formatCurrency(displayDiscountAmount) }}</span>
                                </div>
                                <div class="summary-item">
                                    <span>{{ 'admin.orders.tax' | translate }}</span>
                                    <span>{{ formatCurrency(displayTaxAmount) }}</span>
                                </div>
                                <div class="summary-item">
                                    <span>{{ 'admin.orders.shipping' | translate }}</span>
                                    <span>{{ formatCurrency(displayShippingAmount) }}</span>
                                </div>
                                <div class="summary-divider"></div>
                                <div class="summary-total">
                                    <span class="total-label">{{ 'admin.orders.total' | translate }}</span>
                                    <span class="total-value">{{ formatCurrency(displayTotal) }}</span>
                                </div>
                            </div>
                        </p-card>

                        <!-- Actions Card -->
                        <p-card [styleClass]="'order-actions-card'">
                            <ng-template #header>
                                <div class="card-header">
                                    <i class="pi pi-cog"></i>
                                    <span>{{ 'admin.orders.actions' | translate }}</span>
                                </div>
                            </ng-template>
                            <div class="actions-content">
                                <div class="action-group">
                                    <label class="action-label">{{ 'admin.orders.changeStatus' | translate }}</label>
                                    <p-select 
                                        [options]="statusOptions"
                                        [(ngModel)]="selectedStatusId"
                                        optionLabel="name"
                                        optionValue="id"
                                        [placeholder]="'admin.orders.selectStatus' | translate"
                                        styleClass="w-full"
                                    ></p-select>
                                    <p-button 
                                        [label]="'admin.orders.updateStatus' | translate" 
                                        icon="pi pi-check"
                                        (onClick)="updateStatus()"
                                        [disabled]="updatingStatus || !selectedStatusId || selectedStatusId === orderStatusId"
                                        [loading]="updatingStatus"
                                        styleClass="w-full mt-2"
                                    ></p-button>
                                </div>
                                
                                <div class="action-divider"></div>
                                
                                <p-button 
                                    [label]="'admin.orders.downloadInvoice' | translate" 
                                    icon="pi pi-download"
                                    [outlined]="true"
                                    (onClick)="downloadInvoice()"
                                    [loading]="downloadingInvoice"
                                    styleClass="w-full"
                                ></p-button>
                            </div>
                        </p-card>
                    </div>
                </div>
            </div>

            <!-- Error State -->
            <div *ngIf="!loading && !order" class="error-container">
                <i class="pi pi-exclamation-triangle text-4xl text-red-500"></i>
                <p>{{ 'messages.errorLoadingOrder' | translate }}</p>
                <p-button 
                    [label]="'common.back' | translate" 
                    icon="pi pi-arrow-left"
                    (onClick)="goBack()"
                ></p-button>
            </div>
        </div>
    `,
    styles: [`
        .order-detail-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .order-detail-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .order-detail-container {
                padding: 1.5rem;
            }
        }

        .order-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .order-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0 0 0.5rem 0;
        }

        .header-subtitle {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
            margin: 0;
        }

        @media (min-width: 768px) {
            .order-header h1 {
                font-size: 2rem;
            }
            .header-subtitle {
                font-size: 1rem;
            }
        }

        .loading-container,
        .error-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 1rem;
            padding: 3rem 1rem;
            text-align: center;
        }

        .order-content {
            width: 100%;
        }

        .order-main-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1.5rem;
        }

        @media (min-width: 1024px) {
            .order-main-grid {
                grid-template-columns: 2fr 1fr;
            }
        }

        .order-main-column {
            display: flex;
            flex-direction: column;
            gap: 2rem;
        }

        .order-sidebar {
            display: flex;
            flex-direction: column;
            gap: 2rem;
        }

        .card-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-weight: 600;
        }

        .card-header i {
            font-size: 1.25rem;
        }

        .items-count {
            color: var(--text-color-secondary);
            font-weight: normal;
            margin-left: 0.5rem;
        }

        .order-info-card,
        .order-items-card,
        .order-summary-card,
        .order-actions-card,
        .address-card {
            width: 100%;
        }

        ::ng-deep .order-info-card .p-card-body,
        ::ng-deep .order-items-card .p-card-body,
        ::ng-deep .order-summary-card .p-card-body,
        ::ng-deep .order-actions-card .p-card-body,
        ::ng-deep .address-card .p-card-body {
            padding: 1.5rem;
        }

        ::ng-deep .order-info-card .p-card-header,
        ::ng-deep .order-items-card .p-card-header,
        ::ng-deep .order-summary-card .p-card-header,
        ::ng-deep .order-actions-card .p-card-header,
        ::ng-deep .address-card .p-card-header {
            padding: 1.25rem 1.5rem;
            border-bottom: 1px solid var(--surface-border);
        }

        .order-info-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1.25rem;
        }

        @media (min-width: 576px) {
            .order-info-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .info-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            padding: 0.75rem;
            background: var(--surface-50);
            border-radius: 8px;
        }

        .info-item label {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            font-weight: 500;
        }

        .info-value {
            font-size: 1rem;
            color: var(--text-color);
            font-weight: 500;
        }

        .order-items-list {
            display: flex;
            flex-direction: column;
            gap: 1.25rem;
            padding: 0.5rem 0;
        }

        .order-item {
            display: grid;
            grid-template-columns: auto 1fr auto;
            gap: 1.25rem;
            padding: 1.5rem;
            background: var(--surface-0);
            border-radius: 12px;
            align-items: center;
            border: 1px solid var(--surface-border);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
        }

        @media (max-width: 575px) {
            .order-item {
                grid-template-columns: 1fr;
                gap: 0.75rem;
                padding: 1rem;
            }
        }

        .item-image-wrapper {
            width: 100px;
            height: 100px;
            flex-shrink: 0;
        }

        @media (max-width: 575px) {
            .item-image-wrapper {
                width: 100%;
                height: 200px;
            }
        }

        .item-image {
            width: 100%;
            height: 100%;
            object-fit: cover;
            border-radius: 8px;
        }

        .item-details {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            min-width: 0;
        }

        .item-name {
            font-weight: 600;
            font-size: 1.0625rem;
            color: var(--text-color);
            margin-bottom: 0.25rem;
        }

        .item-meta {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }

        @media (min-width: 576px) {
            .item-meta {
                flex-direction: row;
                gap: 1rem;
            }
        }

        .item-price-per-unit {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }

        .item-total {
            text-align: right;
            min-width: 100px;
        }

        @media (max-width: 575px) {
            .item-total {
                text-align: left;
                min-width: auto;
            }
        }

        .item-total-amount {
            font-weight: 700;
            font-size: 1.25rem;
            color: var(--primary-color);
        }

        .order-items-subtotal {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1.5rem;
            margin: 1.5rem -1.5rem -1.5rem -1.5rem;
            border-top: 2px solid var(--surface-border);
            background: var(--surface-50);
            border-radius: 0 0 12px 12px;
        }

        .subtotal-label {
            font-size: 1.125rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .subtotal-value {
            font-size: 1.375rem;
            font-weight: 700;
            color: var(--primary-color);
        }

        .addresses-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1.5rem;
        }

        @media (min-width: 768px) {
            .addresses-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .address-content {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            line-height: 1.7;
            padding: 0.5rem 0;
        }

        .address-content p {
            margin: 0;
            padding: 0.25rem 0;
        }

        .summary-list {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            padding: 0.5rem 0;
        }

        .summary-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 0.9375rem;
            padding: 0.5rem 0;
        }

        .discount-amount {
            color: var(--primary-500);
            font-weight: 600;
        }

        .summary-divider {
            height: 1px;
            background: var(--surface-border);
            margin: 0.5rem 0;
        }

        .summary-total {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1rem 0.75rem;
            margin: 1rem -1.5rem -1.5rem -1.5rem;
            background: var(--surface-50);
            border-radius: 0 0 12px 12px;
            border-top: 2px solid var(--surface-border);
        }

        .total-label {
            font-size: 1.125rem;
            font-weight: 700;
        }

        .total-value {
            font-size: 1.75rem;
            font-weight: 700;
            color: var(--primary-color);
        }

        @media (max-width: 575px) {
            .total-value {
                font-size: 1.5rem;
            }
        }

        .actions-content {
            display: flex;
            flex-direction: column;
            gap: 1.25rem;
            padding: 0.5rem 0;
        }

        .action-group {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .action-label {
            font-weight: 500;
            font-size: 0.9375rem;
        }

        .action-divider {
            height: 1px;
            background: var(--surface-border);
            margin: 0.5rem 0;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .order-detail-container {
                padding: 0.5rem;
            }

            .order-header h1 {
                font-size: 1.25rem;
            }

            .header-subtitle {
                font-size: 0.8125rem;
            }

            .order-main-grid {
                gap: 1rem;
            }

            .order-main-column,
            .order-sidebar {
                gap: 1rem;
            }

            .order-info-grid {
                grid-template-columns: 1fr;
            }

            .order-item {
                grid-template-columns: 1fr;
                gap: 0.75rem;
                padding: 1rem;
            }

            .item-image-wrapper {
                width: 100%;
                height: 200px;
            }

            .item-total {
                text-align: left;
            }

            .addresses-grid {
                grid-template-columns: 1fr;
            }

            .card-header {
                font-size: 0.9375rem;
            }

            .card-header i {
                font-size: 1rem;
            }

            .total-value {
                font-size: 1.5rem;
            }
        }
    `]
})
export class OrderDetailComponent implements OnInit {
    order: any | null = null;
    loading = false;
    orderStatuses: any[] = [];
    statusOptions: any[] = [];
    selectedStatusId: string = '';
    updatingStatus = false;
    downloadingInvoice = false;

    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private ordersApiService = inject(OrdersApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);

    ngOnInit() {
        const orderId = this.route.snapshot.paramMap.get('id');
        if (orderId) {
            this.loadOrder(orderId);
            this.loadOrderStatuses();
        }
    }

    loadOrder(id: string) {
        this.loading = true;
        this.ordersApiService.getOrderById(id).subscribe({
            next: (order) => {
                this.order = this.transformOrderData(order);
                this.selectedStatusId = String(this.orderStatusId || '');
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('common.error'),
                    detail: this.translateService.instant('messages.errorLoadingOrderDetail')
                });
            }
        });
    }

    loadOrderStatuses() {
        this.ordersApiService.getOrderStatuses().subscribe({
            next: (statuses) => {
                const lang = this.languageService.language;
                this.orderStatuses = statuses;
                this.statusOptions = statuses.map((s: any) => ({
                    id: typeof s.id === 'string' ? s.id : String(s.id),
                    name: lang === 'ar' ? s.nameAr : s.nameEn
                }));
                if (this.order) {
                    this.selectedStatusId = this.orderStatusId;
                }
            },
            error: () => {
                this.messageService.add({
                    severity: 'warn',
                    summary: this.translateService.instant('common.warning'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    transformOrderData(order: any): any {
        // Generate order number if empty
        const orderNumber = order.orderNumber || `ORD-${order.id.substring(0, 8).toUpperCase()}`;
        
        // Get customer name
        const customerName = order.userFullName || order.customerName || 'N/A';
        
        // Get status
        const statusKey = order.orderStatusKey || (order.status?.key) || 'Unknown';
        const statusName = this.languageService.language === 'ar' 
            ? (order.status?.nameAr || statusKey)
            : (order.status?.nameEn || statusKey);
        
        // Get payment status
        const paymentStatus = order.paymentStatus || 'Pending';
        
        // Get addresses
        const shippingAddress = order.shippingAddress || null;
        const billingAddress = order.billingAddress || null;
        
        // Get order items
        const orderItems = order.orderItems || order.items || [];
        
        return {
            ...order,
            orderNumber,
            customerName,
            displayOrderNumber: orderNumber,
            displayCustomerName: customerName,
            displayStatusKey: statusKey,
            displayStatusName: statusName,
            displayPaymentStatus: paymentStatus,
            displaySubtotal: order.subTotal || order.subtotal || 0,
            displayTaxAmount: order.taxAmount || 0,
            displayShippingAmount: order.shippingCost || order.shippingAmount || 0,
            displayDiscountAmount: order.discountAmount || 0,
            displayTotal: order.total || order.totalAmount || 0,
            displayShippingAddress: shippingAddress,
            displayBillingAddress: billingAddress,
            displayOrderItems: orderItems,
            orderStatusId: String(order.orderStatusId ?? order.statusId ?? ''),
            status: order.status || { key: statusKey, nameEn: statusName, nameAr: statusName }
        };
    }

    get displayOrderNumber(): string {
        return this.order?.displayOrderNumber || this.order?.orderNumber || 'N/A';
    }

    get displayCustomerName(): string {
        return this.order?.displayCustomerName || this.order?.customerName || 'N/A';
    }

    get displayStatusKey(): string {
        return this.order?.displayStatusKey || this.order?.orderStatusKey || 'Unknown';
    }

    get displayStatusName(): string {
        return this.order?.displayStatusName || this.displayStatusKey;
    }

    get displayPaymentStatus(): string {
        return this.order?.displayPaymentStatus || 'Pending';
    }

    get displaySubtotal(): number {
        // Calculate subtotal from order items if available
        if (this.displayOrderItems && this.displayOrderItems.length > 0) {
            const calculatedSubtotal = this.displayOrderItems.reduce((sum, item) => {
                return sum + (item.total || item.lineTotal || (item.unitPrice * item.quantity) || 0);
            }, 0);
            if (calculatedSubtotal > 0) {
                return calculatedSubtotal;
            }
        }
        // Fallback to order subtotal
        return this.order?.displaySubtotal || this.order?.subTotal || this.order?.subtotal || 0;
    }

    get displayTaxAmount(): number {
        return this.order?.displayTaxAmount || this.order?.taxAmount || 0;
    }

    get displayShippingAmount(): number {
        return this.order?.displayShippingAmount || this.order?.shippingCost || this.order?.shippingAmount || 0;
    }

    get displayDiscountAmount(): number {
        return this.order?.displayDiscountAmount || this.order?.discountAmount || 0;
    }

    get displayTotal(): number {
        return this.order?.displayTotal || this.order?.total || this.order?.totalAmount || 0;
    }

    get displayShippingAddress(): any {
        return this.order?.displayShippingAddress || this.order?.shippingAddress || null;
    }

    get displayBillingAddress(): any {
        return this.order?.displayBillingAddress || this.order?.billingAddress || null;
    }

    get displayOrderItems(): any[] {
        return this.order?.displayOrderItems || this.order?.orderItems || this.order?.items || [];
    }

    get orderItemsCount(): number {
        return this.displayOrderItems.length;
    }

    get orderStatusId(): string {
        const raw = this.order?.orderStatusId ?? this.order?.statusId;
        return raw != null ? String(raw) : '';
    }

    private statusKeyForId(statusId: string | null | undefined): string | null {
        if (!statusId) return null;
        const id = String(statusId);
        const row = this.orderStatuses.find((s: any) => String(s.id) === id);
        return row?.key ?? null;
    }

    updateStatus() {
        if (!this.order || !this.selectedStatusId || this.updatingStatus) return;

        const targetKey = this.statusKeyForId(this.selectedStatusId);
        const proceed = () => this.executeStatusUpdate();

        if (targetKey === 'Cancelled' || targetKey === 'Delivered') {
            const msgKey =
                targetKey === 'Cancelled'
                    ? 'admin.orders.confirmCancelStatus'
                    : 'admin.orders.confirmDeliveredStatus';
            this.confirmationService.confirm({
                message: this.translateService.instant(msgKey),
                header: this.translateService.instant('common.confirm'),
                icon: 'pi pi-exclamation-triangle',
                acceptLabel: this.translateService.instant('common.yes'),
                rejectLabel: this.translateService.instant('common.no'),
                accept: () => proceed()
            });
            return;
        }

        proceed();
    }

    private executeStatusUpdate() {
        if (!this.order || !this.selectedStatusId) return;

        this.updatingStatus = true;
        this.ordersApiService
            .updateOrderStatus(this.order.id, { orderStatusId: String(this.selectedStatusId) })
            .subscribe({
                next: (updatedOrder) => {
                    this.order = this.transformOrderData(updatedOrder);
                    this.selectedStatusId = this.orderStatusId;
                    this.updatingStatus = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('common.success'),
                        detail: this.translateService.instant('admin.orders.statusUpdated')
                    });
                },
                error: (err: HttpErrorResponse) => {
                    this.updatingStatus = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('common.error'),
                        detail: this.mapStatusUpdateError(err)
                    });
                }
            });
    }

    private mapStatusUpdateError(err: HttpErrorResponse): string {
        if (err.error && typeof err.error === 'object' && 'message' in err.error) {
            const m = (err.error as { message?: string }).message;
            if (m && m.trim()) return m;
        }
        if (err.status === 403 || err.status === 401) {
            return this.translateService.instant('messages.forbidden');
        }
        if (err.status === 404) {
            return this.translateService.instant('messages.notFound');
        }
        if (err.status === 400) {
            return this.translateService.instant('messages.badRequest');
        }
        return this.translateService.instant('messages.errorUpdatingOrderStatus');
    }

    downloadInvoice() {
        if (!this.order) return;

        this.downloadingInvoice = true;
        this.ordersApiService.downloadInvoice(this.order.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `invoice-${this.displayOrderNumber}.pdf`;
                link.click();
                window.URL.revokeObjectURL(url);
                this.downloadingInvoice = false;
            },
            error: () => {
                this.downloadingInvoice = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('common.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    getStatusSeverity(statusKey: string): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" | null | undefined {
        const status = statusKey?.toLowerCase() || '';
        if (status === 'delivered' || status === 'completed') return 'success';
        if (status === 'shipped' || status === 'processing') return 'info';
        if (status === 'pending') return 'warn';
        if (status === 'cancelled' || status === 'canceled') return 'danger';
        return 'secondary';
    }

    getPaymentStatusSeverity(paymentStatus: string): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" | null | undefined {
        const status = paymentStatus?.toLowerCase() || '';
        if (status === 'paid') return 'success';
        if (status === 'partiallypaid') return 'warn';
        if (status === 'refunded') return 'info';
        if (status === 'failed') return 'danger';
        return 'warn';
    }

    getItemName(item: any): string {
        const lang = this.languageService.language;
        return lang === 'ar' ? (item.itemNameAr || item.itemNameEn || 'N/A') : (item.itemNameEn || item.itemNameAr || 'N/A');
    }

    formatCurrency(value: number): string {
        const currency = this.order?.currency || 'USD';
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency
        }).format(value || 0);
    }

    formatDate(date: string | undefined): string {
        if (!date) return 'N/A';
        return new Date(date).toLocaleDateString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    goBack() {
        this.router.navigate(['/admin/orders']);
    }
}
