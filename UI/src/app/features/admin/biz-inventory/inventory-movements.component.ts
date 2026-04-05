import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { BizInventoryApiService } from '../../../shared/services/biz-inventory-api.service';
import { BizInventoryMovement } from '../../../shared/models/biz-inventory.model';

@Component({
    selector: 'app-inventory-movements',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.inventory.movements' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class InventoryMovementsComponent implements OnInit {
    rows: BizInventoryMovement[] = [];
    columns: TableColumn[] = [
        { field: 'createdAt', headerKey: 'common.date', type: 'date' },
        { field: 'warehouseName', headerKey: 'admin.inventory.warehouse' },
        { field: 'itemName', headerKey: 'common.item' },
        { field: 'movementTypeName', headerKey: 'admin.inventory.movementType' },
        { field: 'quantity', headerKey: 'common.quantity', type: 'number' },
        { field: 'sourceDocumentNumber', headerKey: 'admin.common.document' }
    ];
    private api = inject(BizInventoryApiService);
    ngOnInit(): void { this.api.getInventoryMovements().subscribe(x => this.rows = x); }
}
