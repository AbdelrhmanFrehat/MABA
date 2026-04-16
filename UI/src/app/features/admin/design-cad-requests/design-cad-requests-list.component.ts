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
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DesignCadRequestService } from '../../../shared/services/design-cad-request.service';
import { LanguageService } from '../../../shared/services/language.service';
import { ServiceRequestStatusBadgeComponent } from '../../../shared/components/service-request-status-badge/service-request-status-badge.component';
import type { DesignCadRequestDto } from '../../../shared/models/design-cad-request.model';
import {
    ServiceWorkflowStatus,
    denormalizeDesignCadWorkflowStatus,
    getWorkflowOptions,
    getWorkflowTimeline,
    normalizeDesignCadWorkflowStatus
} from '../../../shared/utils/service-request-workflow';

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
        TextareaModule,
        TranslateModule,
        ServiceRequestStatusBadgeComponent
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
                            <td><app-service-request-status-badge module="designCad" [status]="req.workflowStatus || req.status"></app-service-request-status-badge></td>
                            <td>{{ formatDate(req.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button icon="pi pi-eye" [rounded]="true" [text]="true" (onClick)="viewRequest(req)" pTooltip="{{ 'admin.cadRequests.view' | translate }}"></p-button>
                                    <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" (onClick)="editRequest(req)" pTooltip="{{ 'admin.serviceWorkflow.manageWorkflow' | translate }}"></p-button>
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
                    <div class="detail-item"><label>{{ 'admin.cadRequests.status' | translate }}</label><app-service-request-status-badge module="designCad" [status]="selectedRequest.workflowStatus || selectedRequest.status"></app-service-request-status-badge></div>
                    <div class="detail-item full-width"><label>{{ 'admin.cadRequests.requestTitle' | translate }}</label><span>{{ selectedRequest.title }}</span></div>
                    <div class="detail-item full-width workflow-strip">
                        <label>{{ 'admin.serviceWorkflow.progress' | translate }}</label>
                        <div class="workflow-timeline">
                            <div class="workflow-step" *ngFor="let step of buildTimeline(selectedRequest)">
                                <span class="workflow-dot" [class.done]="step.done"></span>
                                <span>{{ step.labelKey | translate }}</span>
                                <small *ngIf="step.at">{{ formatDate(step.at) }}</small>
                            </div>
                        </div>
                    </div>
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
                                        <button class="att-download-btn" (click)="downloadAttachment(att)" pTooltip="Download" tooltipPosition="top">
                                            <i class="pi pi-download"></i>
                                        </button>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
            </div>
        </p-dialog>

        <p-dialog
            [(visible)]="showEditDialog"
            [modal]="true"
            [style]="{ width: '480px', maxWidth: '95vw' }"
            [header]="'admin.serviceWorkflow.manageWorkflow' | translate"
            [draggable]="false"
            [resizable]="false">
            <div class="edit-content" *ngIf="selectedRequest">
                <div class="form-field">
                    <label>{{ 'admin.serviceWorkflow.nextStatus' | translate }}</label>
                    <p-select [(ngModel)]="editStatus" [options]="statusOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                </div>
                <div class="form-field" *ngIf="editStatus === 'Rejected'">
                    <label class="rejection-label">{{ 'admin.requests.rejectionReason' | translate }} <span class="required-star">*</span></label>
                    <textarea pTextarea [(ngModel)]="editRejectionReason" rows="4" class="w-full"
                        [placeholder]="'admin.requests.rejectionReasonPlaceholder' | translate"></textarea>
                    <small class="hint-text">{{ 'admin.requests.rejectionReasonHint' | translate }}</small>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.designRequests.adminNotes' | translate }}</label>
                    <textarea pTextarea [(ngModel)]="editNotes" rows="4" class="w-full" [placeholder]="'admin.designRequests.adminNotesPlaceholder' | translate"></textarea>
                </div>
                <div class="notification-note" *ngIf="editStatus !== (selectedRequest?.workflowStatus || selectedRequest?.status)">
                    <i class="pi pi-envelope"></i>
                    <span>{{ 'admin.requests.customerWillBeNotified' | translate }}</span>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" (onClick)="showEditDialog = false" [outlined]="true"></p-button>
                <p-button [label]="'common.save' | translate" (onClick)="saveRequest()" [loading]="saving"></p-button>
            </ng-template>
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
        .workflow-strip { padding-top: 0.25rem; border-top: 1px solid #e2e8f0; }
        .workflow-timeline { display: flex; flex-wrap: wrap; gap: 0.75rem 1rem; }
        .workflow-step { display: inline-flex; align-items: center; gap: 0.45rem; color: #475569; }
        .workflow-step small { color: #94a3b8; }
        .workflow-dot { width: 0.6rem; height: 0.6rem; border-radius: 999px; background: #cbd5e1; }
        .workflow-dot.done { background: #0f766e; }
        .attachment-list { display: flex; flex-direction: column; gap: 0.5rem; }
        .attachment-row { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem; background: #f8fafc; border-radius: 8px; }
        .attachment-row .att-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .attachment-row .att-size { font-size: 0.8rem; color: #64748b; }
        .att-download-btn { margin-left: auto; display: flex; align-items: center; justify-content: center; width: 28px; height: 28px; border-radius: 6px; border: 1px solid #e2e8f0; background: #fff; color: #667eea; cursor: pointer; transition: background 0.15s, border-color 0.15s; flex-shrink: 0; }
        .att-download-btn:hover { background: #eef2ff; border-color: #c7d2fe; }
        .att-download-btn .pi { font-size: 0.8rem; }
        .edit-content { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
        .rejection-label { color: #dc2626 !important; }
        .required-star { color: #dc2626; margin-left: 2px; }
        .hint-text { color: #64748b; font-size: 0.8rem; display: block; margin-top: 0.25rem; }
        .notification-note { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0.75rem; background: #eff6ff; border-radius: 6px; color: #1e40af; font-size: 0.8rem; margin-top: 0.5rem; }
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

    statusOptions: { value: ServiceWorkflowStatus; label: string }[] = [];

    showViewDialog = false;
    showEditDialog = false;
    selectedRequest: DesignCadRequestDto | null = null;
    editStatus: ServiceWorkflowStatus = 'New';
    editNotes = '';
    editRejectionReason = '';
    saving = false;

    get viewDialogTitle(): string {
        return this.translate.instant('admin.cadRequests.viewDetails');
    }

    ngOnInit() {
        this.rebuildStatusOptions();
        this.translate.onLangChange.subscribe(() => this.rebuildStatusOptions());
        this.loadRequests();
    }

    rebuildStatusOptions() {
        this.statusOptions = getWorkflowOptions('designCad', this.translate);
    }

    loadRequests() {
        this.loading = true;
        this.designCadService.getList({
            page: this.currentPage,
            pageSize: this.pageSize,
            status: this.selectedStatus ? denormalizeDesignCadWorkflowStatus(this.selectedStatus as ServiceWorkflowStatus) : undefined,
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

    editRequest(req: DesignCadRequestDto) {
        this.selectedRequest = req;
        this.editStatus = normalizeDesignCadWorkflowStatus(req.workflowStatus || req.status);
        this.editNotes = '';
        this.editRejectionReason = '';
        this.showEditDialog = true;
    }

    saveRequest() {
        if (!this.selectedRequest) return;
        if (this.editStatus === 'Rejected' && !this.editRejectionReason.trim()) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('messages.validationError'), detail: this.translate.instant('admin.requests.rejectionReasonRequired') });
            return;
        }
        this.saving = true;
        this.designCadService.updateStatus(this.selectedRequest.id, {
            status: denormalizeDesignCadWorkflowStatus(this.editStatus),
            notes: this.editNotes || undefined,
            rejectionReason: this.editStatus === 'Rejected' ? this.editRejectionReason.trim() : undefined
        }).subscribe({
            next: (updated) => {
                this.saving = false;
                this.showEditDialog = false;
                this.selectedRequest = updated;
                this.loadRequests();
                this.messageService.add({ severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('messages.updated') });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('messages.saveError') });
            }
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
        const labelsEn: Record<string, string> = {
            IdeaOnly: 'Idea Only',
            ExistingFiles: 'Existing CAD Files',
            ReverseEngineering: 'Reverse Engineering',
            PhysicalItem: 'Physical Item',
            ModifyProduct: 'Modify Product',
            MechanicalAssembly: 'Mechanical Assembly'
        };
        const labelsAr: Record<string, string> = {
            IdeaOnly: 'فكرة فقط',
            ExistingFiles: 'ملفات CAD جاهزة',
            ReverseEngineering: 'هندسة عكسية',
            PhysicalItem: 'قطعة فعلية',
            ModifyProduct: 'تعديل منتج',
            MechanicalAssembly: 'تجميع ميكانيكي'
        };
        return (this.languageService.language === 'ar' ? labelsAr : labelsEn)[type] ?? type;
    }

    downloadAttachment(att: { id: string; fileName: string; fileSizeBytes: number; uploadedAt: string }) {
        if (!this.selectedRequest) return;
        this.designCadService.downloadAttachment(this.selectedRequest.id, att.id).subscribe({
            next: (blob) => {
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = att.fileName;
                a.click();
                URL.revokeObjectURL(url);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('messages.loadError') });
            }
        });
    }

    buildTimeline(req: DesignCadRequestDto) {
        return getWorkflowTimeline(normalizeDesignCadWorkflowStatus(req.workflowStatus || req.status), {
            createdAt: req.createdAt,
            reviewedAt: req.reviewedAt,
            approvedAt: undefined,
            completedAt: req.completedAt
        });
    }
}
