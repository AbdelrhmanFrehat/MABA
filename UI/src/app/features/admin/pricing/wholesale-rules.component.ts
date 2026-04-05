import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { WholesaleRule } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-wholesale-rules',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.pricing.wholesaleRules' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class WholesaleRulesComponent implements OnInit {
    rows: WholesaleRule[] = [];
    columns: TableColumn[] = [
        { field: 'customerTypeName', headerKey: 'admin.crm.type' },
        { field: 'itemName', headerKey: 'common.item' },
        { field: 'minQuantity', headerKey: 'common.quantity', type: 'number' },
        { field: 'priceListName', headerKey: 'admin.pricing.priceLists' },
        { field: 'priority', headerKey: 'admin.pricing.priority', type: 'number' }
    ];
    private api = inject(PricingApiService);
    ngOnInit(): void { this.api.getWholesaleRules().subscribe(x => this.rows = x); }
}
