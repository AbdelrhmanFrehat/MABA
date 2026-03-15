import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ImageModule } from 'primeng/image';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { PaginatorModule } from 'primeng/paginator';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MediaApiService } from '../../services/media-api.service';
import { MediaAsset } from '../../models/media.model';

const DEFAULT_IMAGE_MEDIA_TYPE_ID = '00000000-0000-0000-0000-000000000001';

@Component({
    selector: 'app-media-picker',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        DialogModule,
        ButtonModule,
        InputTextModule,
        InputIconModule,
        IconFieldModule,
        ImageModule,
        PaginatorModule,
        TagModule,
        TooltipModule,
        TranslateModule
    ],
    template: `
        <p-button 
            [label]="buttonLabel || ('common.selectMedia' | translate)"
            [icon]="buttonIcon || 'pi pi-image'"
            [outlined]="buttonOutlined"
            (click)="openDialog()"
        ></p-button>

        <p-dialog 
            [header]="dialogTitle || ('admin.media.selectMedia' | translate)"
            [(visible)]="visible"
            [modal]="true"
            [style]="{width: '90vw', maxWidth: '1200px'}"
            [closable]="true"
        >
            <div class="media-picker">
                <div class="upload-from-device mb-4 p-3 surface-100 border-round">
                    <label class="block font-medium mb-2">{{ 'admin.heroTicker.uploadFromDevice' | translate }}</label>
                    <input #fileInput type="file" accept="image/*" class="hidden-file-input" (change)="onUploadFile($event)" />
                    <p-button
                        [label]="'admin.heroTicker.uploadFromDevice' | translate"
                        icon="pi pi-upload"
                        [outlined]="true"
                        [loading]="uploading"
                        [disabled]="uploading"
                        (onClick)="fileInput.click()"
                    ></p-button>
                    <span class="text-500 text-sm ml-2">{{ 'admin.heroTicker.orSelectFromLibrary' | translate }}</span>
                </div>
                <div class="flex justify-content-between align-items-center mb-4">
                    <p-iconfield>
                        <p-inputicon styleClass="pi pi-search" />
                        <input 
                            pInputText 
                            type="text" 
                            [(ngModel)]="searchTerm"
                            (input)="onSearch()"
                            [placeholder]="'common.search' | translate"
                            class="w-full"
                        />
                    </p-iconfield>
                </div>

                <div *ngIf="loading" class="flex justify-content-center p-6">
                    <i class="pi pi-spin pi-spinner text-4xl"></i>
                </div>

                <div *ngIf="!loading && mediaList.length === 0" class="text-center p-6 text-500">
                    {{ 'common.noDataFound' | translate }}
                </div>

                <div *ngIf="!loading && mediaList.length > 0" class="media-grid">
                    <div 
                        *ngFor="let media of mediaList" 
                        class="media-item"
                        [class.selected]="isSelected(media)"
                        (click)="toggleSelection(media)"
                    >
                        <div class="media-preview">
                            <p-image 
                                *ngIf="isImage(media)"
                                [src]="media.fileUrl" 
                                [alt]="media.fileName"
                                [preview]="true"
                                width="100%"
                                height="200px"
                                styleClass="w-full"
                            />
                            <div *ngIf="!isImage(media)" class="file-preview">
                                <i class="pi pi-file text-6xl text-500"></i>
                                <span class="text-sm mt-2">{{ media.fileName }}</span>
                            </div>
                            <div class="media-overlay">
                                <i class="pi pi-check-circle text-3xl text-white"></i>
                            </div>
                        </div>
                        <div class="media-info p-2">
                            <div class="text-sm font-medium truncate">{{ media.fileName }}</div>
                            <div class="text-xs text-500">{{ formatFileSize(media.fileSizeBytes) }}</div>
                        </div>
                    </div>
                </div>

                <p-paginator
                    *ngIf="!loading && totalRecords > 0"
                    [rows]="pageSize"
                    [totalRecords]="totalRecords"
                    [first]="first"
                    (onPageChange)="onPageChange($event)"
                    [rowsPerPageOptions]="[12, 24, 48]"
                ></p-paginator>
            </div>

            <ng-template #footer>
                <div class="flex justify-content-end gap-2">
                    <p-button 
                        [label]="'common.cancel' | translate"
                        [outlined]="true"
                        (click)="closeDialog()"
                    ></p-button>
                    <p-button 
                        [label]="'common.confirm' | translate"
                        [disabled]="selectedMedia.length === 0"
                        (click)="confirmSelection()"
                    ></p-button>
                </div>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .media-picker {
            min-height: 400px;
        }
        
        .media-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
            gap: 1rem;
            margin-bottom: 1rem;
        }
        
        .media-item {
            border: 2px solid var(--surface-border);
            border-radius: 8px;
            overflow: hidden;
            cursor: pointer;
            transition: all 0.2s;
        }
        
        .media-item:hover {
            border-color: var(--primary-color);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }
        
        .media-item.selected {
            border-color: var(--primary-color);
            box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb), 0.2);
        }
        
        .media-preview {
            position: relative;
            width: 100%;
            height: 200px;
            overflow: hidden;
            background: var(--surface-ground);
        }
        
        .file-preview {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100%;
        }
        
        .media-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.5);
            display: none;
            align-items: center;
            justify-content: center;
        }
        
        .media-item.selected .media-overlay {
            display: flex;
        }
        
        .media-info {
            background: var(--surface-card);
        }
        
        .truncate {
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }
        .hidden-file-input { position: absolute; width: 0; height: 0; opacity: 0; pointer-events: none; }
    `]
})
export class MediaPickerComponent implements OnInit {
    @Input() buttonLabel?: string;
    @Input() buttonIcon?: string;
    @Input() buttonOutlined: boolean = true;
    @Input() dialogTitle?: string;
    @Input() multiple: boolean = false;
    @Input() selectedMediaIds: string[] = [];
    @Input() mediaType?: string;

