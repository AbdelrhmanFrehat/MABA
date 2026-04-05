import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { Customer } from '../../../../shared/models/crm.model';
import { CustomerFormComponent } from './customer-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-customers-list',
    standalone: true,
    imports: [CommonModule, RouterModule, DataTableComponent, ButtonModule, ToastModule, TranslateModule],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.crm.customers' | translate }}</h1>
                <p-button [label]="'admin.crm.createCustomer' | translate" icon="pi pi-plus" (click)="openCreateDialog()"></p-button>
            </div>
            <app-data-table
                [data]="customers"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [title]="'admin.crm.customersList' | translate"
                [showAddButton]="false"
                (onAction)="handleAction($event)"
            ></app-data-table>
        </div>
    `
})
export class CustomersListComponent implements OnInit {
    customers: Customer[] = [];
    loading = false;

    private crmApi = inject(CrmApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'code', headerKey: 'admin.crm.code', sortable: true },
        { field: 'nameEn', headerKey: 'admin.crm.nameEn', sortable: true },
        { field: 'customerTypeName', headerKey: 'admin.crm.type', sortable: true },
        { field: 'email', headerKey: 'common.email', sortable: true },
        { field: 'phone', headerKey: 'common.phone', sortable: false },
        { field: 'balance', headerKey: 'admin.crm.balance', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'creditLimit', headerKey: 'admin.crm.creditLimit', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-eye', tooltipKey: 'common.view', action: 'view', severity: 'info' },
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-trash', tooltipKey: 'common.delete', action: 'delete', severity: 'danger' }
    ];

    ngOnInit() { this.loadData(); }

    loadData() {
        this.loading = true;
        this.crmApi.getCustomers().subscribe({
            next: (data) => { this.customers = data; this.loading = false; },
            error: () => { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.errorLoadingData') }); this.loading = false; }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(CustomerFormComponent, {
            header: this.translateService.instant('admin.crm.createCustomer'), width: '700px', closable: true, dismissableMask: true, data: {}
        });
        ref?.onClose.subscribe(result => { if (result) this.loadData(); });
    }

    handleAction(event: { action: string; data: Customer }) {
        if (event.action === 'view') {
            window.location.href = `/admin/crm/customers/${event.data.id}`;
        } else if (event.action === 'edit') {
            const ref = this.dialogService.open(CustomerFormComponent, {
                header: this.translateService.instant('admin.crm.editCustomer'), width: '700px', closable: true, dismissableMask: true,
                data: { customer: event.data }
            });
            ref?.onClose.subscribe(result => { if (result) this.loadData(); });
        } else if (event.action === 'delete') {
            const ref = this.dialogService.open(ConfirmDialogComponent, {
                header: this.translateService.instant('common.confirm'), width: '450px',
                data: { title: this.translateService.instant('common.confirm'), message: this.translateService.instant('messages.confirmDelete', { name: event.data.nameEn }) }
            });
            ref?.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.crmApi.deleteCustomer(event.data.id).subscribe({
                        next: () => { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.deletedSuccessfully') }); this.loadData(); },
                        error: (err) => { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: err.error?.message || '' }); }
                    });
                }
            });
        }
    }
}
