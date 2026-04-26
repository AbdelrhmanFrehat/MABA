import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InventoryService } from '../../../../shared/services/inventory.service';
import { CatalogApiService } from '../../../../shared/services/catalog-api.service';
import { LanguageService } from '../../../../shared/services/language.service';
import { Inventory, Item } from '../../../../shared/models/item.model';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table';

interface InventoryItem {
    id: string;
    itemId: string;
    quantityOnHand: number;
    reorderLevel: number;
    lastStockInAt?: string;
    item?: Item;
    itemName?: string;
    itemSku?: string;
    categoryName?: string;
    brandName?: string;
    stockStatus?: string;
}

@Component({
    selector: 'app-inventory-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TableModule,
        SelectModule,
        InputTextModule,
        InputNumberModule,
        TagModule,
        DialogModule,
        ToastModule,
        TranslateModule,
        DataTableComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="inventory-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.inventory.title' | translate }}</h1>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.inventory.filterByCategory' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedCategoryId"
                            [options]="categoryOptions"
                            [placeholder]="'admin.inventory.allCategories' | translate"
                            (onChange)="loadInventory()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.inventory.filterByStockStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStockStatus"
                            [options]="stockStatusOptions"
                            [placeholder]="'admin.inventory.allStatuses' | translate"
                            (onChange)="loadInventory()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'common.search' | translate }}</label>
                        <input 
                            type="text" 
                            pInputText
                            [(ngModel)]="searchTerm"
                            [placeholder]="'admin.inventory.searchPlaceholder' | translate"
                            (input)="onSearchChange()"
                            class="w-full"
                        />
                    </div>
                </div>
            </p-card>

            <!-- Inventory Table -->
            <div class="table-section">
                <p-card>
                    <app-data-table
                        [data]="inventoryItems"
                        [columns]="columns"
                        [loading]="loading"
                        [paginator]="true"
                        [rows]="pageSize"
                        [lazy]="true"
                        [totalRecords]="totalRecords"
                        (onLazyLoad)="onLazyLoad($event)"
                        (onAction)="handleAction($event)">
                    </app-data-table>
                </p-card>
            </div>

            <!-- Adjust Inventory Dialog -->
            <p-dialog
                [(visible)]="showAdjustDialog"
                [modal]="true"
                [style]="{width: '90vw', maxWidth: '500px'}"
                [breakpoints]="{'960px': '90vw', '640px': '95vw'}"
                (onHide)="closeAdjustDialog()">
                <ng-template pTemplate="header">
                    <div class="dlg-header">
                        <span class="dlg-label">Inventory</span>
                        <span class="dlg-title">{{ 'admin.inventory.adjustInventory' | translate }}</span>
                    </div>
                </ng-template>
                <div class="dialog-content" *ngIf="selectedInventory">
                    <div class="dialog-field">
                        <strong>{{ 'admin.inventory.item' | translate }}:</strong> {{ getItemName(selectedInventory) }}
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.inventory.currentStock' | translate }}:</strong> {{ selectedInventory.quantityOnHand }}
                    </div>
                    <div class="dialog-form-field">
                        <label>{{ 'admin.inventory.newQuantity' | translate }} *</label>
                        <p-inputNumber
                            [(ngModel)]="adjustQuantity"
                            [min]="0"
                            class="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="dialog-form-field">
                        <label>{{ 'admin.inventory.reorderLevel' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="adjustReorderLevel"
                            [min]="0"
                            class="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="dialog-actions">
                        <p-button 
                            [label]="'common.cancel' | translate" 
                            [outlined]="true"
                            (click)="closeAdjustDialog()"
                            styleClass="w-full md:w-auto">
                        </p-button>
                        <p-button 
                            [label]="'common.save' | translate" 
                            (click)="saveAdjustment()"
                            [loading]="saving"
                            styleClass="w-full md:w-auto">
                        </p-button>
                    </div>
                </div>
            </p-dialog>
        </div>
    `,
    styles: [`
        .inventory-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .inventory-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .inventory-list-container {
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

        .filters-card {
            margin-bottom: 1.5rem;
        }

        .filters-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .filters-grid {
                grid-template-columns: repeat(3, 1fr);
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

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .inventory-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }

        /* Dialog styles */
        .dialog-content {
            padding: 0.5rem 0;
        }

        @media (min-width: 768px) {
            .dialog-content {
                padding: 1rem 0;
            }
        }

        .dialog-field {
            margin-bottom: 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 1px solid var(--surface-border);
        }

        .dialog-form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }

        .dialog-form-field label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .dialog-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            justify-content: flex-end;
            margin-top: 1.5rem;
        }

        @media (max-width: 767px) {
            .dialog-actions {
                flex-direction: column;
            }

            .dialog-actions p-button {
                width: 100%;
            }
        }
    `]
})
export class InventoryListComponent implements OnInit {
    inventoryItems: InventoryItem[] = [];
    loading = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedCategoryId: string = '';
    selectedStockStatus: string = '';
    searchTerm = '';
    showAdjustDialog = false;
    selectedInventory: InventoryItem | null = null;
    adjustQuantity = 0;
    adjustReorderLevel = 0;
    saving = false;

    categoryOptions: any[] = [];
    stockStatusOptions: any[] = [
        { label: 'All Statuses', value: '' },
        { label: 'In Stock', value: 'inStock' },
        { label: 'Low Stock', value: 'lowStock' },
        { label: 'Out of Stock', value: 'outOfStock' }
    ];

    columns: any[] = [];

    private inventoryService: InventoryService = inject(InventoryService);
    private catalogApiService: CatalogApiService = inject(CatalogApiService);
    private messageService: MessageService = inject(MessageService);
    private translateService: TranslateService = inject(TranslateService);
    private languageService: LanguageService = inject(LanguageService);

    ngOnInit() {
        this.setupColumns();
        this.loadCategories();
        this.loadInventory();
    }

    setupColumns() {
        const lang = this.languageService.language;
        this.columns = [
            { field: 'itemSku', header: lang === 'ar' ? 'SKU' : 'SKU', sortable: true },
            { field: 'itemName', header: lang === 'ar' ? 'المنتج' : 'Item', sortable: true },
            { field: 'categoryName', header: lang === 'ar' ? 'الفئة' : 'Category', sortable: true },
            { field: 'quantityOnHand', header: lang === 'ar' ? 'الكمية المتاحة' : 'Quantity On Hand', sortable: true },
            { field: 'reorderLevel', header: lang === 'ar' ? 'مستوى إعادة الطلب' : 'Reorder Level', sortable: true },
            { field: 'stockStatus', header: lang === 'ar' ? 'حالة المخزون' : 'Stock Status', sortable: true },
            { field: 'actions', header: lang === 'ar' ? 'الإجراءات' : 'Actions', sortable: false }
        ];
    }

    loadCategories() {
        this.catalogApiService.getAllCategories(true, false).subscribe({
            next: (categories: any[]) => {
                const lang = this.languageService.language;
                this.categoryOptions = [
                    { label: this.translateService.instant('admin.inventory.allCategories'), value: '' },
                    ...categories.map((cat: any) => ({
                        label: lang === 'ar' ? cat.nameAr : cat.nameEn,
                        value: cat.id
                    }))
                ];
            }
        });
    }

    loadInventory() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedCategoryId) {
            params.categoryId = this.selectedCategoryId;
        }
        if (this.selectedStockStatus) {
            params.stockStatus = this.selectedStockStatus;
        }
        if (this.searchTerm) {
            params.search = this.searchTerm;
        }

        this.inventoryService.getInventoryList(params).subscribe({
            next: (response: any) => {
                this.inventoryItems = (response.items || []).map((inv: any) => ({
                    ...inv,
                    itemName: this.getItemNameFromInventory(inv),
                    itemSku: inv.item?.sku || '',
                    categoryName: this.getCategoryName(inv),
                    brandName: this.getBrandName(inv),
                    stockStatus: this.getStockStatus(inv)
                }));
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onLazyLoad(event: any) {
        this.currentPage = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.loadInventory();
    }

    onSearchChange() {
        this.currentPage = 1;
        this.loadInventory();
    }

    handleAction(event: { action: string; data: any }) {
        const inventory = event.data as InventoryItem;
        switch (event.action) {
            case 'adjust':
                this.openAdjustDialog(inventory);
                break;
            case 'history':
                // Navigate to inventory history
                window.location.href = `/admin/inventory/${inventory.itemId}/history`;
                break;
        }
    }

    openAdjustDialog(inventory: InventoryItem) {
        this.selectedInventory = inventory;
        this.adjustQuantity = inventory.quantityOnHand;
        this.adjustReorderLevel = inventory.reorderLevel;
        this.showAdjustDialog = true;
    }

    closeAdjustDialog() {
        this.showAdjustDialog = false;
        this.selectedInventory = null;
        this.adjustQuantity = 0;
        this.adjustReorderLevel = 0;
    }

    saveAdjustment() {
        if (!this.selectedInventory) return;

        this.saving = true;
        if (!this.selectedInventory) return;
        
        this.inventoryService.adjustInventory(this.selectedInventory.itemId, {
            quantityOnHand: this.adjustQuantity,
            reorderLevel: this.adjustReorderLevel
        }).subscribe({
            next: () => {
                this.loadInventory();
                this.closeAdjustDialog();
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.updated')
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

    getItemName(inventory: InventoryItem): string {
        return this.getItemNameFromInventory(inventory);
    }

    getItemNameFromInventory(inventory: any): string {
        if (inventory.item) {
            const lang = this.languageService.language;
            return lang === 'ar' ? inventory.item.nameAr : inventory.item.nameEn;
        }
        return inventory.itemNameEn || inventory.itemNameAr || '';
    }

    getCategoryName(inventory: any): string {
        if (inventory.item?.categoryNameEn) {
            const lang = this.languageService.language;
            return lang === 'ar' 
                ? (inventory.item.categoryNameAr || '') 
                : inventory.item.categoryNameEn;
        }
        return '';
    }

    getBrandName(inventory: any): string {
        if (inventory.item?.brandNameEn) {
            return inventory.item.brandNameEn;
        }
        return '';
    }

    getStockStatus(inventory: InventoryItem): string {
        if (inventory.quantityOnHand <= 0) {
            return 'Out of Stock';
        } else if (inventory.quantityOnHand <= inventory.reorderLevel) {
            return 'Low Stock';
        }
        return 'In Stock';
    }
}
