import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge';
import { PaymentVoucherApiService } from '../../../shared/services/payment-voucher-api.service';
import { BizPaymentPlan } from '../../../shared/models/biz-payment.model';

@Component({
    selector: 'app-payment-plans-page',
    standalone: true,
    imports: [CommonModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.payments.plans' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Review financed invoices, remaining balances, and payment-plan health before the next installment cycle.</p>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Plans</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Financed Amount</span><strong>{{ totalAmount | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Remaining</span><strong>{{ remainingAmount | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Installments</span><strong>{{ totalInstallments }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Payment Plans'"
                [globalFilterFields]="['salesInvoiceNumber', 'customerName', 'frequencyName', 'statusName']"
            >
                <ng-template #customColumn let-row let-column="column">
                    <app-status-badge *ngIf="column.field === 'statusName'" [label]="row.statusName" [color]="statusColor(row)"></app-status-badge>
                </ng-template>
            </app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class PaymentPlansPageComponent implements OnInit {
    rows: BizPaymentPlan[] = [];
    loading = false;
    totalAmount = 0;
    remainingAmount = 0;
    totalInstallments = 0;

    columns: TableColumn[] = [
        { field: 'salesInvoiceNumber', header: 'Invoice #', sortable: true },
        { field: 'customerName', header: 'Customer', sortable: true },
        { field: 'installmentsCount', header: 'Installments', sortable: true },
        { field: 'frequencyName', header: 'Frequency', sortable: true },
        { field: 'totalAmount', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'remainingAmount', header: 'Remaining', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'statusName', header: 'Status', type: 'custom' }
    ];

    private api = inject(PaymentVoucherApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.api.getPaymentPlans().subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalAmount = this.rows.reduce((sum, row) => sum + (row.totalAmount || 0), 0);
                this.remainingAmount = this.rows.reduce((sum, row) => sum + (row.remainingAmount || 0), 0);
                this.totalInstallments = this.rows.reduce((sum, row) => sum + (row.installmentsCount || 0), 0);
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalAmount = 0;
                this.remainingAmount = 0;
                this.totalInstallments = 0;
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: 'Failed to load payment plans.' });
            }
        });
    }

    statusColor(row: BizPaymentPlan): string {
        const key = (row.statusLookupId || '').toLowerCase();
        if (key.includes('completed')) return '#22c55e';
        if (key.includes('overdue')) return '#ef4444';
        if (key.includes('active')) return '#0ea5e9';
        return '#64748b';
    }
}
