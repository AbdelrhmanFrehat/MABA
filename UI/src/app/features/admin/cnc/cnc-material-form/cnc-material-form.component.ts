import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CncApiService } from '../../../../shared/services/cnc-api.service';
import {
    CncMaterial,
    CreateCncMaterialRequest,
    UpdateCncMaterialRequest
} from '../../../../shared/models/cnc.model';

@Component({
    selector: 'app-cnc-material-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        TextareaModule,
        CheckboxModule,
        SelectModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="materialForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field">
                    <label for="nameEn">
                        {{ 'admin.cncMaterials.nameEn' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input
                        pInputText
                        id="nameEn"
                        formControlName="nameEn"
                        class="w-full"
                        maxlength="200"
                        [class.ng-invalid]="materialForm.get('nameEn')?.invalid && materialForm.get('nameEn')?.touched"
                    />
                    @if (materialForm.get('nameEn')?.invalid && materialForm.get('nameEn')?.touched) {
                        <small class="p-error">{{ getErrorMessage('nameEn') }}</small>
                    }
                </div>

                <div class="form-field">
                    <label for="nameAr">{{ 'admin.cncMaterials.nameAr' | translate }}</label>
                    <input
                        pInputText
                        id="nameAr"
                        formControlName="nameAr"
                        class="w-full"
                        maxlength="200"
                    />
                </div>

                <div class="form-field">
                    <label for="type">{{ 'admin.cncMaterials.type' | translate }} <span class="text-red-500">*</span></label>
                    <p-select
                        id="type"
                        formControlName="type"
                        [options]="modeOptions"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="form-field">
                    <label for="sortOrder">{{ 'admin.cncMaterials.sortOrder' | translate }}</label>
                    <p-inputNumber
                        id="sortOrder"
                        formControlName="sortOrder"
                        mode="decimal"
                        [minFractionDigits]="0"
                        [maxFractionDigits]="0"
                        styleClass="w-full"
                    ></p-inputNumber>
                </div>

                <div class="form-field form-field-full section-title">
                    <h3>{{ 'admin.cncMaterials.sectionNotes' | translate }}</h3>
                </div>

                <div class="form-field form-field-full">
                    <label for="notesEn">{{ 'admin.cncMaterials.notesEn' | translate }}</label>
                    <textarea pInputTextarea id="notesEn" formControlName="notesEn" rows="2" class="w-full" maxlength="2000"></textarea>
                </div>

                <div class="form-field form-field-full">
                    <label for="notesAr">{{ 'admin.cncMaterials.notesAr' | translate }}</label>
                    <textarea pInputTextarea id="notesAr" formControlName="notesAr" rows="2" class="w-full" maxlength="2000"></textarea>
                </div>

                @if (showRoutingFields()) {
                    <div class="form-field form-field-full section-title">
                        <h3>{{ 'admin.cncMaterials.sectionRouting' | translate }}</h3>
                    </div>
                    <div class="form-field">
                        <label for="minThicknessMm">{{ 'admin.cncMaterials.minThickness' | translate }} (mm)</label>
                        <p-inputNumber
                            id="minThicknessMm"
                            formControlName="minThicknessMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label for="maxThicknessMm">{{ 'admin.cncMaterials.maxThickness' | translate }} (mm)</label>
                        <p-inputNumber
                            id="maxThicknessMm"
                            formControlName="maxThicknessMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                    <div class="form-field form-field-full ops-grid">
                        <span class="ops-label">{{ 'admin.cncMaterials.supportedOperations' | translate }}</span>
                        <div class="checkbox-field">
                            <p-checkbox formControlName="allowCut" binary inputId="allowCut"></p-checkbox>
                            <label for="allowCut">{{ 'admin.cncMaterials.opCut' | translate }}</label>
                        </div>
                        <div class="checkbox-field">
                            <p-checkbox formControlName="allowEngrave" binary inputId="allowEngrave"></p-checkbox>
                            <label for="allowEngrave">{{ 'admin.cncMaterials.opEngrave' | translate }}</label>
                        </div>
                        <div class="checkbox-field">
                            <p-checkbox formControlName="allowPocket" binary inputId="allowPocket"></p-checkbox>
                            <label for="allowPocket">{{ 'admin.cncMaterials.opPocket' | translate }}</label>
                        </div>
                        <div class="checkbox-field">
                            <p-checkbox formControlName="allowDrill" binary inputId="allowDrill"></p-checkbox>
                            <label for="allowDrill">{{ 'admin.cncMaterials.opDrill' | translate }}</label>
                        </div>
                    </div>
                    <div class="form-field">
                        <label for="maxCutDepthMm">{{ 'admin.cncMaterials.maxCutDepth' | translate }}</label>
                        <p-inputNumber
                            id="maxCutDepthMm"
                            formControlName="maxCutDepthMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label for="maxEngraveDepthMm">{{ 'admin.cncMaterials.maxEngraveDepth' | translate }}</label>
                        <p-inputNumber
                            id="maxEngraveDepthMm"
                            formControlName="maxEngraveDepthMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label for="maxPocketDepthMm">{{ 'admin.cncMaterials.maxPocketDepth' | translate }}</label>
                        <p-inputNumber
                            id="maxPocketDepthMm"
                            formControlName="maxPocketDepthMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label for="maxDrillDepthMm">{{ 'admin.cncMaterials.maxDrillDepth' | translate }}</label>
                        <p-inputNumber
                            id="maxDrillDepthMm"
                            formControlName="maxDrillDepthMm"
                            [minFractionDigits]="1"
                            [maxFractionDigits]="2"
                            mode="decimal"
                            styleClass="w-full"
                        ></p-inputNumber>
                    </div>
                }

                @if (showPcbFields()) {
                    <div class="form-field form-field-full section-title">
                        <h3>{{ 'admin.cncMaterials.sectionPcb' | translate }}</h3>
                    </div>
                    <div class="form-field">
                        <label for="pcbMaterialType">{{ 'admin.cncMaterials.pcbMaterialType' | translate }}</label>
                        <input pInputText id="pcbMaterialType" formControlName="pcbMaterialType" class="w-full" maxlength="80" />
                    </div>
                    <div class="form-field form-field-full">
                        <label for="supportedBoardThicknesses">{{ 'admin.cncMaterials.supportedBoardThicknesses' | translate }}</label>
                        <input
                            pInputText
                            id="supportedBoardThicknesses"
                            formControlName="supportedBoardThicknesses"
                            class="w-full"
                            maxlength="500"
                        />
                        <small class="helper-text">{{ 'admin.cncMaterials.supportedBoardThicknessesHint' | translate }}</small>
                    </div>
                    <div class="form-field">
                        <div class="checkbox-field">
                            <p-checkbox formControlName="supportsSingleSided" binary inputId="supportsSingleSided"></p-checkbox>
                            <label for="supportsSingleSided">{{ 'admin.cncMaterials.supportsSingleSided' | translate }}</label>
                        </div>
                    </div>
                    <div class="form-field">
                        <div class="checkbox-field">
                            <p-checkbox formControlName="supportsDoubleSided" binary inputId="supportsDoubleSided"></p-checkbox>
                            <label for="supportsDoubleSided">{{ 'admin.cncMaterials.supportsDoubleSided' | translate }}</label>
                        </div>
                    </div>
                }

                <div class="form-field form-field-full section-title">
                    <h3>{{ 'admin.cncMaterials.sectionFlags' | translate }}</h3>
                </div>
                <div class="form-field">
                    <div class="checkbox-field">
                        <p-checkbox formControlName="isMetal" binary inputId="isMetal"></p-checkbox>
                        <label for="isMetal">{{ 'admin.cncMaterials.isMetal' | translate }}</label>
                    </div>
                </div>
                <div class="form-field">
                    <div class="checkbox-field">
                        <p-checkbox formControlName="isActive" binary inputId="isActive"></p-checkbox>
                        <label for="isActive">{{ 'admin.cncMaterials.isActive' | translate }}</label>
                    </div>
                </div>
            </div>

            <div class="form-actions">
                <p-button
                    [label]="'common.cancel' | translate"
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="saving"
                    type="button"
                    styleClass="w-full md:w-auto"
                ></p-button>
                <p-button
                    [label]="'common.save' | translate"
                    type="submit"
                    [loading]="saving"
                    [disabled]="materialForm.invalid || saving"
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
            max-height: 70vh;
            overflow-y: auto;
            padding-right: 0.25rem;
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
        .section-title h3 {
            margin: 0.5rem 0 0;
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-color-secondary);
        }
        .checkbox-field {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        .checkbox-field label {
            margin: 0;
            cursor: pointer;
            font-weight: 500;
        }
        .ops-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
            gap: 0.75rem;
            align-items: center;
        }
        .ops-label {
            grid-column: 1 / -1;
            font-size: 0.875rem;
            font-weight: 600;
        }
        .helper-text {
            font-size: 0.75rem;
            color: var(--text-color-secondary);
        }
        .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
            margin-top: 1.5rem;
        }
    `]
})
export class CncMaterialFormComponent implements OnInit {
    materialForm!: FormGroup;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    materialId: string | null = null;

    modeOptions: { label: string; value: string }[] = [];

    private fb = inject(FormBuilder);
    private cncApiService = inject(CncApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.materialForm = this.fb.group({
            nameEn: ['', [Validators.required, Validators.maxLength(200)]],
            nameAr: [''],
            type: ['routing', Validators.required],
            sortOrder: [0],
            notesEn: [''],
            notesAr: [''],
            minThicknessMm: [null as number | null],
            maxThicknessMm: [null as number | null],
            allowCut: [true],
            allowEngrave: [true],
            allowPocket: [true],
            allowDrill: [true],
            maxCutDepthMm: [null as number | null],
            maxEngraveDepthMm: [null as number | null],
            maxPocketDepthMm: [null as number | null],
            maxDrillDepthMm: [null as number | null],
            pcbMaterialType: [''],
            supportedBoardThicknesses: [''],
            supportsSingleSided: [true],
            supportsDoubleSided: [true],
            isMetal: [false],
            isActive: [true]
        });
    }

    ngOnInit() {
        this.refreshModeLabels();

        if (this.config.data?.material) {
            const m: CncMaterial = this.config.data.material;
            this.isEditMode = true;
            this.materialId = m.id;
            this.materialForm.patchValue({
                nameEn: m.nameEn,
                nameAr: m.nameAr || '',
                type: m.type,
                sortOrder: m.sortOrder,
                notesEn: m.notesEn || '',
                notesAr: m.notesAr || '',
                minThicknessMm: m.minThicknessMm,
                maxThicknessMm: m.maxThicknessMm,
                allowCut: m.allowCut,
                allowEngrave: m.allowEngrave,
                allowPocket: m.allowPocket,
                allowDrill: m.allowDrill,
                maxCutDepthMm: m.maxCutDepthMm,
                maxEngraveDepthMm: m.maxEngraveDepthMm,
                maxPocketDepthMm: m.maxPocketDepthMm,
                maxDrillDepthMm: m.maxDrillDepthMm,
                pcbMaterialType: m.pcbMaterialType || '',
                supportedBoardThicknesses: m.supportedBoardThicknesses || '',
                supportsSingleSided: m.supportsSingleSided ?? true,
                supportsDoubleSided: m.supportsDoubleSided ?? true,
                isMetal: m.isMetal,
                isActive: m.isActive
            });
        }
    }

    refreshModeLabels() {
        this.modeOptions = [
            { label: this.translateService.instant('admin.cncMaterials.typeRouting'), value: 'routing' },
            { label: this.translateService.instant('admin.cncMaterials.typePcb'), value: 'pcb' },
            { label: this.translateService.instant('admin.cncMaterials.typeBoth'), value: 'both' }
        ];
    }

    showRoutingFields(): boolean {
        const t = this.materialForm.get('type')?.value;
        return t === 'routing' || t === 'both';
    }

    showPcbFields(): boolean {
        const t = this.materialForm.get('type')?.value;
        return t === 'pcb' || t === 'both';
    }

    /** Keeps API fields we do not edit in this dialog so PUT does not clear them. */
    private preserveUneditedFields(m: CncMaterial): Partial<CreateCncMaterialRequest> {
        return {
            descriptionEn: m.descriptionEn,
            descriptionAr: m.descriptionAr,
            cutNotesEn: m.cutNotesEn,
            cutNotesAr: m.cutNotesAr,
            engraveNotesEn: m.engraveNotesEn,
            engraveNotesAr: m.engraveNotesAr,
            pocketNotesEn: m.pocketNotesEn,
            pocketNotesAr: m.pocketNotesAr,
            drillNotesEn: m.drillNotesEn,
            drillNotesAr: m.drillNotesAr
        };
    }

    private buildPayload(): CreateCncMaterialRequest {
        const fv = this.materialForm.value;
        return {
            nameEn: fv.nameEn,
            nameAr: fv.nameAr || undefined,
            type: fv.type,
            sortOrder: fv.sortOrder ?? 0,
            notesEn: fv.notesEn || undefined,
            notesAr: fv.notesAr || undefined,
            isMetal: !!fv.isMetal,
            isActive: !!fv.isActive,
            minThicknessMm: fv.minThicknessMm ?? undefined,
            maxThicknessMm: fv.maxThicknessMm ?? undefined,
            allowCut: !!fv.allowCut,
            allowEngrave: !!fv.allowEngrave,
            allowPocket: !!fv.allowPocket,
            allowDrill: !!fv.allowDrill,
            maxCutDepthMm: fv.maxCutDepthMm ?? undefined,
            maxEngraveDepthMm: fv.maxEngraveDepthMm ?? undefined,
            maxPocketDepthMm: fv.maxPocketDepthMm ?? undefined,
            maxDrillDepthMm: fv.maxDrillDepthMm ?? undefined,
            pcbMaterialType: fv.pcbMaterialType?.trim() || undefined,
            supportedBoardThicknesses: fv.supportedBoardThicknesses?.trim() || undefined,
            supportsSingleSided: !!fv.supportsSingleSided,
            supportsDoubleSided: !!fv.supportsDoubleSided
        };
    }

    onSubmit() {
        if (this.materialForm.invalid) {
            this.markFormGroupTouched(this.materialForm);
            return;
        }

        this.saving = true;
        const payload = this.buildPayload();

        if (this.isEditMode && this.materialId && this.config.data?.material) {
            const m = this.config.data.material as CncMaterial;
            const update: UpdateCncMaterialRequest = {
                ...this.preserveUneditedFields(m),
                ...payload,
                id: this.materialId
            };
            this.cncApiService.updateMaterial(this.materialId, update).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('admin.cncMaterials.updatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(material);
                },
                error: (error) => this.handleError(error)
            });
        } else {
            this.cncApiService.createMaterial(payload).subscribe({
                next: (material) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('admin.cncMaterials.createdSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(material);
                },
                error: (error) => this.handleError(error)
            });
        }
    }

    handleError(error: unknown) {
        this.saving = false;
        const err = error as { error?: { message?: string } };
        this.errorMessage = err.error?.message || this.translateService.instant('admin.cncMaterials.errorSaving');
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.errorMessage,
            life: 5000
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.materialForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', {
                max: control.errors?.['maxlength'].requiredLength
            });
        }
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach((key) => {
            formGroup.get(key)?.markAsTouched();
        });
    }
}
