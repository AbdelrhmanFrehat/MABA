import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { UnitOfMeasure } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-units-of-measure',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.catalog.unitsOfMeasure' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class UnitsOfMeasureComponent implements OnInit {
    rows: UnitOfMeasure[] = [];
    columns: TableColumn[] = [
        { field: 'code', headerKey: 'common.code' },
        { field: 'nameEn', headerKey: 'common.nameEn' },
        { field: 'nameAr', headerKey: 'common.nameAr' },
        { field: 'isBase', headerKey: 'admin.catalog.baseUnit', type: 'boolean' },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean' }
    ];
    private api = inject(PricingApiService);
    ngOnInit(): void { this.api.getUnitsOfMeasure().subscribe(x => this.rows = x); }
}
