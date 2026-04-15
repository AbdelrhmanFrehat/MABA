import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TranslateModule } from '@ngx-translate/core';
import { AssetsService } from '../../../shared/services/assets.service';
import { Asset } from '../../../shared/models/asset.model';

@Component({
    selector: 'app-asset-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, CardModule, TagModule, TranslateModule, DatePipe, CurrencyPipe],
    template: `
        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">Asset {{ asset?.assetNumber }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ asset?.nameEn }} — {{ asset?.nameAr }}</p>
                </div>
                <div class="flex gap-2">
                    <p-button label="Print Label" icon="pi pi-print" severity="info" *ngIf="asset" [routerLink]="['/admin/assets/print', asset.id]"></p-button>
                    <p-button label="Edit" icon="pi pi-pencil" *ngIf="asset" [routerLink]="['/admin/assets', asset.id, 'edit']"></p-button>
                    <p-button label="Back" severity="secondary" [outlined]="true" routerLink="/admin/assets"></p-button>
                </div>
            </div>

            <p-card *ngIf="asset">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div><strong>Asset Number:</strong> {{ asset.assetNumber }}</div>
                    <div><strong>Category:</strong> {{ asset.assetCategoryNameEn }}</div>
                    <div><strong>Investor:</strong> {{ asset.investorUserFullName }}</div>
                    <div><strong>Condition:</strong> {{ asset.acquisitionConditionNameEn }}</div>
                    <div><strong>Status:</strong> {{ asset.statusNameEn }}</div>
                    <div><strong>Price:</strong> {{ asset.approximatePrice | currency:asset.currency }}</div>
                    <div><strong>Acquired At:</strong> {{ asset.acquiredAt | date:'yyyy-MM-dd' }}</div>
                    <div><strong>Location:</strong> {{ asset.locationNotes || '—' }}</div>
                    <div class="md:col-span-2"><strong>Condition Notes:</strong> {{ asset.conditionNotes || '—' }}</div>
                    <div class="md:col-span-2"><strong>Description (EN):</strong> {{ asset.descriptionEn || '—' }}</div>
                    <div class="md:col-span-2"><strong>Description (AR):</strong> {{ asset.descriptionAr || '—' }}</div>
                </div>
            </p-card>
        </div>
    `
})
export class AssetDetailComponent implements OnInit {
    asset: Asset | null = null;
    private svc = inject(AssetsService);
    private route = inject(ActivatedRoute);

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) this.svc.getAsset(id).subscribe(a => this.asset = a);
    }
}
