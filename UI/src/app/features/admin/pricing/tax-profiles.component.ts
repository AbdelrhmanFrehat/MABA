import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { PricingApiService } from '../../../shared/services/pricing-api.service';
import { TaxProfile } from '../../../shared/models/pricing.model';

@Component({
    selector: 'app-tax-profiles',
    standalone: true,
    imports: [CommonModule, TranslateModule, DataTableComponent],
    template: `<div class="p-4"><h1 class="text-2xl font-bold mb-4">{{ 'admin.tax.title' | translate }}</h1><app-data-table [data]="rows" [columns]="columns" [showAddButton]="false" [showDeleteSelected]="false"></app-data-table></div>`
})
export class TaxProfilesComponent implements OnInit {
    rows: TaxProfile[] = [];
    columns: TableColumn[] = [
        { field: 'code', headerKey: 'common.code' },
        { field: 'nameEn', headerKey: 'common.nameEn' },
        { field: 'rate', headerKey: 'admin.tax.rate', type: 'number' },
        { field: 'isDefault', headerKey: 'admin.pricing.isDefault', type: 'boolean' },
        { field: 'isActive', headerKey: 'common.active', type: 'boolean' }
    ];
    private api = inject(PricingApiService);
    ngOnInit(): void { this.api.getTaxProfiles().subscribe(x => this.rows = x); }
}
