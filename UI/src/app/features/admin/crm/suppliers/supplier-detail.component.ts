import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { Supplier, AccountStatement } from '../../../../shared/models/crm.model';

@Component({
    selector: 'app-supplier-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TableModule, TagModule, ToastModule, TranslateModule],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4" *ngIf="supplier">
            <div class="flex items-center gap-3 mb-4">
                <p-button icon="pi pi-arrow-left" [rounded]="true" [outlined]="true" routerLink="/admin/crm/suppliers" size="small"></p-button>
                <h1 class="text-2xl font-bold m-0">{{ supplier.nameEn }} ({{ supplier.code }})</h1>
                <p-tag [value]="supplier.isActive ? ('common.active' | translate) : ('common.inactive' | translate)" [severity]="supplier.isActive ? 'success' : 'danger'"></p-tag>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.balance' | translate }}</div>
                    <div class="text-2xl font-bold">{{ supplier.balance | currency:supplier.currency }}</div>
                </div>
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.creditLimit' | translate }}</div>
                    <div class="text-2xl font-bold">{{ supplier.creditLimit | currency:supplier.currency }}</div>
                </div>
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.paymentTermDays' | translate }}</div>
                    <div class="text-2xl font-bold">{{ supplier.paymentTermDays || 30 }} {{ 'common.days' | translate }}</div>
                </div>
            </div>
            <div class="surface-card p-4 border-round shadow-1">
                <h3>{{ 'admin.crm.statement' | translate }}</h3>
                <p-table [value]="statement?.entries || []" [tableStyle]="{ 'min-width': '50rem' }" styleClass="p-datatable-sm">
                    <ng-template #header>
                        <tr>
                            <th>{{ 'common.date' | translate }}</th>
                            <th>{{ 'common.documentNumber' | translate }}</th>
                            <th>{{ 'common.type' | translate }}</th>
                            <th>{{ 'common.description' | translate }}</th>
                            <th style="text-align: right">{{ 'common.debit' | translate }}</th>
                            <th style="text-align: right">{{ 'common.credit' | translate }}</th>
                            <th style="text-align: right">{{ 'common.balance' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template #body let-entry>
                        <tr>
                            <td>{{ entry.date | date:'mediumDate' }}</td>
                            <td>{{ entry.documentNumber }}</td>
                            <td>{{ entry.documentType }}</td>
                            <td>{{ entry.description }}</td>
                            <td style="text-align: right">{{ entry.debit | number:'1.2-2' }}</td>
                            <td style="text-align: right">{{ entry.credit | number:'1.2-2' }}</td>
                            <td style="text-align: right; font-weight: bold">{{ entry.balance | number:'1.2-2' }}</td>
                        </tr>
                    </ng-template>
                    <ng-template #emptymessage>
                        <tr><td colspan="7" style="text-align: center; padding: 2rem;">{{ 'common.noDataFound' | translate }}</td></tr>
                    </ng-template>
                </p-table>
            </div>
        </div>
    `
})
export class SupplierDetailComponent implements OnInit {
    supplier: Supplier | null = null;
    statement: AccountStatement | null = null;

    private route = inject(ActivatedRoute);
    private crmApi = inject(CrmApiService);

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.crmApi.getSupplierById(id).subscribe(s => this.supplier = s);
            this.crmApi.getSupplierStatement(id).subscribe(s => this.statement = s);
        }
    }
}
