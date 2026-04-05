import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { AccountingApiService } from '../../../shared/services/accounting-api.service';
import { TrialBalanceRow } from '../../../shared/models/accounting.model';

@Component({
    selector: 'app-trial-balance',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.accounting.trialBalance' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class TrialBalanceComponent implements OnInit {
    rows: TrialBalanceRow[] = [];
    columns: TableColumn[] = [
        { field: 'accountNumber', headerKey: 'common.code' },
        { field: 'accountName', headerKey: 'common.name' },
        { field: 'accountType', headerKey: 'admin.accounting.accountType' },
        { field: 'debit', headerKey: 'common.debit', type: 'currency', currencyCode: 'ILS' },
        { field: 'credit', headerKey: 'common.credit', type: 'currency', currencyCode: 'ILS' },
        { field: 'balance', headerKey: 'common.balance', type: 'currency', currencyCode: 'ILS' }
    ];
    private api = inject(AccountingApiService);
    ngOnInit(): void { this.api.getTrialBalance().subscribe(x => this.rows = x); }
}
