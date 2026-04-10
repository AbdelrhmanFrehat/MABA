import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CurrencySelectComponent } from '../../../../shared/components/currency-select/currency-select';
import { ItemsApiService } from '../../../../shared/services/items-api.service';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { Item, CreateItemRequest, UpdateItemRequest } from '../../../../shared/models/item.model';
import { Category, Brand, Tag } from '../../../../shared/models/catalog.model';

interface ItemStatus {
    id: string;
    key: string;
    nameEn: string;
    nameAr: string;
}

@Component({
    selector: 'app-item-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        SelectModule,
        MultiSelectModule,
        CheckboxModule,
        ToastModule,
        MessageModule,
        TranslateModule,
        CurrencySelectComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="itemForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.items.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameEn" 
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="itemForm.get('nameEn')?.invalid && itemForm.get('nameEn')?.touched"
                    />
                    @if (itemForm.get('nameEn')?.invalid && itemForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">
                        {{ 'admin.items.nameAr' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="nameAr" 
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="itemForm.get('nameAr')?.invalid && itemForm.get('nameAr')?.touched"
                    />
                    @if (itemForm.get('nameAr')?.invalid && itemForm.get('nameAr')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameAr') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="sku">
                        {{ 'admin.items.sku' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="sku" 
                        formControlName="sku"
                        class="w-full"
                        maxlength="100"
                        [class.ng-invalid]="itemForm.get('sku')?.invalid && itemForm.get('sku')?.touched"
                    />
                    @if (itemForm.get('sku')?.invalid && itemForm.get('sku')?.touched) {
                        <small class="p-error">{{ getErrorMessage('sku') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="itemStatusId">
                        {{ 'admin.items.status' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-select
                        id="itemStatusId"
                        formControlName="itemStatusId"
                        [options]="itemStatuses"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.items.selectStatus' | translate"
                        styleClass="w-full"
                        [class.ng-invalid]="itemForm.get('itemStatusId')?.invalid && itemForm.get('itemStatusId')?.touched"
                    ></p-select>
                    @if (itemForm.get('itemStatusId')?.invalid && itemForm.get('itemStatusId')?.touched) {
                        <small class="p-error">{{ getErrorMessage('itemStatusId') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="price">
                        {{ 'admin.items.price' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-inputNumber
                        id="price"
                        formControlName="price"
                        mode="decimal"
                        [min]="0"
                        [maxFractionDigits]="2"
                        styleClass="w-full"
                        [class.ng-invalid]="itemForm.get('price')?.invalid && itemForm.get('price')?.touched"
                    ></p-inputNumber>
                    @if (itemForm.get('price')?.invalid && itemForm.get('price')?.touched) {
                        <small class="p-error">{{ getErrorMessage('price') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="currency">
                        {{ 'admin.items.currency' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <app-currency-select formControlName="currency"></app-currency-select>
                    @if (itemForm.get('currency')?.invalid && itemForm.get('currency')?.touched) {
                        <small class="p-error">{{ getErrorMessage('currency') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="brandId">
                        {{ 'admin.items.brand' | translate }}
                    </label>
                    <p-select
                        id="brandId"
                        formControlName="brandId"
                        [options]="brands"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.items.selectBrand' | translate"
                        [showClear]="true"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="form-field">
                    <label for="categoryId">
                        {{ 'admin.items.category' | translate }}
                    </label>
                    <p-select
                        id="categoryId"
                        formControlName="categoryId"
                        [options]="categories"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.items.selectCategory' | translate"
                        [showClear]="true"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="form-field form-field-full">
                    <label for="tagIds">
                        {{ 'admin.items.tags' | translate }}
                    </label>
                    <p-multiSelect
                        id="tagIds"
                        formControlName="tagIds"
                        [options]="tags"
                        optionLabel="nameEn"
                        optionValue="id"
                        [placeholder]="'admin.items.selectTags' | translate"
                        [filter]="true"
                        [showClear]="true"
                        display="chip"
                        styleClass="w-full"
                    ></p-multiSelect>
                </div>

                <!-- Flags Row -->
                <div class="form-field form-field-full flags-row">
                    <div class="flag-item">
                        <p-checkbox
                            id="isFeatured"
                            formControlName="isFeatured"
                            binary>
                        </p-checkbox>
                        <label for="isFeatured" class="checkbox-label">
                            <i class="pi pi-star-fill" style="color: #f59e0b;"></i>
                            {{ 'admin.items.featured' | translate }}
                        </label>
                    </div>
                    <div class="flag-item">
                        <p-checkbox
                            id="isNew"
                            formControlName="isNew"
                            binary>
                        </p-checkbox>
                        <label for="isNew" class="checkbox-label">
                            <i class="pi pi-sparkles" style="color: #667eea;"></i>
                            {{ 'admin.items.new' | translate }}
                        </label>
                    </div>
                    <div class="flag-item">
                        <p-checkbox
                            id="isOnSale"
                            formControlName="isOnSale"
                            binary>
                        </p-checkbox>
                        <label for="isOnSale" class="checkbox-label">
                            <i class="pi pi-percentage" style="color: #ef4444;"></i>
                            {{ 'admin.items.onSale' | translate }}
                        </label>
                    </div>
                </div>

                <div class="form-field form-field-full">
                    <label for="generalDescriptionEn">
                        {{ 'admin.items.descriptionEn' | translate }}
                    </label>
                    <textarea 
                        id="generalDescriptionEn" 
                        formControlName="generalDescriptionEn"
                        class="w-full p-inputtext"
                        rows="4"
                    ></textarea>
                </div>

                <div class="form-field form-field-full">
                    <label for="generalDescriptionAr">
                        {{ 'admin.items.descriptionAr' | translate }}
                    </label>
                    <textarea 
                        id="generalDescriptionAr" 
                        formControlName="generalDescriptionAr"
                        class="w-full p-inputtext"
                        rows="4"
                    ></textarea>
                </div>

                @if (!isEditMode) {
                    <div class="form-field">
                        <label for="initialQuantity">
                            {{ 'admin.items.initialQuantity' | translate }}
                        </label>
                        <p-inputNumber
                            id="initialQuantity"
                            formControlName="initialQuantity"
                            mode="decimal"
                            [min]="0"
                            [maxFractionDigits]="0"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>

                    <div class="form-field">
                        <label for="reorderLevel">
                            {{ 'admin.items.reorderLevel' | translate }}
                        </label>
                        <p-inputNumber
                            id="reorderLevel"
                            formControlName="reorderLevel"
                            mode="decimal"
                            [min]="0"
                            [maxFractionDigits]="0"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                }
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
                    [disabled]="itemForm.invalid || saving"
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

        .flags-row {
            display: flex;
            flex-wrap: wrap;
            gap: 1.5rem;
            padding: 1rem;
            background: var(--surface-ground);
            border-radius: 8px;
            border: 1px solid var(--surface-border);
        }

        .flag-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .flag-item .checkbox-label {
            display: flex;
            align-items: center;
            gap: 0.375rem;
            font-weight: 500;
            cursor: pointer;
            user-select: none;
        }

        .flag-item .checkbox-label i {
            font-size: 0.875rem;
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
export class ItemFormComponent implements OnInit {
    itemForm!: FormGroup;
    categories: Category[] = [];
    brands: Brand[] = [];
    tags: Tag[] = [];
    itemStatuses: ItemStatus[] = [];
    loading = false;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    itemId: string | null = null;

    private fb = inject(FormBuilder);
    private itemsApiService = inject(ItemsApiService);
    private catalogApiService = inject(CatalogApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.itemForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            sku: ['', [Validators.required, Validators.maxLength(100)]],
            itemStatusId: ['', Validators.required],
            price: [0, [Validators.required, Validators.min(0)]],
            currency: ['ILS', Validators.required],
            brandId: [''],
            categoryId: [''],
            tagIds: [[]],
            isFeatured: [false],
            isNew: [false],
            isOnSale: [false],
            generalDescriptionEn: [''],
            generalDescriptionAr: [''],
            initialQuantity: [0, Validators.min(0)],
            reorderLevel: [0, Validators.min(0)]
        });
    }

    ngOnInit() {
        this.loadDependencies();
        this.loadItemStatuses();

        if (this.config.data?.item) {
            const item: Item = this.config.data.item;
            this.isEditMode = true;
            this.itemId = item.id;
            this.itemForm.patchValue({
                nameEn: item.nameEn,
                nameAr: item.nameAr,
                sku: item.sku,
                itemStatusId: item.itemStatusId,
                price: item.price,
                currency: item.currency,
                brandId: item.brandId || '',
                categoryId: item.categoryId || '',
                tagIds: item.tagIds || [],
                isFeatured: item.isFeatured || false,
                isNew: item.isNew || false,
                isOnSale: item.isOnSale || false,
                generalDescriptionEn: item.generalDescriptionEn || '',
                generalDescriptionAr: item.generalDescriptionAr || ''
            });
            // Remove initialQuantity and reorderLevel controls in edit mode
            this.itemForm.removeControl('initialQuantity');
            this.itemForm.removeControl('reorderLevel');
        }
    }

    loadDependencies() {
        this.loading = true;
        // Load categories
        this.catalogApiService.getAllCategories(true).subscribe({
            next: (categories) => {
                this.categories = categories;
            }
        });

        // Load brands
        this.catalogApiService.getAllBrands(true).subscribe({
            next: (brands) => {
                this.brands = brands;
            }
        });

        // Load tags
        this.catalogApiService.getAllTags(true).subscribe({
            next: (tags) => {
                this.tags = tags;
                this.loading = false;
            }
        });
    }

    loadItemStatuses() {
        this.itemsApiService.getItemStatuses().subscribe({
            next: (statuses) => {
                this.itemStatuses = statuses.map(s => ({
                    id: s.id,
                    key: s.key,
                    nameEn: s.nameEn,
                    nameAr: s.nameAr
                }));
            },
            error: () => {
                // Fallback to hardcoded statuses if API fails
                this.itemStatuses = [
                    { id: '1', key: 'Active', nameEn: 'Active', nameAr: 'نشط' },
                    { id: '2', key: 'OutOfStock', nameEn: 'Out of Stock', nameAr: 'نفد المخزون' },
                    { id: '3', key: 'Discontinued', nameEn: 'Discontinued', nameAr: 'متوقف' },
                    { id: '4', key: 'ComingSoon', nameEn: 'Coming Soon', nameAr: 'قريباً' },
                    { id: '5', key: 'Draft', nameEn: 'Draft', nameAr: 'مسودة' }
                ];
            }
        });
    }

    onSubmit() {
        if (this.itemForm.invalid) {
            this.markFormGroupTouched(this.itemForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';
        const formValue = this.itemForm.value;

        if (this.isEditMode && this.itemId) {
            const updateRequest: UpdateItemRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                sku: formValue.sku,
                generalDescriptionEn: formValue.generalDescriptionEn || undefined,
                generalDescriptionAr: formValue.generalDescriptionAr || undefined,
                itemStatusId: formValue.itemStatusId,
                price: formValue.price,
                currency: formValue.currency,
                brandId: formValue.brandId || undefined,
                categoryId: formValue.categoryId || undefined,
                tagIds: formValue.tagIds || [],
                isFeatured: formValue.isFeatured || false,
                isNew: formValue.isNew || false,
                isOnSale: formValue.isOnSale || false
            };

            this.itemsApiService.updateItem(this.itemId, updateRequest).subscribe({
                next: (item) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.itemUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(item);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            const createRequest: CreateItemRequest = {
                nameEn: formValue.nameEn,
                nameAr: formValue.nameAr,
                sku: formValue.sku,
                generalDescriptionEn: formValue.generalDescriptionEn || undefined,
                generalDescriptionAr: formValue.generalDescriptionAr || undefined,
                itemStatusId: formValue.itemStatusId,
                price: formValue.price,
                currency: formValue.currency,
                brandId: formValue.brandId || undefined,
                categoryId: formValue.categoryId || undefined,
                tagIds: formValue.tagIds || [],
                initialQuantity: formValue.initialQuantity || undefined,
                reorderLevel: formValue.reorderLevel || undefined
            };

            this.itemsApiService.createItem(createRequest).subscribe({
                next: (item) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.itemCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(item);
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
            this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingItem');
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
            const control = this.itemForm.get(key);
            if (control) {
                control.setErrors({ serverError: errors[key][0] });
            }
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.itemForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', { max: control.errors?.['maxlength'].requiredLength });
        }
        if (control?.hasError('min')) {
            return this.translateService.instant('validation.min', { min: control.errors?.['min'].min });
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

