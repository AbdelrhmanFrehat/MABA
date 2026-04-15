import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { QRCodeComponent } from 'angularx-qrcode';
import { AssetsService } from '../../../shared/services/assets.service';
import { Asset } from '../../../shared/models/asset.model';

@Component({
    selector: 'app-asset-label-print',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, QRCodeComponent],
    styles: [`
        .label-wrap { display: flex; justify-content: center; padding: 2rem; }
        .label {
            width: 60mm; height: 40mm;
            border: 1px dashed #999; padding: 3mm;
            display: flex; gap: 3mm; align-items: center;
            background: white; color: #000;
        }
        .label .meta { flex: 1; display: flex; flex-direction: column; overflow: hidden; }
        .label .num { font-size: 12pt; font-weight: bold; }
        .label .name { font-size: 9pt; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .label .name-ar { font-size: 9pt; direction: rtl; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .actions { display: flex; justify-content: center; gap: .5rem; margin-top: 1rem; }

        @media print {
            .no-print { display: none !important; }
            .label-wrap { padding: 0; }
            .label { border: none; page-break-inside: avoid; }
            @page { size: 60mm 40mm; margin: 0; }
            body { background: white; }
        }
    `],
    template: `
        <div class="p-4 no-print">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <h1 class="text-2xl font-bold m-0">Asset Label</h1>
                <div class="flex gap-2">
                    <p-button label="Print" icon="pi pi-print" (onClick)="print()"></p-button>
                    <p-button label="Back" severity="secondary" [outlined]="true" routerLink="/admin/assets"></p-button>
                </div>
            </div>
        </div>

        <div class="label-wrap" *ngIf="asset">
            <div class="label">
                <qrcode [qrdata]="qrPayload" [width]="110" [margin]="0" [errorCorrectionLevel]="'M'"></qrcode>
                <div class="meta">
                    <div class="num">{{ asset.assetNumber }}</div>
                    <div class="name">{{ asset.nameEn }}</div>
                    <div class="name-ar">{{ asset.nameAr }}</div>
                </div>
            </div>
        </div>
    `
})
export class AssetLabelPrintComponent implements OnInit {
    asset: Asset | null = null;
    qrPayload = '';

    private svc = inject(AssetsService);
    private route = inject(ActivatedRoute);

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) this.svc.getAsset(id).subscribe(a => {
            this.asset = a;
            this.qrPayload = `${window.location.origin}/admin/assets/${a.id}`;
        });
    }

    print() { window.print(); }
}
