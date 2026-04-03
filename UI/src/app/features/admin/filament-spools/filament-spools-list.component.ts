import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { FilamentSpool } from '../../../shared/models/printing.model';
import { FilamentSpoolsDialogComponent } from './filament-spools-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-filament-spools-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent, ButtonModule, TagModule, ToastModule, TranslateModule],
    providers: [MessageService, DialogService],
    templateUrl: './filament-spools-list.component.html',
    styles: [
        `
            .filament-spools-list-container {
                width: 100%;
                max-width: 100%;
                padding: 0.5rem;
                box-sizing: border-box;
            }
            @media (min-width: 768px) {
                .filament-spools-list-container {
                    padding: 1rem;
                }
            }
            @media (min-width: 1024px) {
                .filament-spools-list-container {
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
            .subtitle {
                margin: 0.35rem 0 0;
                color: var(--text-color-secondary);
                font-size: 0.95rem;
            }
            .header-actions {
                display: flex;
                gap: 0.5rem;
            }
            .table-section {
                width: 100%;
            }
            .remaining-stock-cell {
                display: inline-flex;
                align-items: center;
                gap: 0.5rem;
                flex-wrap: wrap;
            }
            .remaining-grams {
                font-weight: 600;
            }
            :host ::ng-deep .remaining-stock-tag.p-tag {
                font-size: 0.7rem;
                padding: 0.2rem 0.45rem;
            }
        `
    ]
})
export class FilamentSpoolsListComponent implements OnInit {
    spools: FilamentSpool[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private printingApi = inject(PrintingApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'name', headerKey: 'admin.filamentSpools.name', sortable: true },
        { field: 'materialNameEn', headerKey: 'admin.filamentSpools.material', sortable: true },
        { field: 'colorNameEn', headerKey: 'admin.filamentSpools.color', sortable: true },
        {
            field: 'remainingWeightGrams',
            headerKey: 'admin.filamentSpools.remaining',
            type: 'custom',
            sortable: true
        },
        {
            field: 'initialWeightGrams',
            headerKey: 'admin.filamentSpools.initial',
            type: 'number',
            sortable: true
        },
        {
            field: 'isActive',
            headerKey: 'admin.filamentSpools.active',
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { field: 'createdAt', headerKey: 'common.createdAt', type: 'date', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-trash', tooltipKey: 'common.delete', action: 'delete', severity: 'danger' }
    ];

    ngOnInit() {
        this.loadSpools();
    }

    loadSpools() {
        this.loading = true;
        this.printingApi.getFilamentSpools().subscribe({
            next: (rows) => {
                this.spools = rows;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('admin.filamentSpools.loadError'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        const ref = this.dialogService.open(FilamentSpoolsDialogComponent, {
            header: this.translate.instant('admin.filamentSpools.create'),
            width: '560px',
            closable: true,
            dismissableMask: true,
            data: { mode: 'create' }
        });
        this.dialogRef = ref || undefined;
        ref?.onClose.subscribe((saved: boolean) => {
            if (saved) {
                this.loadSpools();
            }
        });
    }

    handleAction(event: { action: string; data: FilamentSpool }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(spool: FilamentSpool) {
        const ref = this.dialogService.open(FilamentSpoolsDialogComponent, {
            header: this.translate.instant('admin.filamentSpools.edit'),
            width: '560px',
            closable: true,
            dismissableMask: true,
            data: { mode: 'edit', spool }
        });
        this.dialogRef = ref || undefined;
        ref?.onClose.subscribe((saved: boolean) => {
            if (saved) {
                this.loadSpools();
            }
        });
    }

    confirmDelete(spool: FilamentSpool) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translate.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translate.instant('common.confirm'),
                message: this.translate.instant('admin.filamentSpools.confirmDeactivate', {
                    name: spool.name?.trim() ? spool.name : spool.materialNameEn
                })
            }
        });
        ref?.onClose.subscribe((confirmed: boolean) => {
            if (confirmed) {
                this.runDelete(spool.id);
            }
        });
    }

    remainingSeverity(g: number): 'success' | 'warn' | 'danger' {
        if (g > 300) {
            return 'success';
        }
        if (g >= 100) {
            return 'warn';
        }
        return 'danger';
    }

    remainingStockTierKey(g: number): string {
        if (g > 300) {
            return 'admin.filamentSpools.stockGood';
        }
        if (g >= 100) {
            return 'admin.filamentSpools.stockMedium';
        }
        return 'admin.filamentSpools.stockLow';
    }

    runDelete(id: string) {
        this.loading = true;
        this.printingApi.deleteFilamentSpool(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('messages.success'),
                    detail: this.translate.instant('admin.filamentSpools.deactivated'),
                    life: 3000
                });
                this.loadSpools();
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('admin.filamentSpools.saveError'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
