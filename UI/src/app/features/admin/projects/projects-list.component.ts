import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { Project, ProjectListItem, ProjectCategory, ProjectStatus } from '../../../shared/models/project.model';
import { LanguageService } from '../../../shared/services/language.service';
import { MediaApiService } from '../../../shared/services/media-api.service';
import { DownloadableFilesManagerComponent } from '../../../shared/components/downloadable-files-manager/downloadable-files-manager.component';

@Component({
    selector: 'app-admin-projects-list',
    standalone: true,
    imports: [
        CommonModule, RouterModule, FormsModule, TranslateModule,
        ButtonModule, TableModule, TagModule, InputTextModule,
        ToastModule, DialogModule, ConfirmDialogModule, TooltipModule,
        DownloadableFilesManagerComponent
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast></p-toast>
        <p-confirmDialog></p-confirmDialog>
        
        <div class="projects-admin">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.projects.title' | translate }}</h1>
                        <p>{{ 'admin.projects.subtitle' | translate }}</p>
                    </div>
                    <p-button 
                        [label]="'admin.projects.addProject' | translate"
                        icon="pi pi-plus"
                        (onClick)="openCreateDialog()">
                    </p-button>
                </div>
            </div>

            <div class="table-section">
                <p-table 
                    [value]="projects()" 
                    [loading]="loading()"
                    [paginator]="true"
                    [rows]="10"
                    [showCurrentPageReport]="true"
                    [globalFilterFields]="['titleEn', 'titleAr', 'slug']"
                    styleClass="p-datatable-sm">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.projects.name' | translate }}</th>
                            <th>{{ 'admin.projects.category' | translate }}</th>
                            <th>{{ 'admin.projects.projectStatus' | translate }}</th>
                            <th>{{ 'admin.projects.year' | translate }}</th>
                            <th>{{ 'admin.projects.featured' | translate }}</th>
                            <th>{{ 'admin.projects.active' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-project>
                        <tr>
                            <td>
                                <div class="project-name">
                                    <strong>{{ project.titleEn }}</strong>
                                    <small>{{ project.slug }}</small>
                                </div>
                            </td>
                            <td>
                                <p-tag [value]="'projects.categories.' + project.categoryName | translate" [severity]="getCategorySeverity(project.category)"></p-tag>
                            </td>
                            <td>
                                <p-tag [value]="'projects.statuses.' + project.statusName | translate" [severity]="getStatusSeverity(project.status)"></p-tag>
                            </td>
                            <td>{{ project.year }}</td>
                            <td>
                                <i [class]="project.isFeatured ? 'pi pi-star-fill text-yellow-500' : 'pi pi-star text-gray-300'"></i>
                            </td>
                            <td>
                                <i [class]="project.isActive ? 'pi pi-check-circle status-active' : 'pi pi-times-circle status-inactive'"></i>
                            </td>
                            <td>
                                <div class="action-buttons">
                                    <p-button icon="pi pi-eye" [text]="true" [routerLink]="['/projects', project.slug]" pTooltip="View"></p-button>
                                    <p-button icon="pi pi-pencil" [text]="true" (onClick)="openEditDialog(project)" pTooltip="Edit"></p-button>
                                    <p-button icon="pi pi-trash" [text]="true" severity="danger" (onClick)="confirmDelete(project)" pTooltip="Delete"></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="7" class="text-center p-4">
                                {{ 'admin.projects.noProjects' | translate }}
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>

        <!-- Create/Edit Dialog -->
        <p-dialog
            [(visible)]="dialogVisible"
            [modal]="true"
            [style]="{width: '700px'}"
            [contentStyle]="{overflow: 'auto'}">
            <ng-template pTemplate="header">
                <div class="dlg-header">
                    <span class="dlg-label">Projects</span>
                    <span class="dlg-title">{{ editingProject ? ('admin.projects.editProject' | translate) : ('admin.projects.addProject' | translate) }}</span>
                </div>
            </ng-template>
            <div class="dialog-form">
                <div class="form-grid">
                    <div class="form-field">
                        <label>{{ 'admin.projects.titleEn' | translate }} *</label>
                        <input pInputText [(ngModel)]="formData.titleEn" class="w-full" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.projects.titleAr' | translate }}</label>
                        <input pInputText [(ngModel)]="formData.titleAr" class="w-full" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.projects.slug' | translate }}</label>
                        <input pInputText [(ngModel)]="formData.slug" class="w-full" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.projects.year' | translate }} *</label>
                        <input pInputText type="number" [(ngModel)]="formData.year" class="w-full" />
                    </div>
                </div>
                <div class="form-grid">
                    <div class="form-field">
                        <label>{{ 'admin.projects.category' | translate }} *</label>
                        <select [(ngModel)]="formData.category" class="w-full p-inputtext">
                            @for (cat of categoryOptions; track cat.value) {
                                <option [value]="cat.value">{{ cat.label }}</option>
                            }
                        </select>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.projects.projectStatus' | translate }}</label>
                        <select [(ngModel)]="formData.status" class="w-full p-inputtext">
                            @for (st of statusOptions; track st.value) {
                                <option [value]="st.value">{{ st.label }}</option>
                            }
                        </select>
                    </div>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.projects.summaryEn' | translate }}</label>
                    <textarea pInputText [(ngModel)]="formData.summaryEn" rows="3" class="w-full"></textarea>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.projects.summaryAr' | translate }}</label>
                    <textarea pInputText [(ngModel)]="formData.summaryAr" rows="3" class="w-full"></textarea>
                </div>
                <!-- Cover Image -->
                <div class="form-field">
                    <label>Cover Image</label>
                    <input type="file" #coverInput accept="image/*" class="hidden-input" (change)="onCoverSelected($event)" />
                    <div class="upload-row">
                        <p-button
                            [label]="formData.coverImageUrl ? 'Replace Cover' : 'Upload Cover'"
                            icon="pi pi-image"
                            [outlined]="true"
                            size="small"
                            [loading]="uploadingCover"
                            (click)="coverInput.click()">
                        </p-button>
                        <button *ngIf="formData.coverImageUrl" type="button" class="clear-btn" (click)="formData.coverImageUrl = ''" pTooltip="Remove">
                            <i class="pi pi-times"></i>
                        </button>
                    </div>
                    <img *ngIf="formData.coverImageUrl" [src]="resolveUrl(formData.coverImageUrl)" alt="cover" class="cover-preview" />
                </div>

                <!-- Gallery Images -->
                <div class="form-field">
                    <label>Gallery Images <span class="field-hint">(shown on the project page)</span></label>
                    <input type="file" #galleryInput accept="image/*" multiple class="hidden-input" (change)="onGallerySelected($event)" />
                    <p-button
                        label="Add Images"
                        icon="pi pi-plus"
                        [outlined]="true"
                        size="small"
                        [loading]="uploadingGallery"
                        (click)="galleryInput.click()">
                    </p-button>
                    <div class="gallery-grid" *ngIf="galleryImages.length > 0">
                        <div class="gallery-thumb" *ngFor="let url of galleryImages; let i = index">
                            <img [src]="resolveUrl(url)" alt="gallery" />
                            <button type="button" class="thumb-remove" (click)="removeGalleryImage(i)"><i class="pi pi-times"></i></button>
                        </div>
                    </div>
                </div>

                <div class="form-field">
                    <label>{{ 'admin.projects.techStack' | translate }}</label>
                    <input pInputText [(ngModel)]="techStackInput" class="w-full" [placeholder]="'admin.projects.techStackPlaceholder' | translate" />
                </div>
                <div class="form-grid checkbox-grid">
                    <label class="checkbox-field">
                        <input type="checkbox" [(ngModel)]="formData.isFeatured" />
                        {{ 'admin.projects.featured' | translate }}
                    </label>
                    <label class="checkbox-field">
                        <input type="checkbox" [(ngModel)]="formData.isActive" />
                        {{ 'admin.projects.active' | translate }}
                    </label>
                </div>

                <!-- Downloadable Files (PDFs, docs) -->
                @if (editingProject) {
                    <div class="downloads-divider">
                        <div class="section-label">
                            <i class="pi pi-file"></i> Downloadable Files <span class="field-hint">(PDFs, docs — visible on project page)</span>
                        </div>
                        <app-downloadable-files-manager
                            entityType="Project"
                            [entityId]="editingProject.id">
                        </app-downloadable-files-manager>
                    </div>
                } @else {
                    <div class="create-files-note">
                        <i class="pi pi-info-circle"></i>
                        Save the project first to attach downloadable files (PDFs, documents).
                    </div>
                }
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" [text]="true" (onClick)="dialogVisible = false"></p-button>
                <p-button [label]="'common.save' | translate" (onClick)="saveProject()" [loading]="saving()"></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .projects-admin { padding: 1.5rem; }
        .page-header { margin-bottom: 1.5rem; }
        .header-content { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
        .header-content h1 { margin: 0 0 0.25rem 0; font-size: 1.5rem; }
        .header-content p { margin: 0; color: #6b7280; }
        .project-name { display: flex; flex-direction: column; gap: 0.25rem; }
        .project-name small { color: #9ca3af; font-size: 0.75rem; }
        .action-buttons { display: flex; gap: 0.25rem; }
        .dialog-form { display: flex; flex-direction: column; gap: 1rem; }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        .form-field { display: flex; flex-direction: column; gap: 0.5rem; }
        .form-field label { font-weight: 600; font-size: 0.875rem; }
        .field-hint { font-weight: 400; font-size: 0.78rem; color: #9ca3af; margin-left: 0.35rem; }
        .checkbox-grid { display: flex; gap: 2rem; }
        .checkbox-field { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
        textarea { resize: vertical; min-height: 60px; }
        .hidden-input { display: none; }
        .upload-row { display: flex; align-items: center; gap: 0.5rem; }
        .clear-btn { background: none; border: none; cursor: pointer; color: #9ca3af; padding: 0.25rem; border-radius: 4px; display: flex; align-items: center; }
        .clear-btn:hover { color: #ef4444; }
        .cover-preview { width: 100%; max-height: 140px; object-fit: cover; border-radius: 8px; border: 1px solid #e5e7eb; margin-top: 0.5rem; }
        .gallery-grid { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 0.5rem; }
        .gallery-thumb { position: relative; }
        .gallery-thumb img { width: 80px; height: 60px; object-fit: cover; border-radius: 6px; border: 1px solid #e5e7eb; display: block; }
        .thumb-remove { position: absolute; top: -6px; right: -6px; background: #ef4444; border: none; border-radius: 50%; width: 18px; height: 18px; color: white; font-size: 0.6rem; cursor: pointer; display: flex; align-items: center; justify-content: center; padding: 0; }
        .downloads-divider { margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid #e2e8f0; }
        .section-label { font-weight: 600; font-size: 0.875rem; margin-bottom: 0.75rem; display: flex; align-items: center; gap: 0.4rem; }
        .create-files-note { display: flex; align-items: center; gap: 0.5rem; font-size: 0.82rem; color: #9ca3af; background: #f9fafb; border-radius: 8px; padding: 0.75rem 1rem; border: 1px dashed #e5e7eb; }
    `]
})
export class AdminProjectsListComponent implements OnInit {
    private projectsApi = inject(ProjectsApiService);
    private mediaApi = inject(MediaApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);
    languageService = inject(LanguageService);

    private readonly DEFAULT_IMAGE_MEDIA_TYPE_ID = '00000000-0000-0000-0000-000000000001';

    loading = signal(false);
    saving = signal(false);
    projects = signal<ProjectListItem[]>([]);
    dialogVisible = false;
    editingProject: ProjectListItem | null = null;

    uploadingCover = false;
    uploadingGallery = false;
    galleryImages: string[] = [];

    formData = {
        titleEn: '',
        titleAr: '',
        slug: '',
        summaryEn: '',
        summaryAr: '',
        coverImageUrl: '',
        category: ProjectCategory.Software,
        status: ProjectStatus.Draft,
        year: new Date().getFullYear(),
        isFeatured: false,
        isActive: true
    };

    techStackInput = '';

    categoryOptions = [
        { value: ProjectCategory.Robotics, label: 'Robotics' },
        { value: ProjectCategory.CNC, label: 'CNC' },
        { value: ProjectCategory.Embedded, label: 'Embedded' },
        { value: ProjectCategory.Monitoring, label: 'Monitoring' },
        { value: ProjectCategory.Software, label: 'Software' },
        { value: ProjectCategory.RnD, label: 'R&D' },
        { value: ProjectCategory.Electronics, label: 'Electronics' },
        { value: ProjectCategory.Mechanical, label: 'Mechanical' },
        { value: ProjectCategory.Automation, label: 'Automation' }
    ];

    statusOptions = [
        { value: ProjectStatus.Draft, label: 'Draft' },
        { value: ProjectStatus.Concept, label: 'Concept' },
        { value: ProjectStatus.Prototype, label: 'Prototype' },
        { value: ProjectStatus.InProgress, label: 'In Progress' },
        { value: ProjectStatus.Delivered, label: 'Delivered' },
        { value: ProjectStatus.Completed, label: 'Completed' },
        { value: ProjectStatus.Archived, label: 'Archived' }
    ];

    ngOnInit() {
        this.loadProjects();
    }

    loadProjects() {
        this.loading.set(true);
        this.projectsApi.getProjectsAdmin({ pageSize: 100 }).subscribe({
            next: (response) => {
                this.projects.set(response.items);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load projects' });
            }
        });
    }

    openCreateDialog() {
        this.editingProject = null;
        this.formData = {
            titleEn: '',
            titleAr: '',
            slug: '',
            summaryEn: '',
            summaryAr: '',
            coverImageUrl: '',
            category: ProjectCategory.Software,
            status: ProjectStatus.Draft,
            year: new Date().getFullYear(),
            isFeatured: false,
            isActive: true
        };
        this.techStackInput = '';
        this.galleryImages = [];
        this.dialogVisible = true;
    }

    openEditDialog(project: ProjectListItem) {
        this.editingProject = project;
        this.projectsApi.getProjectById(project.id).subscribe({
            next: (fullProject) => {
                this.formData = {
                    titleEn: fullProject.titleEn,
                    titleAr: fullProject.titleAr || '',
                    slug: fullProject.slug,
                    summaryEn: fullProject.summaryEn || '',
                    summaryAr: fullProject.summaryAr || '',
                    coverImageUrl: fullProject.coverImageUrl || '',
                    category: fullProject.category,
                    status: fullProject.status,
                    year: fullProject.year,
                    isFeatured: fullProject.isFeatured,
                    isActive: fullProject.isActive
                };
                this.techStackInput = fullProject.techStack?.join(', ') || '';
                this.galleryImages = [...(fullProject.gallery || [])];
                this.dialogVisible = true;
            }
        });
    }

    onCoverSelected(event: Event) {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;
        this.uploadingCover = true;
        const form = new FormData();
        form.append('file', file);
        form.append('mediaTypeId', this.DEFAULT_IMAGE_MEDIA_TYPE_ID);
        this.mediaApi.uploadMedia(form).subscribe({
            next: (asset) => { this.formData.coverImageUrl = asset.fileUrl; this.uploadingCover = false; },
            error: () => {
                this.uploadingCover = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Cover image upload failed.' });
            }
        });
    }

    onGallerySelected(event: Event) {
        const files = Array.from((event.target as HTMLInputElement).files || []);
        if (!files.length) return;
        this.uploadingGallery = true;
        let completed = 0;
        files.forEach(file => {
            const form = new FormData();
            form.append('file', file);
            form.append('mediaTypeId', this.DEFAULT_IMAGE_MEDIA_TYPE_ID);
            this.mediaApi.uploadMedia(form).subscribe({
                next: (asset) => {
                    this.galleryImages = [...this.galleryImages, asset.fileUrl];
                    if (++completed === files.length) this.uploadingGallery = false;
                },
                error: () => {
                    if (++completed === files.length) this.uploadingGallery = false;
                    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'One or more gallery images failed to upload.' });
                }
            });
        });
    }

    removeGalleryImage(index: number) {
        this.galleryImages = this.galleryImages.filter((_, i) => i !== index);
    }

    resolveUrl(url: string): string {
        if (!url) return '';
        if (url.startsWith('http') || url.startsWith('/assets')) return url;
        const base = (window as any).__env?.apiUrl || '';
        return base ? `${base.replace(/\/api\/v1\/?$/, '')}${url.startsWith('/') ? url : '/' + url}` : url;
    }

    saveProject() {
        if (!this.formData.titleEn || !this.formData.year) {
            this.messageService.add({ severity: 'warn', summary: 'Warning', detail: this.translate.instant('admin.projects.fillRequired') });
            return;
        }

        this.saving.set(true);
        const techStack = this.techStackInput ? this.techStackInput.split(',').map(t => t.trim()).filter(t => t) : [];

        const payload = {
            ...this.formData,
            techStack,
            gallery: [...this.galleryImages],
            category: Number(this.formData.category),
            status: Number(this.formData.status)
        };

        if (this.editingProject) {
            this.projectsApi.updateProject(this.editingProject.id, { ...payload, id: this.editingProject.id }).subscribe({
                next: () => {
                    this.saving.set(false);
                    this.dialogVisible = false;
                    this.loadProjects();
                    this.messageService.add({ severity: 'success', summary: 'Success', detail: this.translate.instant('admin.projects.updated') });
                },
                error: () => {
                    this.saving.set(false);
                    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update project' });
                }
            });
        } else {
            this.projectsApi.createProject(payload).subscribe({
                next: (created: Project) => {
                    this.saving.set(false);
                    this.loadProjects();
                    this.messageService.add({ severity: 'success', summary: 'Success', detail: this.translate.instant('admin.projects.created') });
                    // Switch to edit mode so the DownloadableFilesManager becomes available for PDF uploads
                    this.editingProject = { id: created.id, titleEn: created.titleEn, titleAr: created.titleAr, slug: created.slug, summaryEn: created.summaryEn, summaryAr: created.summaryAr, coverImageUrl: created.coverImageUrl, category: created.category, categoryName: created.categoryName, status: created.status, statusName: created.statusName, year: created.year, techStack: created.techStack, isFeatured: created.isFeatured };
                },
                error: () => {
                    this.saving.set(false);
                    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create project' });
                }
            });
        }
    }

    confirmDelete(project: ProjectListItem) {
        this.confirmationService.confirm({
            message: this.translate.instant('admin.projects.deleteConfirm'),
            header: 'Confirm Delete',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.projectsApi.deleteProject(project.id).subscribe({
                    next: () => {
                        this.loadProjects();
                        this.messageService.add({ severity: 'success', summary: 'Success', detail: this.translate.instant('admin.projects.deleted') });
                    },
                    error: () => {
                        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete project' });
                    }
                });
            }
        });
    }

    getCategorySeverity(category: ProjectCategory): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectCategory.Robotics]: 'info',
            [ProjectCategory.CNC]: 'warn',
            [ProjectCategory.Embedded]: 'success',
            [ProjectCategory.Monitoring]: 'contrast',
            [ProjectCategory.Software]: 'secondary',
            [ProjectCategory.RnD]: 'danger'
        };
        return map[category] || 'secondary';
    }

    getStatusSeverity(status: ProjectStatus): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectStatus.Concept]: 'warn',
            [ProjectStatus.Prototype]: 'info',
            [ProjectStatus.Delivered]: 'success',
            [ProjectStatus.Draft]: 'secondary'
        };
        return map[status] || 'secondary';
    }
}
