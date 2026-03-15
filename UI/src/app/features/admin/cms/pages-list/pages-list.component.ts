import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CmsApiService } from '../../../../shared/services/cms-api.service';
import { Page } from '../../../../shared/models/cms.model';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';

@Component({
    selector: 'app-pages-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TableModule,
        InputTextModule,
        InputIconModule,
        IconFieldModule,
        TagModule,
        TooltipModule,
        ToastModule,
        ConfirmDialogModule,
        TranslateModule,
        EmptyStateComponent
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="pages-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.cms.pages' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'admin.cms.createPage' | translate" 
                            icon="pi pi-plus"
                            (click)="createPage()">
                        </p-button>
                    </div>
                </div>
            </div>

            <p-card>
                <div class="search-section">
                    <p-iconfield class="search-field">
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
                    <div class="filter-actions">
                        <p-button 
                            icon="pi pi-filter"
                            [outlined]="true"
                            [label]="'common.filter' | translate"
                            (click)="toggleFilters()"
                        ></p-button>
                    </div>
                </div>

                <div *ngIf="showFilters" class="filters-section">
                    <div class="filters-grid">
                        <div class="filter-item">
                            <label class="filter-label">{{ 'common.status' | translate }}</label>
                            <select 
                                pInputText 
                                [(ngModel)]="filterStatus"
                                (change)="applyFilters()"
                                class="w-full"
                            >
                                <option [value]="null">{{ 'common.all' | translate }}</option>
                                <option [value]="true">{{ 'common.active' | translate }}</option>
                                <option [value]="false">{{ 'common.inactive' | translate }}</option>
                            </select>
                        </div>
                        <div class="filter-item">
                            <label class="filter-label">{{ 'admin.cms.published' | translate }}</label>
                            <select 
                                pInputText 
                                [(ngModel)]="filterPublished"
                                (change)="applyFilters()"
                                class="w-full"
                            >
                                <option [value]="null">{{ 'common.all' | translate }}</option>
                                <option [value]="true">{{ 'common.yes' | translate }}</option>
                                <option [value]="false">{{ 'common.no' | translate }}</option>
                            </select>
                        </div>
                    </div>
                </div>

                <div *ngIf="loading" class="flex justify-content-center p-6">
                    <i class="pi pi-spin pi-spinner text-4xl"></i>
                </div>

                <app-empty-state
                    *ngIf="!loading && pages.length === 0"
                    icon="pi pi-file"
                    [titleKey]="'admin.cms.noPages'"
                    [descriptionKey]="'admin.cms.noPagesDescription'"
                    [actionLabelKey]="'admin.cms.createPage'"
                    actionIcon="pi pi-plus"
                    (onAction)="createPage()"
                ></app-empty-state>

                <p-table 
                    *ngIf="!loading && pages.length > 0" 
                    [value]="pages" 
                    [tableStyle]="{'min-width': '50rem'}"
                    [paginator]="true"
                    [rows]="10"
                    [rowsPerPageOptions]="[10, 25, 50]"
                    [globalFilterFields]="['titleEn', 'titleAr', 'slug']"
                >
                    <ng-template #header>
                        <tr>
                            <th pSortableColumn="titleEn">{{ 'admin.cms.title' | translate }}</th>
                            <th pSortableColumn="slug">{{ 'admin.cms.slug' | translate }}</th>
                            <th>{{ 'common.status' | translate }}</th>
                            <th>{{ 'admin.cms.published' | translate }}</th>
                            <th pSortableColumn="updatedAt">{{ 'admin.cms.updatedAt' | translate }}</th>
                            <th style="width: 12rem; text-align: center">{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template #body let-page>
                        <tr>
                            <td>
                                <div class="font-medium">{{ getPageTitle(page) }}</div>
                                <div class="text-500 text-sm">{{ page.slug }}</div>
                            </td>
                            <td>
                                <code class="text-sm">{{ page.slug }}</code>
                            </td>
                            <td>
                                <p-tag 
                                    [value]="page.isActive ? ('common.active' | translate) : ('common.inactive' | translate)" 
                                    [severity]="page.isActive ? 'success' : 'danger'">
                                </p-tag>
                            </td>
                            <td>
                                <p-tag 
                                    [value]="isPublished(page) ? ('common.yes' | translate) : ('common.no' | translate)" 
                                    [severity]="isPublished(page) ? 'success' : 'secondary'">
                                </p-tag>
                            </td>
                            <td>{{ formatDate(page.updatedAt || page.createdAt) }}</td>
                            <td>
                                <div class="flex gap-2 justify-content-center">
                                    <p-button 
                                        icon="pi pi-pencil"
                                        [text]="true"
                                        [rounded]="true"
                                        (click)="editPage(page)"
                                        [pTooltip]="'common.edit' | translate">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-eye"
                                        [text]="true"
                                        [rounded]="true"
                                        severity="info"
                                        (click)="previewPage(page)"
                                        [pTooltip]="'common.view' | translate">
                                    </p-button>
                                    <p-button 
                                        *ngIf="!isPublished(page)"
                                        icon="pi pi-check"
                                        [text]="true"
                                        [rounded]="true"
                                        severity="success"
                                        (click)="publishPage(page)"
                                        [pTooltip]="'admin.cms.publish' | translate">
                                    </p-button>
                                    <p-button 
                                        *ngIf="isPublished(page)"
                                        icon="pi pi-times"
                                        [text]="true"
                                        [rounded]="true"
                                        severity="warn"
                                        (click)="unpublishPage(page)"
                                        [pTooltip]="'admin.cms.unpublish' | translate">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-trash"
                                        [text]="true"
                                        [rounded]="true"
                                        severity="danger"
                                        (click)="deletePage(page)"
                                        [pTooltip]="'common.delete' | translate">
                                    </p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>
    `,
    styles: [`
        .pages-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .pages-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .pages-list-container {
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

        .search-section {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        @media (min-width: 768px) {
            .search-section {
                flex-direction: row;
                align-items: center;
            }
        }

        .search-field {
            flex: 1;
            width: 100%;
        }

        @media (min-width: 768px) {
            .search-field {
                max-width: 30rem;
            }
        }

        .filter-actions {
            display: flex;
            gap: 0.5rem;
        }

        .filters-section {
            margin-bottom: 1rem;
            padding: 1rem;
            background: var(--surface-ground);
            border-radius: 8px;
        }

        .filters-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .filters-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .filter-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .filter-label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .pages-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class PagesListComponent implements OnInit {
    pages: Page[] = [];
    loading = false;
    searchTerm = '';
    showFilters = false;
    filterStatus: boolean | null = null;
    filterPublished: boolean | null = null;

    constructor(
        private cmsApiService: CmsApiService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private translateService: TranslateService,
        private router: Router
    ) {}

    ngOnInit() {
        this.loadPages();
    }

    loadPages() {
        this.loading = true;
        const params: any = {};
        if (this.filterStatus !== null) {
            params.isActive = this.filterStatus;
        }
        // Note: isPublished may not exist in backend yet
        // if (this.filterPublished !== null) {
        //     params.isPublished = this.filterPublished;
        // }

        this.cmsApiService.getPages(params).subscribe({
            next: (response) => {
                this.pages = response.items || [];
                if (this.searchTerm) {
                    const term = this.searchTerm.toLowerCase();
                    this.pages = this.pages.filter(p => 
                        p.titleEn.toLowerCase().includes(term) ||
                        p.titleAr.toLowerCase().includes(term) ||
                        p.slug.toLowerCase().includes(term)
                    );
                }
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

    onSearch() {
        this.loadPages();
    }

    toggleFilters() {
        this.showFilters = !this.showFilters;
    }

    applyFilters() {
        this.loadPages();
    }

    createPage() {
        this.router.navigate(['/admin/cms/pages/new']);
    }

    editPage(page: Page) {
        this.router.navigate(['/admin/cms/pages', page.id]);
    }

    previewPage(page: Page) {
        window.open(`/${page.slug}`, '_blank');
    }

    publishPage(page: Page) {
        this.confirmationService.confirm({
            message: this.translateService.instant('admin.cms.confirmPublish'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.cmsApiService.publishPage(page.id).subscribe({
                    next: () => {
                        this.loadPages();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('admin.cms.pagePublished')
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
        });
    }

    unpublishPage(page: Page) {
        this.confirmationService.confirm({
            message: this.translateService.instant('admin.cms.confirmUnpublish'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.cmsApiService.unpublishPage(page.id).subscribe({
                    next: () => {
                        this.loadPages();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('admin.cms.pageUnpublished')
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
        });
    }

    deletePage(page: Page) {
        this.confirmationService.confirm({
            message: this.translateService.instant('admin.cms.confirmDelete'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.cmsApiService.deletePage(page.id).subscribe({
                    next: () => {
                        this.loadPages();
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

    getPageTitle(page: Page): string {
        return this.translateService.currentLang === 'ar' ? page.titleAr : page.titleEn;
    }

    isPublished(page: Page): boolean {
        return (page as any).isPublished === true;
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString(this.translateService.currentLang, {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }
}

