import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge';
import { SalesApiService } from '../../../../shared/services/sales-api.service';
import { SalesReturn } from '../../../../shared/models/sales.model';

@Component({
    selector: 'app-sales-returns-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.sales.returns.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.sales.returns.subtitle' | translate }}</p>
                </div>
                <p-button [label]="'admin.sales.returns.new' | translate" icon="pi pi-plus" routerLink="/admin/sales/returns/new"></p-button>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Returns</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Returned Value</span><strong>{{ totalValue | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Restocked</span><strong>{{ restockedCount }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Pending Completion</span><strong>{{ pendingCount }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Sales Returns'"
                [globalFilterFields]="['returnNumber', 'customerName', 'salesInvoiceNumber', 'statusName']"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    <ng-container [ngSwitch]="column.field">
                        <app-status-badge *ngSwitchCase="'statusName'" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                        <span *ngSwitchCase="'restockItems'">{{ row.restockItems ? 'Yes' : 'No' }}</span>
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
export class SalesReturnsListComponent implements OnInit {
    rows: SalesReturn[] = [];
    loading = false;
    totalValue = 0;
    restockedCount = 0;
    pendingCount = 0;

    columns: TableColumn[] = [
        { field: 'returnNumber', header: 'Return #', sortable: true },
        { field: 'customerName', header: 'Customer', sortable: true },
        { field: 'salesInvoiceNumber', header: 'Invoice #', sortable: true },
        { field: 'returnDate', header: 'Date', type: 'date', sortable: true },
        { field: 'restockItems', header: 'Restock', type: 'custom' },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltip: 'Edit', action: 'edit' },
        { icon: 'pi pi-check', tooltip: 'Approve', action: 'approve', severity: 'success', visible: row => this.getStatusKey(row) === 'draft' },
        { icon: 'pi pi-verified', tooltip: 'Complete', action: 'complete', severity: 'info', visible: row => this.getStatusKey(row) === 'approved' }
    ];

    private salesApi = inject(SalesApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.salesApi.getSalesReturns().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalValue = this.rows.reduce((sum, row) => sum + (row.total || 0), 0);
                this.restockedCount = this.rows.filter(row => row.restockItems).length;
                this.pendingCount = this.rows.filter(row => ['draft', 'approved'].includes(this.getStatusKey(row))).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalValue = 0;
                this.restockedCount = 0;
                this.pendingCount = 0;
                this.loading = false;
                this.showError('Failed to load sales returns.');
            }
        });
    }

    handleAction(event: { action: string; data: SalesReturn }): void {
        if (event.action === 'edit') {
            this.router.navigate(['/admin/sales/returns', event.data.id, 'edit']);
            return;
        }

        const request = event.action === 'approve'
            ? this.salesApi.approveSalesReturn(event.data.id)
            : this.salesApi.completeSalesReturn(event.data.id);

        request.subscribe({
            next: () => { this.showSuccess(event.action === 'approve' ? 'Sales return approved.' : 'Sales return completed.'); this.load(); },
            error: () => this.showError('Failed to update sales return.')
        });
    }

    private getStatusKey(row: SalesReturn): string { return (row.statusLookupId || '').toLowerCase().replace(/-/g, '_'); }
    private showSuccess(detail: string): void { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail }); }
    private showError(detail: string): void { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail }); }
}
