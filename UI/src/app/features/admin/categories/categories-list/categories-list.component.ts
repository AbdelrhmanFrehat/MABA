import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { Category } from '../../../../shared/models/catalog.model';
import { CategoryFormComponent } from '../category-form/category-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-categories-list',
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
        <div class="categories-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.categories.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'common.refresh' | translate" 
                            icon="pi pi-refresh" 
                            [outlined]="true"
                            (click)="loadCategories()"
                            [loading]="loading"
                        ></p-button>
                        <p-button 
                            [label]="'admin.categories.createCategory' | translate" 
                            icon="pi pi-plus" 
                            (click)="openCreateDialog()"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="flattenedCategories"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.categories.list' | translate"
                    [showAddButton]="false"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .categories-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .categories-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .categories-list-container {
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
            flex-direction: column;
            gap: 0.5rem;
            width: 100%;
        }

        @media (min-width: 768px) {
            .header-actions {
                flex-direction: row;
                width: auto;
            }
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .categories-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class CategoriesListComponent implements OnInit {
    categories: Category[] = [];
    flattenedCategories: any[] = [];
    loading = false;
    dialogRef: DynamicDialogRef | undefined;

    private catalogApiService = inject(CatalogApiService);
    private dialogService = inject(DialogService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'nameEn', headerKey: 'admin.categories.name', sortable: true },
        { field: 'slug', headerKey: 'admin.categories.slug', sortable: true },
        { field: 'sortOrder', headerKey: 'admin.categories.sortOrder', type: 'number', sortable: true },
        { 
            field: 'isActive', 
            headerKey: 'admin.categories.isActive', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-plus',
            tooltipKey: 'admin.categories.addChild',
            action: 'addChild',
            severity: 'success'
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
        this.loadCategories();
    }

    loadCategories() {
        this.loading = true;
        this.catalogApiService.getAllCategories(undefined, true).subscribe({
            next: (categories) => {
                this.categories = categories;
                this.flattenedCategories = this.flattenTree(categories);
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingCategories'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    flattenTree(categories: Category[]): any[] {
        const result: any[] = [];
        const map = new Map<string, Category>();
        categories.forEach(cat => map.set(cat.id, cat));

        const buildTree = (parentId: string | null, level: number = 0): void => {
            categories
                .filter(cat => (cat.parentId || null) === parentId)
                .sort((a, b) => a.sortOrder - b.sortOrder)
                .forEach(cat => {
                    result.push({
                        ...cat,
                        displayName: `${'  '.repeat(level)}${cat.nameEn} / ${cat.nameAr}`,
                        level
                    });
                    buildTree(cat.id, level + 1);
                });
        };

        buildTree(null);
        return result;
    }

    handleAction(event: { action: string; data: Category }) {
        if (event.action === 'addChild') {
            this.openCreateDialog(event.data);
        } else if (event.action === 'edit') {
            this.openEditDialog(event.data);
        } else if (event.action === 'delete') {
            this.confirmDelete(event.data);
        }
    }

    openCreateDialog(parent?: Category) {
        const ref = this.dialogService.open(CategoryFormComponent, {
            header: this.translateService.instant('admin.categories.createCategory'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { parentId: parent?.id }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Category | boolean) => {
                if (result) {
                    this.loadCategories();
                }
            });
        }
    }

    openEditDialog(category: Category) {
        const ref = this.dialogService.open(CategoryFormComponent, {
            header: this.translateService.instant('admin.categories.editCategory'),
            width: '600px',
            closable: true,
            dismissableMask: true,
            data: { category }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((result: Category | boolean) => {
                if (result) {
                    this.loadCategories();
                }
            });
        }
    }

    confirmDelete(category: Category) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translateService.instant('common.confirm'),
            width: '450px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translateService.instant('common.confirm'),
                message: this.translateService.instant('messages.confirmDelete', { name: category.nameEn })
            }
        });

        if (ref) {
            this.dialogRef = ref || undefined;
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.deleteCategory(category.id);
                }
            });
        }
    }

    deleteCategory(id: string) {
        this.loading = true;
        this.catalogApiService.deleteCategory(id).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.categoryDeletedSuccessfully'),
                    life: 3000
                });
                this.loadCategories();
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('messages.errorDeletingCategory'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}
