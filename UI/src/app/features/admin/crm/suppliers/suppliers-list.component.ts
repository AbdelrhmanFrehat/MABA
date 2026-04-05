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
import { Supplier } from '../../../../shared/models/crm.model';
import { SupplierFormComponent } from './supplier-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-suppliers-list',
    standalone: true,
    imports: [CommonModule, RouterModule, DataTableComponent, ButtonModule, ToastModule, TranslateModule],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.crm.suppliers' | translate }}</h1>
                <p-button [label]="'admin.crm.createSupplier' | translate" icon="pi pi-plus" (click)="openCreateDialog()"></p-button>
            </div>
            <app-data-table
                [data]="suppliers"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [title]="'admin.crm.suppliersList' | translate"
                [showAddButton]="false"
                (onAction)="handleAction($event)"
            ></app-data-table>
        </div>
    `
})
export class SuppliersListComponent implements OnInit {
    suppliers: Supplier[] = [];
    loading = false;

    private crmApi = inject(CrmApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'code', headerKey: 'admin.crm.code', sortable: true },
        { field: 'nameEn', headerKey: 'admin.crm.nameEn', sortable: true },
        { field: 'supplierTypeName', headerKey: 'admin.crm.type', sortable: true },
        { field: 'email', headerKey: 'common.email', sortable: true },
        { field: 'phone', headerKey: 'common.phone' },
        { field: 'balance', headerKey: 'admin.crm.balance', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean' }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-eye', tooltipKey: 'common.view', action: 'view', severity: 'info' },
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-trash', tooltipKey: 'common.delete', action: 'delete', severity: 'danger' }
    ];

    ngOnInit() { this.loadData(); }

    loadData() {
        this.loading = true;
        this.crmApi.getSuppliers().subscribe({
            next: (data) => { this.suppliers = data; this.loading = false; },
            error: () => { this.loading = false; }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(SupplierFormComponent, {
            header: this.translateService.instant('admin.crm.createSupplier'), width: '700px', closable: true, dismissableMask: true, data: {}
        });
        ref?.onClose.subscribe(result => { if (result) this.loadData(); });
    }

    handleAction(event: { action: string; data: Supplier }) {
        if (event.action === 'view') {
            window.location.href = `/admin/crm/suppliers/${event.data.id}`;
        } else if (event.action === 'edit') {
            const ref = this.dialogService.open(SupplierFormComponent, {
                header: this.translateService.instant('admin.crm.editSupplier'), width: '700px', closable: true, dismissableMask: true,
                data: { supplier: event.data }
            });
            ref?.onClose.subscribe(result => { if (result) this.loadData(); });
        } else if (event.action === 'delete') {
            const ref = this.dialogService.open(ConfirmDialogComponent, {
                header: this.translateService.instant('common.confirm'), width: '450px',
                data: { title: this.translateService.instant('common.confirm'), message: this.translateService.instant('messages.confirmDelete', { name: event.data.nameEn }) }
            });
            ref?.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) { this.crmApi.deleteSupplier(event.data.id).subscribe({ next: () => this.loadData() }); }
            });
        }
    }
}
