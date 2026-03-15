import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { HeroTickerApiService } from '../../../shared/services/hero-ticker-api.service';
import { MediaApiService } from '../../../shared/services/media-api.service';
import { environment } from '../../../../environments/environment';

const DEFAULT_IMAGE_MEDIA_TYPE_ID = '00000000-0000-0000-0000-000000000001';

@Component({
    selector: 'app-hero-ticker-new',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        CheckboxModule,
        CardModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="hero-ticker-new-page">
            <div class="page-header">
                <h1>{{ 'admin.heroTicker.addImage' | translate }}</h1>
                <p-button
                    [label]="'common.back' | translate"
                    icon="pi pi-arrow-left"
                    [outlined]="true"
                    routerLink="/admin/hero-ticker"
                ></p-button>
            </div>

            <p-card>
                <div class="form-fields">
                    <div class="field mb-3">
                        <label class="block mb-2 font-medium">{{ 'admin.heroTicker.image' | translate }} <span class="text-red-500">*</span></label>
                        <p class="text-sm text-500 mb-2">{{ 'admin.heroTicker.uploadFromDevice' | translate }}</p>
                        <input #fileInput type="file" accept="image/*" class="hidden-file-input" (change)="onFileChange($event)" />
                        <p-button
                            [label]="'admin.heroTicker.uploadFromDevice' | translate"
                            icon="pi pi-upload"
                            [outlined]="true"
                            [loading]="uploading"
                            [disabled]="uploading"
                            (onClick)="fileInput.click()"
                        ></p-button>
                        @if (selectedFileName) {
                            <span class="ml-2 text-500">{{ selectedFileName }}</span>
                        }
                        @if (imageUrl) {
                            <img [src]="previewImageUrl" alt="Preview" class="preview-img mt-2" />
                        }
                    </div>

                    <div class="field mb-3">
                        <label class="block mb-2 font-medium">{{ 'admin.heroTicker.titleOptional' | translate }}</label>
                        <input type="text" pInputText [(ngModel)]="title" class="w-full" />
                    </div>

                    <div class="field mb-3">
                        <label class="block mb-2 font-medium">{{ 'admin.heroTicker.sortOrder' | translate }}</label>
                        <p-inputNumber [(ngModel)]="sortOrder" [min]="0" [showButtons]="true" class="w-full"></p-inputNumber>
                    </div>

                    <div class="field mb-4 flex align-items-center gap-2">
                        <p-checkbox [(ngModel)]="isActive" [binary]="true" inputId="active"></p-checkbox>
                        <label for="active">{{ 'admin.heroTicker.showOnHome' | translate }}</label>
                    </div>

                    <div class="flex gap-2">
                        <p-button [label]="'common.cancel' | translate" [outlined]="true" routerLink="/admin/hero-ticker"></p-button>
                        <p-button [label]="'common.save' | translate" (onClick)="save()" [loading]="saving" [disabled]="!imageUrl?.trim()"></p-button>
                    </div>
                </div>
            </p-card>
        </div>
    `,
    styles: [`
        .hero-ticker-new-page { padding: 1rem; max-width: 600px; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; }
        .hidden-file-input { position: absolute; width: 0; height: 0; opacity: 0; pointer-events: none; }
        .preview-img { max-width: 200px; max-height: 120px; object-fit: cover; border-radius: 8px; display: block; }
    `]
})
export class HeroTickerNewComponent {
    private heroTickerApi = inject(HeroTickerApiService);
    private mediaApi = inject(MediaApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    title = '';
    imageUrl = '';
    sortOrder = 0;
    isActive = true;
    saving = false;
    uploading = false;
    selectedFileName = '';

    get previewImageUrl(): string {
        if (!this.imageUrl) return '';
        if (this.imageUrl.startsWith('http')) return this.imageUrl;
        const base = (environment.apiUrl || '').replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '') || '';
        const path = this.imageUrl.startsWith('/') ? this.imageUrl : '/' + this.imageUrl;
        return base ? `${base}${path}` : this.imageUrl;
    }

    onFileChange(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file || !file.type.startsWith('image/')) {
            input.value = '';
            return;
        }
        this.uploading = true;
        this.selectedFileName = file.name;
        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaTypeId', DEFAULT_IMAGE_MEDIA_TYPE_ID);
        this.mediaApi.uploadMedia(formData).subscribe({
            next: (media) => {
                this.imageUrl = media.fileUrl;
                this.uploading = false;
                this.selectedFileName = file.name;
            },
            error: (err) => {
                this.uploading = false;
                this.selectedFileName = '';
                const msg = err?.status === 401
                    ? (this.translate.instant('messages.unauthorized') || 'Please sign in.')
                    : (err?.error?.message || err?.message || this.translate.instant('common.error'));
                this.messageService.add({ severity: 'error', summary: '', detail: String(msg) });
            }
        });
        input.value = '';
    }

    save() {
        if (!this.imageUrl?.trim()) return;
        this.saving = true;
        this.heroTickerApi.create({
            title: this.title?.trim() || undefined,
            imageUrl: this.imageUrl.trim(),
            sortOrder: this.sortOrder,
            isActive: this.isActive
        }).subscribe({
            next: () => {
                this.saving = false;
                this.messageService.add({ severity: 'success', summary: '', detail: String(this.translate.instant('common.saved') || 'Saved') });
                this.router.navigate(['/admin/hero-ticker']);
            },
            error: (err) => {
                this.saving = false;
                const msg = err?.status === 401
                    ? (this.translate.instant('messages.unauthorized') || 'Please sign in.')
                    : (err?.error?.message || err?.message || this.translate.instant('common.error'));
                this.messageService.add({ severity: 'error', summary: '', detail: String(msg) });
            }
        });
    }
}
