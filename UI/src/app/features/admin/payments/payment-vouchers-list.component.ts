import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge';
import { PaymentVoucherApiService } from '../../../shared/services/payment-voucher-api.service';
import { PaymentVoucher } from '../../../shared/models/biz-payment.model';

@Component({
    selector: 'app-payment-vouchers-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.payments.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.payments.subtitle' | translate }}</p>
                </div>
                <p-button icon="pi pi-plus" [label]="'admin.payments.new' | translate" routerLink="/admin/payments/new"></p-button>
            </div>
            <app-data-table
                [data]="vouchers"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [title]="'admin.payments.title' | translate"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    @if (column.field === 'paymentStatusName') {
                        <app-status-badge lookupTypeKey="payment_status" [valueId]="row.paymentStatusLookupId" [label]="row.paymentStatusName" [color]="row.paymentStatusColor"></app-status-badge>
                    }
                </ng-template>
            </app-data-table>
        </div>
    `
})
export class PaymentVouchersListComponent implements OnInit {
    vouchers: PaymentVoucher[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'voucherNumber', headerKey: 'admin.payments.voucherNumber', sortable: true },
        { field: 'paymentDate', headerKey: 'common.date', type: 'date', sortable: true },
        { field: 'paymentDirectionName', headerKey: 'admin.payments.direction', sortable: true },
        { field: 'paymentMethodName', headerKey: 'admin.payments.method', sortable: true },
        { field: 'paymentStatusName', headerKey: 'common.status', type: 'custom' },
        { field: 'amount', headerKey: 'common.amount', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-check', tooltipKey: 'admin.common.confirm', action: 'confirm', severity: 'success' },
        { icon: 'pi pi-times', tooltipKey: 'admin.common.void', action: 'void', severity: 'danger' }
    ];

    private api = inject(PaymentVoucherApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading = true;
        this.api.getPaymentVouchers().subscribe({
            next: data => {
                this.vouchers = data;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.loadError') });
            }
        });
    }

    handleAction(event: { action: string; data: PaymentVoucher }): void {
        if (event.action === 'edit') {
            this.router.navigate(['/admin/payments', event.data.id, 'edit']);
            return;
        }
        const request = event.action === 'confirm'
            ? this.api.confirmPaymentVoucher(event.data.id)
            : this.api.voidPaymentVoucher(event.data.id);
        request.subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.saved') });
                this.load();
            },
            error: () => this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.saveError') })
        });
    }
}
