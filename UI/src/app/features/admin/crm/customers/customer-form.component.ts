import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { LookupDropdownComponent } from '../../../../shared/components/lookup-dropdown/lookup-dropdown';
import { Customer } from '../../../../shared/models/crm.model';

@Component({
    selector: 'app-customer-form',
    standalone: true,
    imports: [
        CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, InputNumberModule,
        CheckboxModule, MessageModule, ToastModule, TranslateModule, LookupDropdownComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }
            <div class="form-grid">
                <div class="form-field">
                    <label>{{ 'admin.crm.nameEn' | translate }} <span class="text-red-500">*</span></label>
                    <input pInputText formControlName="nameEn" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.nameAr' | translate }} <span class="text-red-500">*</span></label>
                    <input pInputText formControlName="nameAr" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.type' | translate }} <span class="text-red-500">*</span></label>
                    <app-lookup-dropdown lookupTypeKey="customer_type" formControlName="customerTypeLookupId" [placeholder]="'admin.crm.selectType' | translate"></app-lookup-dropdown>
                </div>
                <div class="form-field">
                    <label>{{ 'common.email' | translate }}</label>
                    <input pInputText formControlName="email" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'common.phone' | translate }}</label>
                    <input pInputText formControlName="phone" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.taxNumber' | translate }}</label>
                    <input pInputText formControlName="taxNumber" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.creditLimit' | translate }}</label>
                    <p-inputNumber formControlName="creditLimit" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.currency' | translate }}</label>
                    <input pInputText formControlName="currency" class="w-full" maxlength="3" />
                </div>
                <div class="form-field form-field-full">
                    <label>{{ 'admin.crm.address' | translate }}</label>
                    <input pInputText formControlName="addressLine1" class="w-full" [placeholder]="'admin.crm.addressLine1' | translate" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.city' | translate }}</label>
                    <input pInputText formControlName="city" class="w-full" />
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.country' | translate }}</label>
                    <input pInputText formControlName="country" class="w-full" />
                </div>
                <div class="form-field form-field-full">
                    <label>{{ 'common.notes' | translate }}</label>
                    <textarea formControlName="notes" class="w-full p-inputtext" rows="3"></textarea>
                </div>
            </div>
            <div class="flex justify-end gap-2 mt-4">
                <p-button [label]="'common.cancel' | translate" [outlined]="true" (click)="ref.close(false)" [disabled]="saving"></p-button>
                <p-button [label]="'common.save' | translate" type="submit" [loading]="saving" [disabled]="form.invalid || saving"></p-button>
            </div>
        </form>
    `,
    styles: [`
        .form-grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
        @media (min-width: 768px) { .form-grid { grid-template-columns: repeat(2, 1fr); } }
        .form-field { display: flex; flex-direction: column; gap: 0.5rem; }
        .form-field label { font-size: 0.875rem; font-weight: 600; }
        .form-field-full { grid-column: 1 / -1; }
    `]
})
export class CustomerFormComponent implements OnInit {
    form!: FormGroup;
    isEditMode = false;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private crmApi = inject(CrmApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(public ref: DynamicDialogRef, public config: DynamicDialogConfig) {
        this.form = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            customerTypeLookupId: ['', Validators.required],
            email: [''],
            phone: [''],
            taxNumber: [''],
            addressLine1: [''],
            city: [''],
            country: [''],
            creditLimit: [0],
            currency: ['ILS'],
            notes: ['']
        });
    }

    ngOnInit() {
        if (this.config.data?.customer) {
            this.isEditMode = true;
            this.form.patchValue(this.config.data.customer);
        }
    }

    onSubmit() {
        if (this.form.invalid) return;
        this.saving = true;
        this.errorMessage = '';
        const val = this.form.value;

        const apiCall = this.isEditMode
            ? this.crmApi.updateCustomer(this.config.data.customer.id, { ...val, isActive: this.config.data.customer.isActive ?? true })
            : this.crmApi.createCustomer(val);

        apiCall.subscribe({
            next: (result) => {
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.savedSuccessfully') });
                this.ref.close(result);
            },
            error: (err) => { this.saving = false; this.errorMessage = err.error?.message || this.translateService.instant('messages.errorSavingItem'); }
        });
    }
}
