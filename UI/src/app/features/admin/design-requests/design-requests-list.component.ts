import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DesignRequestService } from '../../../shared/services/design-request.service';
import { LanguageService } from '../../../shared/services/language.service';
import {
    DesignServiceRequestDto,
    DesignRequestStatus,
    DesignRequestType,
    DESIGN_REQUEST_TYPES
} from '../../../shared/models/design-request.model';

const DESIGN_STATUS_OPTIONS: { value: DesignRequestStatus; label: string }[] = [
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

@Component({
    selector: 'app-design-requests-list',
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
        InputNumberModule,
        TextareaModule,
        ProgressSpinnerModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="design-requests-container">
            <div class="page-header">
                <div class="header-content">
                    <h1>{{ 'admin.designRequests.title' | translate }}</h1>
                    <p class="subtitle">{{ 'admin.designRequests.subtitle' | translate }}</p>
                </div>
            </div>

            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.designRequests.filterStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStatus"
                            [options]="statusOptions"
                            [placeholder]="'admin.designRequests.allStatuses' | translate"
                            (onChange)="loadRequests()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.designRequests.filterType' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedType"
                            [options]="typeOptions"
                            [placeholder]="'admin.designRequests.allTypes' | translate"
                            (onChange)="loadRequests()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item flex-grow">
                        <label class="filter-label">{{ 'admin.designRequests.search' | translate }}</label>
                        <input
                            pInputText
                            [(ngModel)]="searchTerm"
                            (keyup.enter)="loadRequests()"
                            [placeholder]="'admin.designRequests.searchPlaceholder' | translate"
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
                            <th>{{ 'admin.designRequests.reference' | translate }}</th>
                            <th>{{ 'admin.designRequests.type' | translate }}</th>
                            <th>{{ 'admin.designRequests.customer' | translate }}</th>
                            <th>{{ 'admin.designRequests.status' | translate }}</th>
                            <th>{{ 'admin.designRequests.created' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-req>
                        <tr>
                            <td><span class="ref-number">{{ req.referenceNumber }}</span></td>
                            <td>{{ getTypeLabel(req.requestType) }}</td>
                            <td>
                                <div class="customer-cell">
                                    <span *ngIf="req.customerName">{{ req.customerName }}</span>
                                    <span *ngIf="req.customerEmail" class="email">{{ req.customerEmail }}</span>
                                    <span *ngIf="!req.customerName && !req.customerEmail">-</span>
                                </div>
                            </td>
                            <td><p-tag [value]="req.status" [severity]="getStatusSeverity(req.status)"></p-tag></td>
                            <td>{{ formatDate(req.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button icon="pi pi-eye" [rounded]="true" [text]="true" (onClick)="viewRequest(req)" pTooltip="{{ 'admin.designRequests.view' | translate }}"></p-button>
                                    <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" (onClick)="editRequest(req)" pTooltip="{{ 'admin.designRequests.updateStatus' | translate }}"></p-button>
                                    <p-button icon="pi pi-download" [rounded]="true" [text]="true" (onClick)="downloadAll(req)" [disabled]="!(req.attachmentCount || (req.attachments?.length ?? 0))" pTooltip="{{ 'admin.designRequests.downloadAll' | translate }}"></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="6" class="empty-message">
                                <i class="pi pi-inbox"></i>
                                <span>{{ 'admin.designRequests.noRequests' | translate }}</span>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>

        <!-- View / Detail Dialog -->
        <p-dialog
            [(visible)]="showViewDialog"
            [modal]="true"
            [style]="{ width: '700px', maxWidth: '95vw' }"
            [header]="viewDialogTitle"
            [draggable]="false"
            [resizable]="false">
            <div class="detail-content" *ngIf="selectedRequest">
                <div class="detail-grid">
                    <div class="detail-item"><label>{{ 'admin.designRequests.reference' | translate }}</label><span class="ref-number">{{ selectedRequest.referenceNumber }}</span></div>
                    <div class="detail-item"><label>{{ 'admin.designRequests.type' | translate }}</label><span>{{ getTypeLabel(selectedRequest.requestType) }}</span></div>
                    <div class="detail-item"><label>{{ 'admin.designRequests.status' | translate }}</label><p-tag [value]="selectedRequest.status" [severity]="getStatusSeverity(selectedRequest.status)"></p-tag></div>
                    <div class="detail-item"><label>{{ 'design.wizard.titleLabel' | translate }}</label><span>{{ selectedRequest.title }}</span></div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.description"><label>{{ 'design.wizard.descriptionLabel' | translate }}</label><p class="notes-text">{{ selectedRequest.description }}</p></div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.customerName || selectedRequest.customerEmail">
                        <label>{{ 'admin.designRequests.customer' | translate }}</label>
                        <div class="customer-details">
                            <span *ngIf="selectedRequest.customerName"><i class="pi pi-user"></i> {{ selectedRequest.customerName }}</span>
                            <span *ngIf="selectedRequest.customerEmail"><i class="pi pi-envelope"></i> {{ selectedRequest.customerEmail }}</span>
                            <span *ngIf="selectedRequest.customerPhone"><i class="pi pi-phone"></i> {{ selectedRequest.customerPhone }}</span>
                        </div>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.adminNotes">
                        <label>{{ 'admin.designRequests.adminNotes' | translate }}</label>
                        <p class="notes-text">{{ selectedRequest.adminNotes }}</p>
                    </div>
                    <div class="detail-item" *ngIf="selectedRequest.quotedPrice != null"><label>{{ 'admin.designRequests.quotedPrice' | translate }}</label><span class="price">{{ selectedRequest.quotedPrice | number:'1.2-2' }} ILS</span></div>
                    <div class="detail-item" *ngIf="selectedRequest.finalPrice != null"><label>{{ 'admin.designRequests.finalPrice' | translate }}</label><span class="price final">{{ selectedRequest.finalPrice | number:'1.2-2' }} ILS</span></div>
                    <div class="detail-item"><label>{{ 'admin.designRequests.created' | translate }}</label><span>{{ formatDate(selectedRequest.createdAt) }}</span></div>
                    <div class="detail-item full-width" *ngIf="(selectedRequest.attachments?.length ?? 0) > 0">
                        <label>{{ 'admin.designRequests.attachments' | translate }}</label>
                        <div class="attachment-list">
                            @for (att of selectedRequest.attachments; track att.id) {
                                <div class="attachment-row">
                                    <i class="pi pi-file"></i>
                                    <span class="att-name">{{ att.fileName }}</span>
                                    <span class="att-size">{{ formatFileSize(att.fileSizeBytes) }}</span>
                                    <p-button icon="pi pi-download" [rounded]="true" [text]="true" size="small" (onClick)="downloadAttachment(selectedRequest.id, att.id, att.fileName)"></p-button>
                                </div>
                            }
                        </div>
                        <p-button icon="pi pi-download" [label]="'admin.designRequests.downloadAll' | translate" (onClick)="downloadAll(selectedRequest)" [outlined]="true" size="small" class="mt-2"></p-button>
                    </div>
                </div>
            </div>
        </p-dialog>

        <!-- Edit Status / Admin Dialog -->
        <p-dialog
            [(visible)]="showEditDialog"
            [modal]="true"
            [style]="{ width: '500px', maxWidth: '95vw' }"
            [header]="editDialogTitle"
            [draggable]="false"
            [resizable]="false">
            <div class="edit-content" *ngIf="selectedRequest">
                <div class="form-field">
                    <label>{{ 'admin.designRequests.status' | translate }}</label>
                    <p-select [(ngModel)]="editStatus" [options]="statusOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.designRequests.quotedPrice' | translate }}</label>
                    <p-inputNumber [(ngModel)]="editQuotedPrice" mode="decimal" [minFractionDigits]="2" [maxFractionDigits]="2" styleClass="w-full" placeholder="0.00"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.designRequests.finalPrice' | translate }}</label>
                    <p-inputNumber [(ngModel)]="editFinalPrice" mode="decimal" [minFractionDigits]="2" [maxFractionDigits]="2" styleClass="w-full" placeholder="0.00"></p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.designRequests.adminNotes' | translate }}</label>
                    <textarea pTextarea [(ngModel)]="editAdminNotes" rows="4" class="w-full" [placeholder]="'admin.designRequests.adminNotesPlaceholder' | translate"></textarea>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.designRequests.deliveryNotes' | translate }}</label>
                    <textarea pTextarea [(ngModel)]="editDeliveryNotes" rows="2" class="w-full" [placeholder]="'admin.designRequests.deliveryNotesPlaceholder' | translate"></textarea>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" (onClick)="showEditDialog = false" [outlined]="true"></p-button>
                <p-button [label]="'common.save' | translate" (onClick)="saveRequest()" [loading]="saving"></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .design-requests-container { padding: 1.5rem; }
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
        .customer-cell { display: flex; flex-direction: column; gap: 0.125rem; }
        .customer-cell .email { font-size: 0.8rem; color: #64748b; }
        .action-buttons { display: flex; gap: 0.25rem; }
        .empty-message { text-align: center; padding: 3rem; color: #94a3b8; }
        .empty-message i { font-size: 2rem; display: block; margin-bottom: 0.5rem; }
        .detail-content { padding: 0.5rem 0; }
        .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        .detail-item { display: flex; flex-direction: column; gap: 0.25rem; }
        .detail-item.full-width { grid-column: span 2; }
        .detail-item label { font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
        .notes-text { margin: 0; color: #475569; line-height: 1.5; white-space: pre-wrap; }
        .customer-details { display: flex; flex-direction: column; gap: 0.25rem; }
        .customer-details span { display: flex; align-items: center; gap: 0.5rem; }
        .price { font-weight: 600; color: #667eea; }
        .price.final { color: #764ba2; }
        .attachment-list { display: flex; flex-direction: column; gap: 0.5rem; }
        .attachment-row { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem; background: #f8fafc; border-radius: 8px; }
        .attachment-row .att-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .attachment-row .att-size { font-size: 0.8rem; color: #64748b; }
        .mt-2 { margin-top: 0.5rem; }
        .edit-content { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
        .w-full { width: 100%; }
    `]
})
export class DesignRequestsListComponent implements OnInit {
    private designService = inject(DesignRequestService);
    private http = inject(HttpClient);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    languageService = inject(LanguageService);

    requests: DesignServiceRequestDto[] = [];
    loading = true;
    saving = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedStatus = '';
    selectedType = '';
    searchTerm = '';

    statusOptions = DESIGN_STATUS_OPTIONS;
    typeOptions = DESIGN_REQUEST_TYPES.map(t => ({ value: t, label: getTypeLabelStatic(t) }));

    showViewDialog = false;
    showEditDialog = false;
    selectedRequest: DesignServiceRequestDto | null = null;

    editStatus = 'New';
    editQuotedPrice: number | null = null;
    editFinalPrice: number | null = null;
    editAdminNotes = '';
    editDeliveryNotes = '';

    get viewDialogTitle(): string {
        return this.translate.instant('admin.designRequests.viewDetails');
    }
    get editDialogTitle(): string {
        return this.translate.instant('admin.designRequests.updateRequest');
    }

    ngOnInit() {
        this.loadRequests();
    }

    loadRequests() {
        this.loading = true;
        this.designService.getMyRequests({
            page: this.currentPage,
            pageSize: this.pageSize,
            status: this.selectedStatus || undefined,
            requestType: this.selectedType || undefined,
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

    viewRequest(req: DesignServiceRequestDto) {
        this.selectedRequest = req;
        if ((req.attachments?.length ?? 0) === 0 && (req.attachmentCount ?? 0) > 0) {
            this.designService.getById(req.id).subscribe({
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

    editRequest(req: DesignServiceRequestDto) {
        this.selectedRequest = req;
        this.editStatus = req.status;
        this.editQuotedPrice = req.quotedPrice ?? null;
        this.editFinalPrice = req.finalPrice ?? null;
        this.editAdminNotes = req.adminNotes ?? '';
        this.editDeliveryNotes = req.deliveryNotes ?? '';
        this.showEditDialog = true;
    }

    saveRequest() {
        if (!this.selectedRequest) return;
        this.saving = true;
        this.designService.updateStatus(this.selectedRequest.id, { status: this.editStatus }).subscribe({
            next: () => {
                this.designService.updateAdmin(this.selectedRequest!.id, {
                    adminNotes: this.editAdminNotes || undefined,
                    quotedPrice: this.editQuotedPrice ?? undefined,
                    finalPrice: this.editFinalPrice ?? undefined,
                    deliveryNotes: this.editDeliveryNotes || undefined
                }).subscribe({
                    next: () => {
                        this.saving = false;
                        this.showEditDialog = false;
                        this.loadRequests();
                        if (this.showViewDialog && this.selectedRequest) {
                            this.designService.getById(this.selectedRequest.id).subscribe(r => this.selectedRequest = r);
                        }
                        this.messageService.add({ severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('messages.updated') });
                    },
                    error: () => { this.saving = false; this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('messages.saveError') }); }
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('messages.saveError') });
            }
        });
    }

    downloadAttachment(requestId: string, attachmentId: string, fileName: string) {
        const url = this.designService.getAttachmentDownloadUrl(requestId, attachmentId);
        this.http.get(url, { responseType: 'blob' }).subscribe({
            next: (blob) => {
                const u = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = u;
                a.download = fileName || 'attachment';
                a.click();
                window.URL.revokeObjectURL(u);
            },
            error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: 'Download failed' })
        });
    }

    downloadAll(req: DesignServiceRequestDto) {
        const url = this.designService.getDownloadAllUrl(req.id);
        this.http.get(url, { responseType: 'blob' }).subscribe({
            next: (blob) => {
                const u = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = u;
                a.download = `design-request-${req.referenceNumber}-attachments.zip`;
                a.click();
                window.URL.revokeObjectURL(u);
            },
            error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: 'Download failed' })
        });
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
        return getTypeLabelStatic(type);
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

function getTypeLabelStatic(type: string): string {
    const labels: Record<string, string> = {
        IdeaOnly: 'Idea Only',
        BrokenDesign: 'Idea + Broken Design',
        PhysicalObject: 'Physical Object',
        ExistingCAD: 'Existing CAD',
        TechnicalDrawings: 'Technical Drawings',
        ImproveExistingProduct: 'Improve Existing'
    };
    return labels[type] ?? type;
}
