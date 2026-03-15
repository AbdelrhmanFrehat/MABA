import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { Brand, CreateBrandRequest, UpdateBrandRequest } from '../../../../shared/models/catalog.model';

@Component({
    selector: 'app-brand-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        CheckboxModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="brandForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.brands.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="brandForm.get('nameEn')?.invalid && brandForm.get('nameEn')?.touched"
                    />
                    @if (brandForm.get('nameEn')?.invalid && brandForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.brands.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="brandForm.get('nameAr')?.invalid && brandForm.get('nameAr')?.touched"
                    />
                    @if (brandForm.get('nameAr')?.invalid && brandForm.get('nameAr')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameAr') }}</small>
                    }
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
                            {{ 'admin.brands.isActive' | translate }}
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
                    [disabled]="brandForm.invalid || saving"
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
export class BrandFormComponent implements OnInit {
    brandForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    brandId: string | null = null;

    private fb = inject(FormBuilder);
    private catalogApiService = inject(CatalogApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.brandForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            isActive: [true]
        });
    }

    ngOnInit() {
        if (this.config.data?.brand) {
            const brand: Brand = this.config.data.brand;
            this.isEditMode = true;
            this.brandId = brand.id;
            this.brandForm.patchValue({
                nameEn: brand.nameEn,
                nameAr: brand.nameAr,
                isActive: brand.isActive
            });
        }
    }

    onSubmit() {
        if (this.brandForm.invalid) {
            this.markFormGroupTouched(this.brandForm);
            return;
        }

        this.saving = true;
        const formValue = this.brandForm.value;

        if (this.isEditMode && this.brandId) {
            const updateRequest: UpdateBrandRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                isActive: formValue.isActive
            };

            this.catalogApiService.updateBrand(this.brandId, updateRequest).subscribe({
                next: (brand) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.brandUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(brand);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateBrandRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                isActive: formValue.isActive
            };

            this.catalogApiService.createBrand(createRequest).subscribe({
                next: (brand) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.brandCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(brand);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingBrand');
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.errorMessage,
            life: 5000
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.brandForm.get(fieldName);
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

