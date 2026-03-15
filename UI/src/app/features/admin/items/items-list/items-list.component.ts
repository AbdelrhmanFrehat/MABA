import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ItemsApiService } from '../../../../shared/services/items-api.service';
import { Item } from '../../../../shared/models/item.model';
import { ItemFormComponent } from '../item-form/item-form.component';
import { InventoryDialogComponent } from '../inventory-dialog/inventory-dialog.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ItemMediaComponent } from '../item-media/item-media.component';

@Component({
    selector: 'app-items-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        DataTableComponent,
        ButtonModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="items-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.items.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.items.createItem' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="items"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.items.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .items-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .items-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .items-list-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .page-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0;
        }

        @media (min-width: 768px) {
            .page-header h1 {
                font-size: 2rem;
            }
        }

        .header-actions {
            display: flex;
            gap: 0.5rem;
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .items-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class ItemsListComponent implements OnInit {
    items: Item[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private itemsApiService = inject(ItemsApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.items.nameEn', sortable: true },
        { field: 'sku', headerKey: 'admin.items.sku', sortable: true },
        { field: 'itemStatusKey', headerKey: 'admin.items.status', sortable: true },
        { field: 'price', headerKey: 'admin.items.price', type: 'currency', sortable: true },
        { field: 'brandNameEn', headerKey: 'admin.items.brand', sortable: false },
        { field: 'categoryNameEn', headerKey: 'admin.items.category', sortable: false },
        { field: 'inventory', headerKey: 'admin.items.quantity', type: 'custom', sortable: false },
        { field: 'reviewsCount', headerKey: 'admin.items.reviews', type: 'number', sortable: true }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        },
        {
            icon: 'pi pi-images',
            tooltipKey: 'admin.items.manageImages',
            action: 'images',
            severity: 'info'
        },
        {
            icon: 'pi pi-warehouse',
            tooltipKey: 'admin.items.inventory',
            action: 'inventory',
            severity: 'info'
        },
        {
            icon: 'pi pi-trash',
            tooltipKey: 'common.delete',
            action: 'delete',
            severity: 'danger'
        }
    ];

    ngOnInit() {
        this.loadItems();
    }

    loadItems() {
        this.loading = true;
        this.itemsApiService.getAllItems().subscribe({
            next: (response) => {
                this.items = response;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingItems'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(ItemFormComponent, {
            header: this.translateService.instant('admin.items.createItem'),
            width: '800px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Item | boolean) => {
                if (result) {
                    this.loadItems();
                }
            });
        }
    }

    handleAction(event: { action: string; data: Item }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'images') {
            this.openImagesDialog(event.data);
        } else if (event.action === 'inventory') {
            this.openInventoryDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(item: Item) {
        const ref = this.dialogService.open(ItemFormComponent, {
            header: this.translateService.instant('admin.items.editItem'),
            width: '800px',
            closable: true,
            dismissableMask: true,
            data: { item }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Item | boolean) => {
                if (result) {
                    this.loadItems();
                }
            });
        }
    }

    openInventoryDialog(item: Item) {
        const ref = this.dialogService.open(InventoryDialogComponent, {
            header: this.translateService.instant('admin.items.manageInventory'),
            width: '500px',
            closable: true,
            dismissableMask: true,
            data: { item }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe(() => {
                this.loadItems();
            });
        }
    }

    openImagesDialog(item: Item) {
        const ref = this.dialogService.open(ItemMediaComponent, {
            header: this.translateService.instant('admin.items.manageImages') + ': ' + item.nameEn,
            width: '700px',
            closable: true,
            dismissableMask: true,
            data: { itemId: item.id }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe(() => {
                this.loadItems();
            });
        }
    }

    confirmDelete(item: Item) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: item.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteItem(item.id);
                }
            });
        }
    }

    deleteItem(id: string) {
        this.loading = true;
        this.itemsApiService.deleteItem(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.itemDeletedSuccessfully'),
                    life: 3000
                });
                this.loadItems();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingItem'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
