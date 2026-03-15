import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MediaApiService } from '../../../../shared/services/media-api.service';
import { MediaAsset } from '../../../../shared/models/media.model';

@Component({
    selector: 'app-media-upload',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        FileUploadModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="uploadForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="grid">
                <div class="col-12">
                    <label for="file" class="block text-900 font-medium mb-2">
                        {{ 'admin.media.file' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <p-fileUpload
                        mode="basic"
                        [accept]="acceptedTypes"
                        [maxFileSize]="maxFileSize"
                        (onSelect)="onFileSelect($event)"
                        [chooseLabel]="'admin.media.chooseFile' | translate"
                        [disabled]="uploading"
                    ></p-fileUpload>
                    @if (selectedFile) {
                        <small class="text-surface-600 block mt-2">{{ selectedFile.name }} ({{ formatFileSize(selectedFile.size) }})</small>
                    }
                </div>

                <div class="col-12">
                    <label for="titleEn" class="block text-900 font-medium mb-2">
                        {{ 'admin.media.titleEn' | translate }}
                    </label>
                    <input pInputText id="titleEn" formControlName="titleEn" class="w-full" />
                </div>

                <div class="col-12">
                    <label for="titleAr" class="block text-900 font-medium mb-2">
                        {{ 'admin.media.titleAr' | translate }}
                    </label>
                    <input pInputText id="titleAr" formControlName="titleAr" class="w-full" />
                </div>
            </div>

            <div class="flex justify-content-end gap-2 mt-4">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="uploading"
                ></p-button>
                <p-button 
                    [label]="'admin.media.upload' | translate" 
                    type="submit"
                    [loading]="uploading"
                    [disabled]="!selectedFile || uploading"
                ></p-button>
            </div>
        </form>
    `
})
export class MediaUploadComponent implements OnInit {
    uploadForm!: FormGroup;
    selectedFile: File | null = null;
    uploading = false;
    errorMessage = '';
    acceptedTypes = 'image/*,video/*,application/pdf';
    maxFileSize = 10485760; // 10MB

    private fb = inject(FormBuilder);
    private mediaApiService = inject(MediaApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.uploadForm = this.fb.group({
            titleEn: [''],
            titleAr: ['']
        });
    }

    ngOnInit() {}

    onFileSelect(event: any) {
        this.selectedFile = event.files[0];
    }

    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    onSubmit() {
        if (!this.selectedFile) {
            this.errorMessage = this.translateService.instant('admin.media.fileRequired');
            return;
        }

        this.uploading = true;
        this.errorMessage = '';

        const formData = new FormData();
        formData.append('file', this.selectedFile);
        
        // Note: mediaTypeId should be determined based on file type or provided by user
        // For now, we'll need to handle this - assuming backend can auto-detect or we need a mediaTypeId field
        
        const formValue = this.uploadForm.value;
        if (formValue.titleEn) formData.append('titleEn', formValue.titleEn);
        if (formValue.titleAr) formData.append('titleAr', formValue.titleAr);

        this.mediaApiService.uploadMedia(formData).subscribe({
            next: (media) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.mediaUploadedSuccessfully'),
                    life: 3000
                });
                this.ref.close(media);
            },
            error: (error) => {
                this.uploading = false;
                this.errorMessage = error.error?.message || this.translateService.instant('messages.errorUploadingMedia');
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.errorMessage,
                    life: 5000
                });
            }
        });
    }
}

