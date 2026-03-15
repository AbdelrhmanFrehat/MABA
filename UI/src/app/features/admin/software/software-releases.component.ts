import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUploadModule } from 'primeng/fileupload';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SoftwareApiService } from '../../../core/services/software-api.service';
import { SoftwareRelease, SoftwareFile, CreateSoftwareReleaseRequest } from '../../../shared/models/software.model';

@Component({
    selector: 'app-software-releases',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        TableModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        TextareaModule,
        CheckboxModule,
        SelectModule,
        DatePickerModule,
        FileUploadModule,
        TagModule,
        TooltipModule,
        AccordionModule,
        ConfirmDialogModule,
        ToastModule
    ],
    providers: [ConfirmationService, MessageService],
    template: `
        <p-toast></p-toast>
        <p-confirmDialog></p-confirmDialog>

        <div class="software-releases-admin">
            <div class="page-header">
                <div class="header-content">
                    <a routerLink="/admin/software" class="back-link">
                        <i class="pi pi-arrow-left"></i>
                        {{ 'admin.software.backToProducts' | translate }}
                    </a>
                    <h1>{{ 'admin.software.manageReleases' | translate }}</h1>
                </div>
                <p-button
                    [label]="'admin.software.addRelease' | translate"
                    icon="pi pi-plus"
                    (onClick)="openCreateDialog()"
                ></p-button>
            </div>

            <div class="releases-list">
                <p-accordion [value]="['0']" [multiple]="true">
                    <p-accordion-panel *ngFor="let release of releases; let i = index" [value]="i.toString()">
                        <p-accordion-header>
                            <div class="release-header">
                                <span class="version">v{{ release.version }}</span>
                                <p-tag
                                    [value]="release.status"
                                    [severity]="getStatusSeverity(release.status)"
                                    [rounded]="true"
                                ></p-tag>
                                <p-tag
                                    *ngIf="!release.isActive"
                                    value="Inactive"
                                    severity="danger"
                                    [rounded]="true"
                                ></p-tag>
                                <span class="release-date">{{ formatDate(release.releaseDate) }}</span>
                                <div class="release-actions">
                                    <p-button
                                        icon="pi pi-pencil"
                                        [rounded]="true"
                                        [text]="true"
                                        severity="secondary"
                                        (onClick)="openEditDialog(release); $event.stopPropagation()"
                                    ></p-button>
                                    <p-button
                                        icon="pi pi-trash"
                                        [rounded]="true"
                                        [text]="true"
                                        severity="danger"
                                        (onClick)="confirmDeleteRelease(release); $event.stopPropagation()"
                                    ></p-button>
                                </div>
                            </div>
                        </p-accordion-header>
                        <p-accordion-content>
                            <div class="release-content">
                                <div class="files-section">
                                    <div class="files-header">
                                        <h4>{{ 'admin.software.files' | translate }}</h4>
                                        <p-button
                                            [label]="'admin.software.uploadFile' | translate"
                                            icon="pi pi-upload"
                                            size="small"
                                            (onClick)="openUploadDialog(release)"
                                        ></p-button>
                                    </div>
                                    
                                    <p-table [value]="release.files || []" *ngIf="release.files && release.files.length > 0">
                                        <ng-template pTemplate="header">
                                            <tr>
                                                <th>{{ 'software.fileName' | translate }}</th>
                                                <th>{{ 'software.platform' | translate }}</th>
                                                <th>{{ 'software.architecture' | translate }}</th>
                                                <th>{{ 'software.type' | translate }}</th>
                                                <th>{{ 'software.size' | translate }}</th>
                                                <th>{{ 'software.downloads' | translate }}</th>
                                                <th></th>
                                            </tr>
                                        </ng-template>
                                        <ng-template pTemplate="body" let-file>
                                            <tr>
                                                <td>{{ file.fileName }}</td>
                                                <td>{{ file.os }}</td>
                                                <td>{{ file.arch }}</td>
                                                <td><p-tag [value]="file.fileType" severity="secondary"></p-tag></td>
                                                <td>{{ formatFileSize(file.fileSizeBytes) }}</td>
                                                <td>{{ file.downloadCount }}</td>
                                                <td>
                                                    <p-button
                                                        icon="pi pi-trash"
                                                        [rounded]="true"
                                                        [text]="true"
                                                        severity="danger"
                                                        (onClick)="confirmDeleteFile(file)"
                                                    ></p-button>
                                                </td>
                                            </tr>
                                        </ng-template>
                                    </p-table>

                                    <div class="empty-files" *ngIf="!release.files || release.files.length === 0">
                                        <i class="pi pi-inbox"></i>
                                        <p>{{ 'admin.software.noFiles' | translate }}</p>
                                    </div>
                                </div>
                            </div>
                        </p-accordion-content>
                    </p-accordion-panel>
                </p-accordion>

                <div class="empty-state" *ngIf="!loading && releases.length === 0">
                    <i class="pi pi-inbox"></i>
                    <h3>{{ 'admin.software.noReleases' | translate }}</h3>
                    <p-button
                        [label]="'admin.software.addFirstRelease' | translate"
                        icon="pi pi-plus"
                        (onClick)="openCreateDialog()"
                    ></p-button>
                </div>
            </div>
        </div>

        <!-- Create/Edit Release Dialog -->
        <p-dialog
            [(visible)]="showReleaseDialog"
            [header]="editMode ? ('admin.software.editRelease' | translate) : ('admin.software.addRelease' | translate)"
            [modal]="true"
            [style]="{width: '600px'}"
        >
            <div class="dialog-content" *ngIf="releaseFormData">
                <div class="form-grid">
                    <div class="form-field">
                        <label>{{ 'admin.software.version' | translate }} *</label>
                        <input pInputText [(ngModel)]="releaseFormData.version" placeholder="1.0.0" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.status' | translate }}</label>
                        <p-select
                            [(ngModel)]="releaseFormData.status"
                            [options]="statusOptions"
                            optionLabel="label"
                            optionValue="value"
                        ></p-select>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.releaseDate' | translate }}</label>
                        <p-datepicker [(ngModel)]="releaseDateValue" dateFormat="yy-mm-dd"></p-datepicker>
                    </div>
                    <div class="form-field checkbox-field">
                        <p-checkbox
                            [(ngModel)]="releaseFormData.isActive"
                            [binary]="true"
                            inputId="releaseIsActive"
                        ></p-checkbox>
                        <label for="releaseIsActive">{{ 'admin.software.isActive' | translate }}</label>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.changelogEn' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="releaseFormData.changelogEn" rows="4"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.changelogAr' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="releaseFormData.changelogAr" rows="4" dir="rtl"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.requirementsEn' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="releaseFormData.requirementsEn" rows="3"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.requirementsAr' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="releaseFormData.requirementsAr" rows="3" dir="rtl"></textarea>
                    </div>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" severity="secondary" (onClick)="showReleaseDialog = false"></p-button>
                <p-button [label]="'common.save' | translate" (onClick)="saveRelease()" [loading]="saving"></p-button>
            </ng-template>
        </p-dialog>

        <!-- Upload File Dialog -->
        <p-dialog
            [(visible)]="showUploadDialog"
            [header]="'admin.software.uploadFile' | translate"
            [modal]="true"
            [style]="{width: '500px'}"
        >
            <div class="dialog-content">
                <div class="form-grid">
                    <div class="form-field">
                        <label>{{ 'software.platform' | translate }}</label>
                        <p-select [(ngModel)]="uploadOs" [options]="osOptions" optionLabel="label" optionValue="value"></p-select>
                    </div>
                    <div class="form-field">
                        <label>{{ 'software.architecture' | translate }}</label>
                        <p-select [(ngModel)]="uploadArch" [options]="archOptions" optionLabel="label" optionValue="value"></p-select>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'software.type' | translate }}</label>
                        <p-select [(ngModel)]="uploadFileType" [options]="fileTypeOptions" optionLabel="label" optionValue="value"></p-select>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.selectFile' | translate }}</label>
                        <input type="file" (change)="onFileSelect($event)" class="file-input" />
                    </div>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" severity="secondary" (onClick)="showUploadDialog = false"></p-button>
                <p-button [label]="'admin.software.upload' | translate" (onClick)="uploadFile()" [loading]="uploading" [disabled]="!selectedUploadFile"></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .software-releases-admin {
            padding: 1.5rem;
        }

        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 1.5rem;
        }

        .back-link {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            color: #6b7280;
            text-decoration: none;
            font-size: 0.9rem;
            margin-bottom: 0.5rem;
        }

        .back-link:hover {
            color: #667eea;
        }

        .header-content h1 {
            margin: 0;
            font-size: 1.5rem;
        }

        .releases-list {
            background: white;
            border-radius: 8px;
            padding: 1rem;
        }

        .release-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            width: 100%;
        }

        .version {
            font-weight: 600;
            font-size: 1.1rem;
        }

        .release-date {
            color: #6b7280;
            margin-inline-start: auto;
        }

        .release-actions {
            display: flex;
            gap: 0.25rem;
        }

        .release-content {
            padding: 1rem 0;
        }

        .files-section {
            background: #f8fafc;
            border-radius: 8px;
            padding: 1rem;
        }

        .files-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .files-header h4 {
            margin: 0;
        }

        .empty-files {
            text-align: center;
            padding: 2rem;
            color: #6b7280;
        }

        .empty-files i {
            font-size: 2rem;
            margin-bottom: 0.5rem;
        }

        .empty-state {
            text-align: center;
            padding: 3rem;
            color: #6b7280;
        }

        .empty-state i {
            font-size: 3rem;
            margin-bottom: 1rem;
        }

        .dialog-content {
            padding: 1rem 0;
        }

        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
        }

        .form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-field.full-width {
            grid-column: 1 / -1;
        }

        .form-field.checkbox-field {
            flex-direction: row;
            align-items: center;
        }

        .form-field label {
            font-weight: 500;
            font-size: 0.9rem;
        }

        .form-field input,
        .form-field textarea {
            width: 100%;
        }

        .file-input {
            padding: 0.5rem;
            border: 1px solid #e5e7eb;
            border-radius: 6px;
        }
    `]
})
export class SoftwareReleasesComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly softwareApi = inject(SoftwareApiService);
    private readonly translate = inject(TranslateService);
    private readonly confirmationService = inject(ConfirmationService);
    private readonly messageService = inject(MessageService);

    productId: string = '';
    releases: SoftwareRelease[] = [];
    loading = true;

    showReleaseDialog = false;
    editMode = false;
    saving = false;
    releaseFormData: CreateSoftwareReleaseRequest | null = null;
    editingReleaseId: string | null = null;
    releaseDateValue: Date | null = null;

    showUploadDialog = false;
    uploading = false;
    uploadReleaseId: string = '';
    uploadOs = 'Windows';
    uploadArch = 'x64';
    uploadFileType = 'Installer';
    selectedUploadFile: File | null = null;

    statusOptions = [
        { label: 'Stable', value: 'Stable' },
        { label: 'Beta', value: 'Beta' },
        { label: 'Alpha', value: 'Alpha' },
        { label: 'Deprecated', value: 'Deprecated' }
    ];

    osOptions = [
        { label: 'Windows', value: 'Windows' },
        { label: 'macOS', value: 'MacOS' },
        { label: 'Linux', value: 'Linux' },
        { label: 'Cross-Platform', value: 'CrossPlatform' }
    ];

    archOptions = [
        { label: 'x64', value: 'x64' },
        { label: 'x86', value: 'x86' },
        { label: 'ARM64', value: 'ARM64' },
        { label: 'Universal', value: 'Universal' }
    ];

    fileTypeOptions = [
        { label: 'Installer', value: 'Installer' },
        { label: 'Portable', value: 'Portable' },
        { label: 'ZIP Archive', value: 'Zip' },
        { label: 'Documentation', value: 'Docs' },
        { label: 'Source Code', value: 'Source' }
    ];

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.productId = params['id'];
            if (this.productId) {
                this.loadReleases();
            }
        });
    }

    loadReleases() {
        this.loading = true;
        this.softwareApi.getReleases(this.productId).subscribe({
            next: (releases) => {
                this.releases = releases;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        this.editMode = false;
        this.editingReleaseId = null;
        this.releaseDateValue = new Date();
        this.releaseFormData = {
            version: '',
            status: 'Stable',
            changelogEn: '',
            changelogAr: '',
            requirementsEn: '',
            requirementsAr: '',
            isActive: true
        };
        this.showReleaseDialog = true;
    }

    openEditDialog(release: SoftwareRelease) {
        this.editMode = true;
        this.editingReleaseId = release.id;
        this.releaseDateValue = new Date(release.releaseDate);
        this.releaseFormData = {
            version: release.version,
            status: release.status,
            changelogEn: release.changelogEn || '',
            changelogAr: release.changelogAr || '',
            requirementsEn: release.requirementsEn || '',
            requirementsAr: release.requirementsAr || '',
            isActive: release.isActive
        };
        this.showReleaseDialog = true;
    }

    saveRelease() {
        if (!this.releaseFormData || !this.releaseFormData.version) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translate.instant('common.warning'),
                detail: this.translate.instant('admin.software.versionRequired')
            });
            return;
        }

        this.releaseFormData.releaseDate = this.releaseDateValue?.toISOString();
        this.saving = true;

        const request = this.editMode && this.editingReleaseId
            ? this.softwareApi.updateRelease(this.editingReleaseId, this.releaseFormData)
            : this.softwareApi.createRelease(this.productId, this.releaseFormData);

        request.subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('common.success'),
                    detail: this.translate.instant(this.editMode ? 'admin.software.releaseUpdated' : 'admin.software.releaseCreated')
                });
                this.showReleaseDialog = false;
                this.saving = false;
                this.loadReleases();
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: err.error || this.translate.instant('common.saveFailed')
                });
                this.saving = false;
            }
        });
    }

    confirmDeleteRelease(release: SoftwareRelease) {
        this.confirmationService.confirm({
            message: this.translate.instant('admin.software.deleteReleaseConfirm', { version: release.version }),
            header: this.translate.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.softwareApi.deleteRelease(release.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translate.instant('common.success'),
                            detail: this.translate.instant('admin.software.releaseDeleted')
                        });
                        this.loadReleases();
                    }
                });
            }
        });
    }

    openUploadDialog(release: SoftwareRelease) {
        this.uploadReleaseId = release.id;
        this.selectedUploadFile = null;
        this.uploadOs = 'Windows';
        this.uploadArch = 'x64';
        this.uploadFileType = 'Installer';
        this.showUploadDialog = true;
    }

    onFileSelect(event: any) {
        this.selectedUploadFile = event.target.files[0] || null;
    }

    uploadFile() {
        if (!this.selectedUploadFile) return;

        this.uploading = true;
        this.softwareApi.uploadFile(
            this.uploadReleaseId,
            this.selectedUploadFile,
            this.uploadOs,
            this.uploadArch,
            this.uploadFileType
        ).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('common.success'),
                    detail: this.translate.instant('admin.software.fileUploaded')
                });
                this.showUploadDialog = false;
                this.uploading = false;
                this.loadReleases();
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: err.error || this.translate.instant('admin.software.uploadFailed')
                });
                this.uploading = false;
            }
        });
    }

    confirmDeleteFile(file: SoftwareFile) {
        this.confirmationService.confirm({
            message: this.translate.instant('admin.software.deleteFileConfirm', { name: file.fileName }),
            header: this.translate.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.softwareApi.deleteFile(file.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translate.instant('common.success'),
                            detail: this.translate.instant('admin.software.fileDeleted')
                        });
                        this.loadReleases();
                    }
                });
            }
        });
    }

    getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
        switch (status?.toLowerCase()) {
            case 'stable': return 'success';
            case 'beta': return 'warn';
            case 'alpha': return 'info';
            case 'deprecated': return 'danger';
            default: return 'secondary';
        }
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    formatFileSize(bytes: number): string {
        if (bytes >= 1073741824) return (bytes / 1073741824).toFixed(1) + ' GB';
        if (bytes >= 1048576) return (bytes / 1048576).toFixed(1) + ' MB';
        if (bytes >= 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return bytes + ' B';
    }
}
