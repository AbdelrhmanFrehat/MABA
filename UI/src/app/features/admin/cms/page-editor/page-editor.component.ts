import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CmsApiService } from '../../../../shared/services/cms-api.service';
import { Page, PageSection, SectionType, CreatePageRequest, UpdatePageRequest, CreatePageSectionRequest } from '../../../../shared/models/cms.model';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { SectionEditorComponent } from '../section-editor/section-editor.component';

@Component({
    selector: 'app-page-editor',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        FormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        CheckboxModule,
        ToastModule,
        DialogModule,
        ConfirmDialogModule,
        TranslateModule,
        EmptyStateComponent,
        TooltipModule,
        TagModule,
        SectionEditorComponent
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="container mx-auto px-4 py-6">
            <div class="flex justify-content-between align-items-center mb-4">
                <div>
                    <h1 class="text-3xl font-bold mb-2">
                        {{ isNewPage ? ('admin.cms.createPage' | translate) : ('admin.cms.editPage' | translate) }}
                    </h1>
                    <p class="text-500" *ngIf="!isNewPage && page">
                        {{ 'admin.cms.lastUpdated' | translate }}: {{ formatDate(page.updatedAt || page.createdAt) }}
                    </p>
                </div>
                <div class="flex gap-2">
                    <p-button 
                        [label]="'common.cancel' | translate"
                        [outlined]="true"
                        (click)="cancel()"
                    ></p-button>
                    <p-button 
                        [label]="'common.save' | translate"
                        icon="pi pi-save"
                        [loading]="saving"
                        [disabled]="pageForm.invalid"
                        (click)="savePage()"
                    ></p-button>
                    <p-button 
                        *ngIf="!isNewPage && page"
                        [label]="'admin.cms.publish' | translate"
                        icon="pi pi-check"
                        severity="success"
                        [disabled]="!canPublish()"
                        (click)="publishPage()"
                    ></p-button>
                    <p-button 
                        *ngIf="!isNewPage && page"
                        [label]="'admin.cms.preview' | translate"
                        icon="pi pi-eye"
                        [outlined]="true"
                        (click)="previewPage()"
                    ></p-button>
                </div>
            </div>

            <div class="grid">
                <!-- Page Info Form -->
                <div class="col-12 lg:col-4">
                    <p-card [header]="'admin.cms.pageInfo' | translate">
                        <form [formGroup]="pageForm" class="flex flex-column gap-3">
                            <div>
                                <label class="block mb-2 font-medium">{{ 'admin.cms.slug' | translate }} *</label>
                                <input 
                                    pInputText 
                                    formControlName="slug"
                                    [placeholder]="'admin.cms.slugPlaceholder' | translate"
                                    class="w-full"
                                    [disabled]="!isNewPage"
                                />
                                <small class="text-red-500" *ngIf="pageForm.get('slug')?.invalid && pageForm.get('slug')?.touched">
                                    {{ 'validation.required' | translate }}
                                </small>
                            </div>

                            <div>
                                <label class="block mb-2 font-medium">{{ 'common.nameEn' | translate }} *</label>
                                <input 
                                    pInputText 
                                    formControlName="titleEn"
                                    [placeholder]="'admin.cms.titleEnPlaceholder' | translate"
                                    class="w-full"
                                />
                                <small class="text-red-500" *ngIf="pageForm.get('titleEn')?.invalid && pageForm.get('titleEn')?.touched">
                                    {{ 'validation.required' | translate }}
                                </small>
                            </div>

                            <div>
                                <label class="block mb-2 font-medium">{{ 'common.nameAr' | translate }} *</label>
                                <input 
                                    pInputText 
                                    formControlName="titleAr"
                                    [placeholder]="'admin.cms.titleArPlaceholder' | translate"
                                    class="w-full"
                                />
                                <small class="text-red-500" *ngIf="pageForm.get('titleAr')?.invalid && pageForm.get('titleAr')?.touched">
                                    {{ 'validation.required' | translate }}
                                </small>
                            </div>

                            <div>
                                <label class="block mb-2 font-medium">{{ 'admin.cms.metaDescriptionEn' | translate }}</label>
                                <textarea 
                                    pInputTextarea 
                                    formControlName="metaDescriptionEn"
                                    [rows]="3"
                                    [placeholder]="'admin.cms.metaDescriptionPlaceholder' | translate"
                                    class="w-full"
                                ></textarea>
                            </div>

                            <div>
                                <label class="block mb-2 font-medium">{{ 'admin.cms.metaDescriptionAr' | translate }}</label>
                                <textarea 
                                    pInputTextarea 
                                    formControlName="metaDescriptionAr"
                                    [rows]="3"
                                    [placeholder]="'admin.cms.metaDescriptionPlaceholder' | translate"
                                    class="w-full"
                                ></textarea>
                            </div>

                            <div class="flex align-items-center">
                                <p-checkbox 
                                    formControlName="isActive"
                                    [binary]="true"
                                    inputId="isActive"
                                ></p-checkbox>
                                <label for="isActive" class="ml-2">{{ 'common.active' | translate }}</label>
                            </div>
                        </form>
                    </p-card>
                </div>

                <!-- Sections Builder -->
                <div class="col-12 lg:col-8">
                    <p-card [header]="'admin.cms.sections' | translate">
                        <div class="flex justify-content-between align-items-center mb-4">
                            <p class="text-500 m-0">{{ 'admin.cms.sectionsDescription' | translate }}</p>
                            <p-button 
                                [label]="'admin.cms.addSection' | translate"
                                icon="pi pi-plus"
                                (click)="openAddSectionDialog()"
                            ></p-button>
                        </div>

                        <div *ngIf="loadingSections" class="flex justify-content-center p-6">
                            <i class="pi pi-spin pi-spinner text-4xl"></i>
                        </div>

                        <app-empty-state
                            *ngIf="!loadingSections && (!sections || sections.length === 0)"
                            icon="pi pi-list"
                            [titleKey]="'admin.cms.noSections'"
                            [descriptionKey]="'admin.cms.noSectionsDescription'"
                            [actionLabelKey]="'admin.cms.addSection'"
                            actionIcon="pi pi-plus"
                            (onAction)="openAddSectionDialog()"
                        ></app-empty-state>

                        <div *ngIf="!loadingSections && sections && sections.length > 0" class="sections-list">
                            <div 
                                *ngFor="let section of sortedSections; let i = index" 
                                class="section-card p-3 mb-3 border-round surface-border"
                            >
                                <div class="flex align-items-center justify-content-between">
                                    <div class="flex align-items-center gap-3 flex-1">
                                        <div class="flex flex-column gap-1">
                                            <p-button 
                                                icon="pi pi-arrow-up"
                                                [text]="true"
                                                [rounded]="true"
                                                [disabled]="i === 0"
                                                [pTooltip]="'admin.cms.moveUp' | translate"
                                                (click)="moveSectionUp(section, i)"
                                                styleClass="p-button-sm"
                                            ></p-button>
                                            <p-button 
                                                icon="pi pi-arrow-down"
                                                [text]="true"
                                                [rounded]="true"
                                                [disabled]="i === sortedSections.length - 1"
                                                [pTooltip]="'admin.cms.moveDown' | translate"
                                                (click)="moveSectionDown(section, i)"
                                                styleClass="p-button-sm"
                                            ></p-button>
                                        </div>
                                        <div class="flex-1">
                                            <div class="flex align-items-center gap-2 mb-2">
                                                <p-tag [value]="section.type" severity="info"></p-tag>
                                                <span class="font-medium">{{ getSectionTitle(section) }}</span>
                                            </div>
                                            <div class="text-500 text-sm">
                                                {{ 'admin.cms.order' | translate }}: {{ section.order }}
                                            </div>
                                        </div>
                                    </div>
                                    <div class="flex gap-2">
                                        <p-button 
                                            icon="pi pi-pencil"
                                            [text]="true"
                                            [rounded]="true"
                                            (click)="editSection(section)"
                                            [pTooltip]="'common.edit' | translate"
                                        ></p-button>
                                        <p-button 
                                            icon="pi pi-copy"
                                            [text]="true"
                                            [rounded]="true"
                                            (click)="duplicateSection(section)"
                                            [pTooltip]="'admin.cms.duplicate' | translate"
                                        ></p-button>
                                        <p-button 
                                            icon="pi pi-trash"
                                            [text]="true"
                                            [rounded]="true"
                                            severity="danger"
                                            (click)="deleteSection(section)"
                                            [pTooltip]="'common.delete' | translate"
                                        ></p-button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>

        <!-- Add Section Dialog -->
        <p-dialog 
            [header]="'admin.cms.addSection' | translate"
            [(visible)]="showAddSectionDialog"
            [modal]="true"
            [style]="{width: '500px'}"
        >
            <div class="flex flex-column gap-3">
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.sectionType' | translate }} *</label>
                    <select 
                        pInputText 
                        [(ngModel)]="newSectionType"
                        class="w-full"
                    >
                        <option [value]="null">{{ 'common.select' | translate }}</option>
                        <option [value]="SectionType.Hero">{{ 'admin.cms.sectionTypeHero' | translate }}</option>
                        <option [value]="SectionType.Banner">{{ 'admin.cms.sectionTypeBanner' | translate }}</option>
                        <option [value]="SectionType.FeaturedCategories">{{ 'admin.cms.sectionTypeFeaturedCategories' | translate }}</option>
                        <option [value]="SectionType.FeaturedItems">{{ 'admin.cms.sectionTypeFeaturedItems' | translate }}</option>
                        <option [value]="SectionType.Testimonials">{{ 'admin.cms.sectionTypeTestimonials' | translate }}</option>
                        <option [value]="SectionType.Newsletter">{{ 'admin.cms.sectionTypeNewsletter' | translate }}</option>
                        <option [value]="SectionType.Content">{{ 'admin.cms.sectionTypeContent' | translate }}</option>
                        <option [value]="SectionType.Custom">{{ 'admin.cms.sectionTypeCustom' | translate }}</option>
                    </select>
                </div>
            </div>
            <ng-template #footer>
                <div class="flex justify-content-end gap-2">
                    <p-button 
                        [label]="'common.cancel' | translate"
                        [outlined]="true"
                        (click)="closeAddSectionDialog()"
                    ></p-button>
                    <p-button 
                        [label]="'common.add' | translate"
                        [disabled]="!newSectionType"
                        (click)="addSection()"
                    ></p-button>
                </div>
            </ng-template>
        </p-dialog>

        <!-- Section Editor Dialog -->
        <p-dialog 
            [header]="'admin.cms.editSection' | translate"
            [(visible)]="showSectionEditor"
            [modal]="true"
            [style]="{width: '800px'}"
        >
            <app-section-editor
                *ngIf="editingSection"
                [section]="editingSection"
                (onSave)="saveSection($event)"
                (onCancel)="closeSectionEditor()"
            ></app-section-editor>
        </p-dialog>
    `
})
export class PageEditorComponent implements OnInit {
    pageForm: FormGroup;
    page: Page | null = null;
    sections: PageSection[] = [];
    loading = false;
    loadingSections = false;
    saving = false;
    isNewPage = true;
    pageId: string | null = null;

    showAddSectionDialog = false;
    newSectionType: SectionType | null = null;
    showSectionEditor = false;
    editingSection: PageSection | null = null;

    SectionType = SectionType;

    private fb = inject(FormBuilder);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private cmsApiService = inject(CmsApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);

    constructor() {
        this.pageForm = this.fb.group({
            slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
            titleEn: ['', Validators.required],
            titleAr: ['', Validators.required],
            metaDescriptionEn: [''],
            metaDescriptionAr: [''],
            isActive: [true]
        });
    }

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.pageId = params['id'];
            this.isNewPage = !this.pageId || this.pageId === 'new';
            
            if (!this.isNewPage) {
                this.loadPage();
            }
        });
    }

    loadPage() {
        if (!this.pageId) return;
        
        this.loading = true;
        this.cmsApiService.getPageById(this.pageId).subscribe({
            next: (page) => {
                this.page = page;
                this.pageForm.patchValue({
                    slug: page.slug,
                    titleEn: page.titleEn,
                    titleAr: page.titleAr,
                    metaDescriptionEn: page.metaDescriptionEn || '',
                    metaDescriptionAr: page.metaDescriptionAr || '',
                    isActive: page.isActive
                });
                this.sections = page.sections || [];
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    savePage() {
        if (this.pageForm.invalid) {
            this.pageForm.markAllAsTouched();
            return;
        }

        this.saving = true;
        const formValue = this.pageForm.value;

        if (this.isNewPage) {
            const request: CreatePageRequest = {
                slug: formValue.slug,
                titleEn: formValue.titleEn,
                titleAr: formValue.titleAr,
                metaDescriptionEn: formValue.metaDescriptionEn,
                metaDescriptionAr: formValue.metaDescriptionAr,
                isActive: formValue.isActive
            };

            this.cmsApiService.createPage(request).subscribe({
                next: (page) => {
                    this.page = page;
                    this.pageId = page.id;
                    this.isNewPage = false;
                    this.saving = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.createSuccess')
                    });
                    this.router.navigate(['/admin/cms/pages', page.id]);
                },
                error: () => {
                    this.saving = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('messages.saveError')
                    });
                }
            });
        } else {
            if (!this.pageId) return;

            const request: UpdatePageRequest = {
                titleEn: formValue.titleEn,
                titleAr: formValue.titleAr,
                metaDescriptionEn: formValue.metaDescriptionEn,
                metaDescriptionAr: formValue.metaDescriptionAr,
                isActive: formValue.isActive
            };

            this.cmsApiService.updatePage(this.pageId, request).subscribe({
                next: (page) => {
                    this.page = page;
                    this.saving = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.updateSuccess')
                    });
                },
                error: () => {
                    this.saving = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('messages.saveError')
                    });
                }
            });
        }
    }

    publishPage() {
        if (!this.pageId) return;

        this.confirmationService.confirm({
            message: this.translateService.instant('admin.cms.confirmPublish'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.cmsApiService.publishPage(this.pageId!).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('admin.cms.pagePublished')
                        });
                        this.loadPage();
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
        });
    }

    previewPage() {
        if (this.page) {
            window.open(`/${this.page.slug}`, '_blank');
        }
    }

    cancel() {
        this.router.navigate(['/admin/cms/pages']);
    }

    canPublish(): boolean {
        return !!(this.sections && this.sections.length > 0 && this.sections.some(s => s.titleEn || s.titleAr));
    }

    get sortedSections(): PageSection[] {
        return [...(this.sections || [])].sort((a, b) => a.order - b.order);
    }

    openAddSectionDialog() {
        this.newSectionType = null;
        this.showAddSectionDialog = true;
    }

    closeAddSectionDialog() {
        this.showAddSectionDialog = false;
        this.newSectionType = null;
    }

    addSection() {
        if (!this.newSectionType || !this.pageId) return;

        const maxOrder = this.sections.length > 0 
            ? Math.max(...this.sections.map(s => s.order)) 
            : 0;

        const request: CreatePageSectionRequest = {
            type: this.newSectionType,
            order: maxOrder + 1,
            titleEn: '',
            titleAr: ''
        };

        this.cmsApiService.addPageSection(this.pageId, request).subscribe({
            next: (page) => {
                this.page = page;
                this.sections = page.sections || [];
                this.closeAddSectionDialog();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.createSuccess')
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

    editSection(section: PageSection) {
        this.editingSection = { ...section };
        this.showSectionEditor = true;
    }

    saveSection(section: PageSection) {
        if (!this.pageId) return;

        this.cmsApiService.updatePageSection(this.pageId, section.id, {
            titleEn: section.titleEn,
            titleAr: section.titleAr,
            contentEn: section.contentEn,
            contentAr: section.contentAr,
            settings: section.settings
        }).subscribe({
            next: (page) => {
                this.page = page;
                this.sections = page.sections || [];
                this.closeSectionEditor();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.updateSuccess')
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

    duplicateSection(section: PageSection) {
        if (!this.pageId) return;

        const maxOrder = this.sections.length > 0 
            ? Math.max(...this.sections.map(s => s.order)) 
            : 0;

        const request: CreatePageSectionRequest = {
            type: section.type,
            order: maxOrder + 1,
            titleEn: section.titleEn,
            titleAr: section.titleAr,
            contentEn: section.contentEn,
            contentAr: section.contentAr,
            settings: section.settings
        };

        this.cmsApiService.addPageSection(this.pageId, request).subscribe({
            next: (page) => {
                this.page = page;
                this.sections = page.sections || [];
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.createSuccess')
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

    deleteSection(section: PageSection) {
        if (!this.pageId) return;

        this.confirmationService.confirm({
            message: this.translateService.instant('admin.cms.confirmDeleteSection'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.cmsApiService.deletePageSection(this.pageId!, section.id).subscribe({
                    next: (page) => {
                        this.page = page;
                        this.sections = page.sections || [];
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('messages.deleted')
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

    closeSectionEditor() {
        this.showSectionEditor = false;
        this.editingSection = null;
    }

    moveSectionUp(section: PageSection, index: number) {
        if (index === 0 || !this.pageId) return;
        
        const prevSection = this.sortedSections[index - 1];
        const tempOrder = section.order;
        section.order = prevSection.order;
        prevSection.order = tempOrder;

        // Update both sections
        this.cmsApiService.updatePageSection(this.pageId, section.id, { order: section.order }).subscribe({
            next: () => {
                this.cmsApiService.updatePageSection(this.pageId!, prevSection.id, { order: prevSection.order }).subscribe({
                    next: () => {
                        this.loadPage();
                    }
                });
            }
        });
    }

    moveSectionDown(section: PageSection, index: number) {
        if (index === this.sortedSections.length - 1 || !this.pageId) return;
        
        const nextSection = this.sortedSections[index + 1];
        const tempOrder = section.order;
        section.order = nextSection.order;
        nextSection.order = tempOrder;

        // Update both sections
        this.cmsApiService.updatePageSection(this.pageId, section.id, { order: section.order }).subscribe({
            next: () => {
                this.cmsApiService.updatePageSection(this.pageId!, nextSection.id, { order: nextSection.order }).subscribe({
                    next: () => {
                        this.loadPage();
                    }
                });
            }
        });
    }

    getSectionTitle(section: PageSection): string {
        return this.translateService.currentLang === 'ar' && section.titleAr 
            ? section.titleAr 
            : (section.titleEn || section.type);
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString(this.translateService.currentLang, {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}

