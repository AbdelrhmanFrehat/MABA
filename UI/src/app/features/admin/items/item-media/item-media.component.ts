import { Component, OnInit, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService, DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ItemsApiService, LinkMediaRequest } from '../../../../shared/services/items-api.service';
import { MediaApiService } from '../../../../shared/services/media-api.service';
import { ItemMediaLink } from '../../../../shared/models/item.model';
import { MediaAsset } from '../../../../shared/models/media.model';
import { environment } from '../../../../../environments/environment';

@Component({
    selector: 'app-item-media',
    standalone: true,
    imports: [
        CommonModule,
        ButtonModule,
        FileUploadModule,
        DialogModule,
        ToastModule,
        TooltipModule,
        ConfirmDialogModule,
        TranslateModule
    ],
    providers: [MessageService, ConfirmationService, DialogService],
    template: `
        <div class="item-media-manager">
            <div class="media-header">
                <h3>{{ 'admin.items.productImages' | translate }}</h3>
                <div class="media-actions">
                    <p-button 
                        [label]="'admin.items.uploadImage' | translate"
                        icon="pi pi-upload"
                        (click)="showUploadDialog = true"
                        [outlined]="true"
                        size="small">
                    </p-button>
                    <p-button 
                        [label]="'admin.items.selectFromLibrary' | translate"
                        icon="pi pi-images"
                        (click)="showLibraryDialog = true"
                        [outlined]="true"
                        size="small">
                    </p-button>
                </div>
            </div>

            <div class="media-grid" *ngIf="mediaItems.length > 0">
                <div 
                    *ngFor="let media of mediaItems; let i = index" 
                    class="media-item"
                    [class.primary]="media.isPrimary">
                    <div class="media-image">
                        <img [src]="getImageUrl(media.fileUrl || media.mediaAssetUrl)" [alt]="media.titleEn || media.altTextEn || 'Product image'" />
                        <div class="media-overlay">
                            <p-button 
                                icon="pi pi-star"
                                [rounded]="true"
                                [outlined]="!media.isPrimary"
                                [severity]="media.isPrimary ? 'warn' : 'secondary'"
                                size="small"
                                (click)="setPrimary(media)"
                                [pTooltip]="'admin.items.setPrimary' | translate">
                            </p-button>
                            <p-button 
                                icon="pi pi-trash"
                                [rounded]="true"
                                severity="danger"
                                size="small"
                                (click)="removeMedia(media)"
                                [pTooltip]="'common.delete' | translate">
                            </p-button>
                        </div>
                    </div>
                    <span *ngIf="media.isPrimary" class="primary-badge">
                        {{ 'admin.items.primaryImage' | translate }}
                    </span>
                </div>
            </div>

            <div class="empty-state" *ngIf="mediaItems.length === 0 && !loading">
                <i class="pi pi-images"></i>
                <p>{{ 'admin.items.noImages' | translate }}</p>
            </div>

            <!-- Upload Dialog -->
            <p-dialog 
                [(visible)]="showUploadDialog" 
                [header]="'admin.items.uploadImage' | translate"
                [modal]="true"
                [closable]="true"
                [dismissableMask]="true"
                [style]="{ width: '500px' }">
                <div class="upload-area">
                    <p-fileUpload
                        mode="advanced"
                        [multiple]="true"
                        accept="image/*"
                        [maxFileSize]="5000000"
                        (onSelect)="onFilesSelected($event)"
                        [auto]="false"
                        [chooseLabel]="'admin.media.chooseFile' | translate"
                        [uploadLabel]="'admin.media.upload' | translate"
                        [cancelLabel]="'common.cancel' | translate"
                        (uploadHandler)="uploadFiles($event)"
                        [customUpload]="true">
                        <ng-template pTemplate="content">
                            <div class="upload-hint">
                                <p>{{ 'admin.items.dragDropImages' | translate }}</p>
                                <small>{{ 'admin.items.maxSize5MB' | translate }}</small>
                            </div>
                        </ng-template>
                    </p-fileUpload>
                </div>
            </p-dialog>

            <!-- Library Dialog -->
            <p-dialog 
                [(visible)]="showLibraryDialog" 
                [header]="'admin.items.selectFromLibrary' | translate"
                [modal]="true"
                [closable]="true"
                [dismissableMask]="true"
                [style]="{ width: '800px', height: '600px' }">
                <div class="library-grid" *ngIf="libraryImages.length > 0">
                    <div 
                        *ngFor="let asset of libraryImages" 
                        class="library-item"
                        [class.selected]="selectedLibraryAsset?.id === asset.id"
                        (click)="selectLibraryAsset(asset)">
                        <img [src]="getImageUrl(asset.fileUrl)" [alt]="asset.titleEn || 'Media asset'" />
                        <i *ngIf="selectedLibraryAsset?.id === asset.id" class="pi pi-check-circle"></i>
                    </div>
                </div>
                <div class="empty-state" *ngIf="libraryImages.length === 0">
                    <p>{{ 'admin.items.noMediaInLibrary' | translate }}</p>
                </div>
                <ng-template pTemplate="footer">
                    <p-button 
                        [label]="'common.cancel' | translate"
                        [outlined]="true"
                        (click)="showLibraryDialog = false">
                    </p-button>
                    <p-button 
                        [label]="'admin.items.addSelected' | translate"
                        [disabled]="!selectedLibraryAsset"
                        (click)="addFromLibrary()">
                    </p-button>
                </ng-template>
            </p-dialog>
        </div>
        <p-toast></p-toast>
        <p-confirmDialog></p-confirmDialog>
    `,
    styles: [`
        .item-media-manager {
            padding: 1rem;
            border: 1px solid var(--surface-border);
            border-radius: 8px;
            background: var(--surface-ground);
        }

        .media-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .media-header h3 {
            margin: 0;
            font-size: 1rem;
            font-weight: 600;
        }

        .media-actions {
            display: flex;
            gap: 0.5rem;
        }

        .media-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
            gap: 1rem;
        }

        .media-item {
            position: relative;
            border-radius: 8px;
            overflow: hidden;
            border: 2px solid transparent;
            transition: all 0.2s;
        }

        .media-item.primary {
            border-color: var(--primary-color);
        }

        .media-image {
            position: relative;
            aspect-ratio: 1;
            background: var(--surface-100);
        }

        .media-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .media-overlay {
            position: absolute;
            inset: 0;
            background: rgba(0,0,0,0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            opacity: 0;
            transition: opacity 0.2s;
        }

        .media-item:hover .media-overlay {
            opacity: 1;
        }

        .primary-badge {
            display: block;
            text-align: center;
            padding: 0.25rem;
            background: var(--primary-color);
            color: white;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .empty-state {
            text-align: center;
            padding: 2rem;
            color: var(--text-color-secondary);
        }

        .empty-state i {
            font-size: 2rem;
            margin-bottom: 0.5rem;
        }

        .upload-area {
            padding: 1rem 0;
        }

        .upload-hint {
            text-align: center;
            padding: 2rem;
            color: var(--text-color-secondary);
        }

        .library-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
            gap: 0.5rem;
            max-height: 400px;
            overflow-y: auto;
        }

        .library-item {
            position: relative;
            aspect-ratio: 1;
            border-radius: 4px;
            overflow: hidden;
            cursor: pointer;
            border: 2px solid transparent;
            transition: all 0.2s;
        }

        .library-item:hover {
            border-color: var(--primary-color);
        }

        .library-item.selected {
            border-color: var(--primary-color);
        }

        .library-item img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .library-item .pi-check-circle {
            position: absolute;
            top: 4px;
            right: 4px;
            color: var(--primary-color);
            background: white;
            border-radius: 50%;
        }
    `]
})
export class ItemMediaComponent implements OnInit {
    @Input() itemId!: string;

