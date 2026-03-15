import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ItemsApiService } from '../../../../shared/services/items-api.service';
import { Item, Inventory, UpdateInventoryRequest } from '../../../../shared/models/item.model';

@Component({
    selector: 'app-inventory-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputNumberModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="inventoryForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            @if (loading) {
                <div class="flex justify-content-center align-items-center p-4">
                    <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                </div>
            } @else {
                <div class="form-grid">
                    <div class="form-field form-field-full">
                        <h3 class="m-0">{{ item?.nameEn }}</h3>
                        <small class="text-600">{{ 'admin.items.sku' | translate }}: {{ item?.sku }}</small>
                    </div>

                    <div class="form-field">
                        <label for="quantityOnHand">
                            {{ 'admin.items.quantityOnHand' | translate }} <span class="text-red-500">*</span>
                        </label>
                        <p-inputNumber
                            id="quantityOnHand"
                            formControlName="quantityOnHand"
                            mode="decimal"
                            [min]="0"
                            [maxFractionDigits]="0"
                            styleClass="w-full"
                            [class.ng-invalid]="inventoryForm.get('quantityOnHand')?.invalid && inventoryForm.get('quantityOnHand')?.touched"
                        ></p-inputNumber>
                        @if (inventoryForm.get('quantityOnHand')?.invalid && inventoryForm.get('quantityOnHand')?.touched) {
                            <small class="p-error">{{ getErrorMessage('quantityOnHand') }}</small>
                        }
                    </div>

                    <div class="form-field">
                        <label for="reorderLevel">
                            {{ 'admin.items.reorderLevel' | translate }} <span class="text-red-500">*</span>
                        </label>
                        <p-inputNumber
                            id="reorderLevel"
                            formControlName="reorderLevel"
                            mode="decimal"
                            [min]="0"
                            [maxFractionDigits]="0"
                            styleClass="w-full"
                            [class.ng-invalid]="inventoryForm.get('reorderLevel')?.invalid && inventoryForm.get('reorderLevel')?.touched"
                        ></p-inputNumber>
                        @if (inventoryForm.get('reorderLevel')?.invalid && inventoryForm.get('reorderLevel')?.touched) {
                            <small class="p-error">{{ getErrorMessage('reorderLevel') }}</small>
                        }
                    </div>

                    @if (inventory) {
                        <div class="form-field form-field-full">
                            <small class="text-600">
                                {{ 'admin.items.lastStockIn' | translate }}: 
                                {{ inventory.lastStockInAt ? (inventory.lastStockInAt | date:'short') : ('common.never' | translate) }}
                            </small>
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
                        [disabled]="inventoryForm.invalid || saving"
                        styleClass="w-full md:w-auto"
                    ></p-button>
                </div>
            }
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
export class InventoryDialogComponent implements OnInit {
    inventoryForm!: FormGroup;
    inventory: Inventory | null = null;
    item: Item | null = null;
    loading = false;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private itemsApiService = inject(ItemsApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.inventoryForm = this.fb.group({
            quantityOnHand: [0, [Validators.required, Validators.min(0)]],
            reorderLevel: [0, [Validators.required, Validators.min(0)]]
        });
    }

    ngOnInit() {
        const item: Item = this.config.data?.item;
        if (!item) {
            this.errorMessage = this.translateService.instant('messages.error');
            this.ref.close(false);
            return;
        }

        this.item = item;
        this.loadInventory(item.id);
    }

    loadInventory(itemId: string) {
        this.loading = true;
        this.itemsApiService.getInventoryByItemId(itemId).subscribe({
            next: (inventory: any) => {
                this.inventory = inventory;
                this.inventoryForm.patchValue({
                    quantityOnHand: inventory.quantityOnHand,
                    reorderLevel: inventory.reorderLevel
                });
                this.loading = false;
            },
            error: () => {
                // If inventory doesn't exist yet, use default values
                this.inventoryForm.patchValue({
                    quantityOnHand: 0,
                    reorderLevel: 0
                });
                this.loading = false;
            }
        });
    }

    onSubmit() {
        if (this.inventoryForm.invalid || !this.item) {
            this.markFormGroupTouched(this.inventoryForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';
        const formValue = this.inventoryForm.value;

        const updateRequest: UpdateInventoryRequest = {
            quantityOnHand: formValue.quantityOnHand,
            reorderLevel: formValue.reorderLevel
        };

        this.itemsApiService.updateInventory(this.item.id, updateRequest).subscribe({
            next: (inventory: any) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.inventoryUpdatedSuccessfully'),
                    life: 3000
                });
                this.ref.close(inventory);
            },
            error: (err: any) => {
                this.saving = false;
                this.errorMessage = err.error?.message || this.translateService.instant('messages.errorUpdatingInventory');
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
        const control = this.inventoryForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
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


