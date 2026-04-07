import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { DatePickerModule } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { CncApiService } from '../../../shared/services/cnc-api.service';
import { CncServiceRequestDto, CncServiceRequestStatus } from '../../../shared/models/cnc.model';
import { ServiceRequestStatusBadgeComponent } from '../../../shared/components/service-request-status-badge/service-request-status-badge.component';
import {
    ServiceWorkflowStatus,
    denormalizeCncWorkflowStatus,
    getWorkflowOptions,
    normalizeCncWorkflowStatus
} from '../../../shared/utils/service-request-workflow';

@Component({
    selector: 'app-cnc-requests-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TableModule,
        SelectModule,
        TagModule,
        InputTextModule,
        ToastModule,
        DatePickerModule,
        TooltipModule,
        TranslateModule,
        ServiceRequestStatusBadgeComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="requests-list-container">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.cncRequests.title' | translate }}</h1>
                        <p class="subtitle">{{ 'admin.cncRequests.subtitle' | translate }}</p>
                    </div>
                </div>
            </div>

            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item filter-span-2">
                        <label class="filter-label">{{ 'common.search' | translate }}</label>
                        <input
                            pInputText
                            class="w-full"
                            [(ngModel)]="searchText"
                            (keyup.enter)="applyFilters()"
                            [placeholder]="'admin.cncRequests.searchPlaceholder' | translate"
                        />
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.cncRequests.filterByMode' | translate }}</label>
                        <p-select
                            [(ngModel)]="filterMode"
                            [options]="modeOptions"
                            optionLabel="label"
                            optionValue="value"
                            [placeholder]="'admin.cncRequests.allModes' | translate"
                            [showClear]="true"
                            styleClass="w-full"
                        ></p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.cncRequests.filterByStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="filterStatus"
                            [options]="statusOptions"
                            optionLabel="label"
                            optionValue="value"
                            [placeholder]="'admin.cncRequests.allStatuses' | translate"
                            [showClear]="true"
                            styleClass="w-full"
                        ></p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.cncRequests.dateFrom' | translate }}</label>
                        <p-datepicker
                            [(ngModel)]="dateFrom"
                            [showIcon]="true"
                            dateFormat="yy-mm-dd"
                            styleClass="w-full"
                            [showButtonBar]="true"
                        ></p-datepicker>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.cncRequests.dateTo' | translate }}</label>
                        <p-datepicker
                            [(ngModel)]="dateTo"
                            [showIcon]="true"
                            dateFormat="yy-mm-dd"
                            styleClass="w-full"
                            [showButtonBar]="true"
                        ></p-datepicker>
                    </div>
                    <div class="filter-actions">
                        <p-button
                            [label]="'admin.cncRequests.applyFilters' | translate"
                            icon="pi pi-filter"
                            (onClick)="applyFilters()"
                        ></p-button>
                        <p-button
                            icon="pi pi-refresh"
                            [label]="'common.refresh' | translate"
                            (onClick)="refresh()"
                            [outlined]="true"
                        ></p-button>
                    </div>
                </div>
            </p-card>

            <p-card class="table-card">
                <p-table
                    [value]="requests"
                    [lazy]="true"
                    [(first)]="first"
                    (onLazyLoad)="onLazyLoad($event)"
                    [paginator]="true"
                    [rows]="rows"
                    [totalRecords]="totalRecords"
                    [loading]="loading"
                    [rowsPerPageOptions]="[10, 25, 50]"
                    [showCurrentPageReport]="true"
                    [currentPageReportTemplate]="('common.showing' | translate)"
                    styleClass="p-datatable-striped"
                    [tableStyle]="{ 'min-width': '72rem' }"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.cncRequests.reference' | translate }}</th>
                            <th>{{ 'admin.cncRequests.serviceMode' | translate }}</th>
                            <th>{{ 'admin.cncRequests.customer' | translate }}</th>
                            <th>{{ 'admin.cncRequests.summary' | translate }}</th>
                            <th>{{ 'admin.cncRequests.quantity' | translate }}</th>
                            <th>{{ 'admin.cncRequests.status' | translate }}</th>
                            <th>{{ 'admin.cncRequests.date' | translate }}</th>
                            <th class="col-icons">{{ 'admin.cncRequests.indicators' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="body" let-req>
                        <tr>
                            <td>
                                <div class="ref-cell">
                                    <span class="ref-number">{{ req.referenceNumber }}</span>
                                    <span class="id-sub text-muted">{{ req.id | slice : 0 : 8 }}…</span>
                                </div>
                            </td>
                            <td>
                                <p-tag
                                    [value]="modeLabel(req.serviceMode)"
                                    [severity]="req.serviceMode === 'pcb' ? 'info' : 'warn'"
                                ></p-tag>
                            </td>
                            <td>
                                <div class="customer-cell">
                                    <span>{{ req.customerName || '—' }}</span>
                                    <span class="email-sub" *ngIf="req.customerEmail">{{ req.customerEmail }}</span>
                                </div>
                            </td>
                            <td class="summary-cell">{{ buildSummary(req) }}</td>
                            <td>{{ req.quantity }}</td>
                            <td>
                                <app-service-request-status-badge module="cnc" [status]="req.workflowStatus ?? req.status"></app-service-request-status-badge>
                            </td>
                            <td>{{ formatDate(req.createdAt) }}</td>
                            <td class="col-icons">
                                <i
                                    class="pi pi-paperclip"
                                    [class.text-muted]="!req.fileName"
                                    [class.text-primary]="!!req.fileName"
                                    [pTooltip]="
                                        req.fileName
                                            ? ('admin.cncRequests.fileHint' | translate) + ': ' + req.fileName
                                            : ('admin.cncRequests.noFile' | translate)
                                    "
                                ></i>
                                <i
                                    class="pi pi-file-edit ml-2"
                                    [class.text-muted]="!hasNotes(req)"
                                    [class.text-primary]="hasNotes(req)"
                                    [pTooltip]="
                                        hasNotes(req)
                                            ? ('admin.cncRequests.notesHint' | translate)
                                            : ('admin.cncRequests.noNotes' | translate)
                                    "
                                ></i>
                            </td>
                            <td>
                                <p-button
                                    icon="pi pi-eye"
                                    [rounded]="true"
                                    [text]="true"
                                    (onClick)="viewRequest(req)"
                                    [pTooltip]="'admin.cncRequests.viewQuick' | translate"
                                ></p-button>
                                <p-button
                                    icon="pi pi-sitemap"
                                    [rounded]="true"
                                    [text]="true"
                                    (onClick)="viewRequest(req)"
                                    [pTooltip]="'admin.serviceWorkflow.manageWorkflow' | translate"
                                ></p-button>
                            </td>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="9" class="empty-message">
                                <i class="pi pi-inbox"></i>
                                <span>{{ 'admin.cncRequests.noRequests' | translate }}</span>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>
    `,
    styles: [
        `
            .requests-list-container {
                width: 100%;
                max-width: 100%;
                padding: 0.5rem;
                box-sizing: border-box;
            }
            @media (min-width: 768px) {
                .requests-list-container {
                    padding: 1rem;
                }
            }
            .page-header {
                margin-bottom: 1.5rem;
            }
            .header-content h1 {
                font-size: 1.5rem;
                font-weight: bold;
                margin: 0;
            }
            @media (min-width: 768px) {
                .header-content h1 {
                    font-size: 2rem;
                }
            }
            .subtitle {
                font-size: 0.875rem;
                color: var(--text-color-secondary);
                margin: 0.25rem 0 0;
            }
            .filters-card {
                margin-bottom: 1rem;
            }
            .filters-grid {
                display: grid;
                grid-template-columns: 1fr;
                gap: 1rem;
                align-items: end;
            }
            @media (min-width: 900px) {
                .filters-grid {
                    grid-template-columns: repeat(4, 1fr);
                }
                .filter-span-2 {
                    grid-column: span 2;
                }
            }
            .filter-label {
                display: block;
                font-size: 0.875rem;
                font-weight: 600;
                margin-bottom: 0.35rem;
            }
            .filter-actions {
                display: flex;
                flex-wrap: wrap;
                gap: 0.5rem;
                align-items: center;
            }
            .table-card {
                width: 100%;
            }
            .ref-number {
                font-weight: 600;
                font-family: ui-monospace, monospace;
            }
            .id-sub,
            .email-sub {
                display: block;
                font-size: 0.75rem;
                color: var(--text-color-secondary);
            }
            .summary-cell {
                max-width: 22rem;
                white-space: normal;
                font-size: 0.875rem;
            }
            .customer-cell {
                display: flex;
                flex-direction: column;
                gap: 0.15rem;
            }
            .col-icons {
                text-align: center;
                white-space: nowrap;
            }
            .ml-2 {
                margin-left: 0.5rem;
            }
            .empty-message {
                text-align: center;
                padding: 2rem !important;
            }
            .empty-message i {
                font-size: 2rem;
                display: block;
                margin-bottom: 0.5rem;
                opacity: 0.5;
            }
        `
    ]
})
export class CncRequestsListComponent implements OnInit {
    requests: CncServiceRequestDto[] = [];
    totalRecords = 0;
    loading = false;
    rows = 25;
    first = 0;

    searchText = '';
    filterMode: string | null = null;
    filterStatus: ServiceWorkflowStatus | null = null;
    dateFrom: Date | null = null;
    dateTo: Date | null = null;

    modeOptions: { label: string; value: string }[] = [];
    statusOptions: { label: string; value: ServiceWorkflowStatus }[] = [];

    private cncApi = inject(CncApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private router = inject(Router);

    ngOnInit() {
        this.rebuildFilterOptions();
        this.translate.onLangChange.subscribe(() => this.rebuildFilterOptions());
    }

    private rebuildFilterOptions() {
        this.modeOptions = [
            { label: this.translate.instant('admin.cncRequests.modeRouting'), value: 'routing' },
            { label: this.translate.instant('admin.cncRequests.modePcb'), value: 'pcb' }
        ];
        this.statusOptions = getWorkflowOptions('cnc', this.translate);
    }

    onLazyLoad(event: TableLazyLoadEvent) {
        this.first = event.first ?? 0;
        this.rows = event.rows ?? 25;
        this.loadRequests();
    }

    applyFilters() {
        this.first = 0;
        this.loadRequests();
    }

    refresh() {
        this.loadRequests();
    }

    private loadRequests() {
        this.loading = true;
        this.cncApi
            .getAdminRequests({
                search: this.searchText.trim() || undefined,
                serviceMode: this.filterMode || undefined,
                status: this.filterStatus !== null && this.filterStatus !== undefined ? denormalizeCncWorkflowStatus(this.filterStatus as ServiceWorkflowStatus) : undefined,
                createdFrom: this.toYmd(this.dateFrom),
                createdTo: this.toYmd(this.dateTo),
                skip: this.first,
                take: this.rows
            })
            .subscribe({
                next: (res) => {
                    this.requests = res.items;
                    this.totalRecords = res.totalCount;
                    this.loading = false;
                },
                error: () => {
                    this.loading = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translate.instant('messages.error'),
                        detail: this.translate.instant('admin.cncRequests.errorLoading'),
                        life: 5000
                    });
                }
            });
    }

    private toYmd(d: Date | null): string | undefined {
        if (!d) {
            return undefined;
        }
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${y}-${m}-${day}`;
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

    buildSummary(req: CncServiceRequestDto): string {
        if (req.serviceMode === 'pcb') {
            const parts = [
                req.pcbMaterial || undefined,
                req.pcbThickness != null ? `${req.pcbThickness} mm` : undefined,
                req.pcbSide
                    ? req.pcbSide === 'single'
                        ? this.translate.instant('admin.cncRequests.sideSingle')
                        : this.translate.instant('admin.cncRequests.sideDouble')
                    : undefined,
                req.pcbOperation || undefined
            ].filter(Boolean);
            return parts.length ? parts.join(' · ') : '—';
        }
        const mat = req.materialNameEn || req.materialNameAr || '—';
        const dims =
            req.widthMm != null || req.heightMm != null || req.thicknessMm != null
                ? [req.widthMm, req.heightMm, req.thicknessMm].map((x) => (x != null ? String(x) : '—')).join(' × ') + ' mm'
                : null;
        const op = req.operationType || undefined;
        const bits = [mat, dims, op].filter(Boolean);
        return bits.length ? bits.join(' · ') : '—';
    }

    hasNotes(req: CncServiceRequestDto): boolean {
        const d = (req.designNotes || '').trim();
        const p = (req.projectDescription || '').trim();
        return !!(d || p);
    }

    formatDate(iso: string): string {
        try {
            const d = new Date(iso);
            return d.toLocaleString();
        } catch {
            return iso;
        }
    }

    viewRequest(req: CncServiceRequestDto) {
        void this.router.navigate(['/admin/cnc-requests', req.id]);
    }
}