    mediaItems: ItemMediaLink[] = [];
    libraryImages: MediaAsset[] = [];
    selectedLibraryAsset: MediaAsset | null = null;
    showUploadDialog = false;
    showLibraryDialog = false;
    loading = false;
    uploading = false;

    private itemsApiService = inject(ItemsApiService);
    private mediaApiService = inject(MediaApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    private config = inject(DynamicDialogConfig, { optional: true });

    ngOnInit() {
        // If opened as dialog, get itemId from config
        if (this.config?.data?.itemId) {
            this.itemId = this.config.data.itemId;
        }
        
        if (this.itemId) {
            this.loadMedia();
        }
    }

    loadMedia() {
        this.loading = true;
        this.itemsApiService.getItemMedia(this.itemId).subscribe({
            next: (media) => {
                this.mediaItems = media;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    loadLibrary() {
        this.mediaApiService.getAllMedia().subscribe({
            next: (assets) => {
                // Filter to only show images
                this.libraryImages = assets.filter(a => 
                    a.mimeType?.startsWith('image/') || 
                    a.fileExtension?.match(/\.(jpg|jpeg|png|gif|webp)$/i)
                );
            }
        });
    }

    getImageUrl(url: string | null | undefined): string {
        if (!url?.trim()) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        const base = (environment.apiUrl ?? '').replace(/\/api\/v1\/?$/, '');
        return base ? `${base}${url.startsWith('/') ? url : '/' + url}` : url;
    }

    onFilesSelected(event: any) {
        // Files are ready for upload
    }

    uploadFiles(event: any) {
        const files = event.files as File[];
        if (!files || files.length === 0) return;

        this.uploading = true;
        let uploadedCount = 0;

        files.forEach((file, index) => {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('mediaTypeId', '00000000-0000-0000-0000-000000000001'); // Default image type

            this.mediaApiService.uploadMedia(formData).subscribe({
                next: (asset) => {
                    // Link to item
                    const request: LinkMediaRequest = {
                        mediaAssetId: asset.id,
                        isPrimary: this.mediaItems.length === 0 && index === 0,
                        sortOrder: this.mediaItems.length + index
                    };

                    this.itemsApiService.linkMediaToItem(this.itemId, request).subscribe({
                        next: () => {
                            uploadedCount++;
                            if (uploadedCount === files.length) {
                                this.uploading = false;
                                this.showUploadDialog = false;
                                this.loadMedia();
                                this.messageService.add({
                                    severity: 'success',
                                    summary: this.translateService.instant('messages.success'),
                                    detail: this.translateService.instant('admin.items.imagesUploaded')
                                });
                            }
                        }
                    });
                },
                error: () => {
                    this.uploading = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('admin.items.uploadFailed')
                    });
                }
            });
        });
    }

    selectLibraryAsset(asset: MediaAsset) {
        this.selectedLibraryAsset = asset;
    }

    addFromLibrary() {
        if (!this.selectedLibraryAsset) return;

        const request: LinkMediaRequest = {
            mediaAssetId: this.selectedLibraryAsset.id,
            isPrimary: this.mediaItems.length === 0,
            sortOrder: this.mediaItems.length
        };

        this.itemsApiService.linkMediaToItem(this.itemId, request).subscribe({
            next: () => {
                this.showLibraryDialog = false;
                this.selectedLibraryAsset = null;
                this.loadMedia();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.items.imageAdded')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    setPrimary(media: ItemMediaLink) {
        if (media.isPrimary) return;

        this.itemsApiService.setPrimaryMedia(this.itemId, media.id).subscribe({
            next: () => {
                this.loadMedia();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.items.primarySet')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    removeMedia(media: ItemMediaLink) {
        this.confirmationService.confirm({
            message: this.translateService.instant('admin.items.confirmRemoveImage'),
            accept: () => {
                this.itemsApiService.unlinkMediaFromItem(this.itemId, media.id).subscribe({
                    next: () => {
                        this.loadMedia();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('admin.items.imageRemoved')
                        });
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    openLibraryDialog() {
        this.loadLibrary();
        this.showLibraryDialog = true;
    }
}
