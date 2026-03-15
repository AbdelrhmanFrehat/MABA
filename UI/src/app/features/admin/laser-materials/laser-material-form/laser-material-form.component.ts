import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LaserApiService } from '../../../../shared/services/laser-api.service';
import { LaserMaterial, CreateLaserMaterialRequest, UpdateLaserMaterialRequest } from '../../../../shared/models/laser.model';

@Component({
    selector: 'app-laser-material-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        TextareaModule,
        CheckboxModule,
        SelectModule,
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
                        {{ 'admin.laserMaterials.nameEn' | translate }} <span class="text-red-500">*</span>
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
                        {{ 'admin.laserMaterials.nameAr' | translate }} <span class="text-red-500">*</span>
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
                    <label for="type">
                        {{ 'admin.laserMaterials.type' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-select
                        id="type"
                        formControlName="type"
                        [options]="typeOptions"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="form-field">
                    <label for="sortOrder">
                        {{ 'admin.laserMaterials.sortOrder' | translate }}
                    </label>
                    <p-inputNumber
                        id="sortOrder"
                        formControlName="sortOrder"
                        mode="decimal"
                        [minFractionDigits]="0"
                        [maxFractionDigits]="0"
                        styleClass="w-full"
                    ></p-inputNumber>
                </div>

                <div class="form-field">
                    <label for="minThicknessMm">
                        {{ 'admin.laserMaterials.minThickness' | translate }} (mm)
                    </label>
                    <p-inputNumber
                        id="minThicknessMm"
                        formControlName="minThicknessMm"
                        [minFractionDigits]="1"
                        [maxFractionDigits]="2"
                        mode="decimal"
                        styleClass="w-full"
                    ></p-inputNumber>
                </div>

                <div class="form-field">
                    <label for="maxThicknessMm">
                        {{ 'admin.laserMaterials.maxThickness' | translate }} (mm)
                    </label>
                    <p-inputNumber
                        id="maxThicknessMm"
                        formControlName="maxThicknessMm"
                        [minFractionDigits]="1"
                        [maxFractionDigits]="2"
                        mode="decimal"
                        styleClass="w-full"
                    ></p-inputNumber>
                </div>

                <div class="form-field form-field-full">
                    <label for="notesEn">
                        {{ 'admin.laserMaterials.notesEn' | translate }}
                    </label>
                    <textarea 
                        pInputTextarea 
                        id="notesEn" 
                        formControlName="notesEn"
                        rows="2"
                        class="w-full"
                        maxlength="500"
                    ></textarea>
                </div>

                <div class="form-field form-field-full">
                    <label for="notesAr">
                        {{ 'admin.laserMaterials.notesAr' | translate }}
                    </label>
                    <textarea 
                        pInputTextarea 
                        id="notesAr" 
                        formControlName="notesAr"
                        rows="2"
                        class="w-full"
                        maxlength="500"
                    ></textarea>
                </div>

                <div class="form-field">
                    <div class="checkbox-field">
                        <p-checkbox 
                            id="isMetal" 
                            formControlName="isMetal"
                            binary
                            inputId="isMetal"
                        ></p-checkbox>
                        <label for="isMetal">
                            {{ 'admin.laserMaterials.isMetal' | translate }}
                        </label>
                    </div>
                    <small class="helper-text">{{ 'admin.laserMaterials.isMetalHint' | translate }}</small>
                </div>

                <div class="form-field">
                    <div class="checkbox-field">
                        <p-checkbox 
                            id="isActive" 
                            formControlName="isActive"
                            binary
                            inputId="isActive"
                        ></p-checkbox>
                        <label for="isActive">
                            {{ 'admin.laserMaterials.isActive' | translate }}
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

        .helper-text {
            font-size: 0.75rem;
            color: var(--text-color-secondary);
        }

        .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
            margin-top: 1.5rem;
        }

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
export class LaserMaterialFormComponent implements OnInit {
    materialForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    materialId: string | null = null;

    typeOptions = [
        { label: 'Cut & Engrave', value: 'both' },
        { label: 'Cut Only', value: 'cut' },
        { label: 'Engrave Only', value: 'engrave' }
    ];

    private fb = inject(FormBuilder);
    private laserApiService = inject(LaserApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.materialForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            type: ['both', [Validators.required]],
            minThicknessMm: [null],
            maxThicknessMm: [null],
            notesEn: [''],
            notesAr: [''],
            isMetal: [false],
            isActive: [true],
            sortOrder: [0]
        });
    }

    ngOnInit() {
        this.updateTypeOptionsLabels();

        if (this.config.data?.material) {
            const material: LaserMaterial = this.config.data.material;
            this.isEditMode = true;
            this.materialId = material.id;
            this.materialForm.patchValue({
                nameEn: material.nameEn,
                nameAr: material.nameAr,
                type: material.type,
                minThicknessMm: material.minThicknessMm,
                maxThicknessMm: material.maxThicknessMm,
                notesEn: material.notesEn || '',
                notesAr: material.notesAr || '',
                isMetal: material.isMetal,
                isActive: material.isActive,
                sortOrder: material.sortOrder
            });
        }
    }

    updateTypeOptionsLabels() {
        const lang = this.translateService.currentLang;
        this.typeOptions = [
            { label: lang === 'ar' ? 'قطع ونقش' : 'Cut & Engrave', value: 'both' },
            { label: lang === 'ar' ? 'قطع فقط' : 'Cut Only', value: 'cut' },
            { label: lang === 'ar' ? 'نقش فقط' : 'Engrave Only', value: 'engrave' }
        ];
    }

    onSubmit() {
        if (this.materialForm.invalid) {
            this.markFormGroupTouched(this.materialForm);
            return;
        }

        this.saving = true;
        const formValue = this.materialForm.value;

        if (this.isEditMode && this.materialId) {
            const updateRequest: UpdateLaserMaterialRequest = {
                id: this.materialId,
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                type: formValue.type,
                minThicknessMm: formValue.minThicknessMm,
                maxThicknessMm: formValue.maxThicknessMm,
                notesEn: formValue.notesEn || null,
                notesAr: formValue.notesAr || null,
                isMetal: formValue.isMetal,
                isActive: formValue.isActive,
                sortOrder: formValue.sortOrder
            };

            this.laserApiService.updateMaterial(this.materialId, updateRequest).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('admin.laserMaterials.updatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(material);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateLaserMaterialRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                type: formValue.type,
                minThicknessMm: formValue.minThicknessMm,
                maxThicknessMm: formValue.maxThicknessMm,
                notesEn: formValue.notesEn || null,
                notesAr: formValue.notesAr || null,
                isMetal: formValue.isMetal,
                isActive: formValue.isActive,
                sortOrder: formValue.sortOrder
            };

            this.laserApiService.createMaterial(createRequest).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('admin.laserMaterials.createdSuccessfully'),
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
        this.errorMessage = error.error?.message || this.translateService.instant('admin.laserMaterials.errorSaving');
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
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            formGroup.get(key)?.markAsTouched();
        });
    }
}
