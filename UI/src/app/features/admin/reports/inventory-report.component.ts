import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { ReportsService } from '../../../shared/services/reports.service';
import { LowStockReportItem, StockReportItem } from '../../../shared/models/report.model';

@Component({
    selector: 'app-inventory-report',
    standalone: true,
    imports: [CommonModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.reports.inventory' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Review current stock by warehouse and isolate the items already under their minimum threshold.</p>
            </div>
            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Stock Rows</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Low Stock</span><strong>{{ lowStockRows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Units on Hand</span><strong>{{ totalUnits | number:'1.0-0' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Shortage</span><strong>{{ totalShortage | number:'1.0-0' }}</strong></div></div>
            </div>
            <app-data-table [data]="rows" [columns]="columns" [loading]="loading" [showAddButton]="false" [showDeleteSelected]="false" [title]="'Inventory Report'" [globalFilterFields]="['itemCode', 'itemNameEn', 'warehouseNameEn']"></app-data-table>
            <div class="mt-5">
                <h2 class="text-xl font-semibold mb-3">Low Stock Watchlist</h2>
                <app-data-table [data]="lowStockRows" [columns]="lowStockColumns" [loading]="loadingLowStock" [showAddButton]="false" [showDeleteSelected]="false" [title]="'Low Stock'" [globalFilterFields]="['itemCode', 'itemNameEn', 'warehouseNameEn']"></app-data-table>
            </div>
        </div>
    `,
    styles: [`.summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; } .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }`]
})
export class InventoryReportComponent implements OnInit {
    rows: StockReportItem[] = [];
    lowStockRows: LowStockReportItem[] = [];
    loading = false;
    loadingLowStock = false;
    totalUnits = 0;
    totalShortage = 0;
    columns: TableColumn[] = [
        { field: 'itemCode', header: 'Item Code', sortable: true },
        { field: 'itemNameEn', header: 'Item', sortable: true },
        { field: 'warehouseNameEn', header: 'Warehouse', sortable: true },
        { field: 'currentQuantity', header: 'On Hand', sortable: true },
        { field: 'minQuantity', header: 'Min Qty', sortable: true },
        { field: 'isLowStock', header: 'Low Stock', type: 'boolean', sortable: true }
    ];
    lowStockColumns: TableColumn[] = [
        { field: 'itemCode', header: 'Item Code', sortable: true },
        { field: 'itemNameEn', header: 'Item', sortable: true },
        { field: 'warehouseNameEn', header: 'Warehouse', sortable: true },
        { field: 'currentQuantity', header: 'On Hand', sortable: true },
        { field: 'minQuantity', header: 'Min Qty', sortable: true },
        { field: 'shortage', header: 'Shortage', sortable: true }
    ];
    private reportsService = inject(ReportsService);
    private messageService = inject(MessageService);
    ngOnInit(): void { this.load(); }
    load(): void {
        this.loading = true;
        this.loadingLowStock = true;
        this.reportsService.getStockReport().subscribe({
            next: rows => { this.rows = rows ?? []; this.totalUnits = this.rows.reduce((sum, row) => sum + (row.currentQuantity || 0), 0); this.loading = false; },
            error: err => { this.rows = []; this.totalUnits = 0; this.loading = false; this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load inventory report.' }); }
        });
        this.reportsService.getLowStockReport().subscribe({
            next: rows => { this.lowStockRows = rows ?? []; this.totalShortage = this.lowStockRows.reduce((sum, row) => sum + (row.shortage || 0), 0); this.loadingLowStock = false; },
            error: () => { this.lowStockRows = []; this.totalShortage = 0; this.loadingLowStock = false; }
        });
    }
}
