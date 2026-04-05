import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { BizInventoryApiService } from '../../../shared/services/biz-inventory-api.service';
import { BizWarehouseItem } from '../../../shared/models/biz-inventory.model';

@Component({
    selector: 'app-stock-overview',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.inventory.stockOverview' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class StockOverviewComponent implements OnInit {
    rows: BizWarehouseItem[] = [];
    columns: TableColumn[] = [
        { field: 'warehouseName', headerKey: 'admin.inventory.warehouse' },
        { field: 'itemName', headerKey: 'common.item' },
        { field: 'quantityOnHand', headerKey: 'admin.inventory.onHand', type: 'number' },
        { field: 'quantityReserved', headerKey: 'admin.inventory.reserved', type: 'number' },
        { field: 'quantityAvailable', headerKey: 'admin.inventory.available', type: 'number' },
        { field: 'costPerUnit', headerKey: 'admin.inventory.costPerUnit', type: 'currency', currencyCode: 'ILS' }
    ];
    private api = inject(BizInventoryApiService);
    ngOnInit(): void { this.api.getStockOverview().subscribe(x => this.rows = x); }
}
