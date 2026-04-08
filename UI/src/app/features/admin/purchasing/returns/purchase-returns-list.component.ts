import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge';
import { PurchasingApiService } from '../../../../shared/services/purchasing-api.service';
import { PurchaseReturn } from '../../../../shared/models/purchasing.model';

@Component({
    selector: 'app-purchase-returns-list',
    standalone: true,
    imports: [CommonModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.purchasing.returns.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Track supplier returns, their linked invoice, and whether stock is deducted during completion.</p>
                </div>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Returns</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Return Value</span><strong>{{ totalValue | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Inventory Deducted</span><strong>{{ deductCount }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Awaiting Completion</span><strong>{{ pendingCount }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Purchase Returns'"
                [globalFilterFields]="['returnNumber', 'supplierName', 'purchaseInvoiceNumber', 'statusName']"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    <ng-container [ngSwitch]="column.field">
                        <app-status-badge *ngSwitchCase="'statusName'" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                        <span *ngSwitchCase="'deductFromInventory'">{{ row.deductFromInventory ? 'Yes' : 'No' }}</span>
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
export class PurchaseReturnsListComponent implements OnInit {
    rows: PurchaseReturn[] = [];
    loading = false;
    totalValue = 0;
    deductCount = 0;
    pendingCount = 0;

    columns: TableColumn[] = [
        { field: 'returnNumber', header: 'Return #', sortable: true },
        { field: 'supplierName', header: 'Supplier', sortable: true },
        { field: 'purchaseInvoiceNumber', header: 'Invoice #', sortable: true },
        { field: 'returnDate', header: 'Date', type: 'date', sortable: true },
        { field: 'deductFromInventory', header: 'Deduct Stock', type: 'custom' },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-check', tooltip: 'Approve', action: 'approve', severity: 'success', visible: row => this.getStatusKey(row) === 'draft' },
        { icon: 'pi pi-verified', tooltip: 'Complete', action: 'complete', severity: 'info', visible: row => this.getStatusKey(row) === 'approved' }
    ];

    private purchasingApi = inject(PurchasingApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.purchasingApi.getPurchaseReturns().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalValue = this.rows.reduce((sum, row) => sum + (row.total || 0), 0);
                this.deductCount = this.rows.filter(row => row.deductFromInventory).length;
                this.pendingCount = this.rows.filter(row => ['draft', 'approved'].includes(this.getStatusKey(row))).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalValue = 0;
                this.deductCount = 0;
                this.pendingCount = 0;
                this.loading = false;
                this.showError('Failed to load purchase returns.');
            }
        });
    }

    handleAction(event: { action: string; data: PurchaseReturn }): void {
        const request = event.action === 'approve'
            ? this.purchasingApi.approvePurchaseReturn(event.data.id)
            : this.purchasingApi.completePurchaseReturn(event.data.id);

        request.subscribe({
            next: () => { this.showSuccess(event.action === 'approve' ? 'Purchase return approved.' : 'Purchase return completed.'); this.load(); },
            error: () => this.showError('Failed to update purchase return.')
        });
    }

    private getStatusKey(row: PurchaseReturn): string { return (row.statusLookupId || '').toLowerCase().replace(/-/g, '_'); }
    private showSuccess(detail: string): void { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail }); }
    private showError(detail: string): void { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail }); }
}
