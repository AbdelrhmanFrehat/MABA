import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MachinesApiService } from '../../../../shared/services/machines-api.service';
import { ItemsApiService } from '../../../../shared/services/items-api.service';
import { ItemMachineLink, Machine } from '../../../../shared/models/machine.model';
import { Item } from '../../../../shared/models/item.model';
import { ItemMachineLinkFormComponent } from '../item-machine-link-form/item-machine-link-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-item-machine-links-list',
    standalone: true,
    imports: [
        CommonModule,
        DataTableComponent,
        ButtonModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="item-machine-links-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.itemMachineLinks.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.itemMachineLinks.createLink' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="links"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.itemMachineLinks.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .item-machine-links-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .item-machine-links-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .item-machine-links-list-container {
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
            .item-machine-links-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class ItemMachineLinksListComponent implements OnInit {
    links: ItemMachineLink[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private machinesApiService = inject(MachinesApiService);
    private itemsApiService = inject(ItemsApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'itemId', headerKey: 'admin.itemMachineLinks.item', sortable: false },
        { field: 'machineNameEn', headerKey: 'admin.itemMachineLinks.machine', sortable: true },
        { field: 'partNameEn', headerKey: 'admin.itemMachineLinks.part', sortable: false }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-trash',
            tooltipKey: 'common.delete',
            action: 'delete',
            severity: 'danger'
        }
    ];

    ngOnInit() {
        this.loadLinks();
    }

    loadLinks() {
        this.loading = true;
        this.machinesApiService.getItemMachineLinks().subscribe({
            next: (links) => {
                this.links = links;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingLinks'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(ItemMachineLinkFormComponent, {
            header: this.translateService.instant('admin.itemMachineLinks.createLink'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: ItemMachineLink | boolean) => {
                if (result) {
                    this.loadLinks();
                }
            });
        }
    }

    handleAction(event: { action: string; data: ItemMachineLink }) {
        if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    confirmDelete(link: ItemMachineLink) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDeleteLink')
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteLink(link);
                }
            });
        }
    }

    deleteLink(link: ItemMachineLink) {
        this.loading = true;
        this.machinesApiService.deleteItemMachineLink(link.id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.linkDeletedSuccessfully'),
                    life: 3000
                });
                this.loadLinks();
            },
            error: (error) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingLink'),
                    life: 5000
                });
            }
        });
    }
}
