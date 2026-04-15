import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn, TableAction } from '../../../shared/components/data-table/data-table';
import { AssetsService } from '../../../shared/services/assets.service';
import { Asset } from '../../../shared/models/asset.model';

@Component({
    selector: 'app-assets-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, ConfirmDialogModule, TranslateModule, DataTableComponent],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />

        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.assets.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Track every capital asset: investor, acquisition condition, price and printable label.</p>
                </div>
                <div class="flex gap-2">
                    <p-button label="Categories" icon="pi pi-tags" severity="secondary" [outlined]="true" routerLink="/admin/assets/categories"></p-button>
                    <p-button label="Numbering" icon="pi pi-cog" severity="secondary" [outlined]="true" routerLink="/admin/assets/settings"></p-button>
                    <p-button label="New Asset" icon="pi pi-plus" routerLink="/admin/assets/new"></p-button>
                </div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Assets'"
                [globalFilterFields]="['assetNumber', 'nameEn', 'nameAr', 'investorUserFullName', 'assetCategoryNameEn']"
                (onAction)="onAction($event)"
            ></app-data-table>
        </div>
    `
})
export class AssetsListComponent implements OnInit {
    rows: Asset[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'assetNumber', header: 'Number', sortable: true },
        { field: 'nameEn', header: 'Name', sortable: true },
        { field: 'assetCategoryNameEn', header: 'Category', sortable: true },
        { field: 'investorUserFullName', header: 'Investor', sortable: true },
        { field: 'acquisitionConditionNameEn', header: 'Condition', sortable: true },
        { field: 'approximatePrice', header: 'Price', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'statusNameEn', header: 'Status', sortable: true },
        { field: 'acquiredAt', header: 'Acquired', type: 'date', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-print', tooltip: 'Print label', action: 'print', severity: 'info' },
        { icon: 'pi pi-eye', tooltip: 'View', action: 'view', severity: 'secondary' },
        { icon: 'pi pi-pencil', tooltip: 'Edit', action: 'edit', severity: 'primary' },
        { icon: 'pi pi-trash', tooltip: 'Delete', action: 'delete', severity: 'danger' }
    ];

    private svc = inject(AssetsService);
    private router = inject(Router);
    private msg = inject(MessageService);
    private confirm = inject(ConfirmationService);

    ngOnInit() { this.load(); }

    load() {
        this.loading = true;
        this.svc.getAssets().subscribe({
            next: a => { this.rows = a; this.loading = false; },
            error: err => { this.loading = false; this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load assets.' }); }
        });
    }

    onAction(ev: { action: string; data: Asset }) {
        const a = ev.data;
        switch (ev.action) {
            case 'view': this.router.navigate(['/admin/assets', a.id]); break;
            case 'edit': this.router.navigate(['/admin/assets', a.id, 'edit']); break;
            case 'print': this.router.navigate(['/admin/assets/print', a.id]); break;
            case 'delete':
                this.confirm.confirm({
                    message: `Delete asset ${a.assetNumber}?`,
                    accept: () => {
                        this.svc.remove(a.id).subscribe({
                            next: () => { this.msg.add({ severity: 'success', summary: 'Deleted', detail: 'Asset removed.' }); this.load(); },
                            error: err => this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to delete.' })
                        });
                    }
                });
                break;
        }
    }
}
