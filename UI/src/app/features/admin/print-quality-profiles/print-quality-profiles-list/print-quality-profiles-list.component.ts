import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../../shared/services/printing-api.service';
import { PrintQualityProfile } from '../../../../shared/models/printing.model';
import { PrintQualityProfileFormComponent } from '../print-quality-profile-form/print-quality-profile-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-print-quality-profiles-list',
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
        <div class="profiles-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.printQualityProfiles.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.printQualityProfiles.create' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="profiles"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.printQualityProfiles.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .profiles-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .profiles-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .profiles-list-container {
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

        @media (max-width: 575px) {
            .profiles-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class PrintQualityProfilesListComponent implements OnInit {
    profiles: PrintQualityProfile[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private printingApiService = inject(PrintingApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.printQualityProfiles.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.printQualityProfiles.nameAr', sortable: true },
        { field: 'layerHeightMm', headerKey: 'admin.printQualityProfiles.layerHeight', type: 'number', sortable: true },
        { field: 'speedCategory', headerKey: 'admin.printQualityProfiles.speedCategory', sortable: true },
        { field: 'priceMultiplier', headerKey: 'admin.printQualityProfiles.priceMultiplier', type: 'number', sortable: true },
        { field: 'sortOrder', headerKey: 'admin.printQualityProfiles.sortOrder', type: 'number', sortable: true },
        { 
            field: 'isDefault', 
            headerKey: 'admin.printQualityProfiles.isDefault', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { 
            field: 'isActive', 
            headerKey: 'admin.printQualityProfiles.isActive', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        }
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
        this.loadProfiles();
    }

    loadProfiles() {
        this.loading = true;
        this.printingApiService.getAllPrintQualityProfiles().subscribe({
            next: (profiles) => {
                this.profiles = profiles;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingData'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(PrintQualityProfileFormComponent, {
            header: this.translateService.instant('admin.printQualityProfiles.create'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: PrintQualityProfile | boolean) => {
                if (result) {
                    this.loadProfiles();
                }
            });
        }
    }

    handleAction(event: { action: string; data: PrintQualityProfile }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(profile: PrintQualityProfile) {
        const ref = this.dialogService.open(PrintQualityProfileFormComponent, {
            header: this.translateService.instant('admin.printQualityProfiles.edit'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { profile }
        });

        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((result: PrintQualityProfile | boolean) => {
                if (result) {
                    this.loadProfiles();
                }
            });
        }
    }

    confirmDelete(profile: PrintQualityProfile) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: profile.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteProfile(profile.id);
                }
            });
        }
    }

    deleteProfile(id: string) {
        this.loading = true;
        this.printingApiService.deletePrintQualityProfile(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.deletedSuccessfully'),
                    life: 3000
                });
                this.loadProfiles();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeleting'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
