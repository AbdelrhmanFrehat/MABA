import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TranslateModule } from '@ngx-translate/core';
import { MediaPickerComponent } from '../../../shared/components/media-picker/media-picker.component';
import { MediaAsset } from '../../../shared/models/media.model';
import { HeroTickerApiService, HeroTickerItemDto, CreateHeroTickerItemRequest } from '../../../shared/services/hero-ticker-api.service';
import { MediaApiService } from '../../../shared/services/media-api.service';
import { environment } from '../../../../environments/environment';

const DEFAULT_IMAGE_MEDIA_TYPE_ID = '00000000-0000-0000-0000-000000000001';
const HERO_TICKER_TARGET_UPLOAD_BYTES = 1500 * 1024;
const HERO_TICKER_MAX_DIMENSION = 1920;

@Component({
    selector: 'app-hero-ticker-form-dialog',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        CheckboxModule,
        TranslateModule,
        MediaPickerComponent
    ],
    template: `
        <div class="hero-ticker-form p-4">
            <h3 class="text-xl font-semibold mb-4">{{ (isEdit ? 'admin.heroTicker.editImage' : 'admin.heroTicker.addImage') | translate }}</h3>

            <div class="field mb-3">
                <label class="block mb-2 font-medium">{{ 'admin.heroTicker.image' | translate }}</label>
                <p class="text-sm text-500 mb-2">{{ 'admin.heroTicker.uploadOrSelect' | translate }}</p>
                <div class="image-source-buttons">
                    <input #fileInput type="file" accept="image/*" class="hidden-file-input" (change)="onFileChange($event)" />
                    <p-button
                        [label]="'admin.heroTicker.uploadFromDevice' | translate"
                        icon="pi pi-upload"
                        [outlined]="true"
                        [loading]="uploading"
                        [disabled]="uploading"
                        (onClick)="fileInput.click()"
                    ></p-button>
                    <span class="or-divider">{{ 'admin.heroTicker.orSelectFromLibrary' | translate }}</span>
                    <app-media-picker
                        [buttonLabel]="'admin.heroTicker.selectImage' | translate"
                        [multiple]="false"
                        (mediaSelected)="onMediaSelected($event)"
                    ></app-media-picker>
                </div>
                @if (uploading) {
                    <small class="text-500 block mb-2">{{ 'common.loading' | translate }}</small>
                }
                @if (selectedFileName) {
                    <small class="text-500 block mb-2">{{ selectedFileName }}</small>
                }
                <img
                    *ngIf="imageUrl"
                    [src]="previewImageUrl"
                    alt="Preview"
                    class="mt-2 border-round preview-img"
                />
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

            <div class="flex justify-end gap-2">
                <p-button [label]="'common.cancel' | translate" [outlined]="true" (click)="ref.close(false)"></p-button>
                <p-button [label]="'common.save' | translate" (click)="save()" [loading]="saving"></p-button>
            </div>
        </div>
    `,
    styles: [`
        .preview-img { max-width: 200px; max-height: 120px; object-fit: cover; }
        .image-source-buttons { display: flex; flex-wrap: wrap; align-items: center; gap: 0.75rem; }
        .hidden-file-input { position: absolute; width: 0; height: 0; opacity: 0; pointer-events: none; }
        .or-divider { color: var(--text-color-secondary); font-size: 0.875rem; }
    `]
})
export class HeroTickerFormDialogComponent {
    ref = inject(DynamicDialogRef);
    config = inject(DynamicDialogConfig);
    private heroTickerApi = inject(HeroTickerApiService);
    private mediaApi = inject(MediaApiService);

    item: HeroTickerItemDto | null = this.config.data?.item ?? null;
    isEdit = !!this.item;

    title = this.item?.title ?? '';
    imageUrl = this.item?.imageUrl ?? '';
    sortOrder = this.item?.sortOrder ?? 0;
    isActive = this.item?.isActive ?? true;
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

    onMediaSelected(media: MediaAsset[]) {
        if (media?.length) {
            this.imageUrl = media[0].fileUrl;
            this.selectedFileName = '';
        }
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
        this.prepareImageForUpload(file)
            .then((preparedFile) => {
                const formData = new FormData();
                formData.append('file', preparedFile);
                formData.append('mediaTypeId', DEFAULT_IMAGE_MEDIA_TYPE_ID);
                this.mediaApi.uploadMedia(formData).subscribe({
                    next: (media) => {
                        this.imageUrl = media.fileUrl;
                        this.uploading = false;
                        this.selectedFileName = preparedFile.name;
                    },
                    error: () => {
                        this.uploading = false;
                        this.selectedFileName = '';
                    }
                });
            })
            .catch(() => {
                this.uploading = false;
                this.selectedFileName = '';
            });
        input.value = '';
    }

