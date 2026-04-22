import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
    AppAnnouncementsApiService,
    AppAnnouncementDto
} from '../../../shared/services/app-announcements-api.service';

const TYPE_OPTIONS = [
    { label: 'System', value: 'System' },
    { label: 'Machine', value: 'Machine' },
    { label: 'Module', value: 'Module' },
    { label: 'Catalog', value: 'Catalog' },
    { label: 'Info', value: 'Info' }
];

const PLATFORM_OPTIONS = [
    { label: 'All', value: 'All' },
    { label: 'Desktop', value: 'Desktop' },
    { label: 'Web', value: 'Web' }
];

@Component({
    selector: 'app-announcement-form-dialog',
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
    template: `
        <p-toast />
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="dialog-form">
            <div class="form-field">
                <label>{{ 'admin.appAnnouncements.message' | translate }} *</label>
                <input pInputText formControlName="message" class="w-full" [placeholder]="'admin.appAnnouncements.messagePlaceholder' | translate" />
            </div>

            <div class="form-row">
                <div class="form-field flex-1">
                    <label>{{ 'admin.appAnnouncements.type' | translate }}</label>
                    <p-select
                        formControlName="type"
                        [options]="typeOptions"
                        optionLabel="label"
                        optionValue="value"
                        [showClear]="true"
                        [placeholder]="'admin.appAnnouncements.typePlaceholder' | translate"
                        class="w-full"
                    ></p-select>
                </div>
                <div class="form-field flex-1">
                    <label>{{ 'admin.appAnnouncements.targetPlatform' | translate }}</label>
                    <p-select
                        formControlName="targetPlatform"
                        [options]="platformOptions"
                        optionLabel="label"
                        optionValue="value"
                        class="w-full"
                    ></p-select>
                </div>
            </div>

            <div class="form-field">
                <label>{{ 'admin.appAnnouncements.displayOrder' | translate }}</label>
                <p-inputNumber formControlName="displayOrder" [min]="0" [showButtons]="true" class="w-full"></p-inputNumber>
            </div>

            <div class="form-row">
                <div class="form-field flex-1">
                    <label>{{ 'admin.appAnnouncements.startsAt' | translate }}</label>
                    <input type="datetime-local" pInputText formControlName="startsAt" class="w-full" />
                </div>
                <div class="form-field flex-1">
                    <label>{{ 'admin.appAnnouncements.endsAt' | translate }}</label>
                    <input type="datetime-local" pInputText formControlName="endsAt" class="w-full" />
                </div>
            </div>

            <div class="form-field flex align-items-center gap-2">
                <p-checkbox formControlName="isActive" [binary]="true" inputId="isActive"></p-checkbox>
                <label for="isActive">{{ 'admin.appAnnouncements.isActive' | translate }}</label>
            </div>

            <div class="form-actions">
                <p-button [label]="'common.cancel' | translate" [outlined]="true" (click)="cancel()"></p-button>
                <p-button
                    type="submit"
                    [label]="'common.save' | translate"
                    [loading]="saving"
                    [disabled]="form.invalid"
                ></p-button>
            </div>
        </form>
    `,
    styles: [`
        .dialog-form { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.35rem; font-weight: 600; font-size: 0.875rem; }
        .form-row { display: flex; gap: 1rem; }
        .form-row .form-field { flex: 1; min-width: 0; }
        .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem; padding-top: 1rem; border-top: 1px solid var(--surface-border); }
    `]
})
export class AppAnnouncementFormDialogComponent implements OnInit {
    form!: FormGroup;
    saving = false;
    isEdit = false;

    typeOptions = TYPE_OPTIONS;
    platformOptions = PLATFORM_OPTIONS;

    private fb = inject(FormBuilder);
    private api = inject(AppAnnouncementsApiService);
    private ref = inject(DynamicDialogRef);
    private config = inject(DynamicDialogConfig);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    ngOnInit(): void {
        const item: AppAnnouncementDto | null = this.config.data?.item ?? null;
        this.isEdit = !!item;

        this.form = this.fb.group({
            message: [item?.message ?? '', [Validators.required, Validators.maxLength(1000)]],
            type: [item?.type ?? null],
            isActive: [item?.isActive ?? true],
            displayOrder: [item?.displayOrder ?? 0, [Validators.required, Validators.min(0)]],
            targetPlatform: [item?.targetPlatform ?? 'All', Validators.required],
            startsAt: [item?.startsAt ? this.toDateTimeLocal(item.startsAt) : null],
            endsAt: [item?.endsAt ? this.toDateTimeLocal(item.endsAt) : null]
        });
    }

    onSubmit(): void {
        if (this.form.invalid || this.saving) return;
        this.saving = true;

        const v = this.form.getRawValue();
        const payload = {
            message: v.message.trim(),
            type: v.type || null,
            isActive: v.isActive,
            displayOrder: v.displayOrder,
            targetPlatform: v.targetPlatform,
            startsAt: v.startsAt ? new Date(v.startsAt).toISOString() : null,
            endsAt: v.endsAt ? new Date(v.endsAt).toISOString() : null
        };

        const item: AppAnnouncementDto | null = this.config.data?.item ?? null;
        const call = this.isEdit && item
            ? this.api.update(item.id, payload)
            : this.api.create(payload);

        call.subscribe({
            next: () => {
                this.saving = false;
                this.ref.close(true);
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('messages.saveFailed'),
                    life: 5000
                });
                this.saving = false;
            }
        });
    }

    cancel(): void {
        this.ref.close(false);
    }

    private toDateTimeLocal(iso: string): string {
        const d = new Date(iso);
        const pad = (n: number) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }
}
