import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { environment } from '../../../../environments/environment';
import { MediaApiService } from '../../../shared/services/media-api.service';
import {
    AppAnnouncementsApiService,
    AppAnnouncementDto
} from '../../../shared/services/app-announcements-api.service';

const DEFAULT_IMAGE_MEDIA_TYPE_ID = '00000000-0000-0000-0000-000000000001';

function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
    const starts = group.get('startsAt')?.value;
    const ends = group.get('endsAt')?.value;
    if (starts && ends && new Date(starts) > new Date(ends)) {
        return { dateRange: true };
    }
    return null;
}

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
            <small class="date-range-error" *ngIf="form.hasError('dateRange')">
                <i class="pi pi-exclamation-triangle"></i>
                "Starts At" must be before "Ends At".
            </small>

            <!-- Image section -->
            <div class="form-field">
                <label>{{ 'admin.appAnnouncements.image' | translate }}</label>
                <input #imgFileInput type="file" accept="image/*" class="hidden-file-input" (change)="onImageFileSelected($event)" />

                <div class="image-section">
                    <!-- Current / preview -->
                    <div class="image-preview-wrap" *ngIf="resolvedPreviewUrl">
                        <img [src]="resolvedPreviewUrl" alt="preview" class="image-preview" />
                        <p-button
                            icon="pi pi-times"
                            [rounded]="true"
                            severity="danger"
                            size="small"
                            class="remove-btn"
                            (click)="clearImage()"
                            pTooltip="Remove image"
                        ></p-button>
                    </div>

                    <div class="image-upload-row">
                        <p-button
                            [label]="resolvedPreviewUrl ? ('admin.appAnnouncements.replaceImage' | translate) : ('admin.appAnnouncements.uploadImage' | translate)"
                            icon="pi pi-upload"
                            [outlined]="true"
                            size="small"
                            [loading]="uploadingImage"
                            [disabled]="uploadingImage"
                            (click)="imgFileInput.click()"
                        ></p-button>
                        <small class="text-color-secondary">{{ 'admin.appAnnouncements.imageHint' | translate }}</small>
                    </div>
                </div>

                <div class="form-field mt-2" *ngIf="resolvedPreviewUrl">
                    <label style="font-weight:500">{{ 'admin.appAnnouncements.imageAltText' | translate }}</label>
                    <input pInputText formControlName="imageAltText" class="w-full" placeholder="Describe the image..." />
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
                    [disabled]="form.invalid || uploadingImage"
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
        .date-range-error { display: flex; align-items: center; gap: 0.35rem; color: #ef4444; font-size: 0.8rem; margin-top: -0.5rem; margin-bottom: 0.75rem; }
        .hidden-file-input { display: none; }
        .image-section { display: flex; flex-direction: column; gap: 0.6rem; }
        .image-preview-wrap { position: relative; display: inline-block; }
        .image-preview { width: 100%; max-height: 120px; object-fit: cover; border-radius: 8px; border: 1px solid var(--surface-border); display: block; }
        .remove-btn { position: absolute; top: 0.3rem; right: 0.3rem; }
        .image-upload-row { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
        .mt-2 { margin-top: 0.5rem; }
    `]
})
export class AppAnnouncementFormDialogComponent implements OnInit {
    form!: FormGroup;
    saving = false;
    isEdit = false;
    uploadingImage = false;

    typeOptions = TYPE_OPTIONS;
    platformOptions = PLATFORM_OPTIONS;

    private fb = inject(FormBuilder);
    private api = inject(AppAnnouncementsApiService);
    private mediaApi = inject(MediaApiService);
    private ref = inject(DynamicDialogRef);
    private config = inject(DynamicDialogConfig);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    /** Blob URL for a newly picked (not yet saved) file preview */
    private blobPreviewUrl = '';

    get resolvedPreviewUrl(): string {
        if (this.blobPreviewUrl) return this.blobPreviewUrl;
        const url = this.form?.get('imageUrl')?.value as string | null;
        if (!url?.trim()) return '';
        if (url.startsWith('http') || url.startsWith('blob:')) return url;
        const base = (environment.apiUrl ?? '').replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '');
        return base ? `${base}${url.startsWith('/') ? url : '/' + url}` : url;
    }

    ngOnInit(): void {
        const item: AppAnnouncementDto | null = this.config.data?.item ?? null;
        this.isEdit = !!item;

        this.form = this.fb.group({
            message: [item?.message ?? '', [Validators.required, Validators.maxLength(1000)]],
            type: [item?.type ?? null],
            isActive: [item?.isActive ?? true],
            displayOrder: [item?.displayOrder ?? 0, [Validators.required, Validators.min(0)]],
            targetPlatform: [item?.targetPlatform ?? 'Desktop', Validators.required],
            startsAt: [item?.startsAt ? this.toDateTimeLocal(item.startsAt) : null],
            endsAt: [item?.endsAt ? this.toDateTimeLocal(item.endsAt) : null],
            imageUrl: [item?.imageUrl ?? null],
            imageAltText: [item?.imageAltText ?? null, Validators.maxLength(200)]
        }, { validators: dateRangeValidator });
    }

    onImageFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        input.value = '';
        if (!file || !file.type.startsWith('image/')) return;

        // Revoke previous blob URL
        if (this.blobPreviewUrl) URL.revokeObjectURL(this.blobPreviewUrl);
        this.blobPreviewUrl = URL.createObjectURL(file);

        this.uploadingImage = true;
        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaTypeId', DEFAULT_IMAGE_MEDIA_TYPE_ID);

        this.mediaApi.uploadMedia(formData).subscribe({
            next: (asset) => {
                this.form.patchValue({ imageUrl: asset.fileUrl });
                URL.revokeObjectURL(this.blobPreviewUrl);
                this.blobPreviewUrl = '';
                this.uploadingImage = false;
            },
            error: () => {
                URL.revokeObjectURL(this.blobPreviewUrl);
                this.blobPreviewUrl = '';
                this.uploadingImage = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('messages.errorUploadingMedia'),
                    life: 5000
                });
            }
        });
    }

    clearImage(): void {
        if (this.blobPreviewUrl) {
            URL.revokeObjectURL(this.blobPreviewUrl);
            this.blobPreviewUrl = '';
        }
        this.form.patchValue({ imageUrl: null, imageAltText: null });
    }

    onSubmit(): void {
        if (this.form.invalid || this.saving || this.uploadingImage) return;
        this.saving = true;

        const v = this.form.getRawValue();
        const payload = {
            message: v.message.trim(),
            type: v.type || null,
            isActive: v.isActive,
            displayOrder: v.displayOrder,
            targetPlatform: v.targetPlatform,
            startsAt: v.startsAt ? new Date(v.startsAt).toISOString() : null,
            endsAt: v.endsAt ? new Date(v.endsAt).toISOString() : null,
            imageUrl: v.imageUrl || null,
            imageAltText: v.imageAltText?.trim() || null
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
                    detail: this.translate.instant('messages.saveError'),
                    life: 5000
                });
                this.saving = false;
            }
        });
    }

    cancel(): void {
        if (this.blobPreviewUrl) URL.revokeObjectURL(this.blobPreviewUrl);
        this.ref.close(false);
    }

    private toDateTimeLocal(iso: string): string {
        const d = new Date(iso);
        const pad = (n: number) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }
}
