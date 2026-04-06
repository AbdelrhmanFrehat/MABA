import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { LookupDropdownComponent } from '../../../../shared/components/lookup-dropdown/lookup-dropdown';
import { Supplier } from '../../../../shared/models/crm.model';

@Component({
    selector: 'app-supplier-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, InputNumberModule, MessageModule, ToastModule, TranslateModule, LookupDropdownComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            @if (errorMessage) { <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" /> }
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
                    <label>{{ 'admin.crm.type' | translate }}</label>
                    <app-lookup-dropdown lookupTypeKey="supplier_type" formControlName="supplierTypeLookupId" [placeholder]="'admin.crm.selectType' | translate"></app-lookup-dropdown>
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
                    <label>{{ 'admin.crm.paymentTermDays' | translate }}</label>
                    <p-inputNumber formControlName="paymentTermDays" [min]="0" styleClass="w-full"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.creditLimit' | translate }}</label>
                    <p-inputNumber formControlName="creditLimit" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.crm.taxNumber' | translate }}</label>
                    <input pInputText formControlName="taxNumber" class="w-full" />
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
export class SupplierFormComponent implements OnInit {
    form!: FormGroup;
    isEditMode = false;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private crmApi = inject(CrmApiService);
    private translateService = inject(TranslateService);

    constructor(public ref: DynamicDialogRef, public config: DynamicDialogConfig) {
        this.form = this.fb.group({
            nameEn: ['', [Validators.required]], nameAr: ['', [Validators.required]],
            supplierTypeLookupId: [''], email: [''], phone: [''], taxNumber: [''],
            creditLimit: [0], currency: ['ILS'], paymentTermDays: [30], notes: ['']
        });
    }

    ngOnInit() {
        if (this.config.data?.supplier) {
            this.isEditMode = true;
            const supplier = this.config.data.supplier as Supplier & { supplierTypeId?: string };
            this.form.patchValue({
                ...supplier,
                supplierTypeLookupId: supplier.supplierTypeLookupId || supplier.supplierTypeId || ''
            });
        }
    }

    onSubmit() {
        if (this.form.invalid) return;
        this.saving = true; this.errorMessage = '';
        const val = this.form.value;
        const request = {
            ...val,
            supplierTypeId: val.supplierTypeLookupId || undefined
        };
        delete request.supplierTypeLookupId;
        const apiCall = this.isEditMode
            ? this.crmApi.updateSupplier(this.config.data.supplier.id, { ...request, isActive: this.config.data.supplier.isActive ?? true })
            : this.crmApi.createSupplier(request);
        apiCall.subscribe({
            next: (result) => { this.ref.close(result); },
            error: (err) => { this.saving = false; this.errorMessage = err.error?.message || this.translateService.instant('messages.errorSavingItem'); }
        });
    }
}
