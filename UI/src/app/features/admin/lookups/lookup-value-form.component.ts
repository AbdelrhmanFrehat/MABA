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
import { LookupApiService } from '../../../shared/services/lookup-api.service';
import { LookupValue } from '../../../shared/models/lookup.model';

@Component({
    selector: 'app-lookup-value-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, InputNumberModule, CheckboxModule, MessageModule, ToastModule, TranslateModule],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }
            <div class="flex flex-col gap-4">
                <div class="form-field">
                    <label for="key">{{ 'admin.lookups.key' | translate }} <span class="text-red-500">*</span></label>
                    <input pInputText id="key" formControlName="key" class="w-full" [readonly]="isEditMode" />
                </div>
                <div class="form-field">
                    <label for="nameEn">{{ 'admin.lookups.nameEn' | translate }} <span class="text-red-500">*</span></label>
                    <input pInputText id="nameEn" formControlName="nameEn" class="w-full" />
                </div>
                <div class="form-field">
                    <label for="nameAr">{{ 'admin.lookups.nameAr' | translate }} <span class="text-red-500">*</span></label>
                    <input pInputText id="nameAr" formControlName="nameAr" class="w-full" />
                </div>
                <div class="form-field">
                    <label for="description">{{ 'common.description' | translate }}</label>
                    <input pInputText id="description" formControlName="description" class="w-full" />
                </div>
                <div class="form-field">
                    <label for="sortOrder">{{ 'admin.lookups.sortOrder' | translate }}</label>
                    <p-inputNumber id="sortOrder" formControlName="sortOrder" [min]="0" styleClass="w-full"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label for="color">{{ 'admin.lookups.color' | translate }}</label>
                    <input pInputText id="color" formControlName="color" class="w-full" placeholder="#22c55e" />
                </div>
                <div class="form-field">
                    <label for="icon">{{ 'admin.lookups.icon' | translate }}</label>
                    <input pInputText id="icon" formControlName="icon" class="w-full" placeholder="pi pi-check" />
                </div>
                <div class="flex gap-4">
                    <div class="flex items-center gap-2">
                        <p-checkbox id="isDefault" formControlName="isDefault" binary></p-checkbox>
                        <label for="isDefault">{{ 'admin.lookups.isDefault' | translate }}</label>
                    </div>
                    <div class="flex items-center gap-2">
                        <p-checkbox id="isActive" formControlName="isActive" binary></p-checkbox>
                        <label for="isActive">{{ 'common.active' | translate }}</label>
                    </div>
                </div>
            </div>
            <div class="flex justify-end gap-2 mt-4">
                <p-button [label]="'common.cancel' | translate" [outlined]="true" (click)="ref.close(false)" [disabled]="saving"></p-button>
                <p-button [label]="'common.save' | translate" type="submit" [loading]="saving" [disabled]="form.invalid || saving"></p-button>
            </div>
        </form>
    `,
    styles: [`.form-field { display: flex; flex-direction: column; gap: 0.5rem; } .form-field label { font-size: 0.875rem; font-weight: 600; }`]
})
export class LookupValueFormComponent implements OnInit {
    form!: FormGroup;
    isEditMode = false;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private lookupApi = inject(LookupApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(public ref: DynamicDialogRef, public config: DynamicDialogConfig) {
        this.form = this.fb.group({
            key: ['', [Validators.required, Validators.maxLength(100)]],
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: ['', [Validators.required, Validators.maxLength(200)]],
            description: [''],
            sortOrder: [0],
            color: [''],
            icon: [''],
            isDefault: [false],
            isActive: [true]
        });
    }

    ngOnInit() {
        if (this.config.data?.lookupValue) {
            const lv: LookupValue = this.config.data.lookupValue;
            this.isEditMode = true;
            this.form.patchValue(lv);
        }
    }

    onSubmit() {
        if (this.form.invalid) return;
        this.saving = true;
        this.errorMessage = '';

        if (this.isEditMode) {
            this.lookupApi.updateLookupValue(this.config.data.lookupValue.id, this.form.value).subscribe({
                next: (result) => { this.ref.close(result); },
                error: (err) => { this.saving = false; this.errorMessage = err.error?.message || this.translateService.instant('messages.errorSavingItem'); }
            });
        } else {
            this.lookupApi.createLookupValue({
                ...this.form.value,
                lookupTypeId: this.config.data.lookupTypeId
            }).subscribe({
                next: (result) => { this.ref.close(result); },
                error: (err) => { this.saving = false; this.errorMessage = err.error?.message || this.translateService.instant('messages.errorSavingItem'); }
            });
        }
    }
}
