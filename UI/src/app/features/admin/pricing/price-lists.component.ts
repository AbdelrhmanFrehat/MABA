import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { PriceList } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-price-lists',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.pricing.priceLists' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class PriceListsComponent implements OnInit {
    rows: PriceList[] = [];
    columns: TableColumn[] = [
        { field: 'code', headerKey: 'common.code' },
        { field: 'nameEn', headerKey: 'common.nameEn' },
        { field: 'currency', headerKey: 'common.currency' },
        { field: 'priority', headerKey: 'admin.pricing.priority', type: 'number' },
        { field: 'isDefault', headerKey: 'admin.pricing.isDefault', type: 'boolean' }
    ];
    private api = inject(PricingApiService);
    ngOnInit(): void { this.api.getPriceLists().subscribe(x => this.rows = x); }
}
