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
import { Tag, CreateTagRequest, UpdateTagRequest } from '../../../../shared/models/catalog.model';

@Component({
    selector: 'app-tag-form',
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
        <form [formGroup]="tagForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.tags.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="tagForm.get('nameEn')?.invalid && tagForm.get('nameEn')?.touched"
                    />
                    @if (tagForm.get('nameEn')?.invalid && tagForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.tags.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="tagForm.get('nameAr')?.invalid && tagForm.get('nameAr')?.touched"
                    />
                    @if (tagForm.get('nameAr')?.invalid && tagForm.get('nameAr')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameAr') }}</small>
                    }
                </div>

                <div class="form-field form-field-full">
                    <label for="slug">
                        {{ 'admin.tags.slug' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="slug" 
                        formControlName="slug"
                        class="w-full"
                        [class.ng-invalid]="tagForm.get('slug')?.invalid && tagForm.get('slug')?.touched"
                    />
                    @if (tagForm.get('slug')?.invalid && tagForm.get('slug')?.touched) {
                        <small class="p-error">{{ getErrorMessage('slug') }}</small>
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
                            {{ 'admin.tags.isActive' | translate }}
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
                    [disabled]="tagForm.invalid || saving"
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
export class TagFormComponent implements OnInit {
    tagForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    tagId: string | null = null;

    private fb = inject(FormBuilder);
    private catalogApiService = inject(CatalogApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.tagForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            slug: ['', [Validators.required]],
            isActive: [true]
        });
    }

    ngOnInit() {
        if (this.config.data?.tag) {
            const tag: Tag = this.config.data.tag;
            this.isEditMode = true;
            this.tagId = tag.id;
            this.tagForm.patchValue({
                nameEn: tag.nameEn,
                nameAr: tag.nameAr,
                slug: tag.slug,
                isActive: tag.isActive
            });
        }
    }

    onSubmit() {
        if (this.tagForm.invalid) {
            this.markFormGroupTouched(this.tagForm);
            return;
        }

        this.saving = true;
        const formValue = this.tagForm.value;

        if (this.isEditMode && this.tagId) {
            const updateRequest: UpdateTagRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                slug: formValue.slug,
                isActive: formValue.isActive
            };

            this.catalogApiService.updateTag(this.tagId, updateRequest).subscribe({
                next: (tag) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.tagUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(tag);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateTagRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                slug: formValue.slug,
                isActive: formValue.isActive
            };

            this.catalogApiService.createTag(createRequest).subscribe({
                next: (tag) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.tagCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(tag);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingTag');
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.errorMessage,
            life: 5000
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.tagForm.get(fieldName);
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

