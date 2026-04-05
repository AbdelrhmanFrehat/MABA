import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { BizInventoryApiService } from '../../../shared/services/biz-inventory-api.service';
import { BizWarehouse } from '../../../shared/models/biz-inventory.model';

@Component({
    selector: 'app-warehouses-list',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.inventory.warehouses' | translate }}</h1><app-data-table [data]="warehouses" [columns]="columns" [showAddButton]="false" [showExport]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class WarehousesListComponent implements OnInit {
    warehouses: BizWarehouse[] = [];
    columns: TableColumn[] = [
        { field: 'code', headerKey: 'common.code' },
        { field: 'nameEn', headerKey: 'common.nameEn' },
        { field: 'nameAr', headerKey: 'common.nameAr' },
        { field: 'isDefault', headerKey: 'admin.inventory.defaultWarehouse', type: 'boolean' },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean' }
    ];
    private api = inject(BizInventoryApiService);
    ngOnInit(): void { this.api.getWarehouses().subscribe(x => this.warehouses = x); }
}
