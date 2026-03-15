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
        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaTypeId', DEFAULT_IMAGE_MEDIA_TYPE_ID);
        this.mediaApi.uploadMedia(formData).subscribe({
            next: (media) => {
                this.imageUrl = media.fileUrl;
                this.uploading = false;
                this.selectedFileName = file.name;
            },
            error: () => {
                this.uploading = false;
                this.selectedFileName = '';
            }
        });
        input.value = '';
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
