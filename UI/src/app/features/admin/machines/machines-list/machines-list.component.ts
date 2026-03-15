import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { InputIconModule } from 'primeng/inputicon';
import { IconFieldModule } from 'primeng/iconfield';
import { MachinesApiService } from '../../../../shared/services/machines-api.service';
import { Machine } from '../../../../shared/models/machine.model';
import { MachineFormComponent } from '../machine-form/machine-form.component';
import { MachineCardComponent } from '../machine-card/machine-card.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-machines-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        DataTableComponent,
        ButtonModule,
        ToastModule,
        TranslateModule,
        InputTextModule,
        SelectModule,
        CardModule,
        TagModule,
        TooltipModule,
        InputIconModule,
        IconFieldModule,
        MachineCardComponent
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="machines-list-container">
            <!-- Statistics Cards -->
            <div class="statistics-cards mb-4">
                <p-card class="stat-card">
                    <div class="stat-content">
                        <div class="stat-icon">
                            <i class="pi pi-cog text-4xl text-primary"></i>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ machines.length }}</div>
                            <div class="stat-label">{{ 'admin.machines.totalMachines' | translate }}</div>
                        </div>
                    </div>
                </p-card>
                <p-card class="stat-card">
                    <div class="stat-content">
                        <div class="stat-icon">
                            <i class="pi pi-wrench text-4xl text-orange-500"></i>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ totalParts }}</div>
                            <div class="stat-label">{{ 'admin.machines.totalParts' | translate }}</div>
                        </div>
                    </div>
                </p-card>
                <p-card class="stat-card">
                    <div class="stat-content">
                        <div class="stat-icon">
                            <i class="pi pi-building text-4xl text-primary-500"></i>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ uniqueManufacturers }}</div>
                            <div class="stat-label">{{ 'admin.machines.manufacturers' | translate }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.machines.title' | translate }}</h1>
                        @if (filteredMachines.length !== machines.length || searchTerm || hasActiveFilters()) {
                            <div class="results-info">
                                <span class="text-500 text-sm">
                                    {{ 'common.showing' | translate: {first: 1, last: filteredMachines.length, total: machines.length} }}
                                </span>
                            </div>
                        }
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.machines.createMachine' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <p-card class="filters-card mb-4">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'common.search' | translate }}</label>
                        <p-iconfield>
                            <p-inputicon styleClass="pi pi-search" />
                            <input 
                                type="text" 
                                pInputText
                                [(ngModel)]="searchTerm"
                                [placeholder]="'admin.machines.searchPlaceholder' | translate"
                                (input)="applyFilters()"
                                class="w-full"
                            />
                        </p-iconfield>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.machines.manufacturer' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.manufacturer"
                            [options]="manufacturerOptions"
                            [placeholder]="'admin.machines.allManufacturers' | translate"
                            (onChange)="applyFilters()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.machines.model' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.model"
                            [options]="modelOptions"
                            [placeholder]="'admin.machines.allModels' | translate"
                            (onChange)="applyFilters()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.machines.yearRange' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.yearRange"
                            [options]="yearRangeOptions"
                            [placeholder]="'admin.machines.allYears' | translate"
                            (onChange)="applyFilters()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.machines.partsCount' | translate }}</label>
                        <p-select
                            [(ngModel)]="filters.partsCount"
                            [options]="partsCountOptions"
                            [placeholder]="'admin.machines.allPartsCounts' | translate"
                            (onChange)="applyFilters()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    @if (hasActiveFilters()) {
                        <div class="filter-item filter-actions">
                            <p-button 
                                [label]="'common.clearFilters' | translate"
                                icon="pi pi-times"
                                [outlined]="true"
                                severity="secondary"
                                (click)="clearFilters()"
                                styleClass="w-full">
                            </p-button>
                        </div>
                    }
                </div>
            </p-card>

            <!-- View Toggle -->
            <div class="view-toggle mb-3">
                <p-button 
                    icon="pi pi-table"
                    [outlined]="viewMode !== 'table'"
                    [pTooltip]="'admin.machines.tableView' | translate"
                    (click)="viewMode = 'table'">
                </p-button>
                <p-button 
                    icon="pi pi-th-large"
                    [outlined]="viewMode !== 'cards'"
                    [pTooltip]="'admin.machines.cardView' | translate"
                    (click)="viewMode = 'cards'">
                </p-button>
            </div>

            <!-- Data Table View -->
            @if (viewMode === 'table') {
                <div class="table-section">
                    <app-data-table
                        [data]="filteredMachines"
                        [columns]="columns"
                        [actions]="actions"
                        [loading]="loading"
                        [title]="'admin.machines.list' | translate"
                        [showAddButton]="false"
                        (onAction)="handleAction($event)"
                    ></app-data-table>
                </div>
            }

            <!-- Card View -->
            @if (viewMode === 'cards') {
                <div *ngIf="loading" class="loading-container">
                    <i class="pi pi-spin pi-spinner text-4xl"></i>
                </div>
                <div *ngIf="!loading && filteredMachines.length === 0" class="no-results">
                    <i class="pi pi-inbox text-6xl text-500 mb-4"></i>
                    <p>{{ 'admin.machines.noMachinesFound' | translate }}</p>
                </div>
                <div *ngIf="!loading && filteredMachines.length > 0" class="machines-grid">
                    <app-machine-card
                        *ngFor="let machine of filteredMachines"
                        [machine]="machine"
                        (onEdit)="openEditDialog($event)"
                        (onDelete)="confirmDelete($event)">
                    </app-machine-card>
                </div>
            }
        </div>
    `,
    styles: [`
        .machines-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .machines-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .machines-list-container {
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

        .results-info {
            margin-top: 0.5rem;
        }
        .filters-card {
            margin-bottom: 1.5rem;
        }
        .filters-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }
        @media (min-width: 768px) {
            .filters-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }
        @media (min-width: 1024px) {
            .filters-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }
        .filter-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        .filter-label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }
        .filter-actions {
            grid-column: 1 / -1;
        }
        @media (min-width: 768px) {
            .filter-actions {
                grid-column: auto;
            }
        }
        .view-toggle {
            display: flex;
            gap: 0.5rem;
            justify-content: flex-end;
        }
        .machines-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
            gap: 1.5rem;
        }
        .loading-container,
        .no-results {
            text-align: center;
            padding: 4rem 2rem;
        }
        .no-results p {
            font-size: 1.125rem;
            color: var(--text-color-secondary);
        }
        .statistics-cards {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
        }
        .stat-card {
            background: linear-gradient(135deg, var(--surface-card) 0%, var(--surface-ground) 100%);
        }
        .stat-content {
            display: flex;
            align-items: center;
            gap: 1rem;
        }
        .stat-icon {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 60px;
            height: 60px;
            border-radius: 12px;
            background: var(--surface-ground);
        }
        .stat-info {
            flex: 1;
        }
        .stat-value {
            font-size: 2rem;
            font-weight: 700;
            color: var(--text-color);
            line-height: 1;
        }
        .stat-label {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
            margin-top: 0.25rem;
        }
        .table-section {
            width: 100%;
        }
        ::ng-deep .p-datatable {
            .p-datatable-tbody > tr {
                transition: background-color 0.2s;
            }
            .p-datatable-tbody > tr:hover {
                background-color: var(--surface-hover) !important;
            }
            .p-datatable-thead > tr > th {
                background: var(--surface-100);
                font-weight: 600;
            }
        }

        /* Mobile optimizations */
        @media (max-width: 768px) {
            .machines-grid {
                grid-template-columns: 1fr;
            }
        }
        @media (max-width: 768px) {
            .statistics-cards {
                grid-template-columns: 1fr;
            }
            .stat-content {
                flex-direction: column;
                text-align: center;
            }
        }
        @media (max-width: 575px) {
            .machines-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
            .filters-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class MachinesListComponent implements OnInit {
    machines: Machine[] = [];
    filteredMachines: Machine[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;
    
    searchTerm = '';
    filters: {
        manufacturer?: string;
        model?: string;
        yearRange?: string;
        partsCount?: string;
    } = {};
    viewMode: 'table' | 'cards' = 'table';
    
    manufacturerOptions: any[] = [];
    modelOptions: any[] = [];
    yearRangeOptions: any[] = [
        { label: 'Before 2000', value: 'before-2000' },
        { label: '2000-2010', value: '2000-2010' },
        { label: '2011-2020', value: '2011-2020' },
        { label: '2021-2030', value: '2021-2030' },
        { label: 'After 2030', value: 'after-2030' }
    ];
    partsCountOptions: any[] = [
        { label: '0-5 Parts', value: '0-5' },
        { label: '6-10 Parts', value: '6-10' },
        { label: '11-20 Parts', value: '11-20' },
        { label: '21+ Parts', value: '21+' }
    ];

    private machinesApiService = inject(MachinesApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.machines.nameEn', sortable: true },
        { field: 'nameAr', headerKey: 'admin.machines.nameAr', sortable: true },
        { field: 'manufacturer', headerKey: 'admin.machines.manufacturer', sortable: true },
        { field: 'model', headerKey: 'admin.machines.model', sortable: true },
        { 
            field: 'yearRange', 
            headerKey: 'admin.machines.yearRange', 
            type: 'custom',
            sortable: false 
        },
        { 
            field: 'partsCount', 
            headerKey: 'admin.machines.partsCount', 
            type: 'number',
            sortable: true 
        },
        { 
            field: 'createdAt', 
            headerKey: 'common.createdAt', 
            type: 'date',
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
        this.loadMachines();
    }

    loadMachines() {
        this.loading = true;
        this.machinesApiService.getAllMachines().subscribe({
            next: (machines) => {
                // Transform data for display
                this.machines = machines.map(machine => ({
                    ...machine,
                    yearRange: this.formatYearRange(machine.yearFrom, machine.yearTo),
                    partsCount: machine.parts?.length || 0
                })) as any;
                
                // Extract unique manufacturers and models
                const manufacturers = new Set<string>();
                const models = new Set<string>();
                this.machines.forEach(m => {
                    if (m.manufacturer) manufacturers.add(m.manufacturer);
                    if (m.model) models.add(m.model);
                });
                
                this.manufacturerOptions = Array.from(manufacturers).map(m => ({ label: m, value: m }));
                this.modelOptions = Array.from(models).map(m => ({ label: m, value: m }));
                
                this.updateStatistics();
                this.applyFilters();
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingMachines'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    applyFilters() {
        let filtered = [...this.machines];
        
        // Search filter
        if (this.searchTerm) {
            const searchLower = this.searchTerm.toLowerCase();
            filtered = filtered.filter(m => 
                m.nameEn?.toLowerCase().includes(searchLower) ||
                m.nameAr?.toLowerCase().includes(searchLower) ||
                m.manufacturer?.toLowerCase().includes(searchLower) ||
                m.model?.toLowerCase().includes(searchLower)
            );
        }
        
        // Manufacturer filter
        if (this.filters.manufacturer) {
            filtered = filtered.filter(m => m.manufacturer === this.filters.manufacturer);
        }
        
        // Model filter
        if (this.filters.model) {
            filtered = filtered.filter(m => m.model === this.filters.model);
        }
        
        // Year range filter
        if (this.filters.yearRange) {
            filtered = filtered.filter(m => {
                const yearFrom = m.yearFrom || 0;
                const yearTo = m.yearTo || 9999;
                switch (this.filters.yearRange) {
                    case 'before-2000':
                        return yearTo < 2000;
                    case '2000-2010':
                        return yearFrom >= 2000 && yearTo <= 2010;
                    case '2011-2020':
                        return yearFrom >= 2011 && yearTo <= 2020;
                    case '2021-2030':
                        return yearFrom >= 2021 && yearTo <= 2030;
                    case 'after-2030':
                        return yearFrom > 2030;
                    default:
                        return true;
                }
            });
        }
        
        // Parts count filter
        if (this.filters.partsCount) {
            filtered = filtered.filter(m => {
                const count = (m as any).partsCount || (m.parts?.length || 0);
                switch (this.filters.partsCount) {
                    case '0-5':
                        return count >= 0 && count <= 5;
                    case '6-10':
                        return count >= 6 && count <= 10;
                    case '11-20':
                        return count >= 11 && count <= 20;
                    case '21+':
                        return count >= 21;
                    default:
                        return true;
                }
            });
        }
        
        this.filteredMachines = filtered;
    }

    hasActiveFilters(): boolean {
        return !!(this.filters.manufacturer || this.filters.model || this.filters.yearRange || this.filters.partsCount);
    }

    clearFilters() {
        this.searchTerm = '';
        this.filters = {};
        this.applyFilters();
    }

    get totalParts(): number {
        return this.machines.reduce((sum, m) => sum + ((m as any).partsCount || m.parts?.length || 0), 0);
    }

    get uniqueManufacturers(): number {
        const manufacturers = new Set(this.machines.map(m => m.manufacturer).filter(Boolean));
        return manufacturers.size;
    }

    updateStatistics() {
        // Statistics are computed via getters
    }

    formatYearRange(yearFrom?: number, yearTo?: number): string {
        if (!yearFrom && !yearTo) return '-';
        if (yearFrom && yearTo) return `${yearFrom} - ${yearTo}`;
        return yearFrom?.toString() || yearTo?.toString() || '-';
    }

    openCreateDialog() {
        const ref = this.dialogService.open(MachineFormComponent, {
            header: this.translateService.instant('admin.machines.createMachine'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Machine | boolean) => {
                if (result) {
                    this.loadMachines();
                }
            });
        }
    }

    handleAction(event: { action: string; data: Machine }) {
        if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(machine: Machine) {
        const ref = this.dialogService.open(MachineFormComponent, {
            header: this.translateService.instant('admin.machines.editMachine'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { machine }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Machine | boolean) => {
                if (result) {
                    this.loadMachines();
                }
            });
        }
    }

    confirmDelete(machine: Machine) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: machine.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteMachine(machine.id);
                }
            });
        }
    }

    deleteMachine(id: string) {
        this.loading = true;
        this.machinesApiService.deleteMachine(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.machineDeletedSuccessfully'),
                    life: 3000
                });
                this.loadMachines();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingMachine'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
