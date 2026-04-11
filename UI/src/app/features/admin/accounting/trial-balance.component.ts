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
        { field: 'accountCode', headerKey: 'common.code', sortable: true },
        { field: 'accountNameEn', headerKey: 'common.name', sortable: true },
        { field: 'accountTypeName', headerKey: 'admin.accounting.accountType' },
        { field: 'totalDebit', headerKey: 'common.debit', type: 'currency', currencyCode: 'ILS' },
        { field: 'totalCredit', headerKey: 'common.credit', type: 'currency', currencyCode: 'ILS' },
        { field: 'netBalance', headerKey: 'common.balance', type: 'currency', currencyCode: 'ILS' }
    ];
    private api = inject(AccountingApiService);
    ngOnInit(): void { this.api.getTrialBalance().subscribe(x => this.rows = x?.rows ?? []); }
}
