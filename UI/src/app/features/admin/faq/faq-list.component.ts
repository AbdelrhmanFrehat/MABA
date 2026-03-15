import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule as PTable } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FaqApiService } from '../../../shared/services/faq-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { FaqItem, FaqCategory } from '../../../shared/models/faq.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';

const CATEGORY_OPTIONS: { value: FaqCategory; label: string }[] = [
    { value: 'Print3d', label: 'faq.categories.Print3d' },
    { value: 'Laser', label: 'faq.categories.Laser' },
    { value: 'Cnc', label: 'faq.categories.Cnc' },
    { value: 'Software', label: 'faq.categories.Software' },
    { value: 'OrdersShipping', label: 'faq.categories.OrdersShipping' },
    { value: 'Payments', label: 'faq.categories.Payments' },
    { value: 'Support', label: 'faq.categories.Support' },
    { value: 'General', label: 'faq.categories.General' }
];

@Component({
    selector: 'app-faq-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        PTable,
        ButtonModule,
        InputTextModule,
        SelectModule,
        TagModule,
        ToastModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="faq-list-page">
            <div class="page-header">
                <h1>{{ 'admin.faq.title' | translate }}</h1>
                <div class="header-actions">
                    <p-button
                        [label]="'admin.faq.addNew' | translate"
                        icon="pi pi-plus"
                        routerLink="/admin/faq/new"
                    ></p-button>
                </div>
            </div>

            <div class="filters">
                <input
                    type="text"
                    pInputText
                    [(ngModel)]="searchTerm"
                    (ngModelChange)="loadFaq()"
                    [placeholder]="'admin.faq.searchPlaceholder' | translate"
                    class="search-input"
                />
                <p-select
                    [options]="categoryOptions"
                    [(ngModel)]="filterCategory"
                    (onChange)="loadFaq()"
                    optionLabel="label"
                    [placeholder]="'faq.categories.all' | translate"
                    [showClear]="true"
                    styleClass="category-filter"
                ></p-select>
                <p-select
                    [options]="statusOptions"
                    [(ngModel)]="filterActive"
                    (onChange)="loadFaq()"
                    optionLabel="label"
                    [placeholder]="'common.status' | translate"
                    [showClear]="true"
                ></p-select>
            </div>

            <p-table
                [value]="items"
                [loading]="loading"
                [paginator]="true"
                [rows]="20"
                [rowsPerPageOptions]="[10, 20, 50]"
                styleClass="p-datatable-sm p-datatable-striped"
                [tableStyle]="{ 'min-width': '50rem' }"
            >
                <ng-template pTemplate="header">
                    <tr>
                        <th style="width:3rem">{{ 'admin.faq.sortOrder' | translate }}</th>
                        <th>{{ 'admin.faq.question' | translate }}</th>
                        <th style="width:8rem">{{ 'admin.faq.category' | translate }}</th>
                        <th style="width:6rem">{{ 'common.status' | translate }}</th>
                        <th style="width:6rem">{{ 'admin.faq.featured' | translate }}</th>
                        <th style="width:10rem">{{ 'common.actions' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-item>
                    <tr>
                        <td>{{ item.sortOrder }}</td>
                        <td>{{ getQuestion(item) }}</td>
                        <td><span class="category-badge">{{ getCategoryLabel(item.category) | translate }}</span></td>
                        <td>
                            <p-tag [value]="item.isActive ? ('common.active' | translate) : ('common.inactive' | translate)" [severity]="item.isActive ? 'success' : 'secondary'" />
                        </td>
                        <td>
                            <p-tag *ngIf="item.isFeatured" value="★" severity="info" />
                        </td>
                        <td>
                            <div class="action-buttons">
                                <p-button [icon]="'pi pi-pencil'" [rounded]="true" [text]="true" severity="secondary" (click)="edit(item)" [pTooltip]="'common.edit' | translate"></p-button>
                                <p-button [icon]="'pi pi-power-off'" [rounded]="true" [text]="true" [severity]="item.isActive ? 'warn' : 'success'" (click)="toggleActive(item)" [pTooltip]="(item.isActive ? 'admin.faq.disable' : 'admin.faq.enable') | translate"></p-button>
                                <p-button [icon]="'pi pi-trash'" [rounded]="true" [text]="true" severity="danger" (click)="deleteItem(item)" [pTooltip]="'common.delete' | translate"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                    <tr><td colspan="6">{{ 'common.noDataFound' | translate }}</td></tr>
                </ng-template>
            </p-table>
        </div>
    `,
    styles: [`
        .faq-list-page { padding: 1rem; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; }
        .filters { display: flex; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
        .search-input { min-width: 200px; }
        .category-badge { font-size: 0.8rem; }
        .action-buttons { display: flex; gap: 0.25rem; }
    `]
})
export class FaqListComponent implements OnInit {
    private faqApi = inject(FaqApiService);
    private languageService = inject(LanguageService);
    private messageService = inject(MessageService);
    private dialogService = inject(DialogService);

    items: FaqItem[] = [];
    loading = false;
    searchTerm = '';
    filterCategory: { value: FaqCategory; label: string } | null = null;
    filterActive: { value: boolean; label: string } | null = null;

    categoryOptions = [{ value: null, label: 'faq.categories.all' }, ...CATEGORY_OPTIONS.map(c => ({ value: c.value, label: c.label }))];
    statusOptions = [
        { value: true, label: 'common.active' },
        { value: false, label: 'common.inactive' }
    ];

    ngOnInit() {
        this.loadFaq();
    }

    loadFaq() {
        this.loading = true;
        const params: { search?: string; category?: FaqCategory; isActive?: boolean } = {};
        if (this.searchTerm?.trim()) params.search = this.searchTerm.trim();
        if (this.filterCategory?.value) params.category = this.filterCategory.value;
        if (this.filterActive?.value !== undefined) params.isActive = this.filterActive.value;

        this.faqApi.getAdminFaq(params).subscribe({
            next: data => {
                this.items = data ?? [];
                this.loading = false;
            },
            error: () => { this.loading = false; }
        });
    }

    getQuestion(item: FaqItem): string {
        return this.languageService.language === 'ar' && item.questionAr ? item.questionAr : item.questionEn;
    }

    getCategoryLabel(cat: string | number): string {
        if (typeof cat === 'number') {
            const names: Record<number, string> = {
                0: 'faq.categories.Print3d', 1: 'faq.categories.Laser', 2: 'faq.categories.Cnc',
                3: 'faq.categories.Software', 4: 'faq.categories.OrdersShipping', 5: 'faq.categories.Payments',
                6: 'faq.categories.Support', 7: 'faq.categories.General'
            };
            return names[cat] ?? 'faq.categories.General';
        }
        return `faq.categories.${cat}`;
    }

    private router = inject(Router);
    private translateService = inject(TranslateService);

    edit(item: FaqItem) {
        this.router.navigate(['/admin/faq', item.id, 'edit']);
    }

    toggleActive(item: FaqItem) {
        this.faqApi.toggleActive(item.id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('common.success'), detail: this.translateService.instant('messages.updateSuccess') });
                this.loadFaq();
            }
        });
    }

    deleteItem(item: FaqItem) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            data: { message: this.translateService.instant('messages.confirmDelete'), title: this.translateService.instant('common.confirm') },
            header: this.translateService.instant('common.confirm')
        });
        if (ref) {
            ref.onClose.subscribe((confirmed: boolean) => {
                if (confirmed) {
                    this.faqApi.delete(item.id).subscribe({
                        next: () => {
                            this.messageService.add({ severity: 'success', summary: this.translateService.instant('common.success'), detail: this.translateService.instant('messages.deleteSuccess') });
                            this.loadFaq();
                        }
                    });
                }
            });
        }
    }
}
