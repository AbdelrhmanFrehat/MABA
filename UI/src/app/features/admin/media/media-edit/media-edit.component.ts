import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MediaApiService } from '../../../../shared/services/media-api.service';
import { MediaAsset, UpdateMediaRequest } from '../../../../shared/models/media.model';

@Component({
    selector: 'app-media-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="editForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="grid">
                <div class="col-12">
                    <label class="block text-900 font-medium mb-2">
                        {{ 'admin.media.fileName' | translate }}
                    </label>
                    <p class="text-surface-600">{{ media?.fileName }}</p>
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

                <div class="col-12">
                    <label for="altEn" class="block text-900 font-medium mb-2">
                        {{ 'admin.media.altEn' | translate }}
                    </label>
                    <input pInputText id="altEn" formControlName="altEn" class="w-full" />
                </div>

                <div class="col-12">
                    <label for="altAr" class="block text-900 font-medium mb-2">
                        {{ 'admin.media.altAr' | translate }}
                    </label>
                    <input pInputText id="altAr" formControlName="altAr" class="w-full" />
                </div>
            </div>

            <div class="flex justify-content-end gap-2 mt-4">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="saving"
                ></p-button>
                <p-button 
                    [label]="'common.save' | translate" 
                    type="submit"
                    [loading]="saving"
                    [disabled]="saving"
                ></p-button>
            </div>
        </form>
    `
})
export class MediaEditComponent implements OnInit {
    editForm!: FormGroup;
    media: MediaAsset | null = null;
    saving = false;
    errorMessage = '';

    private fb = inject(FormBuilder);
    private mediaApiService = inject(MediaApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.editForm = this.fb.group({
            titleEn: [''],
            titleAr: [''],
            altEn: [''],
            altAr: ['']
        });
    }

    ngOnInit() {
        if (this.config.data?.media) {
            this.media = this.config.data.media;
            this.editForm.patchValue({
                titleEn: this.media?.titleEn || '',
                titleAr: this.media?.titleAr || '',
                altEn: this.media?.altEn || '',
                altAr: this.media?.altAr || ''
            });
        }
    }

    onSubmit() {
        if (!this.media) return;

        this.saving = true;
        this.errorMessage = '';

        const updateRequest: UpdateMediaRequest = {
            titleEn: this.editForm.value.titleEn || undefined,
            titleAr: this.editForm.value.titleAr || undefined,
            altEn: this.editForm.value.altEn || undefined,
            altAr: this.editForm.value.altAr || undefined
        };

        this.mediaApiService.updateMedia(this.media.id, updateRequest).subscribe({
            next: (updatedMedia) => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.mediaUpdatedSuccessfully'),
                    life: 3000
                });
                this.ref.close(updatedMedia);
            },
            error: (error) => {
                this.saving = false;
                this.errorMessage = error.error?.message || this.translateService.instant('messages.errorUpdatingMedia');
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

