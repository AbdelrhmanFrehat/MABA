import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { AccountingApiService } from '../../../shared/services/accounting-api.service';
import { JournalEntry } from '../../../shared/models/accounting.model';

@Component({
    selector: 'app-journal-entries-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><div class="flex justify-between items-center mb-4"><h1 class="text-2xl font-bold m-0">{{ 'admin.accounting.journalEntries' | translate }}</h1><p-button [label]="'admin.accounting.newJournalEntry' | translate" icon="pi pi-plus" routerLink="/admin/accounting/journal-entries/new"></p-button></div><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class JournalEntriesListComponent implements OnInit {
    rows: JournalEntry[] = [];
    columns: TableColumn[] = [
        { field: 'entryNumber', headerKey: 'admin.accounting.entryNumber' },
        { field: 'entryDate', headerKey: 'common.date', type: 'date' },
        { field: 'journalEntryTypeName', headerKey: 'admin.accounting.journalEntryType' },
        { field: 'description', headerKey: 'common.description' },
        { field: 'isPosted', headerKey: 'admin.common.posted', type: 'boolean' }
    ];
    private api = inject(AccountingApiService);
    ngOnInit(): void { this.api.getJournalEntries().subscribe(x => this.rows = x); }
}
