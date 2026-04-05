import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { Customer, AccountStatement } from '../../../../shared/models/crm.model';

@Component({
    selector: 'app-customer-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TabsModule, TableModule, TagModule, ToastModule, TranslateModule],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4" *ngIf="customer">
            <div class="flex items-center gap-3 mb-4">
                <p-button icon="pi pi-arrow-left" [rounded]="true" [outlined]="true" routerLink="/admin/crm/customers" size="small"></p-button>
                <h1 class="text-2xl font-bold m-0">{{ customer.nameEn }} ({{ customer.code }})</h1>
                <p-tag [value]="customer.isActive ? ('common.active' | translate) : ('common.inactive' | translate)" [severity]="customer.isActive ? 'success' : 'danger'"></p-tag>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.balance' | translate }}</div>
                    <div class="text-2xl font-bold">{{ customer.balance | currency:customer.currency }}</div>
                </div>
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.creditLimit' | translate }}</div>
                    <div class="text-2xl font-bold">{{ customer.creditLimit | currency:customer.currency }}</div>
                </div>
                <div class="surface-card p-4 border-round shadow-1">
                    <div class="text-500 mb-2">{{ 'admin.crm.type' | translate }}</div>
                    <div class="text-2xl font-bold">{{ customer.customerTypeName || '-' }}</div>
                </div>
            </div>

            <div class="surface-card p-4 border-round shadow-1 mb-4">
                <h3>{{ 'admin.crm.contactInfo' | translate }}</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                    <div><strong>{{ 'common.email' | translate }}:</strong> {{ customer.email || '-' }}</div>
                    <div><strong>{{ 'common.phone' | translate }}:</strong> {{ customer.phone || '-' }}</div>
                    <div><strong>{{ 'admin.crm.taxNumber' | translate }}:</strong> {{ customer.taxNumber || '-' }}</div>
                    <div><strong>{{ 'admin.crm.address' | translate }}:</strong> {{ customer.addressLine1 || '-' }}, {{ customer.city || '' }} {{ customer.country || '' }}</div>
                </div>
            </div>

            <div class="surface-card p-4 border-round shadow-1">
                <div class="flex justify-between items-center mb-3">
                    <h3 class="m-0">{{ 'admin.crm.statement' | translate }}</h3>
                    <p-button [label]="'common.refresh' | translate" icon="pi pi-refresh" [outlined]="true" size="small" (click)="loadStatement()"></p-button>
                </div>
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
export class CustomerDetailComponent implements OnInit {
    customer: Customer | null = null;
    statement: AccountStatement | null = null;

    private route = inject(ActivatedRoute);
    private crmApi = inject(CrmApiService);

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.crmApi.getCustomerById(id).subscribe(c => this.customer = c);
            this.loadStatement(id);
        }
    }

    loadStatement(id?: string) {
        const customerId = id || this.customer?.id;
        if (customerId) {
            this.crmApi.getCustomerStatement(customerId).subscribe(s => this.statement = s);
        }
    }
}
