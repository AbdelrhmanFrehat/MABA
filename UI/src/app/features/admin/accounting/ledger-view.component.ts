import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { AccountingApiService } from '../../../shared/services/accounting-api.service';
import { Account, AccountLedgerEntry } from '../../../shared/models/accounting.model';

@Component({
    selector: 'app-ledger-view',
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.accounting.ledger' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Pick an account to inspect movements, running balance, and the source document behind each posting.</p>
            </div>

            <div class="surface-card border-1 surface-border border-round p-3 mb-4">
                <label class="block font-medium mb-2">Account</label>
                <p-select
                    [options]="accounts"
                    [(ngModel)]="selectedAccountId"
                    optionLabel="nameEn"
                    optionValue="id"
                    placeholder="Select account"
                    [filter]="true"
                    filterBy="nameEn,nameAr,accountNumber"
                    styleClass="w-full"
                    (onChange)="loadLedger()"
                ></p-select>
            </div>

            <div class="grid mb-4" *ngIf="selectedAccount">
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Account</span><strong>{{ selectedAccount.accountNumber }} - {{ selectedAccount.nameEn }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Total Debit</span><strong>{{ totalDebit | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Total Credit</span><strong>{{ totalCredit | currency:'ILS' }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Account Ledger'"
                [globalFilterFields]="['entryNumber', 'description', 'sourceDocumentType', 'sourceDocumentNumber']"
            ></app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class LedgerViewComponent implements OnInit {
    accounts: Account[] = [];
    rows: AccountLedgerEntry[] = [];
    selectedAccountId: string | null = null;
    loading = false;
    totalDebit = 0;
    totalCredit = 0;

    columns: TableColumn[] = [
        { field: 'entryDate', header: 'Date', type: 'date', sortable: true },
        { field: 'entryNumber', header: 'Entry #', sortable: true },
        { field: 'description', header: 'Description', sortable: true },
        { field: 'sourceDocumentType', header: 'Source Type', sortable: true },
        { field: 'sourceDocumentNumber', header: 'Source #', sortable: true },
        { field: 'debit', header: 'Debit', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'credit', header: 'Credit', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'runningBalance', header: 'Balance', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    private api = inject(AccountingApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    get selectedAccount(): Account | undefined {
        return this.accounts.find(x => x.id === this.selectedAccountId);
    }

    ngOnInit(): void {
        this.loadAccounts();
    }

    loadAccounts(): void {
        this.api.getAccounts().subscribe({
            next: rows => {
                this.accounts = rows ?? [];
                this.selectedAccountId = this.accounts[0]?.id || null;
                this.loadLedger();
            },
            error: () => {
                this.accounts = [];
                this.showError('Failed to load accounts.');
            }
        });
    }

    loadLedger(): void {
        if (!this.selectedAccountId) {
            this.rows = [];
            this.totalDebit = 0;
            this.totalCredit = 0;
            return;
        }

        this.loading = true;
        this.api.getAccountLedger(this.selectedAccountId).subscribe({
            next: result => {
                this.rows = result?.lines ?? [];
                this.totalDebit = this.rows.reduce((sum, row: any) => sum + (row.debit || 0), 0);
                this.totalCredit = this.rows.reduce((sum, row: any) => sum + (row.credit || 0), 0);
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.totalDebit = 0;
                this.totalCredit = 0;
                this.loading = false;
                this.showError('Failed to load account ledger.');
            }
        });
    }

    private showError(detail: string): void {
        this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail });
    }
}
