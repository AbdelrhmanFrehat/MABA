import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeTableModule } from 'primeng/treetable';
import { TranslateModule } from '@ngx-translate/core';
import { AccountingApiService } from '../../../shared/services/accounting-api.service';
import { Account } from '../../../shared/models/accounting.model';

@Component({
    selector: 'app-chart-of-accounts',
    standalone: true,
    imports: [CommonModule, TreeTableModule, TranslateModule],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.accounting.chartOfAccounts' | translate }}</h1><p-treeTable [value]="nodes"><ng-template pTemplate="header"><tr><th>{{ 'common.code' | translate }}</th><th>{{ 'common.nameEn' | translate }}</th><th>{{ 'admin.accounting.accountType' | translate }}</th><th>{{ 'common.status' | translate }}</th></tr></ng-template><ng-template pTemplate="body" let-rowNode let-rowData="rowData"><tr><td><p-treeTableToggler [rowNode]="rowNode"></p-treeTableToggler>{{ rowData.accountNumber }}</td><td>{{ rowData.nameEn }}</td><td>{{ rowData.accountTypeName }}</td><td>{{ rowData.isActive ? ('common.active' | translate) : ('common.inactive' | translate) }}</td></tr></ng-template></p-treeTable></div>`
})
export class ChartOfAccountsComponent implements OnInit {
    nodes: any[] = [];
    private api = inject(AccountingApiService);
    ngOnInit(): void { this.api.getAccountTree().subscribe(accounts => this.nodes = this.toNodes(accounts)); }
    private toNodes(accounts: Account[]): any[] { return accounts.map(a => ({ data: a, children: this.toNodes(a.children ?? []) })); }
}
