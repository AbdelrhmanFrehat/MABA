import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../../shared/models/catalog.model';

@Component({
    selector: 'app-category-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        SelectModule,
        CheckboxModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="categoryForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field form-field-full">
                    <label for="parentId">
                        {{ 'admin.categories.parentCategory' | translate }}
                    </label>
                    <p-select
                        id="parentId"
                        formControlName="parentId"
                        [options]="parentCategories"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.categories.selectParent' | translate"
                        [showClear]="true"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.categories.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        [placeholder]="'admin.categories.nameEnPlaceholder' | translate"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="categoryForm.get('nameEn')?.invalid && categoryForm.get('nameEn')?.touched"
                    />
                    @if (categoryForm.get('nameEn')?.invalid && categoryForm.get('nameEn')?.touched) {
                        <small class="p-error">
                            {{ getErrorMessage('nameEn') }}
                        </small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.categories.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        [placeholder]="'admin.categories.nameArPlaceholder' | translate"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="categoryForm.get('nameAr')?.invalid && categoryForm.get('nameAr')?.touched"
                    />
                    @if (categoryForm.get('nameAr')?.invalid && categoryForm.get('nameAr')?.touched) {
                        <small class="p-error">
                            {{ getErrorMessage('nameAr') }}
                        </small>
                    }
                </div>

                <div class="form-field form-field-full">
                    <label for="slug">
                        {{ 'admin.categories.slug' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="slug" 
                        formControlName="slug"
                        [placeholder]="'admin.categories.slugPlaceholder' | translate"
                        class="w-full"
                        [class.ng-invalid]="categoryForm.get('slug')?.invalid && categoryForm.get('slug')?.touched"
                    />
                    @if (categoryForm.get('slug')?.invalid && categoryForm.get('slug')?.touched) {
                        <small class="p-error">
                            {{ getErrorMessage('slug') }}
                        </small>
                    }
                </div>

                <div class="form-field">
                    <label for="sortOrder">
                        {{ 'admin.categories.sortOrder' | translate }}
                    </label>
                    <p-inputNumber
                        id="sortOrder"
                        formControlName="sortOrder"
                        [showButtons]="true"
                        [min]="0"
                        class="w-full"
                    ></p-inputNumber>
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
                            {{ 'admin.categories.isActive' | translate }}
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
                    [disabled]="categoryForm.invalid || saving"
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
            padding-top: 1.5rem;
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

            .checkbox-field {
                padding-top: 0.5rem;
            }
        }
    `]
})
export class CategoryFormComponent implements OnInit {
    categoryForm!: FormGroup;
    parentCategories: Category[] = [];
    loading = false;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    categoryId: string | null = null;

    private fb = inject(FormBuilder);
    private catalogApiService = inject(CatalogApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.categoryForm = this.fb.group({
            parentId: [null],
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            slug: ['', [Validators.required]],
            sortOrder: [0],
            isActive: [true]
        });
    }

    ngOnInit() {
        this.loadParentCategories();

        if (this.config.data?.category) {
            const category: Category = this.config.data.category;
            this.isEditMode = true;
            this.categoryId = category.id;
            this.categoryForm.patchValue({
                parentId: category.parentId || null,
                nameEn: category.nameEn,
                nameAr: category.nameAr,
                slug: category.slug,
                sortOrder: category.sortOrder,
                isActive: category.isActive
            });
        } else if (this.config.data?.parentId) {
            this.categoryForm.patchValue({
                parentId: this.config.data.parentId
            });
        }
    }

    loadParentCategories() {
        this.loading = true;
        this.catalogApiService.getAllCategories(undefined, true).subscribe({
            next: (categories) => {
                // Filter out the current category if editing
                if (this.categoryId) {
                    this.parentCategories = categories.filter(c => c.id !== this.categoryId);
                } else {
                    this.parentCategories = categories;
                }
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onSubmit() {
        if (this.categoryForm.invalid) {
            this.markFormGroupTouched(this.categoryForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';

        const formValue = this.categoryForm.value;

        if (this.isEditMode && this.categoryId) {
            const updateRequest: UpdateCategoryRequest = {
                parentId: formValue.parentId || undefined,
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                slug: formValue.slug,
                sortOrder: formValue.sortOrder || 0,
                isActive: formValue.isActive
            };

            this.catalogApiService.updateCategory(this.categoryId, updateRequest).subscribe({
                next: (category) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.categoryUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(category);
                },
                error: (error) => {
                    this.handleError(error);
                }
            });
        } else {
            const createRequest: CreateCategoryRequest = {
                parentId: formValue.parentId || undefined,
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                slug: formValue.slug,
                sortOrder: formValue.sortOrder || 0,
                isActive: formValue.isActive
            };

            this.catalogApiService.createCategory(createRequest).subscribe({
                next: (category) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.categoryCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(category);
                },
                error: (error) => {
                    this.handleError(error);
                }
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        if (error.error?.errors) {
            this.handleValidationErrors(error.error.errors);
        } else {
            this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingCategory');
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
            const control = this.categoryForm.get(key);
            if (control) {
                control.setErrors({ serverError: errors[key][0] });
            }
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.categoryForm.get(fieldName);
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
            const control = formGroup.get(key);
            control?.markAsTouched();
        });
    }
}

