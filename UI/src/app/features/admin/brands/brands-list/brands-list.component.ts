import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { Brand } from '../../../../shared/models/catalog.model';
import { BrandFormComponent } from '../brand-form/brand-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-brands-list',
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
        <div class="brands-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.brands.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.brands.createBrand' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="brands"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.brands.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .brands-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .brands-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .brands-list-container {
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
            .brands-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class BrandsListComponent implements OnInit {
    brands: Brand[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private catalogApiService = inject(CatalogApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.brands.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.brands.nameAr', sortable: true },
        { 
            field: 'isActive', 
            headerKey: 'admin.brands.isActive', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { field: 'createdAt', headerKey: 'common.createdAt', type: 'date', sortable: true }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        },
        {
            icon: 'pi pi-trash',
            tooltipKey: 'common.delete',
            action: 'delete',
            severity: 'danger'
        }
    ];

    ngOnInit() {
        this.loadBrands();
    }

    loadBrands() {
        this.loading = true;
        this.catalogApiService.getAllBrands().subscribe({
            next: (brands) => {
                this.brands = brands;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingBrands'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(BrandFormComponent, {
            header: this.translateService.instant('admin.brands.createBrand'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Brand | boolean) => {
                if (result) {
                    this.loadBrands();
                }
            });
        }
    }

    handleAction(event: { action: string; data: Brand }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(brand: Brand) {
        const ref = this.dialogService.open(BrandFormComponent, {
            header: this.translateService.instant('admin.brands.editBrand'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { brand }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Brand | boolean) => {
                if (result) {
                    this.loadBrands();
                }
            });
        }
    }

    confirmDelete(brand: Brand) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: brand.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteBrand(brand.id);
                }
            });
        }
    }

    deleteBrand(id: string) {
        this.loading = true;
        this.catalogApiService.deleteBrand(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.brandDeletedSuccessfully'),
                    life: 3000
                });
                this.loadBrands();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingBrand'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
