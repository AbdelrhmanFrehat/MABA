import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    DataTableComponent,
    TableColumn,
    TableAction
} from '../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CncApiService } from '../../../shared/services/cnc-api.service';
import { CncMaterial, UpdateCncMaterialRequest } from '../../../shared/models/cnc.model';
import { CncMaterialFormComponent } from './cnc-material-form/cnc-material-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

export interface CncMaterialTableRow extends CncMaterial {
    thicknessInfo: string;
    pcbInfo: string;
}

@Component({
    selector: 'app-cnc-materials-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent, ButtonModule, ToastModule, TranslateModule],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="cnc-materials-list-container">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.cncMaterials.title' | translate }}</h1>
                        <p class="subtitle">{{ 'admin.cncMaterials.subtitle' | translate }}</p>
                    </div>
                    <div class="header-actions">
                        <p-button
                            [label]="'admin.cncMaterials.addMaterial' | translate"
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
                    [title]="'admin.cncMaterials.list' | translate"
                    [showAddButton]="false"
                    [globalFilterFields]="['nameEn', 'nameAr', 'type', 'thicknessInfo', 'pcbInfo']"
                    (onAction)="handleAction($event)"
                >
                    <ng-template #customColumn let-row let-column="column">
                        @if (column.field === 'type') {
                            <span>{{ typeLabelKey(row.type) | translate }}</span>
                        }
                    </ng-template>
                </app-data-table>
            </div>
        </div>
    `,
    styles: [
        `
            .cnc-materials-list-container {
                width: 100%;
                max-width: 100%;
                padding: 0.5rem;
                box-sizing: border-box;
            }
            @media (min-width: 768px) {
                .cnc-materials-list-container {
                    padding: 1rem;
                }
            }
            @media (min-width: 1024px) {
                .cnc-materials-list-container {
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
        `
    ]
})
export class CncMaterialsListComponent implements OnInit {
    materials: CncMaterialTableRow[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private cncApiService = inject(CncApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.cncMaterials.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.cncMaterials.nameAr', sortable: true },
        { field: 'type', headerKey: 'admin.cncMaterials.type', type: 'custom', sortable: true },
        {
            field: 'isActive',
            headerKey: 'admin.cncMaterials.isActive',
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { field: 'thicknessInfo', headerKey: 'admin.cncMaterials.thicknessColumn', sortable: false },
        { field: 'pcbInfo', headerKey: 'admin.cncMaterials.pcbColumn', sortable: false },
        { field: 'sortOrder', headerKey: 'admin.cncMaterials.sortOrder', type: 'number', sortable: true }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        },
        {
            icon: 'pi pi-check-circle',
            tooltipKey: 'admin.cncMaterials.activate',
            action: 'activate',
            visible: (row: CncMaterialTableRow) => !row.isActive,
            severity: 'success'
        },
        {
            icon: 'pi pi-ban',
            tooltipKey: 'admin.cncMaterials.deactivate',
            action: 'deactivate',
            visible: (row: CncMaterialTableRow) => row.isActive,
            severity: 'warn'
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

    typeLabelKey(type: string | undefined): string {
        switch (type) {
            case 'routing':
                return 'admin.cncMaterials.typeRouting';
            case 'pcb':
                return 'admin.cncMaterials.typePcb';
            case 'both':
                return 'admin.cncMaterials.typeBoth';
            default:
                return 'admin.cncMaterials.type';
        }
    }

    private mapRow(m: CncMaterial): CncMaterialTableRow {
        const thicknessInfo = this.cncApiService.getThicknessRange(m) || '—';
        const pcbInfo = this.formatPcbInfo(m);
        return { ...m, thicknessInfo, pcbInfo };
    }

    private formatPcbInfo(m: CncMaterial): string {
        if (m.type === 'routing') {
            return '—';
        }
        const parts = [m.pcbMaterialType, m.supportedBoardThicknesses].filter(
            (x) => x != null && String(x).trim() !== ''
        );
        return parts.length ? parts.map(String).join(' · ') : '—';
    }

    loadMaterials() {
        this.loading = true;
        this.cncApiService.getAllMaterials().subscribe({
            next: (list) => {
                this.materials = list.map((m) => this.mapRow(m));
                this.loading = false;
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('admin.cncMaterials.errorLoading'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(CncMaterialFormComponent, {
            header: this.translateService.instant('admin.cncMaterials.addMaterial'),
            width: '920px',
            closable: true,
            dismissableMask: true,
            data: {}
        });
        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: CncMaterial | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    handleAction(event: { action: string; data: CncMaterialTableRow }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        } else if (event.action === 'activate' || event.action === 'deactivate') {
            this.setActive(event.data, event.action === 'activate');
        }
    }

    openEditDialog(row: CncMaterialTableRow) {
        const { thicknessInfo: _t, pcbInfo: _p, ...material } = row;
        const ref = this.dialogService.open(CncMaterialFormComponent, {
            header: this.translateService.instant('admin.cncMaterials.editMaterial'),
            width: '920px',
            closable: true,
            dismissableMask: true,
            data: { material }
        });
        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: CncMaterial | boolean) => {
                if (result) {
                    this.loadMaterials();
                }
            });
        }
    }

    confirmDelete(row: CncMaterialTableRow) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: row.nameEn })
            }
        });
        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteMaterial(row.id);
                }
            });
        }
    }

    deleteMaterial(id: string) {
        this.loading = true;
        this.cncApiService.deleteMaterial(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.cncMaterials.deletedSuccessfully'),
                    life: 3000
                });
                this.loadMaterials();
            },
            error: (error: { error?: { message?: string } }) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('admin.cncMaterials.errorDeleting'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    setActive(row: CncMaterialTableRow, active: boolean) {
        const { thicknessInfo: _t, pcbInfo: _p, ...mat } = row;
        const update: UpdateCncMaterialRequest = {
            ...mat,
            id: mat.id,
            isActive: active
        };
        this.loading = true;
        this.cncApiService.updateMaterial(row.id, update).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant(
                        active ? 'admin.cncMaterials.activatedSuccessfully' : 'admin.cncMaterials.deactivatedSuccessfully'
                    ),
                    life: 3000
                });
                this.loadMaterials();
            },
            error: (error: { error?: { message?: string } }) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('admin.cncMaterials.errorSaving'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
