import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { LanguageService, DashboardService } from '../../shared/services';

interface DashboardSummary {
    todaySalesTotal: number;
    todayPurchasesTotal: number;
    totalStockValue: number;
    totalCustomers: number;
    totalSuppliers: number;
    lowStockItemsCount: number;
}

interface RecentSaleInvoice {
    id: number;
    invoiceNumber: string;
    customerNameAr: string;
    customerNameEn: string;
    invoiceDate: string;
    netAmount: number;
}

interface RecentPurchaseInvoice {
    id: number;
    invoiceNumber: string;
    supplierNameAr: string;
    supplierNameEn: string;
    invoiceDate: string;
    netAmount: number;
}

interface LowStockItem {
    id: number;
    itemCode: string;
    itemNameAr: string;
    itemNameEn: string;
    warehouseNameAr: string;
    warehouseNameEn: string;
    quantity: number;
    minQuantity: number;
}

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        TableModule,
        TagModule,
        ButtonModule,
        SkeletonModule,
        TranslateModule
    ],
    template: `
        <div class="grid grid-cols-12 gap-4">
            <!-- Summary Cards -->
            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
                            <i class="pi pi-shopping-cart text-blue-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.totalSales' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ summary().todaySalesTotal | currency:'USD' }}</span>
                            <span class="text-sm text-surface-500 block">{{ 'dashboard.today' | translate }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-orange-100 dark:bg-orange-900 flex items-center justify-center">
                            <i class="pi pi-truck text-orange-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.totalPurchases' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ summary().todayPurchasesTotal | currency:'USD' }}</span>
                            <span class="text-sm text-surface-500 block">{{ 'dashboard.today' | translate }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                            <i class="pi pi-box text-green-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.stockValue' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ summary().totalStockValue | currency:'USD' }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-cyan-100 dark:bg-cyan-900 flex items-center justify-center">
                            <i class="pi pi-users text-cyan-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.totalCustomers' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ summary().totalCustomers }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-purple-100 dark:bg-purple-900 flex items-center justify-center">
                            <i class="pi pi-building text-purple-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.totalSuppliers' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ summary().totalSuppliers }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-span-12 lg:col-span-6 xl:col-span-4">
                <div class="card h-full">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900 flex items-center justify-center">
                            <i class="pi pi-exclamation-triangle text-red-500 text-2xl"></i>
                        </div>
                        <div class="flex-1">
                            <span class="text-surface-500 dark:text-surface-400 block mb-1">{{ 'dashboard.lowStockItems' | translate }}</span>
                            <span class="text-2xl font-bold text-surface-900 dark:text-surface-0" [class.text-red-500]="summary().lowStockItemsCount > 0">{{ summary().lowStockItemsCount }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Recent Sales -->
            <div class="col-span-12 xl:col-span-6">
                <div class="card">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="text-xl font-semibold m-0">{{ 'dashboard.recentSales' | translate }}</h5>
                    </div>
                    <p-table [value]="recentSales()" [loading]="loading" [tableStyle]="{ 'min-width': '100%' }">
                        <ng-template #header>
                            <tr>
                                <th>{{ 'purchases.invoiceNumber' | translate }}</th>
                                <th>{{ 'sales.customer' | translate }}</th>
                                <th>{{ 'common.date' | translate }}</th>
                                <th style="text-align: right">{{ 'common.amount' | translate }}</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-sale>
                            <tr>
                                <td class="font-medium">{{ sale.invoiceNumber }}</td>
                                <td>{{ getCustomerName(sale) }}</td>
                                <td>{{ sale.invoiceDate | date:'shortDate' }}</td>
                                <td style="text-align: right">{{ sale.netAmount | currency:'USD' }}</td>
                            </tr>
                        </ng-template>
                        <ng-template #emptymessage>
                            <tr><td colspan="4" class="text-center p-4 text-surface-500">{{ 'common.noDataFound' | translate }}</td></tr>
                        </ng-template>
                    </p-table>
                </div>
            </div>

            <!-- Recent Purchases -->
            <div class="col-span-12 xl:col-span-6">
                <div class="card">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="text-xl font-semibold m-0">{{ 'dashboard.recentPurchases' | translate }}</h5>
                    </div>
                    <p-table [value]="recentPurchases()" [loading]="loading" [tableStyle]="{ 'min-width': '100%' }">
                        <ng-template #header>
                            <tr>
                                <th>{{ 'purchases.invoiceNumber' | translate }}</th>
                                <th>{{ 'purchases.supplier' | translate }}</th>
                                <th>{{ 'common.date' | translate }}</th>
                                <th style="text-align: right">{{ 'common.amount' | translate }}</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-purchase>
                            <tr>
                                <td class="font-medium">{{ purchase.invoiceNumber }}</td>
                                <td>{{ getSupplierName(purchase) }}</td>
                                <td>{{ purchase.invoiceDate | date:'shortDate' }}</td>
                                <td style="text-align: right">{{ purchase.netAmount | currency:'USD' }}</td>
                            </tr>
                        </ng-template>
                        <ng-template #emptymessage>
                            <tr><td colspan="4" class="text-center p-4 text-surface-500">{{ 'common.noDataFound' | translate }}</td></tr>
                        </ng-template>
                    </p-table>
                </div>
            </div>

            <!-- Low Stock Items -->
            <div class="col-span-12">
                <div class="card">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="text-xl font-semibold m-0">{{ 'dashboard.lowStockItems' | translate }}</h5>
                    </div>
                    <p-table [value]="lowStockItems()" [loading]="loading" [tableStyle]="{ 'min-width': '100%' }">
                        <ng-template #header>
                            <tr>
                                <th>{{ 'items.code' | translate }}</th>
                                <th>{{ 'common.name' | translate }}</th>
                                <th>{{ 'items.warehouse' | translate }}</th>
                                <th style="text-align: right">{{ 'common.quantity' | translate }}</th>
                                <th style="text-align: right">{{ 'items.minQty' | translate }}</th>
                                <th>{{ 'common.status' | translate }}</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-item>
                            <tr>
                                <td class="font-medium">{{ item.itemCode }}</td>
                                <td>{{ getItemName(item) }}</td>
                                <td>{{ getWarehouseName(item) }}</td>
                                <td style="text-align: right" class="text-red-500 font-bold">{{ item.quantity }}</td>
                                <td style="text-align: right">{{ item.minQuantity }}</td>
                                <td>
                                    <p-tag 
                                        [value]="item.quantity <= 0 ? ('items.outOfStock' | translate) : ('items.lowStock' | translate)" 
                                        [severity]="item.quantity <= 0 ? 'danger' : 'warn'" 
                                    />
                                </td>
                            </tr>
                        </ng-template>
                        <ng-template #emptymessage>
                            <tr><td colspan="6" class="text-center p-4 text-blue-500"><i class="pi pi-check-circle mr-2"></i>{{ 'dashboard.noLowStockItems' | translate }}</td></tr>
                        </ng-template>
                    </p-table>
                </div>
            </div>
        </div>
    `
})
export class Dashboard implements OnInit {
    summary = signal<DashboardSummary>({
        todaySalesTotal: 0,
        todayPurchasesTotal: 0,
        totalStockValue: 0,
        totalCustomers: 0,
        totalSuppliers: 0,
        lowStockItemsCount: 0
    });
    recentSales = signal<RecentSaleInvoice[]>([]);
    recentPurchases = signal<RecentPurchaseInvoice[]>([]);
    lowStockItems = signal<LowStockItem[]>([]);
    loading = false;

