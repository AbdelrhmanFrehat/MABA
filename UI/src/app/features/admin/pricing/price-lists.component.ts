import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { PriceList } from '../../../shared/models/pricing.model';
import { PriceListFormComponent } from './price-list-form.component';

@Component({
    selector: 'app-price-lists',
    standalone: true,
    imports: [CommonModule, TranslateModule, ButtonModule, ToastModule, DataTableComponent],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">Price Lists</h1>
                    <p class="text-600 mt-2 mb-0">Manage retail and wholesale pricing lists for commercial customers.</p>
                </div>
                <p-button label="New Price List" icon="pi pi-plus" (click)="openCreateDialog()" />
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [title]="'Price Lists'"
                (onAction)="handleAction($event)"
            />
        </div>
    `
})
export class PriceListsComponent implements OnInit {
    rows: PriceList[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'nameEn', header: 'Name (English)', sortable: true },
        { field: 'nameAr', header: 'Name (Arabic)', sortable: true },
        { field: 'priceListTypeName', header: 'Type', sortable: true },
        { field: 'currency', header: 'Currency', sortable: true },
        { field: 'isDefault', header: 'Default', type: 'boolean', sortable: true },
        { field: 'isActive', header: 'Active', type: 'boolean', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltip: 'Edit', action: 'edit' }
    ];

    private pricingApi = inject(PricingApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading = true;
        this.pricingApi.getPriceLists().subscribe({
            next: rows => {
                this.rows = rows;
                this.loading = false;
            },
            error: err => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.message || 'Failed to load price lists.'
                });
            }
        });
    }

    openCreateDialog(): void {
        const ref = this.dialogService.open(PriceListFormComponent, {
            header: 'New Price List',
            width: '700px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        ref?.onClose.subscribe(result => {
            if (result) {
                this.load();
            }
        });
    }

    handleAction(event: { action: string; data: PriceList }): void {
        if (event.action !== 'edit') {
            return;
        }

        const ref = this.dialogService.open(PriceListFormComponent, {
            header: 'Edit Price List',
            width: '700px',
            closable: true,
            dismissableMask: true,
            data: { priceList: event.data }
        });

        ref?.onClose.subscribe(result => {
            if (result) {
                this.load();
            }
        });
    }
}
