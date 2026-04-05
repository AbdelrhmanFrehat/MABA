import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DataTableComponent, TableColumn, TableAction } from '../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupApiService } from '../../../shared/services/lookup-api.service';
import { LookupType } from '../../../shared/models/lookup.model';
import { LookupTypeFormComponent } from './lookup-type-form.component';

@Component({
    selector: 'app-lookups-list',
    standalone: true,
    imports: [CommonModule, RouterModule, DataTableComponent, ButtonModule, ToastModule, TranslateModule],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.lookups.title' | translate }}</h1>
                <p-button
                    [label]="'admin.lookups.createType' | translate"
                    icon="pi pi-plus"
                    (click)="openCreateDialog()"
                ></p-button>
            </div>

            <app-data-table
                [data]="lookupTypes"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [title]="'admin.lookups.types' | translate"
                [showAddButton]="false"
                (onAction)="handleAction($event)"
            ></app-data-table>
        </div>
    `
})
export class LookupsListComponent implements OnInit {
    lookupTypes: LookupType[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private lookupApi = inject(LookupApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'key', headerKey: 'admin.lookups.key', sortable: true },
        { field: 'nameEn', headerKey: 'admin.lookups.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.lookups.nameAr', sortable: true },
        { field: 'isSystem', headerKey: 'admin.lookups.isSystem', type: 'boolean', sortable: true },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-list', tooltipKey: 'admin.lookups.manageValues', action: 'values', severity: 'info' },
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-trash', tooltipKey: 'common.delete', action: 'delete', severity: 'danger', visible: (row) => !row.isSystem }
    ];

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading = true;
        this.lookupApi.getLookupTypes().subscribe({
            next: (data) => { this.lookupTypes = data; this.loading = false; },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.errorLoadingData') });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(LookupTypeFormComponent, {
            header: this.translateService.instant('admin.lookups.createType'),
            width: '500px', closable: true, dismissableMask: true, data: {}
        });
        ref?.onClose.subscribe(result => { if (result) this.loadData(); });
    }

    handleAction(event: { action: string; data: LookupType }) {
        if (event.action === 'values') {
            // Navigate to values page
            window.location.href = `/admin/lookups/${event.data.key}`;
        } else if (event.action === 'edit') {
            const ref = this.dialogService.open(LookupTypeFormComponent, {
                header: this.translateService.instant('admin.lookups.editType'),
                width: '500px', closable: true, dismissableMask: true,
                data: { lookupType: event.data }
            });
            ref?.onClose.subscribe(result => { if (result) this.loadData(); });
        } else if (event.action === 'delete') {
            this.lookupApi.deleteLookupType(event.data.id).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.deletedSuccessfully') });
                    this.loadData();
                },
                error: (err) => {
                    this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: err.error?.message || this.translateService.instant('messages.errorDeletingItem') });
                }
            });
        }
    }
}
