import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DataTableComponent, TableColumn, TableAction } from '../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupApiService } from '../../../shared/services/lookup-api.service';
import { LookupValue } from '../../../shared/models/lookup.model';
import { LookupValueFormComponent } from './lookup-value-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-lookup-values-list',
    standalone: true,
    imports: [CommonModule, RouterModule, DataTableComponent, ButtonModule, ToastModule, TranslateModule, TagModule],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div class="flex items-center gap-3">
                    <p-button icon="pi pi-arrow-left" [rounded]="true" [outlined]="true" routerLink="/admin/lookups" size="small"></p-button>
                    <h1 class="text-2xl font-bold m-0">{{ typeKey }}</h1>
                </div>
                <p-button
                    [label]="'admin.lookups.createValue' | translate"
                    icon="pi pi-plus"
                    (click)="openCreateDialog()"
                ></p-button>
            </div>

            <app-data-table
                [data]="values"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [title]="'admin.lookups.values' | translate"
                [showAddButton]="false"
                (onAction)="handleAction($event)"
            ></app-data-table>
        </div>
    `
})
export class LookupValuesListComponent implements OnInit {
    typeKey = '';
    typeId = '';
    values: LookupValue[] = [];
    loading = false;

    private route = inject(ActivatedRoute);
    private lookupApi = inject(LookupApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'key', headerKey: 'admin.lookups.key', sortable: true },
        { field: 'nameEn', headerKey: 'admin.lookups.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.lookups.nameAr', sortable: true },
        { field: 'sortOrder', headerKey: 'admin.lookups.sortOrder', type: 'number', sortable: true },
        { field: 'isDefault', headerKey: 'admin.lookups.isDefault', type: 'boolean' },
        { field: 'isSystem', headerKey: 'admin.lookups.isSystem', type: 'boolean' },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean' }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-trash', tooltipKey: 'common.delete', action: 'delete', severity: 'danger', visible: (row) => !row.isSystem }
    ];

    ngOnInit() {
        this.typeKey = this.route.snapshot.paramMap.get('typeKey') || '';
        this.loadData();
    }

    loadData() {
        this.loading = true;
        this.lookupApi.getValues(this.typeKey, true).subscribe({
            next: (data) => { this.values = data; this.loading = false; },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.errorLoadingData') });
                this.loading = false;
            }
        });
        // Also get the type to get its ID
        this.lookupApi.getLookupTypeByKey(this.typeKey).subscribe({
            next: (type) => { this.typeId = type.id; }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(LookupValueFormComponent, {
            header: this.translateService.instant('admin.lookups.createValue'),
            width: '500px', closable: true, dismissableMask: true,
            data: { lookupTypeId: this.typeId, lookupTypeKey: this.typeKey }
        });
        ref?.onClose.subscribe(result => { if (result) this.loadData(); });
    }

    handleAction(event: { action: string; data: LookupValue }) {
        if (event.action === 'edit') {
            const ref = this.dialogService.open(LookupValueFormComponent, {
                header: this.translateService.instant('admin.lookups.editValue'),
                width: '500px', closable: true, dismissableMask: true,
                data: { lookupValue: event.data, lookupTypeId: this.typeId, lookupTypeKey: this.typeKey }
            });
            ref?.onClose.subscribe(result => { if (result) this.loadData(); });
        } else if (event.action === 'delete') {
            const ref = this.dialogService.open(ConfirmDialogComponent, {
                header: this.translateService.instant('common.confirm'), width: '450px',
                data: { title: this.translateService.instant('common.confirm'), message: this.translateService.instant('messages.confirmDelete', { name: event.data.nameEn }) }
            });
            ref?.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.lookupApi.deleteLookupValue(event.data.id).subscribe({
                        next: () => { this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.deletedSuccessfully') }); this.loadData(); },
                        error: (err) => { this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: err.error?.message || '' }); }
                    });
                }
            });
        }
    }
}
