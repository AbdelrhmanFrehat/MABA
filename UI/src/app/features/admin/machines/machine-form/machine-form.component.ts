import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MachinesApiService } from '../../../../shared/services/machines-api.service';
import { Machine, CreateMachineRequest, UpdateMachineRequest } from '../../../../shared/models/machine.model';

@Component({
    selector: 'app-machine-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="machineForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.machines.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="machineForm.get('nameEn')?.invalid && machineForm.get('nameEn')?.touched"
                    />
                    @if (machineForm.get('nameEn')?.invalid && machineForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.machines.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="machineForm.get('nameAr')?.invalid && machineForm.get('nameAr')?.touched"
                    />
                    @if (machineForm.get('nameAr')?.invalid && machineForm.get('nameAr')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameAr') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="manufacturer">
                        {{ 'admin.machines.manufacturer' | translate }}
                    </label>
                    <input 
                        pInputText 
                        id="manufacturer" 
                        formControlName="manufacturer"
                        class="w-full"
                        maxlength="200"
                    />
                </div>

                <div class="form-field">
                    <label for="model">
                        {{ 'admin.machines.model' | translate }}
                    </label>
                    <input 
                        pInputText 
                        id="model" 
                        formControlName="model"
                        class="w-full"
                        maxlength="200"
                    />
                </div>

                <div class="form-field">
                    <label for="yearFrom">
                        {{ 'admin.machines.yearFrom' | translate }}
                    </label>
                    <p-inputNumber
                        id="yearFrom"
                        formControlName="yearFrom"
                        mode="decimal"
                        [min]="1900"
                        [max]="2100"
                        [maxFractionDigits]="0"
                        styleClass="w-full"
                        [placeholder]="'admin.machines.yearFromPlaceholder' | translate"
                    ></p-inputNumber>
                </div>

                <div class="form-field">
                    <label for="yearTo">
                        {{ 'admin.machines.yearTo' | translate }}
                    </label>
                    <p-inputNumber
                        id="yearTo"
                        formControlName="yearTo"
                        mode="decimal"
                        [min]="1900"
                        [max]="2100"
                        [maxFractionDigits]="0"
                        styleClass="w-full"
                        [placeholder]="'admin.machines.yearToPlaceholder' | translate"
                    ></p-inputNumber>
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
                    [disabled]="machineForm.invalid || saving"
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

        @media (min-width: 768px) {
            .form-grid {
                grid-template-columns: repeat(2, 1fr);
            }
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
export class MachineFormComponent implements OnInit {
    machineForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    machineId: string | null = null;

    private fb = inject(FormBuilder);
    private machinesApiService = inject(MachinesApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.machineForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            manufacturer: [''],
            model: [''],
            yearFrom: [null],
            yearTo: [null]
        });
    }

    ngOnInit() {
        if (this.config.data?.machine) {
            const machine: Machine = this.config.data.machine;
            this.isEditMode = true;
            this.machineId = machine.id;
            this.machineForm.patchValue({
                nameEn: machine.nameEn,
                nameAr: machine.nameAr,
                manufacturer: machine.manufacturer || '',
                model: machine.model || '',
                yearFrom: machine.yearFrom || null,
                yearTo: machine.yearTo || null
            });
        }
    }

    onSubmit() {
        if (this.machineForm.invalid) {
            this.markFormGroupTouched(this.machineForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';
        const formValue = this.machineForm.value;

        if (this.isEditMode && this.machineId) {
            const updateRequest: UpdateMachineRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                manufacturer: formValue.manufacturer || undefined,
                model: formValue.model || undefined,
                yearFrom: formValue.yearFrom || undefined,
                yearTo: formValue.yearTo || undefined
            };

            this.machinesApiService.updateMachine(this.machineId, updateRequest).subscribe({
                next: (machine) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.machineUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(machine);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateMachineRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                manufacturer: formValue.manufacturer || undefined,
                model: formValue.model || undefined,
                yearFrom: formValue.yearFrom || undefined,
                yearTo: formValue.yearTo || undefined
            };

            this.machinesApiService.createMachine(createRequest).subscribe({
                next: (machine) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.machineCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(machine);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        if (error.error?.errors) {
            this.handleValidationErrors(error.error.errors);
        } else {
            this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingMachine');
            this.messageService.add({
                severity: 'error',
                summary: this.translateService.instant('messages.error'),
                detail: this.errorMessage,
                life: 5000
            });
        }
    }

    handleValidationErrors(errors: Record<string, string[]>) {
        Object.keys(errors).forEach(key => {
            const control = this.machineForm.get(key);
            if (control) {
                control.setErrors({ serverError: errors[key][0] });
            }
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.machineForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', { max: control.errors?.['maxlength'].requiredLength });
        }
        if (control?.hasError('serverError')) {
            return control.errors?.['serverError'];
        }
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            formGroup.get(key)?.markAsTouched();
        });
    }
}


