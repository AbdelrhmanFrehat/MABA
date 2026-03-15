import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../../shared/services/printing-api.service';
import { Material } from '../../../../shared/models/printing.model';
import { MaterialFormComponent } from '../material-form/material-form.component';
import { MaterialColorsDialogComponent } from '../material-colors-dialog/material-colors-dialog.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-materials-list',
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
        <div class="materials-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.materials.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.materials.createMaterial' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="materials"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.materials.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .materials-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .materials-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .materials-list-container {
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
            .materials-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class MaterialsListComponent implements OnInit {
    materials: Material[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private printingApiService = inject(PrintingApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.materials.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.materials.nameAr', sortable: true },
        { field: 'pricePerGram', headerKey: 'admin.materials.pricePerGram', type: 'number', sortable: true },
        { field: 'density', headerKey: 'admin.materials.density', type: 'number', sortable: true },
        { field: 'stockQuantity', headerKey: 'admin.materials.stockQuantity', type: 'number', sortable: true },
        { 
            field: 'isActive', 
            headerKey: 'admin.materials.isActive', 
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
        this.loadMaterials();
    }

    loadMaterials() {
        this.loading = true;
        this.printingApiService.getAllMaterials().subscribe({
            next: (materials) => {
                this.materials = materials;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingMaterials'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(MaterialFormComponent, {
            header: this.translateService.instant('admin.materials.createMaterial'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Material | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    handleAction(event: { action: string; data: Material }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        } else if (event.action === 'colors') {
            this.openColorsDialog(event.data);
        }
    }

    openEditDialog(material: Material) {
        const ref = this.dialogService.open(MaterialFormComponent, {
            header: this.translateService.instant('admin.materials.editMaterial'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { material }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Material | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    openColorsDialog(material: Material) {
        const ref = this.dialogService.open(MaterialColorsDialogComponent, {
            header: this.translateService.instant('admin.materials.manageColors') + ': ' + material.nameEn,
            width: '800px',
            height: '80vh',
            closable: true,
            dismissableMask: true,
            data: { material }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe(() => {
                this.loadMaterials();
            });
        }
    }

    confirmDelete(material: Material) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: material.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteMaterial(material.id);
                }
            });
        }
    }

    deleteMaterial(id: string) {
        this.loading = true;
        this.printingApiService.deleteMaterial(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.materialDeletedSuccessfully'),
                    life: 3000
                });
                this.loadMaterials();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingMaterial'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
