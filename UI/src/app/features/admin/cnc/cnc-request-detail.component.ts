import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CncApiService } from '../../../shared/services/cnc-api.service';
import { CncServiceRequestDto, CncServiceRequestStatus } from '../../../shared/models/cnc.model';

@Component({
    selector: 'app-cnc-request-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TagModule,
        SelectModule,
        TextareaModule,
        InputNumberModule,
        ToastModule,
        ProgressSpinnerModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="detail-page">
            <div class="page-toolbar">
                <p-button
                    [label]="'admin.cncRequests.backToList' | translate"
                    icon="pi pi-arrow-left"
                    [outlined]="true"
                    routerLink="/admin/cnc-requests"
                ></p-button>
            </div>

            <div *ngIf="loading && !request" class="loading-wrap">
                <p-progressSpinner />
            </div>

            <ng-container *ngIf="!loading && request as r">
                <div class="page-header">
                    <div>
                        <h1>{{ r.referenceNumber }}</h1>
                        <p class="sub">
                            <span class="mono">{{ r.id }}</span>
                            · {{ formatDate(r.createdAt) }}
                        </p>
                    </div>
                    <p-tag [value]="statusLabel(r.status)" [severity]="statusSeverity(r.status)"></p-tag>
                </div>

                <div class="grid">
                    <p-card class="span-2">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.requestInfo' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <dl class="dl">
                                <dt>{{ 'admin.cncRequests.serviceMode' | translate }}</dt>
                                <dd>
                                    <p-tag
                                        [value]="modeLabel(r.serviceMode)"
                                        [severity]="r.serviceMode === 'pcb' ? 'info' : 'warn'"
                                    ></p-tag>
                                </dd>
                                <dt>{{ 'admin.cncRequests.status' | translate }}</dt>
                                <dd>{{ statusLabel(r.status) }}</dd>
                                <dt>{{ 'admin.cncRequests.date' | translate }}</dt>
                                <dd>{{ formatDate(r.createdAt) }}</dd>
                                <dt *ngIf="r.reviewedAt">{{ 'admin.cncRequests.reviewedAt' | translate }}</dt>
                                <dd *ngIf="r.reviewedAt">{{ formatDate(r.reviewedAt!) }}</dd>
                                <dt *ngIf="r.completedAt">{{ 'admin.cncRequests.completedAt' | translate }}</dt>
                                <dd *ngIf="r.completedAt">{{ formatDate(r.completedAt!) }}</dd>
                            </dl>
                        </ng-template>
                    </p-card>

                    <p-card>
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.customerInfo' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <dl class="dl">
                                <dt>{{ 'admin.cncRequests.name' | translate }}</dt>
                                <dd>{{ r.customerName || '—' }}</dd>
                                <dt>{{ 'admin.cncRequests.email' | translate }}</dt>
                                <dd>{{ r.customerEmail }}</dd>
                                <dt *ngIf="r.customerPhone">{{ 'admin.cncRequests.phone' | translate }}</dt>
                                <dd *ngIf="r.customerPhone">{{ r.customerPhone }}</dd>
                            </dl>
                        </ng-template>
                    </p-card>

                    <p-card class="span-2" *ngIf="r.serviceMode === 'routing'">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.sectionRoutingDetail' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <dl class="dl">
                                <dt>{{ 'admin.cncRequests.material' | translate }}</dt>
                                <dd>{{ materialLabel(r) }}</dd>
                                <dt>{{ 'admin.cncRequests.operation' | translate }}</dt>
                                <dd>{{ opLabel(r.operationType) }}</dd>
                                <dt>{{ 'admin.cncRequests.dimensions' | translate }} (mm)</dt>
                                <dd>{{ dimLine(r) }}</dd>
                                <dt>{{ 'admin.cncRequests.depthMode' | translate }}</dt>
                                <dd>{{ depthLine(r) }}</dd>
                                <dt>{{ 'admin.cncRequests.quantity' | translate }}</dt>
                                <dd>{{ r.quantity }}</dd>
                                <dt>{{ 'admin.cncRequests.designSource' | translate }}</dt>
                                <dd>{{ designSourceLabel(r.designSourceType) }}</dd>
                            </dl>
                        </ng-template>
                    </p-card>

                    <p-card class="span-2" *ngIf="r.serviceMode === 'pcb'">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.sectionPcbDetail' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <dl class="dl">
                                <dt>{{ 'admin.cncRequests.pcbMaterialLabel' | translate }}</dt>
                                <dd>{{ r.pcbMaterial || '—' }}</dd>
                                <dt>{{ 'admin.cncRequests.thickness' | translate }}</dt>
                                <dd>{{ r.pcbThickness != null ? r.pcbThickness + ' mm' : '—' }}</dd>
                                <dt>{{ 'admin.cncRequests.pcbSide' | translate }}</dt>
                                <dd>{{ pcbSideLabel(r.pcbSide) }}</dd>
                                <dt>{{ 'admin.cncRequests.pcbOperations' | translate }}</dt>
                                <dd>
                                    <ul class="op-list" *ngIf="pcbOpLines(r).length; else noPcbOp">
                                        <li *ngFor="let line of pcbOpLines(r)">{{ line }}</li>
                                    </ul>
                                    <ng-template #noPcbOp>—</ng-template>
                                </dd>
                                <dt>{{ 'admin.cncRequests.quantity' | translate }}</dt>
                                <dd>{{ r.quantity }}</dd>
                            </dl>
                        </ng-template>
                    </p-card>

                    <p-card class="span-2">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.description' | translate }} / {{ 'admin.cncRequests.designNotes' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <div class="block" *ngIf="r.projectDescription">
                                <h4>{{ 'admin.cncRequests.description' | translate }}</h4>
                                <p class="pre">{{ r.projectDescription }}</p>
                            </div>
                            <div class="block" *ngIf="r.designNotes">
                                <h4>{{ 'admin.cncRequests.designNotes' | translate }}</h4>
                                <p class="pre">{{ r.designNotes }}</p>
                            </div>
                            <p *ngIf="!r.projectDescription && !r.designNotes" class="muted">—</p>
                        </ng-template>
                    </p-card>

                    <p-card class="span-2">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.filesSection' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <div *ngIf="r.fileName || r.filePath; else noFiles" class="file-row">
                                <i class="pi pi-file"></i>
                                <div>
                                    <div class="file-name">{{ r.fileName || ('admin.cncRequests.unnamedFile' | translate) }}</div>
                                    <div class="file-path muted" *ngIf="r.filePath">{{ r.filePath }}</div>
                                </div>
                                <p-button
                                    icon="pi pi-download"
                                    [label]="'admin.cncRequests.downloadFile' | translate"
                                    (onClick)="downloadFile(r)"
                                    [loading]="downloading"
                                ></p-button>
                            </div>
                            <ng-template #noFiles>
                                <p class="muted">{{ 'admin.cncRequests.noFile' | translate }}</p>
                            </ng-template>
                        </ng-template>
                    </p-card>

                    <p-card class="span-2 admin-card">
                        <ng-template pTemplate="title">{{ 'admin.cncRequests.adminSection' | translate }}</ng-template>
                        <ng-template pTemplate="content">
                            <div class="form-grid">
                                <div class="field">
                                    <label>{{ 'admin.cncRequests.updateStatusField' | translate }}</label>
                                    <p-select
                                        [(ngModel)]="editStatus"
                                        [options]="statusOptions"
                                        optionLabel="label"
                                        optionValue="value"
                                        styleClass="w-full"
                                    ></p-select>
                                </div>
                                <div class="field">
                                    <label>{{ 'admin.cncRequests.estimatedPrice' | translate }}</label>
                                    <p-inputNumber
                                        [(ngModel)]="editEstimated"
                                        mode="decimal"
                                        [minFractionDigits]="0"
                                        [maxFractionDigits]="2"
                                        styleClass="w-full"
                                    ></p-inputNumber>
                                </div>
                                <div class="field">
                                    <label>{{ 'admin.cncRequests.finalPrice' | translate }}</label>
                                    <p-inputNumber
                                        [(ngModel)]="editFinal"
                                        mode="decimal"
                                        [minFractionDigits]="0"
                                        [maxFractionDigits]="2"
                                        styleClass="w-full"
                                    ></p-inputNumber>
                                </div>
                                <div class="field full">
                                    <label>{{ 'admin.cncRequests.adminNotes' | translate }}</label>
                                    <textarea
                                        pTextarea
                                        [(ngModel)]="editAdminNotes"
                                        rows="4"
                                        class="w-full"
                                    ></textarea>
                                </div>
                                <div class="actions">
                                    <p-button
                                        [label]="'common.save' | translate"
                                        icon="pi pi-check"
                                        (onClick)="save()"
                                        [loading]="saving"
                                        [disabled]="saving"
                                    ></p-button>
                                </div>
                            </div>
                        </ng-template>
                    </p-card>
                </div>
            </ng-container>

            <div *ngIf="!loading && !request" class="empty">
                <p>{{ 'admin.cncRequests.notFound' | translate }}</p>
                <p-button [label]="'admin.cncRequests.backToList' | translate" routerLink="/admin/cnc-requests"></p-button>
            </div>
        </div>
    `,
    styles: [
        `
            .detail-page {
                max-width: 1100px;
                margin: 0 auto;
                padding: 1rem 1.5rem 2rem;
            }
            .page-toolbar {
                margin-bottom: 1rem;
            }
            .loading-wrap {
                display: flex;
                justify-content: center;
                padding: 3rem;
            }
            .page-header {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                gap: 1rem;
                margin-bottom: 1.5rem;
            }
            .page-header h1 {
                margin: 0;
                font-size: 1.75rem;
            }
            .sub {
                margin: 0.35rem 0 0;
                font-size: 0.85rem;
                color: var(--text-color-secondary);
            }
            .mono {
                font-family: ui-monospace, monospace;
            }
            .grid {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 1rem;
            }
            @media (max-width: 768px) {
                .grid {
                    grid-template-columns: 1fr;
                }
            }
            .span-2 {
                grid-column: 1 / -1;
            }
            .dl {
                display: grid;
                grid-template-columns: 12rem 1fr;
                gap: 0.5rem 1rem;
                margin: 0;
            }
            .dl dt {
                margin: 0;
                font-weight: 600;
                color: var(--text-color-secondary);
                font-size: 0.85rem;
            }
            .dl dd {
                margin: 0;
            }
            .op-list {
                margin: 0;
                padding-left: 1.25rem;
            }
            .block h4 {
                margin: 0 0 0.35rem;
                font-size: 0.9rem;
                color: var(--text-color-secondary);
            }
            .pre {
                margin: 0;
                white-space: pre-wrap;
                line-height: 1.5;
            }
            .muted {
                color: var(--text-color-secondary);
            }
            .file-row {
                display: flex;
                align-items: flex-start;
                gap: 1rem;
                flex-wrap: wrap;
            }
            .file-row .pi {
                font-size: 1.5rem;
                color: var(--primary-color);
                margin-top: 0.2rem;
            }
            .file-name {
                font-weight: 600;
            }
            .file-path {
                font-size: 0.8rem;
                word-break: break-all;
            }
            .form-grid {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 1rem;
            }
            .field.full {
                grid-column: 1 / -1;
            }
            .actions {
                grid-column: 1 / -1;
                display: flex;
                justify-content: flex-end;
            }
            .empty {
                text-align: center;
                padding: 3rem;
            }
        `
    ]
})
export class CncRequestDetailComponent implements OnInit {
    request: CncServiceRequestDto | null = null;
    loading = true;
    saving = false;
    downloading = false;

    editStatus: CncServiceRequestStatus = CncServiceRequestStatus.Pending;
    editAdminNotes = '';
    editEstimated: number | null = null;
    editFinal: number | null = null;

    statusOptions: { label: string; value: CncServiceRequestStatus }[] = [];

    private route = inject(ActivatedRoute);
    private cncApi = inject(CncApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    ngOnInit() {
        this.rebuildStatusOptions();
        this.translate.onLangChange.subscribe(() => this.rebuildStatusOptions());
        const id = this.route.snapshot.paramMap.get('id');
        if (!id) {
            this.loading = false;
            return;
        }
        this.load(id);
    }

    private rebuildStatusOptions() {
        const keys: { k: string; v: CncServiceRequestStatus }[] = [
            { k: 'admin.cncRequests.stPending', v: CncServiceRequestStatus.Pending },
            { k: 'admin.cncRequests.stInReview', v: CncServiceRequestStatus.InReview },
            { k: 'admin.cncRequests.stQuoted', v: CncServiceRequestStatus.Quoted },
            { k: 'admin.cncRequests.stAccepted', v: CncServiceRequestStatus.Accepted },
            { k: 'admin.cncRequests.stInProgress', v: CncServiceRequestStatus.InProgress },
            { k: 'admin.cncRequests.stCompleted', v: CncServiceRequestStatus.Completed },
            { k: 'admin.cncRequests.stCancelled', v: CncServiceRequestStatus.Cancelled },
            { k: 'admin.cncRequests.stRejected', v: CncServiceRequestStatus.Rejected }
        ];
        this.statusOptions = keys.map((x) => ({
            label: this.translate.instant(x.k),
            value: x.v
        }));
    }

    private load(id: string) {
        this.loading = true;
        this.cncApi.getAdminRequestById(id).subscribe({
            next: (r) => {
                this.request = r;
                this.editStatus = r.status;
                this.editAdminNotes = r.adminNotes ?? '';
                this.editEstimated = r.estimatedPrice ?? null;
                this.editFinal = r.finalPrice ?? null;
                this.loading = false;
            },
            error: () => {
                this.request = null;
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('admin.cncRequests.errorLoadingDetail'),
                    life: 5000
                });
            }
        });
    }

    save() {
        if (!this.request) {
            return;
        }
        this.saving = true;
        const payload: {
            status: CncServiceRequestStatus;
            adminNotes: string;
            estimatedPrice?: number;
            finalPrice?: number;
        } = {
            status: this.editStatus,
            adminNotes: this.editAdminNotes
        };
        if (this.editEstimated != null) {
            payload.estimatedPrice = this.editEstimated;
        }
        if (this.editFinal != null) {
            payload.finalPrice = this.editFinal;
        }
        this.cncApi.updateAdminRequest(this.request.id, payload).subscribe({
            next: (r) => {
                this.request = r;
                this.editStatus = r.status;
                this.editAdminNotes = r.adminNotes ?? '';
                this.editEstimated = r.estimatedPrice ?? null;
                this.editFinal = r.finalPrice ?? null;
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('messages.success'),
                    detail: this.translate.instant('admin.cncRequests.saved'),
                    life: 3000
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('admin.cncRequests.errorSave'),
                    life: 5000
                });
            }
        });
    }

    downloadFile(r: CncServiceRequestDto) {
        this.downloading = true;
        this.cncApi.downloadRequestFile(r.id).subscribe({
            next: (blob) => {
                const name = r.fileName || 'cnc-file';
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = name;
                a.click();
                window.URL.revokeObjectURL(url);
                this.downloading = false;
            },
            error: () => {
                this.downloading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('admin.cncRequests.errorDownload'),
                    life: 5000
                });
            }
        });
    }

    materialLabel(r: CncServiceRequestDto): string {
        return r.materialNameEn || r.materialNameAr || '—';
    }

    dimLine(r: CncServiceRequestDto): string {
        const w = r.widthMm != null ? String(r.widthMm) : '—';
        const h = r.heightMm != null ? String(r.heightMm) : '—';
        const t = r.thicknessMm != null ? String(r.thicknessMm) : '—';
        return `${w} × ${h} × ${t}`;
    }

    depthLine(r: CncServiceRequestDto): string {
        const mode = (r.depthMode || '').toLowerCase();
        const modeLabel =
            mode === 'through'
                ? this.translate.instant('admin.cncRequests.depthThrough')
                : mode === 'custom'
                  ? this.translate.instant('admin.cncRequests.depthCustom')
                  : r.depthMode || '—';
        if (r.depthMm != null && mode === 'custom') {
            return `${modeLabel} (${r.depthMm} mm)`;
        }
        return modeLabel;
    }

    designSourceLabel(t: string): string {
        const x = (t || '').toLowerCase();
        if (x === 'production') {
            return this.translate.instant('admin.cncRequests.designSourceProduction');
        }
        if (x === 'image') {
            return this.translate.instant('admin.cncRequests.designSourceImage');
        }
        return t || '—';
    }

    opLabel(op: string | null | undefined): string {
        if (!op) {
            return '—';
        }
        const m: Record<string, string> = {
            cut: 'admin.cncRequests.opCut',
            engrave: 'admin.cncRequests.opEngrave',
            pocket: 'admin.cncRequests.opPocket',
            drill: 'admin.cncRequests.opDrill'
        };
        const key = m[op.toLowerCase()];
        return key ? this.translate.instant(key) : op;
    }

    pcbSideLabel(side: string | null | undefined): string {
        if (!side) {
            return '—';
        }
        if (side === 'single') {
            return this.translate.instant('admin.cncRequests.sideSingle');
        }
        if (side === 'double') {
            return this.translate.instant('admin.cncRequests.sideDouble');
        }
        return side;
    }

    pcbOpLines(r: CncServiceRequestDto): string[] {
        const raw = (r.pcbOperation || '').trim();
        if (!raw) {
            return [];
        }
        return raw
            .split(/[,;]+/)
            .map((s) => s.trim())
            .filter(Boolean);
    }

    modeLabel(mode: string): string {
        if (mode === 'pcb') {
            return this.translate.instant('admin.cncRequests.modePcb');
        }
        if (mode === 'routing') {
            return this.translate.instant('admin.cncRequests.modeRouting');
        }
        return mode || '—';
    }

    statusLabel(status: CncServiceRequestStatus): string {
        const map: Record<number, string> = {
            [CncServiceRequestStatus.Pending]: 'admin.cncRequests.stPending',
            [CncServiceRequestStatus.InReview]: 'admin.cncRequests.stInReview',
            [CncServiceRequestStatus.Quoted]: 'admin.cncRequests.stQuoted',
            [CncServiceRequestStatus.Accepted]: 'admin.cncRequests.stAccepted',
            [CncServiceRequestStatus.InProgress]: 'admin.cncRequests.stInProgress',
            [CncServiceRequestStatus.Completed]: 'admin.cncRequests.stCompleted',
            [CncServiceRequestStatus.Cancelled]: 'admin.cncRequests.stCancelled',
            [CncServiceRequestStatus.Rejected]: 'admin.cncRequests.stRejected'
        };
        const k = map[status];
        return k ? this.translate.instant(k) : String(status);
    }

    statusSeverity(
        status: CncServiceRequestStatus
    ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
        switch (status) {
            case CncServiceRequestStatus.Completed:
                return 'success';
            case CncServiceRequestStatus.Cancelled:
            case CncServiceRequestStatus.Rejected:
                return 'danger';
            case CncServiceRequestStatus.InProgress:
            case CncServiceRequestStatus.Accepted:
                return 'info';
            case CncServiceRequestStatus.Quoted:
            case CncServiceRequestStatus.InReview:
                return 'warn';
            default:
                return 'secondary';
        }
    }

    formatDate(iso: string): string {
        try {
            return new Date(iso).toLocaleString();
        } catch {
            return iso;
        }
    }
}
