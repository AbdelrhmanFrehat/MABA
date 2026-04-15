import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { CurrencySelectComponent } from '../../../shared/components/currency-select/currency-select';
import { LookupDropdownComponent } from '../../../shared/components/lookup-dropdown/lookup-dropdown';
import { AdminUserAutocompleteComponent } from '../../../shared/components/admin-user-autocomplete/admin-user-autocomplete';
import { AssetsService } from '../../../shared/services/assets.service';
import { AssetCategory, CreateAssetRequest } from '../../../shared/models/asset.model';

@Component({
    selector: 'app-asset-form',
    standalone: true,
    imports: [
        CommonModule, RouterModule, ReactiveFormsModule,
        ButtonModule, CardModule, InputNumberModule, InputTextModule, TextareaModule,
        SelectModule, DatePickerModule, MessageModule, ToastModule, TranslateModule,
        CurrencySelectComponent, LookupDropdownComponent, AdminUserAutocompleteComponent
    ],
    providers: [MessageService],
    styles: [`
        .grid-3 { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1.25rem; }
        .field { display: flex; flex-direction: column; gap: 0.4rem; }
        .field-full { grid-column: 1 / -1; }
        @media (max-width: 640px) { .grid-3 { grid-template-columns: 1fr; } .field-full { grid-column: 1; } }
    `],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <h1 class="text-2xl font-bold m-0">{{ isEdit ? 'Edit Asset' : 'New Asset' }}</h1>
                <p-button label="Back" severity="secondary" [outlined]="true" routerLink="/admin/assets"></p-button>
            </div>

            <p-card>
                @if (errorMessage) {
                    <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
                }

                <form [formGroup]="form" (ngSubmit)="save()">
                    <div class="grid-3">
                        <div class="field">
                            <label class="font-medium">Name (English) *</label>
                            <input pInputText formControlName="nameEn" class="w-full" />
                        </div>
                        <div class="field">
                            <label class="font-medium">Name (Arabic) *</label>
                            <input pInputText formControlName="nameAr" class="w-full" />
                        </div>
                        <div class="field">
                            <label class="font-medium">Category *</label>
                            <p-select formControlName="assetCategoryId" [options]="categories" optionLabel="nameEn" optionValue="id" [filter]="true" placeholder="Select category" styleClass="w-full"></p-select>
                        </div>

                        <div class="field">
                            <label class="font-medium">Investor (Admin) *</label>
                            <app-admin-user-autocomplete formControlName="investorUserId" placeholder="Search admin..."></app-admin-user-autocomplete>
                        </div>
                        <div class="field">
                            <label class="font-medium">Acquisition Condition *</label>
                            <app-lookup-dropdown lookupTypeKey="AssetCondition" formControlName="acquisitionConditionId" placeholder="Select condition"></app-lookup-dropdown>
                        </div>
                        <div class="field">
                            <label class="font-medium">Status *</label>
                            <app-lookup-dropdown lookupTypeKey="AssetStatus" formControlName="statusId" placeholder="Select status"></app-lookup-dropdown>
                        </div>

                        <div class="field">
                            <label class="font-medium">Approximate Price *</label>
                            <p-inputNumber formControlName="approximatePrice" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber>
                        </div>
                        <div class="field">
                            <label class="font-medium">Currency</label>
                            <app-currency-select formControlName="currency"></app-currency-select>
                        </div>
                        <div class="field">
                            <label class="font-medium">Acquired At *</label>
                            <p-datepicker formControlName="acquiredAt" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker>
                        </div>

                        <div class="field field-full">
                            <label class="font-medium">Condition Notes</label>
                            <textarea pTextarea rows="2" class="w-full" formControlName="conditionNotes"></textarea>
                        </div>
                        <div class="field field-full">
                            <label class="font-medium">Location Notes</label>
                            <input pInputText formControlName="locationNotes" class="w-full" />
                        </div>
                        <div class="field field-full">
                            <label class="font-medium">Description (English)</label>
                            <textarea pTextarea rows="2" class="w-full" formControlName="descriptionEn"></textarea>
                        </div>
                        <div class="field field-full">
                            <label class="font-medium">Description (Arabic)</label>
                            <textarea pTextarea rows="2" class="w-full" formControlName="descriptionAr"></textarea>
                        </div>
                    </div>

                    <div class="flex justify-end gap-2 mt-4">
                        <p-button label="Cancel" severity="secondary" [outlined]="true" routerLink="/admin/assets" [disabled]="saving"></p-button>
                        <p-button label="Save" type="submit" [loading]="saving" [disabled]="form.invalid || saving"></p-button>
                    </div>
                </form>
            </p-card>
        </div>
    `
})
export class AssetFormComponent implements OnInit {
    categories: AssetCategory[] = [];
    saving = false;
    errorMessage = '';
    isEdit = false;
    assetId: string | null = null;

    form = inject(FormBuilder).group({
        nameEn: ['', Validators.required],
        nameAr: ['', Validators.required],
        descriptionEn: [''],
        descriptionAr: [''],
        assetCategoryId: ['', Validators.required],
        investorUserId: ['', Validators.required],
        acquisitionConditionId: ['', Validators.required],
        conditionNotes: [''],
        approximatePrice: [0, [Validators.required, Validators.min(0)]],
        currency: ['ILS', [Validators.required, Validators.maxLength(3)]],
        acquiredAt: [new Date(), Validators.required],
        statusId: ['', Validators.required],
        locationNotes: [''],
        photoMediaId: [null as string | null]
    });

    private svc = inject(AssetsService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private msg = inject(MessageService);

    ngOnInit() {
        this.svc.getCategories().subscribe({
            next: c => this.categories = c.filter(x => x.isActive),
            error: err => this.errorMessage = err.error?.message || 'Failed to load categories.'
        });

        this.assetId = this.route.snapshot.paramMap.get('id');
        if (this.assetId) {
            this.isEdit = true;
            this.svc.getAsset(this.assetId).subscribe({
                next: a => {
                    this.form.patchValue({
                        nameEn: a.nameEn,
                        nameAr: a.nameAr,
                        descriptionEn: a.descriptionEn || '',
                        descriptionAr: a.descriptionAr || '',
                        assetCategoryId: a.assetCategoryId,
                        investorUserId: a.investorUserId,
                        acquisitionConditionId: a.acquisitionConditionId,
                        conditionNotes: a.conditionNotes || '',
                        approximatePrice: a.approximatePrice,
                        currency: a.currency,
                        acquiredAt: new Date(a.acquiredAt) as any,
                        statusId: a.statusId,
                        locationNotes: a.locationNotes || '',
                        photoMediaId: a.photoMediaId || null
                    });
                }
            });
        }
    }

    save() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        const v = this.form.getRawValue();
        const payload: CreateAssetRequest = {
            nameEn: v.nameEn || '',
            nameAr: v.nameAr || '',
            descriptionEn: v.descriptionEn || undefined,
            descriptionAr: v.descriptionAr || undefined,
            assetCategoryId: v.assetCategoryId || '',
            investorUserId: v.investorUserId || '',
            acquisitionConditionId: v.acquisitionConditionId || '',
            conditionNotes: v.conditionNotes || undefined,
            approximatePrice: v.approximatePrice ?? 0,
            currency: (v.currency || 'ILS').toUpperCase(),
            acquiredAt: v.acquiredAt ? (v.acquiredAt as any).toISOString() : new Date().toISOString(),
            statusId: v.statusId || '',
            locationNotes: v.locationNotes || undefined,
            photoMediaId: v.photoMediaId || undefined
        };
        this.saving = true;
        this.errorMessage = '';
        const obs = this.isEdit && this.assetId
            ? this.svc.update(this.assetId, payload)
            : this.svc.create(payload);
        obs.subscribe({
            next: () => {
                this.msg.add({ severity: 'success', summary: 'Saved', detail: 'Asset saved.' });
                this.router.navigate(['/admin/assets']);
            },
            error: err => {
                this.saving = false;
                this.errorMessage = err.error?.message || err.error?.title || 'Failed to save.';
            }
        });
    }
}
