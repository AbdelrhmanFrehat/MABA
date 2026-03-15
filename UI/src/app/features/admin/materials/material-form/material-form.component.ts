import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../../shared/services/printing-api.service';
import { Material, CreateMaterialRequest, UpdateMaterialRequest } from '../../../../shared/models/printing.model';

@Component({
    selector: 'app-material-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        CheckboxModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="materialForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.materials.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="materialForm.get('nameEn')?.invalid && materialForm.get('nameEn')?.touched"
                    />
                    @if (materialForm.get('nameEn')?.invalid && materialForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.materials.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="materialForm.get('nameAr')?.invalid && materialForm.get('nameAr')?.touched"
                    />
                    @if (materialForm.get('nameAr')?.invalid && materialForm.get('nameAr')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameAr') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="pricePerGram">
                        {{ 'admin.materials.pricePerGram' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-inputNumber
                        id="pricePerGram"
                        formControlName="pricePerGram"
                        [minFractionDigits]="2"
                        [maxFractionDigits]="4"
                        mode="decimal"
                        styleClass="w-full"
                        [class.ng-invalid]="materialForm.get('pricePerGram')?.invalid && materialForm.get('pricePerGram')?.touched"
                    ></p-inputNumber>
                    @if (materialForm.get('pricePerGram')?.invalid && materialForm.get('pricePerGram')?.touched) {
                        <small class="p-error">{{ getErrorMessage('pricePerGram') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="density">
                        {{ 'admin.materials.density' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-inputNumber
                        id="density"
                        formControlName="density"
                        [minFractionDigits]="2"
                        [maxFractionDigits]="4"
                        mode="decimal"
                        styleClass="w-full"
                        [class.ng-invalid]="materialForm.get('density')?.invalid && materialForm.get('density')?.touched"
                    ></p-inputNumber>
                    @if (materialForm.get('density')?.invalid && materialForm.get('density')?.touched) {
                        <small class="p-error">{{ getErrorMessage('density') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="color">
                        {{ 'admin.materials.color' | translate }}
                    </label>
                    <input 
                        pInputText 
                        id="color" 
                        formControlName="color"
                        class="w-full"
                        maxlength="50"
                    />
                </div>

                <div class="form-field" *ngIf="isEditMode">
                    <label for="stockQuantity">
                        {{ 'admin.materials.stockQuantity' | translate }}
                    </label>
                    <p-inputNumber
                        id="stockQuantity"
                        formControlName="stockQuantity"
                        [minFractionDigits]="0"
                        [maxFractionDigits]="2"
                        mode="decimal"
                        styleClass="w-full"
                        suffix=" g"
                    ></p-inputNumber>
                </div>

                <div class="form-field form-field-full">
                    <div class="checkbox-field">
                        <p-checkbox 
                            id="isActive" 
                            formControlName="isActive"
                            binary
                            inputId="isActive"
                        ></p-checkbox>
                        <label for="isActive">
                            {{ 'admin.materials.isActive' | translate }}
                        </label>
                    </div>
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
                    [disabled]="materialForm.invalid || saving"
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

        .form-field-full {
            grid-column: 1 / -1;
        }

        .checkbox-field {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .checkbox-field label {
            margin: 0;
            cursor: pointer;
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
export class MaterialFormComponent implements OnInit {
    materialForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    materialId: string | null = null;

    private fb = inject(FormBuilder);
    private printingApiService = inject(PrintingApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.materialForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            pricePerGram: [0, [Validators.required, Validators.min(0)]],
            density: [1.0, [Validators.required, Validators.min(0)]],
            color: [''],
            stockQuantity: [0],
            isActive: [true]
        });
    }

    ngOnInit() {
        if (this.config.data?.material) {
            const material: Material = this.config.data.material;
            this.isEditMode = true;
            this.materialId = material.id;
            this.materialForm.patchValue({
                nameEn: material.nameEn,
                nameAr: material.nameAr,
                pricePerGram: material.pricePerGram,
                density: material.density,
                color: material.color || '',
                stockQuantity: material.stockQuantity,
                isActive: material.isActive
            });
        }
    }

    onSubmit() {
        if (this.materialForm.invalid) {
            this.markFormGroupTouched(this.materialForm);
            return;
        }

        this.saving = true;
        const formValue = this.materialForm.value;

        if (this.isEditMode && this.materialId) {
            const updateRequest: UpdateMaterialRequest = {
                id: this.materialId,
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                pricePerGram: formValue.pricePerGram,
                density: formValue.density,
                color: formValue.color || null,
                stockQuantity: formValue.stockQuantity,
                isActive: formValue.isActive
            };

            this.printingApiService.updateMaterial(this.materialId, updateRequest).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.materialUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(material);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateMaterialRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                pricePerGram: formValue.pricePerGram,
                density: formValue.density,
                color: formValue.color || null
            };

            this.printingApiService.createMaterial(createRequest).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.materialCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(material);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingMaterial');
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.errorMessage,
            life: 5000
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.materialForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', { max: control.errors?.['maxlength'].requiredLength });
        }
        if (control?.hasError('min')) {
            return this.translateService.instant('validation.min', { min: control.errors?.['min'].min });
        }
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            formGroup.get(key)?.markAsTouched();
        });
    }
}
