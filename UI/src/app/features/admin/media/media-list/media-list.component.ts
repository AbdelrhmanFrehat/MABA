import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MediaApiService } from '../../../../shared/services/media-api.service';
import { MediaAsset } from '../../../../shared/models/media.model';
import { MediaUploadComponent } from '../media-upload/media-upload.component';
import { MediaEditComponent } from '../media-edit/media-edit.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-media-list',
    standalone: true,
    imports: [
        CommonModule,
        DataTableComponent,
        ButtonModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="media-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.media.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.media.upload' | translate" 
                            icon="pi pi-upload" 
                            (click)="openUploadDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="mediaAssets"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.media.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .media-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .media-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .media-list-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .page-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0;
        }

        @media (min-width: 768px) {
            .page-header h1 {
                font-size: 2rem;
            }
        }

        .header-actions {
            display: flex;
            gap: 0.5rem;
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .media-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class MediaListComponent implements OnInit {
    mediaAssets: MediaAsset[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private mediaApiService = inject(MediaApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'thumbnail', headerKey: 'admin.media.thumbnail', type: 'custom', sortable: false },
        { field: 'fileName', headerKey: 'admin.media.fileName', sortable: true },
        { field: 'mimeType', headerKey: 'admin.media.mimeType', sortable: true },
        { field: 'fileSizeBytes', headerKey: 'admin.media.size', type: 'custom', sortable: true },
        { field: 'uploadedByUserId', headerKey: 'admin.media.uploadedBy', sortable: false },
        { field: 'createdAt', headerKey: 'common.createdAt', type: 'date', sortable: true }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-eye',
            tooltipKey: 'common.view',
            action: 'view'
        },
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        },
        {
            icon: 'pi pi-trash',
            tooltipKey: 'common.delete',
            action: 'delete',
            severity: 'danger'
        }
    ];

    ngOnInit() {
        this.loadMedia();
    }

    loadMedia() {
        this.loading = true;
        this.mediaApiService.getAllMedia(undefined, undefined).subscribe({
            next: (assets) => {
                this.mediaAssets = assets;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingMedia'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    openUploadDialog() {
        const ref = this.dialogService.open(MediaUploadComponent, {
            header: this.translateService.instant('admin.media.upload'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: {}
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: MediaAsset | boolean) => {
                if (result) {
                    this.loadMedia();
                }
            });
        }
    }

    handleAction(event: { action: string; data: MediaAsset }) {
        if (event.action === 'view') {
            window.open(event.data.fileUrl, '_blank');
        } else if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openEditDialog(media: MediaAsset) {
        const ref = this.dialogService.open(MediaEditComponent, {
            header: this.translateService.instant('admin.media.edit'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { media }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: MediaAsset | boolean) => {
                if (result) {
                    this.loadMedia();
                }
            });
        }
    }

    confirmDelete(media: MediaAsset) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: media.fileName })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteMedia(media.id);
                }
            });
        }
    }

    deleteMedia(id: string) {
        this.loading = true;
        this.mediaApiService.deleteMedia(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.mediaDeletedSuccessfully'),
                    life: 3000
                });
                this.loadMedia();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingMedia'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}

