import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { LookupDropdownComponent } from '../../../shared/components/lookup-dropdown/lookup-dropdown';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { CreatePriceListRequest, PriceList } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-price-list-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        CheckboxModule,
        DatePickerModule,
        MessageModule,
        ToastModule,
        TranslateModule,
        LookupDropdownComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="form" (ngSubmit)="save()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label>Name (English) <span class="text-red-500">*</span></label>
                    <input pInputText formControlName="nameEn" class="w-full" />
                </div>

                <div class="form-field">
                    <label>Name (Arabic) <span class="text-red-500">*</span></label>
                    <input pInputText formControlName="nameAr" class="w-full" />
                </div>

                <div class="form-field">
                    <label>Type <span class="text-red-500">*</span></label>
                    <app-lookup-dropdown
                        lookupTypeKey="PriceListType"
                        formControlName="priceListTypeId"
                        [filter]="true"
                        [placeholder]="'Select type'"
                    />
                </div>

                <div class="form-field">
                    <label>{{ 'admin.crm.currency' | translate }}</label>
                    <input pInputText formControlName="currency" class="w-full" maxlength="3" />
                </div>

                <div class="form-field">
                    <label>Valid From</label>
                    <p-datepicker formControlName="validFrom" appendTo="body" dateFormat="yy-mm-dd" styleClass="w-full" />
                </div>

                <div class="form-field">
                    <label>Valid To</label>
                    <p-datepicker formControlName="validTo" appendTo="body" dateFormat="yy-mm-dd" styleClass="w-full" />
                </div>

                <div class="form-field checkbox-field">
                    <p-checkbox formControlName="isDefault" [binary]="true" inputId="isDefault" />
                    <label for="isDefault">Default price list</label>
                </div>

                <div class="form-field checkbox-field">
                    <p-checkbox formControlName="isActive" [binary]="true" inputId="isActive" />
                    <label for="isActive">Active</label>
                </div>
            </div>

            <div class="flex justify-end gap-2 mt-4">
                <p-button label="Cancel" [outlined]="true" (click)="ref.close(false)" [disabled]="saving" />
                <p-button label="Save" type="submit" [loading]="saving" [disabled]="form.invalid || saving" />
            </div>
        </form>
    `,
    styles: [`
        .form-grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
        @media (min-width: 768px) { .form-grid { grid-template-columns: repeat(2, 1fr); } }
        .form-field { display: flex; flex-direction: column; gap: 0.5rem; }
        .form-field label { font-size: 0.875rem; font-weight: 600; }
        .checkbox-field { flex-direction: row; align-items: center; margin-top: 1.75rem; }
    `]
})
export class PriceListFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private pricingApi = inject(PricingApiService);

    saving = false;
    errorMessage = '';
    isEditMode = false;

    form = this.fb.group({
        nameEn: ['', [Validators.required, Validators.maxLength(200)]],
        nameAr: ['', [Validators.required, Validators.maxLength(200)]],
        priceListTypeId: ['', Validators.required],
        currency: ['ILS', [Validators.required, Validators.maxLength(3)]],
        validFrom: [null as Date | null],
        validTo: [null as Date | null],
        isDefault: [false],
        isActive: [true]
    });

    constructor(public ref: DynamicDialogRef, public config: DynamicDialogConfig) {}

    ngOnInit(): void {
        const priceList = this.config.data?.priceList as PriceList | undefined;
        if (!priceList) {
            return;
        }

        this.isEditMode = true;
        this.form.patchValue({
            nameEn: priceList.nameEn,
            nameAr: priceList.nameAr,
            priceListTypeId: priceList.priceListTypeId,
            currency: priceList.currency,
            validFrom: priceList.validFrom ? new Date(priceList.validFrom) : null,
            validTo: priceList.validTo ? new Date(priceList.validTo) : null,
            isDefault: priceList.isDefault,
            isActive: priceList.isActive
        });
    }

    save(): void {
        if (this.form.invalid) {
            return;
        }

        this.saving = true;
        this.errorMessage = '';

        const value = this.form.getRawValue();
        const request: CreatePriceListRequest = {
            nameEn: value.nameEn ?? '',
            nameAr: value.nameAr ?? '',
            priceListTypeId: value.priceListTypeId ?? '',
            currency: value.currency ?? 'ILS',
            isDefault: value.isDefault ?? false,
            isActive: value.isActive ?? true,
            validFrom: value.validFrom ? value.validFrom.toISOString() : undefined,
            validTo: value.validTo ? value.validTo.toISOString() : undefined
        };

        const apiCall = this.isEditMode
            ? this.pricingApi.updatePriceList(this.config.data.priceList.id, request)
            : this.pricingApi.createPriceList(request);

        apiCall.subscribe({
            next: result => this.ref.close(result),
            error: err => {
                this.saving = false;
                this.errorMessage = err.error?.message || 'Failed to save price list.';
            }
        });
    }
}
