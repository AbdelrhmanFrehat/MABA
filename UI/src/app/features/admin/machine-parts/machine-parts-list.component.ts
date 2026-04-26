import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MachinesApiService } from '../../../shared/services/machines-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { MachinePart, Machine, CreateMachinePartRequest } from '../../../shared/models/machine.model';
import { DataTableComponent } from '../../../shared/components/data-table/data-table';

@Component({
    selector: 'app-machine-parts-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        TableModule,
        SelectModule,
        InputTextModule,
        InputNumberModule,
        TagModule,
        DialogModule,
        ToastModule,
        ConfirmDialogModule,
        TranslateModule,
        DataTableComponent
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />
        <div class="machine-parts-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.machineParts.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.machineParts.addNew' | translate" 
                            icon="pi pi-plus"
                            (click)="openAddDialog()">
                        </p-button>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.machineParts.filterByMachine' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedMachineId"
                            [options]="machineOptions"
                            [placeholder]="'admin.machineParts.allMachines' | translate"
                            (onChange)="loadMachineParts()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'common.search' | translate }}</label>
                        <input 
                            type="text" 
                            pInputText
                            [(ngModel)]="searchTerm"
                            [placeholder]="'admin.machineParts.searchPlaceholder' | translate"
                            (input)="onSearchChange()"
                            class="w-full"
                        />
                    </div>
                </div>
            </p-card>

            <!-- Machine Parts Table -->
            <div class="table-section">
                <p-card>
                    <app-data-table
                        [data]="machineParts"
                        [columns]="columns"
                        [loading]="loading"
                        [paginator]="true"
                        [rows]="pageSize"
                        [lazy]="true"
                        [totalRecords]="totalRecords"
                        (onLazyLoad)="onLazyLoad($event)"
                        (onAction)="handleAction($event)">
                    </app-data-table>
                </p-card>
            </div>

            <!-- Add/Edit Dialog -->
            <p-dialog
                [(visible)]="showDialog"
                [modal]="true"
                [style]="{width: '90vw', maxWidth: '600px'}"
                [breakpoints]="{'960px': '90vw', '640px': '95vw'}"
                (onHide)="closeDialog()">
                <ng-template pTemplate="header">
                    <div class="dlg-header">
                        <span class="dlg-label">Machine Parts</span>
                        <span class="dlg-title">{{ editingPart ? ('admin.machineParts.editPart' | translate) : ('admin.machineParts.addNew' | translate) }}</span>
                    </div>
                </ng-template>
                <form [formGroup]="partForm" (ngSubmit)="savePart()">
                    <div class="dialog-form-grid">
                        <div class="dialog-form-field">
                            <label>{{ 'admin.machineParts.machine' | translate }} *</label>
                            <p-select
                                formControlName="machineId"
                                [options]="machineOptions"
                                optionLabel="label"
                                optionValue="value"
                                [placeholder]="'admin.machineParts.selectMachine' | translate"
                                class="w-full">
                            </p-select>
                        </div>
                        <div class="dialog-form-field">
                            <label>{{ 'admin.machineParts.partCode' | translate }} *</label>
                            <input type="text" pInputText formControlName="partCode" class="w-full" />
                        </div>
                        <div class="dialog-form-field">
                            <label>{{ 'common.name' | translate }} (EN) *</label>
                            <input type="text" pInputText formControlName="partNameEn" class="w-full" />
                        </div>
                        <div class="dialog-form-field">
                            <label>{{ 'common.name' | translate }} (AR) *</label>
                            <input type="text" pInputText formControlName="partNameAr" class="w-full" />
                        </div>
                    </div>
                    <div class="dialog-actions">
                        <p-button 
                            [label]="'common.cancel' | translate" 
                            [outlined]="true"
                            type="button"
                            (click)="closeDialog()"
                            styleClass="w-full md:w-auto">
                        </p-button>
                        <p-button 
                            [label]="'common.save' | translate" 
                            type="submit"
                            [disabled]="!partForm.valid || saving"
                            [loading]="saving"
                            styleClass="w-full md:w-auto">
                        </p-button>
                    </div>
                </form>
            </p-dialog>
        </div>
    `,
    styles: [`
        .machine-parts-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .machine-parts-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .machine-parts-list-container {
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

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .machine-parts-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }

        /* Dialog styles */
        .dialog-form-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        .dialog-form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .dialog-form-field label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .dialog-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            justify-content: flex-end;
            margin-top: 1.5rem;
        }

        @media (max-width: 767px) {
            .dialog-actions {
                flex-direction: column;
            }

            .dialog-actions p-button {
                width: 100%;
            }
        }
    `]
})
export class MachinePartsListComponent implements OnInit {
    machineParts: MachinePart[] = [];
    machines: Machine[] = [];
    loading = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedMachineId: string = '';
    searchTerm = '';
    showDialog = false;
    editingPart: MachinePart | null = null;
    saving = false;
    partForm: FormGroup;

    machineOptions: any[] = [];

    columns: any[] = [];

    private formBuilder = inject(FormBuilder);
    private machinesApiService = inject(MachinesApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);

    constructor() {
        this.partForm = this.formBuilder.group({
            machineId: ['', Validators.required],
            partCode: [''],
            partNameEn: ['', Validators.required],
            partNameAr: ['', Validators.required]
        });
    }

    ngOnInit() {
        this.setupColumns();
        this.loadMachines();
        this.loadMachineParts();
    }

    setupColumns() {
        const lang = this.languageService.language;
        this.columns = [
            { field: 'partCode', header: lang === 'ar' ? 'كود القطعة' : 'Part Code', sortable: true },
            { field: 'name', header: lang === 'ar' ? 'الاسم' : 'Name', sortable: true },
            { field: 'machineName', header: lang === 'ar' ? 'الماكينة' : 'Machine', sortable: true },
            { field: 'actions', header: lang === 'ar' ? 'الإجراءات' : 'Actions', sortable: false }
        ];
    }

    loadMachines() {
        this.machinesApiService.getAllMachines().subscribe({
            next: (machines) => {
                this.machines = machines;
                const lang = this.languageService.language;
                this.machineOptions = [
                    { label: this.translateService.instant('admin.machineParts.allMachines'), value: '' },
                    ...machines.map(m => ({
                        label: lang === 'ar' ? m.nameAr : m.nameEn,
                        value: m.id
                    }))
                ];
            }
        });
    }

    loadMachineParts() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedMachineId) {
            params.machineId = this.selectedMachineId;
        }
        if (this.searchTerm) {
            params.search = this.searchTerm;
        }

        this.machinesApiService.getMachineParts(params).subscribe({
            next: (response) => {
                this.machineParts = (response.items || []).map((part: MachinePart) => ({
                    ...part,
                    name: this.getPartName(part),
                    machineName: this.getMachineName(part.machineId)
                }));
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onLazyLoad(event: any) {
        this.currentPage = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.loadMachineParts();
    }

    onSearchChange() {
        this.currentPage = 1;
        this.loadMachineParts();
    }

    handleAction(event: { action: string; data: any }) {
        const part = event.data as MachinePart;
        switch (event.action) {
            case 'edit':
                this.openEditDialog(part);
                break;
            case 'delete':
                this.deletePart(part);
                break;
        }
    }

    openAddDialog() {
        this.editingPart = null;
        this.partForm.reset();
        this.showDialog = true;
    }

    openEditDialog(part: MachinePart) {
        this.editingPart = part;
        this.partForm.patchValue({
            machineId: part.machineId,
            partCode: part.partCode || '',
            partNameEn: part.partNameEn,
            partNameAr: part.partNameAr
        });
        this.showDialog = true;
    }

    closeDialog() {
        this.showDialog = false;
        this.editingPart = null;
        this.partForm.reset();
    }

    savePart() {
        if (!this.partForm.valid) return;

        this.saving = true;
        const formValue = this.partForm.value;
        const request: CreateMachinePartRequest = {
            machineId: formValue.machineId,
            partCode: formValue.partCode || undefined,
            partNameEn: formValue.partNameEn,
            partNameAr: formValue.partNameAr
        };

        const apiCall = this.editingPart
            ? this.machinesApiService.updateMachinePart(this.editingPart.id, request)
            : this.machinesApiService.createMachinePart(request);

        apiCall.subscribe({
            next: () => {
                this.loadMachineParts();
                this.closeDialog();
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.saved')
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    deletePart(part: MachinePart) {
        this.confirmationService.confirm({
            message: this.translateService.instant('admin.machineParts.confirmDelete'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.machinesApiService.deleteMachinePart(part.id).subscribe({
                    next: () => {
                        this.loadMachineParts();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('messages.deleted')
                        });
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    getPartName(part: MachinePart): string {
        return this.languageService.language === 'ar' ? part.partNameAr : part.partNameEn;
    }

    getMachineName(machineId: string): string {
        const machine = this.machines.find(m => m.id === machineId);
        if (machine) {
            return this.languageService.language === 'ar' ? machine.nameAr : machine.nameEn;
        }
        return '';
    }
}
