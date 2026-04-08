import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge';
import { PaymentVoucherApiService } from '../../../shared/services/payment-voucher-api.service';
import { BizInstallment, BizPaymentPlan } from '../../../shared/models/biz-payment.model';

interface InstallmentRow extends BizInstallment {
    customerName?: string;
    salesInvoiceNumber?: string;
    remainingAmount: number;
}

@Component({
    selector: 'app-installments-page',
    standalone: true,
    imports: [CommonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.payments.installments' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Follow upcoming and overdue installments across all active payment plans in one place.</p>
            </div>

            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Installments</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Outstanding</span><strong>{{ outstandingAmount | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Overdue</span><strong>{{ overdueCount }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Paid</span><strong>{{ paidCount }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Installments'"
                [globalFilterFields]="['customerName', 'salesInvoiceNumber', 'statusName']"
            >
                <ng-template #customColumn let-row let-column="column">
                    <app-status-badge *ngIf="column.field === 'statusName'" [label]="row.statusName" [color]="statusColor(row.statusLookupId)"></app-status-badge>
                </ng-template>
            </app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class InstallmentsPageComponent implements OnInit {
    rows: InstallmentRow[] = [];
    loading = false;
    outstandingAmount = 0;
    overdueCount = 0;
    paidCount = 0;

    columns: TableColumn[] = [
        { field: 'salesInvoiceNumber', header: 'Invoice #', sortable: true },
        { field: 'customerName', header: 'Customer', sortable: true },
        { field: 'seq', header: 'Installment', sortable: true },
        { field: 'dueDate', header: 'Due Date', type: 'date', sortable: true },
        { field: 'amount', header: 'Amount', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'amountPaid', header: 'Paid', type: 'currency', currencyCode: 'ILS', sortable: true },
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
            next: plans => {
                this.rows = this.flatten(plans ?? []);
                this.outstandingAmount = this.rows.reduce((sum, row) => sum + row.remainingAmount, 0);
                this.overdueCount = this.rows.filter(row => this.isOverdue(row)).length;
                this.paidCount = this.rows.filter(row => row.remainingAmount <= 0).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.outstandingAmount = 0;
                this.overdueCount = 0;
                this.paidCount = 0;
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: 'Failed to load installments.' });
            }
        });
    }

    private flatten(plans: BizPaymentPlan[]): InstallmentRow[] {
        return plans.flatMap(plan => (plan.installments || []).map(installment => ({
            ...installment,
            customerName: plan.customerName,
            salesInvoiceNumber: plan.salesInvoiceNumber,
            remainingAmount: Math.max((installment.amount || 0) - (installment.amountPaid || 0), 0)
        })));
    }

    private isOverdue(row: InstallmentRow): boolean {
        return row.remainingAmount > 0 && new Date(row.dueDate).getTime() < Date.now();
    }

    statusColor(status?: string): string {
        const key = (status || '').toLowerCase();
        if (key.includes('paid') || key.includes('completed')) return '#22c55e';
        if (key.includes('overdue')) return '#ef4444';
        if (key.includes('pending')) return '#f59e0b';
        return '#64748b';
    }
}
