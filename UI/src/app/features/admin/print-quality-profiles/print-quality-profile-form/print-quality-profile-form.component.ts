import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../../shared/services/printing-api.service';
import { PrintQualityProfile, CreatePrintQualityProfileRequest, UpdatePrintQualityProfileRequest } from '../../../../shared/models/printing.model';

@Component({
    selector: 'app-print-quality-profile-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        CheckboxModule,
        SelectModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="profileForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.printQualityProfiles.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                    />
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.printQualityProfiles.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                    />
                </div>

                <div class="form-field">
                    <label for="descriptionEn">
                        {{ 'admin.printQualityProfiles.descriptionEn' | translate }}
                    </label>
                    <input 
                        pInputText 
                        id="descriptionEn" 
                        formControlName="descriptionEn"
                        class="w-full"
                    />
                </div>

                <div class="form-field">
                    <label for="descriptionAr">
                        {{ 'admin.printQualityProfiles.descriptionAr' | translate }}
                    </label>
                    <input 
                        pInputText 
                        id="descriptionAr" 
                        formControlName="descriptionAr"
                        class="w-full"
                    />
                </div>

                <div class="form-field">
                    <label for="layerHeightMm">
                        {{ 'admin.printQualityProfiles.layerHeight' | translate }} (mm) <span class="text-red-500">*</span>
                    </label>
                    <p-inputNumber
                        id="layerHeightMm"
                        formControlName="layerHeightMm"
                        [minFractionDigits]="2"
                        [maxFractionDigits]="2"
                        [min]="0.01"
                        [max]="1"
                        mode="decimal"
                        styleClass="w-full">
                    </p-inputNumber>
                </div>

                <div class="form-field">
                    <label for="speedCategory">
                        {{ 'admin.printQualityProfiles.speedCategory' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-select
                        id="speedCategory"
                        formControlName="speedCategory"
                        [options]="speedCategories"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-full">
                    </p-select>
                </div>

                <div class="form-field">
                    <label for="priceMultiplier">
                        {{ 'admin.printQualityProfiles.priceMultiplier' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-inputNumber
                        id="priceMultiplier"
                        formControlName="priceMultiplier"
                        [minFractionDigits]="1"
                        [maxFractionDigits]="2"
                        [min]="0.1"
                        [max]="10"
                        mode="decimal"
                        styleClass="w-full">
                    </p-inputNumber>
                </div>

                <div class="form-field">
                    <label for="sortOrder">
                        {{ 'admin.printQualityProfiles.sortOrder' | translate }}
                    </label>
                    <p-inputNumber
                        id="sortOrder"
                        formControlName="sortOrder"
                        [min]="0"
                        [max]="100"
                        styleClass="w-full">
                    </p-inputNumber>
                </div>

                <div class="form-field form-field-full">
                    <div class="checkbox-group">
                        <div class="checkbox-field">
                            <p-checkbox 
                                id="isDefault" 
                                formControlName="isDefault"
                                binary
                                inputId="isDefault">
                            </p-checkbox>
                            <label for="isDefault">
                                {{ 'admin.printQualityProfiles.isDefault' | translate }}
                            </label>
                        </div>
                        <div class="checkbox-field">
                            <p-checkbox 
                                id="isActive" 
                                formControlName="isActive"
                                binary
                                inputId="isActive">
                            </p-checkbox>
                            <label for="isActive">
                                {{ 'admin.printQualityProfiles.isActive' | translate }}
                            </label>
                        </div>
                    </div>
                </div>
            </div>

            <div class="form-actions">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="saving"
                    styleClass="w-full md:w-auto">
                </p-button>
                <p-button 
                    [label]="'common.save' | translate" 
                    type="submit"
                    [loading]="saving"
                    [disabled]="profileForm.invalid || saving"
                    styleClass="w-full md:w-auto">
                </p-button>
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
        }

        .form-field-full {
            grid-column: 1 / -1;
        }

        .checkbox-group {
            display: flex;
            gap: 2rem;
            flex-wrap: wrap;
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

        @media (max-width: 575px) {
            .form-actions {
                flex-direction: column;
            }
        }
    `]
})
export class PrintQualityProfileFormComponent implements OnInit {
    profileForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    profileId: string | null = null;

    speedCategories = [
        { label: 'Fast', value: 'Fast' },
        { label: 'Normal', value: 'Normal' },
        { label: 'Slow', value: 'Slow' }
    ];

    private fb = inject(FormBuilder);
    private printingApiService = inject(PrintingApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.profileForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            descriptionEn: [''],
            descriptionAr: [''],
            layerHeightMm: [0.15, [Validators.required, Validators.min(0.01), Validators.max(1)]],
            speedCategory: ['Normal', Validators.required],
            priceMultiplier: [1.0, [Validators.required, Validators.min(0.1), Validators.max(10)]],
            sortOrder: [0],
            isDefault: [false],
            isActive: [true]
        });
    }

    ngOnInit() {
        if (this.config.data?.profile) {
            const profile: PrintQualityProfile = this.config.data.profile;
            this.isEditMode = true;
            this.profileId = profile.id;
            this.profileForm.patchValue({
                nameEn: profile.nameEn,
                nameAr: profile.nameAr,
                descriptionEn: profile.descriptionEn || '',
                descriptionAr: profile.descriptionAr || '',
                layerHeightMm: profile.layerHeightMm,
                speedCategory: profile.speedCategory,
                priceMultiplier: profile.priceMultiplier,
                sortOrder: profile.sortOrder,
                isDefault: profile.isDefault,
                isActive: profile.isActive
            });
        }
    }

    onSubmit() {
        if (this.profileForm.invalid) {
            return;
        }

        this.saving = true;
        const formValue = this.profileForm.value;

        if (this.isEditMode && this.profileId) {
            const updateRequest: UpdatePrintQualityProfileRequest = {
                id: this.profileId,
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                descriptionEn: formValue.descriptionEn || null,
                descriptionAr: formValue.descriptionAr || null,
                layerHeightMm: formValue.layerHeightMm,
                speedCategory: formValue.speedCategory,
                priceMultiplier: formValue.priceMultiplier,
                sortOrder: formValue.sortOrder,
                isDefault: formValue.isDefault,
                isActive: formValue.isActive
            };

            this.printingApiService.updatePrintQualityProfile(this.profileId, updateRequest).subscribe({
                next: (profile) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.updatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(profile);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreatePrintQualityProfileRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                descriptionEn: formValue.descriptionEn || null,
                descriptionAr: formValue.descriptionAr || null,
                layerHeightMm: formValue.layerHeightMm,
                speedCategory: formValue.speedCategory,
                priceMultiplier: formValue.priceMultiplier,
                sortOrder: formValue.sortOrder,
                isDefault: formValue.isDefault,
                isActive: formValue.isActive
            };

            this.printingApiService.createPrintQualityProfile(createRequest).subscribe({
                next: (profile) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.createdSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(profile);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSaving');
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.errorMessage,
            life: 5000
        });
    }
}
