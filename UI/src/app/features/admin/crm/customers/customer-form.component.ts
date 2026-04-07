import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CurrencySelectComponent } from '../../../../shared/components/currency-select/currency-select';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { UsersService } from '../../../../shared/services/users.service';
import { LookupDropdownComponent } from '../../../../shared/components/lookup-dropdown/lookup-dropdown';
import { Customer } from '../../../../shared/models/crm.model';
import { User } from '../../../../shared/models/auth.model';

@Component({
    selector: 'app-customer-form',
    standalone: true,
    imports: [
        CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, InputNumberModule,
        CheckboxModule, MessageModule, ToastModule, TranslateModule, LookupDropdownComponent, SelectModule, CurrencySelectComponent
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
                    <label>Linked Website User</label>
                    <p-select
                        formControlName="userId"
                        [options]="userOptions"
                        optionLabel="label"
                        optionValue="value"
                        [filter]="true"
                        [showClear]="true"
                        placeholder="Select a website user"
                        styleClass="w-full"
                    />
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
                    <app-currency-select formControlName="currency"></app-currency-select>
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
    userOptions: { label: string; value: string }[] = [];

    private fb = inject(FormBuilder);
    private crmApi = inject(CrmApiService);
    private usersService = inject(UsersService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(public ref: DynamicDialogRef, public config: DynamicDialogConfig) {
        this.form = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            customerTypeLookupId: ['', Validators.required],
            userId: [null as string | null],
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
        this.loadUsers();

        if (this.config.data?.customer) {
            this.isEditMode = true;
            const customer = this.config.data.customer as Customer & { customerTypeId?: string };
            this.form.patchValue({
                ...customer,
                customerTypeLookupId: customer.customerTypeLookupId || customer.customerTypeId || ''
            });
        }
    }

    private loadUsers() {
        this.usersService.getAllUsersSimple({ isActive: true }).subscribe({
            next: (users: User[]) => {
                this.userOptions = users.map(user => ({
                    value: user.id,
                    label: user.email ? `${user.fullName} (${user.email})` : user.fullName
                }));
            },
            error: () => {
                this.userOptions = [];
            }
        });
    }

    onSubmit() {
        if (this.form.invalid) return;
        this.saving = true;
        this.errorMessage = '';
        const val = this.form.value;
        const request = {
            ...val,
            customerTypeId: val.customerTypeLookupId
        };
        delete request.customerTypeLookupId;

        const apiCall = this.isEditMode
            ? this.crmApi.updateCustomer(this.config.data.customer.id, { ...request, isActive: this.config.data.customer.isActive ?? true })
            : this.crmApi.createCustomer(request);

        apiCall.subscribe({
            next: (result) => {
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.savedSuccessfully') });
                this.ref.close(result);
            },
            error: (err) => { this.saving = false; this.errorMessage = err.error?.message || this.translateService.instant('messages.errorSavingItem'); }
        });
    }
}
