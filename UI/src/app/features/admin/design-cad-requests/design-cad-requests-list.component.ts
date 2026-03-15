import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DesignCadRequestService } from '../../../shared/services/design-cad-request.service';
import { LanguageService } from '../../../shared/services/language.service';
import type { DesignCadRequestDto } from '../../../shared/models/design-cad-request.model';

@Component({
    selector: 'app-design-cad-requests-list',
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
        DialogModule,
        ProgressSpinnerModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="cad-requests-container">
            <div class="page-header">
                <div class="header-content">
                    <h1>{{ 'admin.cadRequests.title' | translate }}</h1>
                    <p class="subtitle">{{ 'admin.cadRequests.subtitle' | translate }}</p>
                </div>
            </div>

            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.cadRequests.filterStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStatus"
                            [options]="statusOptions"
                            [placeholder]="'admin.cadRequests.allStatuses' | translate"
                            (onChange)="loadRequests()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item flex-grow">
                        <label class="filter-label">{{ 'admin.cadRequests.search' | translate }}</label>
                        <input
                            pInputText
                            [(ngModel)]="searchTerm"
                            (keyup.enter)="loadRequests()"
                            [placeholder]="'admin.cadRequests.searchPlaceholder' | translate"
                            class="w-full" />
                    </div>
                    <div class="filter-actions">
                        <p-button icon="pi pi-search" [label]="'common.search' | translate" (onClick)="loadRequests()" styleClass="p-button-outlined"></p-button>
                        <p-button icon="pi pi-refresh" [label]="'common.refresh' | translate" (onClick)="loadRequests()" [outlined]="true"></p-button>
                    </div>
                </div>
            </p-card>

            <p-card class="table-card">
                <p-table
                    [value]="requests"
                    [loading]="loading"
                    [paginator]="true"
                    [rows]="pageSize"
                    [first]="(currentPage - 1) * pageSize"
                    [rowsPerPageOptions]="[10, 25, 50]"
                    [totalRecords]="totalRecords"
                    (onLazyLoad)="onPage($event)"
                    [lazy]="true"
                    styleClass="p-datatable-striped">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.cadRequests.reference' | translate }}</th>
                            <th>{{ 'admin.cadRequests.type' | translate }}</th>
                            <th>{{ 'admin.cadRequests.requestTitle' | translate }}</th>
                            <th>{{ 'admin.cadRequests.status' | translate }}</th>
                            <th>{{ 'admin.cadRequests.created' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-req>
                        <tr>
                            <td><span class="ref-number">{{ req.referenceNumber }}</span></td>
                            <td>{{ getTypeLabel(req.requestType) }}</td>
                            <td>{{ req.title }}</td>
                            <td><p-tag [value]="req.status" [severity]="getStatusSeverity(req.status)"></p-tag></td>
                            <td>{{ formatDate(req.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button icon="pi pi-eye" [rounded]="true" [text]="true" (onClick)="viewRequest(req)" pTooltip="{{ 'admin.cadRequests.view' | translate }}"></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="6" class="empty-message">
                                <i class="pi pi-inbox"></i>
                                <span>{{ 'admin.cadRequests.noRequests' | translate }}</span>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>

            <p-dialog
                [(visible)]="showViewDialog"
                [modal]="true"
                [style]="{ width: '700px', maxWidth: '95vw' }"
                [header]="viewDialogTitle"
                [draggable]="false"
                [resizable]="false">
                <div class="detail-content" *ngIf="selectedRequest">
                    <div class="detail-grid">
                        <div class="detail-item"><label>{{ 'admin.cadRequests.reference' | translate }}</label><span class="ref-number">{{ selectedRequest.referenceNumber }}</span></div>
                        <div class="detail-item"><label>{{ 'admin.cadRequests.type' | translate }}</label><span>{{ getTypeLabel(selectedRequest.requestType) }}</span></div>
                        <div class="detail-item"><label>{{ 'admin.cadRequests.status' | translate }}</label><p-tag [value]="selectedRequest.status" [severity]="getStatusSeverity(selectedRequest.status)"></p-tag></div>
                        <div class="detail-item full-width"><label>{{ 'admin.cadRequests.requestTitle' | translate }}</label><span>{{ selectedRequest.title }}</span></div>
                        <div class="detail-item full-width" *ngIf="selectedRequest.description"><label>{{ 'designCad.new.description' | translate }}</label><p class="notes-text">{{ selectedRequest.description }}</p></div>
                        <div class="detail-item"><label>{{ 'admin.cadRequests.created' | translate }}</label><span>{{ formatDate(selectedRequest.createdAt) }}</span></div>
                        <div class="detail-item full-width" *ngIf="(selectedRequest.attachments?.length ?? 0) > 0">
                            <label>{{ 'admin.cadRequests.attachments' | translate }}</label>
                            <div class="attachment-list">
                                @for (att of selectedRequest.attachments; track att.id) {
                                    <div class="attachment-row">
                                        <i class="pi pi-file"></i>
                                        <span class="att-name">{{ att.fileName }}</span>
                                        <span class="att-size">{{ formatFileSize(att.fileSizeBytes) }}</span>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
                </div>
            </p-dialog>
        </div>
    `,
    styles: [`
        .cad-requests-container { padding: 1.5rem; }
        .page-header { margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; color: #1e293b; }
        .subtitle { margin: 0.25rem 0 0; color: #64748b; font-size: 0.875rem; }
        .filters-card { margin-bottom: 1.5rem; }
        .filters-grid { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; }
        .filter-item { min-width: 180px; }
        .filter-item.flex-grow { flex: 1; min-width: 200px; }
        .filter-label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
        .filter-actions { display: flex; gap: 0.5rem; }
        .ref-number { font-family: monospace; font-weight: 600; color: #667eea; font-size: 0.85rem; }
        .action-buttons { display: flex; gap: 0.25rem; }
        .empty-message { text-align: center; padding: 3rem; color: #94a3b8; }
        .empty-message i { font-size: 2rem; display: block; margin-bottom: 0.5rem; }
        .detail-content { padding: 0.5rem 0; }
        .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        .detail-item { display: flex; flex-direction: column; gap: 0.25rem; }
        .detail-item.full-width { grid-column: span 2; }
        .detail-item label { font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
        .notes-text { margin: 0; color: #475569; line-height: 1.5; white-space: pre-wrap; }
        .attachment-list { display: flex; flex-direction: column; gap: 0.5rem; }
        .attachment-row { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem; background: #f8fafc; border-radius: 8px; }
        .attachment-row .att-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .attachment-row .att-size { font-size: 0.8rem; color: #64748b; }
        .w-full { width: 100%; }
    `]
})
export class DesignCadRequestsListComponent implements OnInit {
    private designCadService = inject(DesignCadRequestService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    languageService = inject(LanguageService);

    requests: DesignCadRequestDto[] = [];
    loading = true;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedStatus = '';
    searchTerm = '';

    statusOptions: { value: string; label: string }[] = [
        { value: 'New', label: 'New' },
        { value: 'UnderReview', label: 'Under Review' },
        { value: 'Quoted', label: 'Quoted' },
        { value: 'Approved', label: 'Approved' },
        { value: 'InProgress', label: 'In Progress' },
        { value: 'Delivered', label: 'Delivered' },
        { value: 'Closed', label: 'Closed' },
        { value: 'Rejected', label: 'Rejected' },
        { value: 'Cancelled', label: 'Cancelled' }
    ];

    showViewDialog = false;
    selectedRequest: DesignCadRequestDto | null = null;

    get viewDialogTitle(): string {
        return this.translate.instant('admin.cadRequests.viewDetails');
    }

    ngOnInit() {
        this.loadRequests();
    }

    loadRequests() {
        this.loading = true;
        this.designCadService.getList({
            page: this.currentPage,
            pageSize: this.pageSize,
            status: this.selectedStatus || undefined,
            search: this.searchTerm?.trim() || undefined
        }).subscribe({
            next: (res) => {
                this.requests = res.items ?? [];
                this.totalRecords = res.totalCount ?? 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('messages.loadError') });
            }
        });
    }

    onPage(event: { first?: number | null; rows?: number | null }) {
        const first = event.first ?? 0;
        const rows = event.rows ?? 10;
        this.currentPage = rows ? Math.floor(first / rows) + 1 : 1;
        this.pageSize = rows;
        this.loadRequests();
    }

    viewRequest(req: DesignCadRequestDto) {
        this.selectedRequest = req;
        if ((req.attachments?.length ?? 0) === 0 && (req.attachmentCount ?? 0) > 0) {
            this.designCadService.getById(req.id).subscribe({
                next: (full) => {
                    this.selectedRequest = full;
                    this.showViewDialog = true;
                },
                error: () => this.showViewDialog = true
            });
        } else {
            this.showViewDialog = true;
        }
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '-';
        return new Date(dateStr).toLocaleDateString(this.languageService.language === 'ar' ? 'ar-EG' : 'en-US', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    formatFileSize(bytes: number): string {
        if (!bytes) return '0 B';
        const k = 1024;
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + ['B', 'KB', 'MB', 'GB'][i];
    }

    getTypeLabel(type: string): string {
        const labels: Record<string, string> = {
            IdeaOnly: 'Idea Only',
            ExistingFiles: 'Existing CAD Files',
            ReverseEngineering: 'Reverse Engineering',
            PhysicalItem: 'Physical Item',
            ModifyProduct: 'Modify Product',
            MechanicalAssembly: 'Mechanical Assembly'
        };
        return labels[type] ?? type;
    }

    getStatusSeverity(status: string): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
        const map: Record<string, 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast'> = {
            New: 'warn',
            UnderReview: 'info',
            Quoted: 'info',
            Approved: 'info',
            InProgress: 'info',
            Delivered: 'success',
            Closed: 'secondary',
            Rejected: 'danger',
            Cancelled: 'secondary'
        };
        return map[status] ?? 'secondary';
    }
}
