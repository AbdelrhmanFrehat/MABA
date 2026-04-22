import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import {
    FilamentSpool,
    Material,
    MaterialColor
} from '../../../shared/models/printing.model';

@Component({
    selector: 'app-filament-spools-dialog',
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
        TranslateModule
    ],
    providers: [MessageService],
    templateUrl: './filament-spools-dialog.component.html',
    styles: [
        `
            .form-field label {
                display: block;
                margin-bottom: 0.35rem;
                font-weight: 600;
                font-size: 0.875rem;
            }
            .form-actions {
                display: flex;
                justify-content: flex-end;
                gap: 0.5rem;
                margin-top: 1rem;
                padding-top: 1rem;
                border-top: 1px solid var(--surface-border);
            }
            .readonly-block {
                padding: 0.75rem 1rem;
                border-radius: var(--border-radius);
                background: var(--surface-ground);
            }
            .field-row {
                display: flex;
                justify-content: space-between;
                gap: 1rem;
                margin-bottom: 0.35rem;
            }
            .field-row .label {
                color: var(--text-color-secondary);
                font-size: 0.85rem;
            }
            .name-hint {
                margin: 0.35rem 0 0;
                font-size: 0.75rem;
                color: var(--text-color-secondary);
                line-height: 1.4;
            }
        `
    ]
})
export class FilamentSpoolsDialogComponent implements OnInit {
    form!: FormGroup;
    isEdit = false;
    spool: FilamentSpool | null = null;
    saving = false;

    materialOptions: { label: string; value: string }[] = [];
    colorOptions: { label: string; value: string }[] = [];

    lang: string;

    private fb = inject(FormBuilder);
    private printingApi = inject(PrintingApiService);
    private ref = inject(DynamicDialogRef);
    private config = inject(DynamicDialogConfig);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private languageService = inject(LanguageService);

    constructor() {
        this.lang = this.languageService.language;
    }

    ngOnInit() {
        const mode = this.config.data?.mode as 'create' | 'edit';
        this.spool = this.config.data?.spool ?? null;
        this.isEdit = mode === 'edit' && !!this.spool;

        if (this.isEdit && this.spool) {
            this.form = this.fb.group({
                name: [this.spool.name ?? ''],
                remainingWeightGrams: [this.spool.remainingWeightGrams, [Validators.required, Validators.min(0)]],
                isActive: [this.spool.isActive]
            });
        } else {
            this.form = this.fb.group({
                materialId: [null, Validators.required],
                materialColorId: [null as string | null, Validators.required],
                name: [''],
                initialWeightGrams: [1000, [Validators.required, Validators.min(1)]]
            });
            this.form.get('materialId')?.valueChanges.subscribe((mid) => {
                this.form.patchValue({ materialColorId: null }, { emitEvent: false });
                this.loadColorsForMaterial(mid as string | null);
            });
        }

        this.printingApi.getAllMaterials().subscribe({
            next: (materials: Material[]) => {
                this.materialOptions = materials
                    .filter((m) => m.isActive)
                    .map((m) => ({
                        label: this.lang === 'ar' && m.nameAr ? m.nameAr : m.nameEn,
                        value: m.id
                    }));
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('messages.errorLoadingMaterials'),
                    life: 5000
                });
            }
        });
    }

    private loadColorsForMaterial(mid: string | null) {
        this.colorOptions = [];
        if (!mid) {
            return;
        }
        this.printingApi.getAllMaterialColors(mid).subscribe({
            next: (colors: MaterialColor[]) => {
                this.colorOptions = colors.map((c) => ({
                    label: `${this.lang === 'ar' && c.nameAr ? c.nameAr : c.nameEn} (${c.hexCode})`,
                    value: c.id
                }));
            },
            error: () => {
                this.colorOptions = [];
            }
        });
    }

    onSubmit() {
        if (this.form.invalid || this.saving) {
            return;
        }
        this.saving = true;
        if (this.isEdit && this.spool) {
            const v = this.form.getRawValue();
            this.printingApi
                .updateFilamentSpool(this.spool.id, {
                    name: v.name?.trim() ? v.name.trim() : null,
                    remainingWeightGrams: v.remainingWeightGrams,
                    isActive: v.isActive
                })
                .subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translate.instant('messages.success'),
                            detail: this.translate.instant('admin.filamentSpools.saved'),
                            life: 3000
                        });
                        this.ref.close(true);
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translate.instant('messages.error'),
                            detail: this.translate.instant('admin.filamentSpools.saveError'),
                            life: 5000
                        });
                        this.saving = false;
                    }
                });
        } else {
            const v = this.form.getRawValue();
            this.printingApi
                .createFilamentSpool({
                    materialId: v.materialId,
                    materialColorId: v.materialColorId,
                    name: v.name?.trim() ? v.name.trim() : null,
                    initialWeightGrams: v.initialWeightGrams
                })
                .subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translate.instant('messages.success'),
                            detail: this.translate.instant('admin.filamentSpools.saved'),
                            life: 3000
                        });
                        this.ref.close(true);
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translate.instant('messages.error'),
                            detail: this.translate.instant('admin.filamentSpools.saveError'),
                            life: 5000
                        });
                        this.saving = false;
                    }
                });
        }
    }

    cancel() {
        this.ref.close(false);
    }
}
