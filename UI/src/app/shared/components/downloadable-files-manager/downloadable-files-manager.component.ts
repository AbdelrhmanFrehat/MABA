import {
    Component, Input, OnInit, OnChanges, SimpleChanges, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressBarModule } from 'primeng/progressbar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DownloadableFilesService } from '../../services/downloadable-files.service';
import {
    DownloadableFileDto,
    DOWNLOAD_CATEGORIES,
    categoryIcon,
    formatFileSize
} from '../../models/downloadable-file.model';

@Component({
    selector: 'app-downloadable-files-manager',
    standalone: true,
    imports: [
        CommonModule, FormsModule,
        ButtonModule, SelectModule, InputTextModule, TextareaModule,
        TagModule, TooltipModule, ProgressBarModule,
        TranslateModule, ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast key="dlm-toast" />
        <div class="dlm-container">
            <div class="dlm-header">
                <h3 class="dlm-title">
                    <i class="pi pi-download"></i>
                    {{ 'downloads.manager.title' | translate }}
                </h3>
                <p class="dlm-subtitle">{{ 'downloads.manager.subtitle' | translate }}</p>
            </div>

            <!-- Upload Panel -->
            <div class="dlm-upload-panel">
                <div class="upload-drop-zone"
                     [class.drag-over]="isDragOver"
                     (dragover)="onDragOver($event)"
                     (dragleave)="isDragOver = false"
                     (drop)="onDrop($event)"
                     (click)="fileInput.click()">
                    <input #fileInput type="file" hidden
                           [accept]="acceptedTypes"
                           (change)="onFileSelected($event)" />
                    @if (!pendingFile) {
                        <div class="drop-placeholder">
                            <i class="pi pi-cloud-upload"></i>
                            <span>{{ 'downloads.manager.dropOrClick' | translate }}</span>
                            <small>{{ 'downloads.manager.allowedTypes' | translate }}</small>
                        </div>
                    } @else {
                        <div class="pending-file-preview">
                            <i class="pi pi-file" style="color: #667eea; font-size: 1.5rem;"></i>
                            <div class="pending-info">
                                <span class="pending-name">{{ pendingFile.name }}</span>
                                <small class="pending-size">{{ formatFileSize(pendingFile.size) }}</small>
                            </div>
                            <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="danger"
                                      (onClick)="clearPending($event)" pTooltip="{{ 'common.remove' | translate }}">
                            </p-button>
                        </div>
                    }
                </div>

                @if (pendingFile) {
                    <div class="upload-meta-grid">
                        <div class="meta-field full">
                            <label>{{ 'downloads.manager.fileTitle' | translate }} <span class="req">*</span></label>
                            <input pInputText [(ngModel)]="newTitle" class="w-full"
                                   [placeholder]="'downloads.manager.fileTitlePlaceholder' | translate" />
                        </div>
                        <div class="meta-field">
                            <label>{{ 'downloads.manager.category' | translate }}</label>
                            <p-select [(ngModel)]="newCategory"
                                      [options]="categoryOptions"
                                      optionLabel="label" optionValue="value"
                                      styleClass="w-full">
                            </p-select>
                        </div>
                        <div class="meta-field">
                            <label>{{ 'downloads.manager.sortOrder' | translate }}</label>
                            <input pInputText type="number" [(ngModel)]="newSortOrder" class="w-full" min="0" />
                        </div>
                        <div class="meta-field full">
                            <label>{{ 'downloads.manager.description' | translate }}</label>
                            <textarea pTextarea [(ngModel)]="newDescription" rows="2" class="w-full"
                                      [placeholder]="'downloads.manager.descriptionPlaceholder' | translate">
                            </textarea>
                        </div>
                    </div>

                    @if (uploading) {
                        <p-progressBar mode="indeterminate" styleClass="upload-progress"></p-progressBar>
                    }

                    <div class="upload-actions">
                        <p-button [label]="'downloads.manager.upload' | translate"
                                  icon="pi pi-upload"
                                  [loading]="uploading"
                                  [disabled]="!newTitle.trim() || uploading"
                                  (onClick)="uploadFile()">
                        </p-button>
                        <p-button [label]="'common.cancel' | translate"
                                  [outlined]="true"
                                  (onClick)="clearPending()">
                        </p-button>
                    </div>
                }
            </div>

            <!-- Files List -->
            <div class="dlm-files-section">
                @if (loading) {
                    <div class="files-loading">
                        <i class="pi pi-spin pi-spinner"></i>
                        <span>{{ 'common.loading' | translate }}</span>
                    </div>
                } @else if (files.length === 0) {
                    <div class="files-empty">
                        <i class="pi pi-inbox"></i>
                        <span>{{ 'downloads.manager.noFiles' | translate }}</span>
                    </div>
                } @else {
                    <div class="files-list">
                        @for (file of files; track file.id) {
                            <div class="file-row" [class.inactive]="!file.isActive">
                                <div class="file-icon-col">
                                    <i [class]="getCategoryIcon(file.category)" class="file-type-icon"></i>
                                </div>

                                <div class="file-info-col">
                                    @if (editingId === file.id) {
                                        <!-- Edit mode -->
                                        <input pInputText [(ngModel)]="editTitle" class="w-full edit-input"
                                               [placeholder]="'downloads.manager.fileTitle' | translate" />
                                        <div class="edit-row">
                                            <p-select [(ngModel)]="editCategory"
                                                      [options]="categoryOptions"
                                                      optionLabel="label" optionValue="value"
                                                      styleClass="edit-select">
                                            </p-select>
                                            <input pInputText type="number" [(ngModel)]="editSortOrder"
                                                   class="edit-order" min="0"
                                                   [placeholder]="'downloads.manager.sortOrder' | translate" />
                                        </div>
                                        <textarea pTextarea [(ngModel)]="editDescription" rows="2"
                                                  class="w-full edit-input"
                                                  [placeholder]="'downloads.manager.description' | translate">
                                        </textarea>
                                    } @else {
                                        <!-- View mode -->
                                        <span class="file-title">{{ file.title }}</span>
                                        <div class="file-meta">
                                            <span class="file-category">{{ getCategoryLabel(file.category) }}</span>
                                            <span class="file-size">{{ formatFileSize(file.fileSizeBytes) }}</span>
                                            <span class="file-downloads">
                                                <i class="pi pi-download"></i> {{ file.downloadCount }}
                                            </span>
                                        </div>
                                        @if (file.description) {
                                            <p class="file-description">{{ file.description }}</p>
                                        }
                                    }
                                </div>

                                <div class="file-actions-col">
                                    @if (editingId === file.id) {
                                        <p-button icon="pi pi-check" [rounded]="true" [text]="true" severity="success"
                                                  pTooltip="{{ 'common.save' | translate }}"
                                                  [loading]="saving"
                                                  (onClick)="saveEdit(file)">
                                        </p-button>
                                        <p-button icon="pi pi-times" [rounded]="true" [text]="true"
                                                  pTooltip="{{ 'common.cancel' | translate }}"
                                                  (onClick)="cancelEdit()">
                                        </p-button>
                                    } @else {
                                        <p-button [icon]="file.isActive ? 'pi pi-eye' : 'pi pi-eye-slash'"
                                                  [rounded]="true" [text]="true"
                                                  [severity]="file.isActive ? 'success' : 'secondary'"
                                                  [pTooltip]="(file.isActive ? 'downloads.manager.deactivate' : 'downloads.manager.activate') | translate"
                                                  (onClick)="toggleActive(file)">
                                        </p-button>
                                        <p-button icon="pi pi-pencil" [rounded]="true" [text]="true"
                                                  pTooltip="{{ 'common.edit' | translate }}"
                                                  (onClick)="startEdit(file)">
                                        </p-button>
                                        <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger"
                                                  pTooltip="{{ 'common.delete' | translate }}"
                                                  (onClick)="deleteFile(file)">
                                        </p-button>
                                    }
                                    @if (!file.isActive) {
                                        <span class="inactive-badge">{{ 'downloads.manager.hidden' | translate }}</span>
                                    }
                                </div>
                            </div>
                        }
                    </div>
                }
            </div>
        </div>
    `,
    styles: [`
        .dlm-container { border: 1px solid #e2e8f0; border-radius: 10px; background: #fff; overflow: hidden; }
        .dlm-header { padding: 1rem 1.25rem 0.75rem; border-bottom: 1px solid #f1f5f9; background: #f8fafc; }
        .dlm-title { margin: 0 0 0.25rem; font-size: 1rem; font-weight: 700; color: #1e293b; display: flex; align-items: center; gap: 0.5rem; }
        .dlm-title i { color: #667eea; }
        .dlm-subtitle { margin: 0; font-size: 0.8rem; color: #64748b; }

        /* Upload panel */
        .dlm-upload-panel { padding: 1rem 1.25rem; border-bottom: 1px solid #f1f5f9; }
        .upload-drop-zone { border: 2px dashed #cbd5e1; border-radius: 8px; padding: 1rem; cursor: pointer; transition: border-color 0.2s, background 0.2s; text-align: center; }
        .upload-drop-zone:hover, .upload-drop-zone.drag-over { border-color: #667eea; background: #f0f4ff; }
        .drop-placeholder { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; color: #94a3b8; }
        .drop-placeholder i { font-size: 2rem; color: #667eea; }
        .drop-placeholder span { font-size: 0.875rem; font-weight: 500; color: #475569; }
        .drop-placeholder small { font-size: 0.75rem; }
        .pending-file-preview { display: flex; align-items: center; gap: 0.75rem; text-align: left; }
        .pending-info { flex: 1; min-width: 0; }
        .pending-name { display: block; font-size: 0.875rem; font-weight: 500; color: #1e293b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .pending-size { font-size: 0.75rem; color: #64748b; }

        .upload-meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-top: 0.75rem; }
        .meta-field { display: flex; flex-direction: column; gap: 0.35rem; }
        .meta-field.full { grid-column: 1/-1; }
        .meta-field label { font-size: 0.8rem; font-weight: 600; color: #475569; }
        .req { color: #dc2626; }
        .upload-progress { margin-top: 0.75rem; }
        .upload-actions { display: flex; gap: 0.5rem; margin-top: 0.75rem; }

        /* Files list */
        .dlm-files-section { padding: 0.75rem 1.25rem 1rem; }
        .files-loading, .files-empty { display: flex; align-items: center; gap: 0.75rem; color: #94a3b8; padding: 1rem 0; }
        .files-empty i { font-size: 1.5rem; }
        .files-list { display: flex; flex-direction: column; gap: 0; }

        .file-row { display: flex; align-items: flex-start; gap: 0.75rem; padding: 0.75rem 0; border-bottom: 1px solid #f1f5f9; transition: background 0.15s; }
        .file-row:last-child { border-bottom: none; }
        .file-row.inactive { opacity: 0.6; background: #fafafa; }

        .file-icon-col { padding-top: 0.1rem; }
        .file-type-icon { font-size: 1.2rem; color: #667eea; }
        .file-info-col { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 0.25rem; }
        .file-title { font-size: 0.875rem; font-weight: 600; color: #1e293b; }
        .file-meta { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
        .file-category { font-size: 0.75rem; background: #f0f4ff; color: #667eea; padding: 0.15rem 0.5rem; border-radius: 4px; font-weight: 500; }
        .file-size { font-size: 0.75rem; color: #64748b; }
        .file-downloads { font-size: 0.75rem; color: #94a3b8; display: flex; align-items: center; gap: 0.25rem; }
        .file-description { margin: 0; font-size: 0.8rem; color: #64748b; }

        .file-actions-col { display: flex; align-items: center; gap: 0.15rem; flex-shrink: 0; }
        .inactive-badge { font-size: 0.7rem; background: #fef3c7; color: #92400e; padding: 0.1rem 0.4rem; border-radius: 4px; font-weight: 600; margin-left: 0.25rem; }

        /* Edit mode inputs */
        .edit-input { margin-bottom: 0.4rem; font-size: 0.875rem; }
        .edit-row { display: flex; gap: 0.5rem; margin-bottom: 0.4rem; }
        .edit-select { min-width: 140px; }
        .edit-order { width: 70px; }

        .w-full { width: 100%; }
    `]
})
export class DownloadableFilesManagerComponent implements OnInit, OnChanges {
    @Input({ required: true }) entityType!: string;
    @Input({ required: true }) entityId!: string;

    private service = inject(DownloadableFilesService);
    private translate = inject(TranslateService);
    private messageService = inject(MessageService);

    files: DownloadableFileDto[] = [];
    loading = false;
    uploading = false;
    saving = false;

    // Upload state
    pendingFile: File | null = null;
    isDragOver = false;
    newTitle = '';
    newCategory = 'Datasheet';
    newDescription = '';
    newSortOrder = 0;

    // Edit state
    editingId: string | null = null;
    editTitle = '';
    editCategory = '';
    editDescription = '';
    editSortOrder = 0;

    readonly acceptedTypes = '.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.png,.jpg,.jpeg,.webp,.svg,.txt,.csv,.dwg,.dxf,.step,.stp,.stl,.obj,.bin,.hex';
    readonly formatFileSize = formatFileSize;

    categoryOptions: { label: string; value: string }[] = [];

    ngOnInit() {
        this.buildCategoryOptions();
        this.translate.onLangChange.subscribe(() => this.buildCategoryOptions());
        if (this.entityId) this.loadFiles();
    }

    ngOnChanges(changes: SimpleChanges) {
        if ((changes['entityId'] || changes['entityType']) && this.entityId) {
            this.loadFiles();
        }
    }

    buildCategoryOptions() {
        this.categoryOptions = DOWNLOAD_CATEGORIES.map(c => ({
            value: c.value,
            label: this.translate.instant(c.labelKey)
        }));
    }

    loadFiles() {
        this.loading = true;
        this.service.listAdmin(this.entityType, this.entityId).subscribe({
            next: (files) => { this.files = files; this.loading = false; },
            error: () => { this.loading = false; }
        });
    }

    // ── Upload ──────────────────────────────────

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files?.[0]) this.setPending(input.files[0]);
    }

    onDragOver(event: DragEvent) {
        event.preventDefault();
        this.isDragOver = true;
    }

    onDrop(event: DragEvent) {
        event.preventDefault();
        this.isDragOver = false;
        const file = event.dataTransfer?.files?.[0];
        if (file) this.setPending(file);
    }

    setPending(file: File) {
        this.pendingFile = file;
        if (!this.newTitle) this.newTitle = file.name.replace(/\.[^/.]+$/, '');
    }

    clearPending(event?: Event) {
        event?.stopPropagation();
        this.pendingFile = null;
        this.newTitle = '';
        this.newCategory = 'Datasheet';
        this.newDescription = '';
        this.newSortOrder = 0;
    }

    uploadFile() {
        if (!this.pendingFile || !this.newTitle.trim()) return;
        this.uploading = true;
        this.service.upload(
            this.entityType, this.entityId,
            this.pendingFile, this.newTitle.trim(),
            this.newCategory, this.newDescription || undefined,
            this.newSortOrder
        ).subscribe({
            next: (file) => {
                this.files = [...this.files, file].sort((a, b) => a.sortOrder - b.sortOrder || a.createdAt.localeCompare(b.createdAt));
                this.uploading = false;
                this.clearPending();
                this.messageService.add({ key: 'dlm-toast', severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('downloads.manager.uploadSuccess') });
            },
            error: (err) => {
                this.uploading = false;
                const detail = err?.error || this.translate.instant('messages.saveError');
                this.messageService.add({ key: 'dlm-toast', severity: 'error', summary: this.translate.instant('messages.error'), detail });
            }
        });
    }

    // ── Edit ──────────────────────────────────

    startEdit(file: DownloadableFileDto) {
        this.editingId = file.id;
        this.editTitle = file.title;
        this.editCategory = file.category;
        this.editDescription = file.description ?? '';
        this.editSortOrder = file.sortOrder;
    }

    cancelEdit() {
        this.editingId = null;
    }

    saveEdit(file: DownloadableFileDto) {
        if (!this.editTitle.trim()) return;
        this.saving = true;
        this.service.update(file.id, {
            title: this.editTitle.trim(),
            category: this.editCategory,
            description: this.editDescription || undefined,
            sortOrder: this.editSortOrder
        }).subscribe({
            next: (updated) => {
                this.files = this.files.map(f => f.id === updated.id ? updated : f);
                this.saving = false;
                this.editingId = null;
            },
            error: () => { this.saving = false; }
        });
    }

    toggleActive(file: DownloadableFileDto) {
        this.service.update(file.id, { isActive: !file.isActive }).subscribe({
            next: (updated) => {
                this.files = this.files.map(f => f.id === updated.id ? updated : f);
            }
        });
    }

    deleteFile(file: DownloadableFileDto) {
        if (!confirm(this.translate.instant('downloads.manager.confirmDelete', { title: file.title }))) return;
        this.service.delete(file.id).subscribe({
            next: () => {
                this.files = this.files.filter(f => f.id !== file.id);
                this.messageService.add({ key: 'dlm-toast', severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('downloads.manager.deleteSuccess') });
            }
        });
    }

    getCategoryIcon(category: string): string {
        return categoryIcon(category);
    }

    getCategoryLabel(category: string): string {
        const opt = DOWNLOAD_CATEGORIES.find(c => c.value === category);
        return opt ? this.translate.instant(opt.labelKey) : category;
    }
}