    @Output() mediaSelected = new EventEmitter<MediaAsset[]>();

    visible = false;
    loading = false;
    uploading = false;
    mediaList: MediaAsset[] = [];
    selectedMedia: MediaAsset[] = [];
    searchTerm = '';
    first = 0;
    pageSize = 24;
    totalRecords = 0;

    private mediaApiService = inject(MediaApiService);
    private translateService = inject(TranslateService);

    ngOnInit() {
        if (this.selectedMediaIds.length > 0) {
            this.loadSelectedMedia();
        }
    }

    openDialog() {
        this.visible = true;
        this.loadMedia();
    }

    closeDialog() {
        this.visible = false;
        this.selectedMedia = [];
    }

    onUploadFile(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file || !file.type.startsWith('image/')) {
            input.value = '';
            return;
        }
        this.uploading = true;
        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaTypeId', DEFAULT_IMAGE_MEDIA_TYPE_ID);
        this.mediaApiService.uploadMedia(formData).subscribe({
            next: (media) => {
                this.uploading = false;
                this.mediaSelected.emit([media]);
                this.closeDialog();
                input.value = '';
            },
            error: () => {
                this.uploading = false;
                input.value = '';
            }
        });
    }

    loadMedia() {
        this.loading = true;
        // For now, use getAllMedia - pagination can be added later
        this.mediaApiService.getAllMedia(this.mediaType).subscribe({
            next: (media) => {
                // Filter by search term if provided
                let filtered = media;
                if (this.searchTerm) {
                    const term = this.searchTerm.toLowerCase();
                    filtered = media.filter(m => 
                        m.fileName.toLowerCase().includes(term) ||
                        m.titleEn?.toLowerCase().includes(term) ||
                        m.titleAr?.toLowerCase().includes(term)
                    );
                }
                
                // Apply pagination
                this.totalRecords = filtered.length;
                const start = this.first;
                const end = start + this.pageSize;
                this.mediaList = filtered.slice(start, end);
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    loadSelectedMedia() {
        // Load selected media details if needed
    }

    onSearch() {
        this.first = 0;
        this.loadMedia();
    }

    onPageChange(event: any) {
        this.first = event.first;
        this.pageSize = event.rows;
        this.loadMedia();
    }

    toggleSelection(media: MediaAsset) {
        if (this.multiple) {
            const index = this.selectedMedia.findIndex(m => m.id === media.id);
            if (index >= 0) {
                this.selectedMedia.splice(index, 1);
            } else {
                this.selectedMedia.push(media);
            }
        } else {
            this.selectedMedia = [media];
        }
    }

    isSelected(media: MediaAsset): boolean {
        return this.selectedMedia.some(m => m.id === media.id);
    }

    isImage(media: MediaAsset): boolean {
        return media.mimeType?.toLowerCase().startsWith('image/') || false;
    }

    formatFileSize(bytes: number): string {
        if (!bytes || bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    confirmSelection() {
        this.mediaSelected.emit([...this.selectedMedia]);
        this.closeDialog();
    }
}

