import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge';
import { PurchasingApiService } from '../../../../shared/services/purchasing-api.service';
import { PurchaseQuotation } from '../../../../shared/models/purchasing.model';

@Component({
    selector: 'app-purchase-quotations-list',
    standalone: true,
    imports: [CommonModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.purchasing.quotations.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Monitor supplier quotations, review expiry dates, and convert accepted offers into purchase orders.</p>
                </div>
                <p-button label="Go to Orders" icon="pi pi-arrow-right" severity="secondary" [outlined]="true" (onClick)="goToOrders()"></p-button>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Quotations</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Quoted Value</span><strong>{{ totalValue | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Expiring Soon</span><strong>{{ expiringSoonCount }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Purchase Quotations'"
                [globalFilterFields]="['quotationNumber', 'supplierName', 'statusName']"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    <ng-container [ngSwitch]="column.field">
                        <app-status-badge *ngSwitchCase="'statusName'" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                        <span *ngSwitchCase="'validUntil'">{{ row.validUntil ? (row.validUntil | date:'mediumDate') : '-' }}</span>
                    </ng-container>
                </ng-template>
            </app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class PurchaseQuotationsListComponent implements OnInit {
    rows: PurchaseQuotation[] = [];
    loading = false;
    totalValue = 0;
    expiringSoonCount = 0;

    columns: TableColumn[] = [
        { field: 'quotationNumber', header: 'Quotation #', sortable: true },
        { field: 'supplierName', header: 'Supplier', sortable: true },
        { field: 'quotationDate', header: 'Date', type: 'date', sortable: true },
        { field: 'validUntil', header: 'Valid Until', type: 'custom' },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-arrow-right', tooltip: 'Convert to Order', action: 'convert', severity: 'success' }
    ];

    private purchasingApi = inject(PurchasingApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.purchasingApi.getPurchaseQuotations().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalValue = this.rows.reduce((sum, row) => sum + (row.total || 0), 0);
                this.expiringSoonCount = this.rows.filter(row => this.isExpiringSoon(row.validUntil)).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalValue = 0;
                this.expiringSoonCount = 0;
                this.loading = false;
                this.showError('Failed to load purchase quotations.');
            }
        });
    }

    handleAction(event: { action: string; data: PurchaseQuotation }): void {
        this.purchasingApi.convertQuotationToOrder(event.data.id).subscribe({
            next: order => {
                this.showSuccess('Quotation converted to purchase order.');
                this.router.navigate(['/admin/purchasing/orders'], { queryParams: { highlight: order.id } });
            },
            error: () => this.showError('Failed to convert quotation to purchase order.')
        });
    }

    goToOrders(): void { this.router.navigate(['/admin/purchasing/orders']); }

    private isExpiringSoon(value?: string): boolean {
        if (!value) return false;
        const diff = new Date(value).getTime() - new Date().getTime();
        return diff >= 0 && diff <= 3 * 24 * 60 * 60 * 1000;
    }

    private showSuccess(detail: string): void { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail }); }
    private showError(detail: string): void { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail }); }
}
