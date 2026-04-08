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
import { PurchaseInvoice } from '../../../../shared/models/purchasing.model';

@Component({
    selector: 'app-purchase-invoices-list',
    standalone: true,
    imports: [CommonModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.purchasing.invoices.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Track supplier invoices, due dates, and the unpaid balance waiting for settlement.</p>
                </div>
                <p-button label="Payments" icon="pi pi-wallet" severity="secondary" [outlined]="true" (onClick)="openPayments()"></p-button>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Invoices</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Total Billed</span><strong>{{ totalValue | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Paid</span><strong>{{ totalPaid | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Outstanding</span><strong>{{ totalDue | currency:'ILS' }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Purchase Invoices'"
                [globalFilterFields]="['invoiceNumber', 'supplierName', 'purchaseOrderNumber', 'statusName']"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    <app-status-badge *ngIf="column.field === 'statusName'" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                </ng-template>
            </app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class PurchaseInvoicesListComponent implements OnInit {
    rows: PurchaseInvoice[] = [];
    loading = false;
    totalValue = 0;
    totalPaid = 0;
    totalDue = 0;

    columns: TableColumn[] = [
        { field: 'invoiceNumber', header: 'Invoice #', sortable: true },
        { field: 'supplierName', header: 'Supplier', sortable: true },
        { field: 'purchaseOrderNumber', header: 'PO #', sortable: true },
        { field: 'invoiceDate', header: 'Invoice Date', type: 'date', sortable: true },
        { field: 'dueDate', header: 'Due Date', type: 'date', sortable: true },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'amountDue', header: 'Amount Due', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-wallet', tooltip: 'Pay', action: 'pay', severity: 'info' },
        { icon: 'pi pi-times', tooltip: 'Cancel Invoice', action: 'cancel', severity: 'danger' }
    ];

    private purchasingApi = inject(PurchasingApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.purchasingApi.getPurchaseInvoices().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalValue = this.rows.reduce((sum, row) => sum + (row.total || 0), 0);
                this.totalPaid = this.rows.reduce((sum, row) => sum + (row.amountPaid || 0), 0);
                this.totalDue = this.rows.reduce((sum, row) => sum + (row.amountDue || 0), 0);
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalValue = 0;
                this.totalPaid = 0;
                this.totalDue = 0;
                this.loading = false;
                this.showError('Failed to load purchase invoices.');
            }
        });
    }

    handleAction(event: { action: string; data: PurchaseInvoice }): void {
        if (event.action === 'pay') {
            this.router.navigate(['/admin/payments/new'], { queryParams: { purchaseInvoiceId: event.data.id } });
            return;
        }

        this.purchasingApi.cancelPurchaseInvoice(event.data.id).subscribe({
            next: () => { this.showSuccess('Purchase invoice cancelled.'); this.load(); },
            error: () => this.showError('Failed to cancel purchase invoice.')
        });
    }

    openPayments(): void { this.router.navigate(['/admin/payments']); }
    private showSuccess(detail: string): void { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail }); }
    private showError(detail: string): void { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail }); }
}
