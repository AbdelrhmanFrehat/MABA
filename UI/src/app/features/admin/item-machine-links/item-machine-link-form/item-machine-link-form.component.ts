import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MachinesApiService } from '../../../../shared/services/machines-api.service';
import { ItemsApiService } from '../../../../shared/services/items-api.service';
import { ItemMachineLink, CreateItemMachineLinkRequest, Machine, MachinePart } from '../../../../shared/models/machine.model';
import { Item } from '../../../../shared/models/item.model';

@Component({
    selector: 'app-item-machine-link-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        SelectModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="linkForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field form-field-full">
                    <label for="itemId">
                        {{ 'admin.itemMachineLinks.item' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-select
                        id="itemId"
                        formControlName="itemId"
                        [options]="items"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.itemMachineLinks.selectItem' | translate"
                        [filter]="true"
                        styleClass="w-full"
                        [class.ng-invalid]="linkForm.get('itemId')?.invalid && linkForm.get('itemId')?.touched"
                    ></p-select>
                    @if (linkForm.get('itemId')?.invalid && linkForm.get('itemId')?.touched) {
                        <small class="p-error">{{ getErrorMessage('itemId') }}</small>
                    }
                </div>

                <div class="form-field form-field-full">
                    <label for="machineId">
                        {{ 'admin.itemMachineLinks.machine' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-select
                        id="machineId"
                        formControlName="machineId"
                        [options]="machines"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.itemMachineLinks.selectMachine' | translate"
                        [filter]="true"
                        styleClass="w-full"
                        [class.ng-invalid]="linkForm.get('machineId')?.invalid && linkForm.get('machineId')?.touched"
                        (onChange)="onMachineChange()"
                    ></p-select>
                    @if (linkForm.get('machineId')?.invalid && linkForm.get('machineId')?.touched) {
                        <small class="p-error">{{ getErrorMessage('machineId') }}</small>
                    }
                </div>

                <div class="form-field form-field-full">
                    <label for="machinePartId">
                        {{ 'admin.itemMachineLinks.part' | translate }}
                    </label>
                    <p-select
                        id="machinePartId"
                        formControlName="machinePartId"
                        [options]="availableParts"
                        optionLabel="partNameEn"
                        optionValue="id"
                        [placeholder]="'admin.itemMachineLinks.selectPart' | translate"
                        [showClear]="true"
                        [disabled]="!selectedMachine"
                        styleClass="w-full"
                    ></p-select>
                    <small class="text-600">{{ 'admin.itemMachineLinks.partHint' | translate }}</small>
                </div>
            </div>

            <div class="form-actions">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="saving"
                    styleClass="w-full md:w-auto"
                ></p-button>
                <p-button 
                    [label]="'common.save' | translate" 
                    type="submit"
                    [loading]="saving"
                    [disabled]="linkForm.invalid || saving"
                    styleClass="w-full md:w-auto"
                ></p-button>
            </div>
        </form>
    `,
    styles: [`
        .form-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        .form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-field label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .form-field-full {
            grid-column: 1 / -1;
        }

        .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
            margin-top: 1.5rem;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .form-grid {
                gap: 0.75rem;
            }

            .form-actions {
                flex-direction: column;
            }

            .form-actions p-button {
                width: 100%;
            }
        }
    `]
})
export class ItemMachineLinkFormComponent implements OnInit {
    linkForm!: FormGroup;
    items: Item[] = [];
    machines: Machine[] = [];
    availableParts: MachinePart[] = [];
    selectedMachine: Machine | null = null;
    loading = false;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private machinesApiService = inject(MachinesApiService);
    private itemsApiService = inject(ItemsApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.linkForm = this.fb.group({
            itemId: ['', Validators.required],
            machineId: ['', Validators.required],
            machinePartId: ['']
        });
    }

    ngOnInit() {
        this.loadDependencies();
    }

    loadDependencies() {
        this.loading = true;

        // Load items
        this.itemsApiService.getAllItems().subscribe({
            next: (response) => {
                this.items = response;
            }
        });

        // Load machines
        this.machinesApiService.getAllMachines().subscribe({
            next: (machines) => {
                this.machines = machines;
                this.loading = false;
            },
            error: (error) => {
                this.loading = false;
            }
        });
    }

    onMachineChange() {
        const machineId = this.linkForm.get('machineId')?.value;
        this.selectedMachine = this.machines.find(m => m.id === machineId) || null;
        
        if (this.selectedMachine) {
            this.availableParts = this.selectedMachine.parts || [];
        } else {
            this.availableParts = [];
        }

        // Clear part selection if machine changes
        this.linkForm.patchValue({ machinePartId: '' });
    }

    onSubmit() {
        if (this.linkForm.invalid) {
            this.markFormGroupTouched(this.linkForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';
        const formValue = this.linkForm.value;

        const createRequest: CreateItemMachineLinkRequest = {
            itemId: formValue.itemId,
            machineId: formValue.machineId,
            machinePartId: formValue.machinePartId || undefined
        };

        this.machinesApiService.createItemMachineLink(createRequest).subscribe({
            next: (link) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.linkCreatedSuccessfully'),
                    life: 3000
                });
                this.ref.close(link);
            },
            error: (error) => {
                this.saving = false;
                this.errorMessage = error.error?.message || this.translateService.instant('messages.errorCreatingLink');
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.errorMessage,
                    life: 5000
                });
            }
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.linkForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            formGroup.get(key)?.markAsTouched();
        });
    }
}

