import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { AccountingApiService } from '../../../shared/services/accounting-api.service';
import { TrialBalanceRow } from '../../../shared/models/accounting.model';

@Component({
    selector: 'app-trial-balance-report',
    standalone: true,
    imports: [CommonModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.reports.trialBalance' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Validate debit and credit equality across all postable accounts before period close.</p>
            </div>
            <div class="grid mb-4">
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Accounts</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Total Debit</span><strong>{{ totalDebit | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Total Credit</span><strong>{{ totalCredit | currency:'ILS' }}</strong></div></div>
            </div>
            <app-data-table [data]="rows" [columns]="columns" [loading]="loading" [showAddButton]="false" [showDeleteSelected]="false" [title]="'Trial Balance'" [globalFilterFields]="['accountNumber', 'accountName', 'accountType']"></app-data-table>
        </div>
    `,
    styles: [`.summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; } .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }`]
})
export class TrialBalanceReportComponent implements OnInit {
    rows: TrialBalanceRow[] = [];
    loading = false;
    totalDebit = 0;
    totalCredit = 0;
    columns: TableColumn[] = [
        { field: 'accountNumber', header: 'Account #', sortable: true },
        { field: 'accountName', header: 'Account Name', sortable: true },
        { field: 'accountType', header: 'Type', sortable: true },
        { field: 'debit', header: 'Debit', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'credit', header: 'Credit', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'balance', header: 'Balance', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];
    private api = inject(AccountingApiService);
    private messageService = inject(MessageService);
    ngOnInit(): void { this.load(); }
    load(): void {
        this.loading = true;
        this.api.getTrialBalance().subscribe({
            next: rows => { this.rows = rows ?? []; this.totalDebit = this.rows.reduce((sum, row) => sum + (row.debit || 0), 0); this.totalCredit = this.rows.reduce((sum, row) => sum + (row.credit || 0), 0); this.loading = false; },
            error: err => { this.rows = []; this.totalDebit = 0; this.totalCredit = 0; this.loading = false; this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load trial balance.' }); }
        });
    }
}
