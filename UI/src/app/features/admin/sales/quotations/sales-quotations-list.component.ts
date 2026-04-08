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
import { SalesQuotation } from '../../../../shared/models/sales.model';

@Component({
    selector: 'app-sales-quotations-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.sales.quotations.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Prepare supplier-ready offers, follow up on sent quotations, and convert winners into sales orders.</p>
                </div>
                <p-button [label]="'admin.sales.quotations.new' | translate" icon="pi pi-plus" routerLink="/admin/sales/quotations/new"></p-button>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Quotations</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Open Value</span><strong>{{ totalValue | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Sent</span><strong>{{ sentCount }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Expiring Soon</span><strong>{{ expiringSoonCount }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Sales Quotations'"
                [globalFilterFields]="['quotationNumber', 'customerName', 'statusName']"
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
export class SalesQuotationsListComponent implements OnInit {
    rows: SalesQuotation[] = [];
    loading = false;
    totalValue = 0;
    sentCount = 0;
    expiringSoonCount = 0;

    columns: TableColumn[] = [
        { field: 'quotationNumber', header: 'Quotation #', sortable: true },
        { field: 'customerName', header: 'Customer', sortable: true },
        { field: 'quotationDate', header: 'Date', type: 'date', sortable: true },
        { field: 'validUntil', header: 'Valid Until', type: 'custom' },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltip: 'Edit', action: 'edit' },
        { icon: 'pi pi-send', tooltip: 'Send', action: 'send', severity: 'info' },
        { icon: 'pi pi-arrow-right', tooltip: 'Convert to Order', action: 'convert', severity: 'success' }
    ];

    private salesApi = inject(SalesApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.salesApi.getSalesQuotations().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalValue = this.rows.reduce((sum, row) => sum + (row.total || 0), 0);
                this.sentCount = this.rows.filter(row => this.getStatusKey(row).includes('sent')).length;
                this.expiringSoonCount = this.rows.filter(row => this.isExpiringSoon(row.validUntil)).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalValue = 0;
                this.sentCount = 0;
                this.expiringSoonCount = 0;
                this.loading = false;
                this.showError('Failed to load sales quotations.');
            }
        });
    }

    handleAction(event: { action: string; data: SalesQuotation }): void {
        if (event.action === 'edit') {
            this.router.navigate(['/admin/sales/quotations', event.data.id, 'edit']);
            return;
        }

        if (event.action === 'send') {
            this.salesApi.sendSalesQuotation(event.data.id).subscribe({
                next: () => { this.showSuccess('Quotation marked as sent.'); this.load(); },
                error: () => this.showError('Failed to send quotation.')
            });
            return;
        }

        this.salesApi.convertQuotationToOrder(event.data.id).subscribe({
            next: order => { this.showSuccess('Quotation converted to sales order.'); this.router.navigate(['/admin/sales/orders', order.id, 'edit']); },
            error: () => this.showError('Failed to convert quotation to order.')
        });
    }

    private getStatusKey(row: SalesQuotation): string { return (row.statusLookupId || '').toLowerCase().replace(/-/g, '_'); }

    private isExpiringSoon(value?: string): boolean {
        if (!value) return false;
        const diff = new Date(value).getTime() - new Date().getTime();
        return diff >= 0 && diff <= 3 * 24 * 60 * 60 * 1000;
    }

    private showSuccess(detail: string): void { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail }); }
    private showError(detail: string): void { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail }); }
}
