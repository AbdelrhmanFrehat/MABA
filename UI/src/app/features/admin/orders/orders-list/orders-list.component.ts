import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrdersApiService } from '../../../../shared/services/orders-api.service';
import { Order, OrderListResponse, OrderStatus } from '../../../../shared/models/order.model';
import { LanguageService } from '../../../../shared/services/language.service';

@Component({
    selector: 'app-orders-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        DataTableComponent,
        ButtonModule,
        InputTextModule,
        SelectModule,
        CardModule,
        IconFieldModule,
        InputIconModule,
        DialogModule,
        ToastModule,
        TagModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="orders-container">
            <!-- Header -->
            <div class="orders-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.orders.title' | translate }}</h1>
                        <p class="header-subtitle">{{ 'admin.orders.manageOrders' | translate }}</p>
                    </div>
                    <div class="header-stats">
                        <div class="stat-card">
                            <div class="stat-icon bg-primary-100">
                                <i class="pi pi-shopping-cart text-primary"></i>
                            </div>
                            <div class="stat-content">
                                <div class="stat-label">{{ 'admin.orders.totalOrders' | translate }}</div>
                                <div class="stat-value">{{ totalRecords }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Filters Card -->
            <p-card [styleClass]="'filters-card'">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.orders.search' | translate }}</label>
                        <p-iconfield>
                            <p-inputicon styleClass="pi pi-search" />
                            <input 
                                type="text" 
                                pInputText
                                [(ngModel)]="filters.search"
                                [placeholder]="'admin.orders.searchPlaceholder' | translate"
                                class="w-full"
                                (input)="onFilterChange()"
                            />
                        </p-iconfield>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.orders.status' | translate }}</label>
                        <p-select 
                            [options]="statusOptions"
                            [(ngModel)]="filters.statusId"
                            optionLabel="name"
                            optionValue="id"
                            [placeholder]="'admin.orders.allStatuses' | translate"
                            [showClear]="true"
                            styleClass="w-full"
                            (onChange)="onFilterChange()"
                        ></p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.orders.dateFrom' | translate }}</label>
                        <input 
                            type="date" 
                            pInputText
                            [(ngModel)]="filters.dateFrom"
                            class="w-full"
                            (change)="onFilterChange()"
                        />
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.orders.dateTo' | translate }}</label>
                        <input 
                            type="date" 
                            pInputText
                            [(ngModel)]="filters.dateTo"
                            class="w-full"
                            (change)="onFilterChange()"
                        />
                    </div>
                    <div class="filter-item filter-actions">
                        <p-button 
                            [label]="'common.clear' | translate" 
                            icon="pi pi-times"
                            [outlined]="true"
                            severity="secondary"
                            (onClick)="clearFilters()"
                            styleClass="w-full"
                        ></p-button>
                    </div>
                </div>
            </p-card>

            <!-- Orders Table -->
            <p-card [styleClass]="'orders-table-card'">
                <app-data-table
                    [data]="orders"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [paginator]="true"
                    [rows]="pageSize"
                    [lazy]="true"
                    [totalRecords]="totalRecords"
                    (onLazyLoad)="onLazyLoad($event)"
                    (onAction)="handleAction($event)"
                >
                    <ng-template #customColumn let-row let-column="column">
                        <ng-container *ngIf="column.field === 'displayStatus'">
                            <p-tag 
                                [value]="row.displayStatusName || row.displayStatus" 
                                [severity]="getStatusSeverity(row.displayStatus)"
                            ></p-tag>
                        </ng-container>
                        <ng-container *ngIf="column.field === 'displayPaymentStatus'">
                            <p-tag 
                                [value]="row.displayPaymentStatus" 
                                [severity]="getPaymentStatusSeverity(row.displayPaymentStatus)"
                            ></p-tag>
                        </ng-container>
                    </ng-template>
                </app-data-table>
            </p-card>
        </div>
    `,
    styles: [`
        .orders-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .orders-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .orders-container {
                padding: 1.5rem;
            }
        }

        .orders-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: flex-start;
            }
        }

        .orders-header h1 {
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
            .orders-header h1 {
                font-size: 2rem;
            }
            .header-subtitle {
                font-size: 1rem;
            }
        }

        .header-stats {
            display: flex;
            gap: 1rem;
        }

        .stat-card {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem 1rem;
            background: var(--surface-50);
            border-radius: 8px;
            min-width: 150px;
        }

        .stat-icon {
            width: 2.5rem;
            height: 2.5rem;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .stat-icon i {
            font-size: 1.25rem;
        }

        .stat-content {
            flex: 1;
            min-width: 0;
        }

        .stat-label {
            font-size: 0.75rem;
            color: var(--text-color-secondary);
            margin-bottom: 0.25rem;
        }

        .stat-value {
            font-size: 1.25rem;
            font-weight: bold;
            color: var(--text-color);
        }

        .filters-card {
            margin-bottom: 1.5rem;
        }

        .filters-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 576px) {
            .filters-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (min-width: 768px) {
            .filters-grid {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        @media (min-width: 1024px) {
            .filters-grid {
                grid-template-columns: repeat(5, 1fr);
            }
        }

        .filter-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .filter-item.filter-actions {
            align-self: flex-end;
        }

        @media (min-width: 1024px) {
            .filter-item.filter-actions {
                align-self: flex-end;
            }
        }

        .filter-label {
            font-size: 0.875rem;
            font-weight: 500;
            color: var(--text-color);
        }

        .orders-table-card {
            margin-bottom: 1rem;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .orders-container {
                padding: 0.5rem;
            }

            .stat-card {
                min-width: 120px;
                padding: 0.5rem 0.75rem;
            }

            .stat-icon {
                width: 2rem;
                height: 2rem;
            }

            .stat-icon i {
                font-size: 1rem;
            }

            .stat-value {
                font-size: 1rem;
            }

            .filter-item.filter-actions {
                grid-column: 1 / -1;
            }
        }
    `]
})
export class OrdersListComponent implements OnInit {
    orders: Order[] = [];
    loading = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    orderStatuses: OrderStatus[] = [];
    statusOptions: any[] = [];

    filters = {
        search: '',
        statusId: '',
        dateFrom: null as Date | string | null,
        dateTo: null as Date | string | null,
        paymentStatus: ''
    };

    columns: TableColumn[] = [
        { field: 'displayOrderNumber', headerKey: 'admin.orders.orderNumber', sortable: true, width: '150px' },
        { field: 'displayCustomerName', headerKey: 'admin.orders.customer', sortable: true },
        { field: 'displayStatus', headerKey: 'admin.orders.status', type: 'custom', sortable: true, width: '120px' },
        { field: 'displayTotal', headerKey: 'admin.orders.total', type: 'currency', sortable: true, width: '120px', align: 'right' },
        { field: 'displayPaymentStatus', headerKey: 'admin.orders.paymentStatus', type: 'custom', sortable: true, width: '130px' },
        { field: 'createdAt', headerKey: 'admin.orders.createdDate', type: 'date', sortable: true, width: '150px' }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-eye',
            tooltipKey: 'common.view',
            action: 'view'
        },
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        }
    ];

    private ordersApiService = inject(OrdersApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);
    private router = inject(Router);

    ngOnInit() {
        this.loadOrderStatuses();
        this.loadOrders();
    }

    loadOrderStatuses() {
        this.ordersApiService.getOrderStatuses().subscribe({
            next: (statuses) => {
                this.orderStatuses = statuses;
                this.statusOptions = [
                    { id: '', name: this.translateService.instant('admin.orders.allStatuses') },
                    ...statuses.map((s: OrderStatus) => ({
                        id: s.id,
                        name: this.languageService.language === 'ar' ? s.nameAr : s.nameEn
                    }))
                ];
            }
        });
    }

    loadOrders() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.filters.search) {
            params.searchTerm = this.filters.search;
        }
        if (this.filters.statusId) {
            params.statusId = this.filters.statusId;
        }
        if (this.filters.dateFrom) {
            const date = this.filters.dateFrom instanceof Date ? this.filters.dateFrom : new Date(this.filters.dateFrom);
            if (!isNaN(date.getTime())) {
                params.dateFrom = date.toISOString().split('T')[0];
            }
        }
        if (this.filters.dateTo) {
            const date = this.filters.dateTo instanceof Date ? this.filters.dateTo : new Date(this.filters.dateTo);
            if (!isNaN(date.getTime())) {
                params.dateTo = date.toISOString().split('T')[0];
            }
        }
        if (this.filters.paymentStatus) {
            params.paymentStatus = this.filters.paymentStatus;
        }

        this.ordersApiService.getOrders(params).subscribe({
            next: (response) => {
                // Transform data to match expected format
                this.orders = (response.items || []).map(order => this.transformOrderData(order));
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onFilterChange() {
        this.currentPage = 1;
        this.loadOrders();
    }

    clearFilters() {
        this.filters = {
            search: '',
            statusId: '',
            dateFrom: null,
            dateTo: null,
            paymentStatus: ''
        };
        this.currentPage = 1;
        this.loadOrders();
    }


    handleAction(event: { action: string; data: any }) {
        const order = event.data;
        if (event.action === 'view') {
            this.router.navigate(['/admin/orders', order.id]);
        } else if (event.action === 'edit') {
            this.router.navigate(['/admin/orders', order.id, 'edit']);
        }
    }

    onLazyLoad(event: any) {
        this.currentPage = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.loadOrders();
    }

    transformOrderData(order: any): any {
        // Generate order number if empty
        const orderNumber = order.orderNumber || `ORD-${order.id.substring(0, 8).toUpperCase()}`;
        
        // Get customer name
        const customerName = order.userFullName || order.customerName || 'N/A';
        
        // Get total amount
        const totalAmount = order.total || order.totalAmount || 0;
        
        // Get status - check multiple possible fields
        let statusKey = 'Unknown';
        let statusName = 'Unknown';
        
        if (order.orderStatusKey) {
            statusKey = order.orderStatusKey;
            statusName = order.orderStatusKey;
        } else if (order.status?.key) {
            statusKey = order.status.key;
            statusName = this.languageService.language === 'ar' ? order.status.nameAr : order.status.nameEn;
        } else if (order.status) {
            statusKey = order.status;
            statusName = order.status;
        }
        
        // Get payment status (default to Pending if not available)
        const paymentStatus = order.paymentStatus || 'Pending';
        
        return {
            ...order,
            displayOrderNumber: orderNumber,
            displayCustomerName: customerName,
            displayStatus: statusKey,
            displayStatusName: statusName,
            displayTotal: totalAmount,
            displayPaymentStatus: paymentStatus,
            // Keep original fields for sorting and compatibility
            orderNumber: orderNumber,
            customerName: customerName,
            totalAmount: totalAmount,
            status: order.status || { key: statusKey, nameEn: statusName, nameAr: statusName },
            // Map orderItems to items for compatibility
            items: order.orderItems || order.items || []
        };
    }

    getStatusSeverity(statusKey: string): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" | null | undefined {
        const status = statusKey?.toLowerCase() || '';
        if (status === 'delivered' || status === 'completed') return 'success';
        if (status === 'shipped' || status === 'processing') return 'info';
        if (status === 'pending') return 'warn';
        if (status === 'cancelled' || status === 'cancelled') return 'danger';
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
}