    constructor(
        private dashboardService: DashboardService,
        public langService: LanguageService
    ) {}

    ngOnInit() {
        this.loadDashboardData();
    }

    loadDashboardData() {
        this.loading = true;
        
        this.dashboardService.getSummary().subscribe({
            next: (data: any) => {
                this.summary.set({
                    todaySalesTotal: data.todaySalesTotal || 0,
                    todayPurchasesTotal: data.todayPurchasesTotal || 0,
                    totalStockValue: data.totalStockValue || 0,
                    totalCustomers: data.totalCustomers || 0,
                    totalSuppliers: data.totalSuppliers || 0,
                    lowStockItemsCount: data.lowStockItemsCount || 0
                });
                
                // Extract tables data if included in summary response
                if (data.recentSalesInvoices) {
                    this.recentSales.set(data.recentSalesInvoices.slice(0, 10));
                }
                if (data.recentPurchaseInvoices) {
                    this.recentPurchases.set(data.recentPurchaseInvoices.slice(0, 10));
                }
                if (data.lowStockItems) {
                    this.lowStockItems.set(data.lowStockItems.slice(0, 10));
                }
                
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                // Load with mock data on error for demo purposes
                this.loadMockData();
            }
        });
    }

    private loadMockData() {
        this.summary.set({
            todaySalesTotal: 15750.00,
            todayPurchasesTotal: 8500.00,
            totalStockValue: 125000.00,
            totalCustomers: 45,
            totalSuppliers: 12,
            lowStockItemsCount: 3
        });
    }

    getCustomerName(sale: RecentSaleInvoice): string {
        return this.langService.language === 'ar' 
            ? (sale.customerNameAr || sale.customerNameEn || '') 
            : (sale.customerNameEn || sale.customerNameAr || '');
    }

    getSupplierName(purchase: RecentPurchaseInvoice): string {
        return this.langService.language === 'ar' 
            ? (purchase.supplierNameAr || purchase.supplierNameEn || '') 
            : (purchase.supplierNameEn || purchase.supplierNameAr || '');
    }

    getItemName(item: LowStockItem): string {
        return this.langService.language === 'ar' 
            ? (item.itemNameAr || item.itemNameEn || '') 
            : (item.itemNameEn || item.itemNameAr || '');
    }

    getWarehouseName(item: LowStockItem): string {
        return this.langService.language === 'ar' 
            ? (item.warehouseNameAr || item.warehouseNameEn || '') 
            : (item.warehouseNameEn || item.warehouseNameAr || '');
    }
}