    private async prepareImageForUpload(file: File): Promise<File> {
        const image = await this.loadImage(file);
        const { width, height } = this.getScaledDimensions(image.width, image.height, HERO_TICKER_MAX_DIMENSION);
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const context = canvas.getContext('2d');
        if (!context) {
            throw new Error('Unable to prepare image upload.');
        }
        context.drawImage(image, 0, 0, width, height);

        let quality = file.size > 5 * 1024 * 1024 ? 0.8 : 0.9;
        let blob = await this.canvasToBlob(canvas, 'image/webp', quality);
        while (blob.size > HERO_TICKER_TARGET_UPLOAD_BYTES && quality > 0.35) {
            quality -= 0.1;
            blob = await this.canvasToBlob(canvas, 'image/webp', quality);
        }

        if (blob.size > HERO_TICKER_TARGET_UPLOAD_BYTES) {
            const secondPass = document.createElement('canvas');
            secondPass.width = Math.max(1, Math.round(width * 0.8));
            secondPass.height = Math.max(1, Math.round(height * 0.8));
            const secondContext = secondPass.getContext('2d');
            if (!secondContext) {
                throw new Error('Unable to prepare image upload.');
            }
            secondContext.drawImage(canvas, 0, 0, secondPass.width, secondPass.height);
            quality = 0.6;
            blob = await this.canvasToBlob(secondPass, 'image/webp', quality);
            while (blob.size > HERO_TICKER_TARGET_UPLOAD_BYTES && quality > 0.35) {
                quality -= 0.1;
                blob = await this.canvasToBlob(secondPass, 'image/webp', quality);
            }
        }

        if (blob.size > HERO_TICKER_TARGET_UPLOAD_BYTES) {
            throw new Error('Image is too large to upload.');
        }

        const safeBaseName = (file.name.replace(/\.[^/.]+$/, '') || 'hero-image').slice(0, 80);
        return new File([blob], `${safeBaseName}.webp`, { type: 'image/webp' });
    }

    private loadImage(file: File): Promise<HTMLImageElement> {
        return new Promise((resolve, reject) => {
            const objectUrl = URL.createObjectURL(file);
            const image = new Image();
            image.onload = () => {
                URL.revokeObjectURL(objectUrl);
                resolve(image);
            };
            image.onerror = () => {
                URL.revokeObjectURL(objectUrl);
                reject(new Error('Invalid image'));
            };
            image.src = objectUrl;
        });
    }

    private getScaledDimensions(width: number, height: number, maxDimension: number): { width: number; height: number } {
        if (width <= maxDimension && height <= maxDimension) {
            return { width, height };
        }

        const ratio = Math.min(maxDimension / width, maxDimension / height);
        return {
            width: Math.max(1, Math.round(width * ratio)),
            height: Math.max(1, Math.round(height * ratio))
        };
    }

    private canvasToBlob(canvas: HTMLCanvasElement, type: string, quality: number): Promise<Blob> {
        return new Promise((resolve, reject) => {
            canvas.toBlob((blob) => {
                if (!blob) {
                    reject(new Error('Failed to encode image.'));
                    return;
                }
                resolve(blob);
            }, type, quality);
        });
    }

    save() {
        if (!this.imageUrl?.trim()) {
            return;
        }
        this.saving = true;
        const req: CreateHeroTickerItemRequest = {
            title: this.title?.trim() || undefined,
            imageUrl: this.imageUrl.trim(),
            sortOrder: this.sortOrder,
            isActive: this.isActive
        };
        if (this.isEdit && this.item) {
            this.heroTickerApi.update(this.item.id, { ...req, id: this.item.id }).subscribe({
                next: () => { this.saving = false; this.ref.close(true); },
                error: () => { this.saving = false; }
            });
        } else {
            this.heroTickerApi.create(req).subscribe({
                next: () => { this.saving = false; this.ref.close(true); },
                error: () => { this.saving = false; }
            });
        }
    }
}
