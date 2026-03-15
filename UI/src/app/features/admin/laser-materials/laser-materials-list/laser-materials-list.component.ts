import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LaserApiService } from '../../../../shared/services/laser-api.service';
import { LaserMaterial } from '../../../../shared/models/laser.model';
import { LaserMaterialFormComponent } from '../laser-material-form/laser-material-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-laser-materials-list',
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
        <div class="laser-materials-list-container">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.laserMaterials.title' | translate }}</h1>
                        <p class="subtitle">{{ 'admin.laserMaterials.subtitle' | translate }}</p>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.laserMaterials.createMaterial' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <div class="table-section">
                <app-data-table
                    [data]="materials"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.laserMaterials.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .laser-materials-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .laser-materials-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .laser-materials-list-container {
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

        .subtitle {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            margin: 0.25rem 0 0;
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

        @media (max-width: 575px) {
            .laser-materials-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class LaserMaterialsListComponent implements OnInit {
    materials: LaserMaterial[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private laserApiService = inject(LaserApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.laserMaterials.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.laserMaterials.nameAr', sortable: true },
        { field: 'type', headerKey: 'admin.laserMaterials.type', sortable: true },
        { field: 'minThicknessMm', headerKey: 'admin.laserMaterials.minThickness', type: 'number', sortable: true },
        { field: 'maxThicknessMm', headerKey: 'admin.laserMaterials.maxThickness', type: 'number', sortable: true },
        { 
            field: 'isMetal', 
            headerKey: 'admin.laserMaterials.isMetal', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { 
            field: 'isActive', 
            headerKey: 'admin.laserMaterials.isActive', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { field: 'sortOrder', headerKey: 'admin.laserMaterials.sortOrder', type: 'number', sortable: true }
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
        this.laserApiService.getAllMaterials().subscribe({
            next: (materials) => {
                this.materials = materials;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('admin.laserMaterials.errorLoading'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(LaserMaterialFormComponent, {
            header: this.translateService.instant('admin.laserMaterials.createMaterial'),
            width: '650px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: LaserMaterial | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    handleAction(event: { action: string; data: LaserMaterial }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(material: LaserMaterial) {
        const ref = this.dialogService.open(LaserMaterialFormComponent, {
            header: this.translateService.instant('admin.laserMaterials.editMaterial'),
            width: '650px',
            closable: true,
            dismissableMask: true,
            data: { material }
        });

        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: LaserMaterial | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    confirmDelete(material: LaserMaterial) {
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
            this.dialogRef = ref;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteMaterial(material.id);
                }
            });
        }
    }

    deleteMaterial(id: string) {
        this.loading = true;
        this.laserApiService.deleteMaterial(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.laserMaterials.deletedSuccessfully'),
                    life: 3000
                });
                this.loadMaterials();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('admin.laserMaterials.errorDeleting'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
