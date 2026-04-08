import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { PriceList, PriceListItem } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-price-list-items',
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.pricing.priceListItems' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Inspect the active item prices inside each price list and review quantity-based tiers before publishing updates.</p>
            </div>

            <div class="surface-card border-1 surface-border border-round p-3 mb-4">
                <label class="block font-medium mb-2">Price List</label>
                <p-select
                    [options]="priceLists"
                    [(ngModel)]="selectedPriceListId"
                    optionLabel="nameEn"
                    optionValue="id"
                    placeholder="Select a price list"
                    [filter]="true"
                    filterBy="nameEn,nameAr,priceListTypeName"
                    styleClass="w-full"
                    (onChange)="loadItems()"
                ></p-select>
            </div>

            <div class="grid mb-4" *ngIf="selectedPriceList">
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Items</span><strong>{{ rows.length }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Average Price</span><strong>{{ averagePrice | currency:(selectedPriceList ? selectedPriceList.currency : 'ILS') }}</strong></div></div>
                <div class="col-12 md:col-4"><div class="summary-card"><span class="summary-label">Tiered Items</span><strong>{{ tieredItems }}</strong></div></div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="selectedPriceList ? selectedPriceList.nameEn : 'Price List Items'"
                [globalFilterFields]="['itemName', 'itemSku', 'unitOfMeasureName']"
            ></app-data-table>
        </div>
    `,
    styles: [`
        .summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; }
        .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }
    `]
})
export class PriceListItemsComponent implements OnInit {
    priceLists: PriceList[] = [];
    rows: PriceListItem[] = [];
    selectedPriceListId: string | null = null;
    loading = false;
    averagePrice = 0;
    tieredItems = 0;

    columns: TableColumn[] = [
        { field: 'itemSku', header: 'SKU', sortable: true },
        { field: 'itemName', header: 'Item', sortable: true },
        { field: 'unitOfMeasureName', header: 'Unit', sortable: true },
        { field: 'minQuantity', header: 'Min Qty', sortable: true },
        { field: 'maxQuantity', header: 'Max Qty', sortable: true },
        { field: 'price', header: 'Price', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    private api = inject(PricingApiService);
    private route = inject(ActivatedRoute);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    get selectedPriceList(): PriceList | undefined {
        return this.priceLists.find(x => x.id === this.selectedPriceListId);
    }

    ngOnInit(): void {
        this.loadPriceLists();
    }

    loadPriceLists(): void {
        this.loading = true;
        this.api.getPriceLists().subscribe({
            next: rows => {
                this.priceLists = rows ?? [];
                this.selectedPriceListId = this.route.snapshot.queryParamMap.get('priceListId') || this.priceLists[0]?.id || null;
                this.loadItems();
            },
            error: () => {
                this.priceLists = [];
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: 'Failed to load price lists.' });
            }
        });
    }

    loadItems(): void {
        if (!this.selectedPriceListId) {
            this.rows = [];
            this.averagePrice = 0;
            this.tieredItems = 0;
            this.loading = false;
            return;
        }

        this.loading = true;
        this.api.getPriceListItems(this.selectedPriceListId).subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.averagePrice = this.rows.length ? this.rows.reduce((sum, row) => sum + (row.price || 0), 0) / this.rows.length : 0;
                this.tieredItems = this.rows.filter(row => !!row.minQuantity || !!row.maxQuantity).length;
                this.loading = false;
            },
            error: () => {
                this.rows = [];
                this.averagePrice = 0;
                this.tieredItems = 0;
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: 'Failed to load price list items.' });
            }
        });
    }
}
