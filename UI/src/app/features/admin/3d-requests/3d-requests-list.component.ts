import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Print3dRequest, Print3dRequestStatus } from '../../../shared/models/printing.model';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-3d-requests-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
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
        <div class="requests-list-container">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.3dRequests.title' | translate }}</h1>
                        <p class="subtitle">{{ 'admin.3dRequests.subtitle' | translate }}</p>
                    </div>
                </div>
            </div>

            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.3dRequests.filterByStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStatus"
                            [options]="statusOptions"
                            [placeholder]="'admin.3dRequests.allStatuses' | translate"
                            (onChange)="loadRequests()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-actions">
                        <p-button 
                            icon="pi pi-refresh" 
                            [label]="'common.refresh' | translate"
                            (onClick)="loadRequests()"
                            [outlined]="true">
                        </p-button>
                    </div>
                </div>
            </p-card>

            <p-card class="table-card">
                <p-table 
                    [value]="requests" 
                    [loading]="loading"
                    [paginator]="true"
                    [rows]="10"
                    [showCurrentPageReport]="true"
                    [rowsPerPageOptions]="[10, 25, 50]"
                    styleClass="p-datatable-striped">
                    
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.3dRequests.referenceId' | translate }}</th>
                            <th>{{ 'admin.3dRequests.material' | translate }}</th>
                            <th>{{ 'admin.3dRequests.profile' | translate }}</th>
                            <th>{{ 'admin.3dRequests.customer' | translate }}</th>
                            <th>{{ 'admin.3dRequests.status' | translate }}</th>
                            <th>{{ 'admin.3dRequests.createdAt' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    
                    <ng-template pTemplate="body" let-request>
                        <tr>
                            <td>
                                <span class="ref-number">{{ request.referenceNumber }}</span>
                            </td>
                            <td>{{ request.materialName || '-' }}</td>
                            <td>{{ request.profileName || '-' }}</td>
                            <td>
                                <div class="customer-info">
                                    <span *ngIf="request.customerName">{{ request.customerName }}</span>
                                    <span *ngIf="request.customerEmail" class="customer-email">{{ request.customerEmail }}</span>
                                    <span *ngIf="!request.customerName && !request.customerEmail" class="no-info">-</span>
                                </div>
                            </td>
                            <td>
                                <p-tag 
                                    [value]="request.status"
                                    [severity]="getStatusSeverity(request.status)">
                                </p-tag>
                            </td>
                            <td>{{ formatDate(request.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button 
                                        icon="pi pi-eye" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="viewRequest(request)"
                                        pTooltip="View Details">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-pencil" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="editRequest(request)"
                                        pTooltip="Update Status">
                                    </p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="7" class="empty-message">
                                <i class="pi pi-inbox"></i>
                                <span>{{ 'admin.3dRequests.noRequests' | translate }}</span>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>

        <!-- View Dialog -->
        <p-dialog 
            [(visible)]="showViewDialog" 
            [modal]="true"
            [style]="{ width: '650px', maxWidth: '95vw' }"
            [header]="viewDialogHeader">
            <div class="view-content" *ngIf="selectedRequest">
                <div class="detail-grid">
                    <div class="detail-item">
                        <label>{{ 'admin.3dRequests.referenceId' | translate }}</label>
                        <span class="ref-number">{{ selectedRequest.referenceNumber }}</span>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.3dRequests.status' | translate }}</label>
                        <p-tag [value]="selectedRequest.status" [severity]="getStatusSeverity(selectedRequest.status)"></p-tag>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.3dRequests.material' | translate }}</label>
                        <span>{{ selectedRequest.materialName || '-' }}</span>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.3dRequests.profile' | translate }}</label>
                        <span>{{ selectedRequest.profileName || '-' }}</span>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.customerName || selectedRequest.customerEmail">
                        <label>{{ 'admin.3dRequests.customer' | translate }}</label>
                        <div class="customer-details">
                            <span *ngIf="selectedRequest.customerName"><i class="pi pi-user"></i> {{ selectedRequest.customerName }}</span>
                            <span *ngIf="selectedRequest.customerEmail"><i class="pi pi-envelope"></i> {{ selectedRequest.customerEmail }}</span>
                        </div>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.customerNotes">
                        <label>{{ 'admin.3dRequests.comments' | translate }}</label>
                        <p class="notes-text">{{ selectedRequest.customerNotes }}</p>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.fileName">
                        <label>{{ 'admin.3dRequests.uploadedFile' | translate }}</label>
                        <div class="file-preview">
                            <div class="file-info-box">
                                <i class="pi pi-file"></i>
                                <div class="file-details">
                                    <span class="file-name">{{ selectedRequest.fileName }}</span>
                                    <span class="file-meta">{{ formatFileSize(selectedRequest.fileSizeBytes) }}</span>
                                </div>
                                <p-button 
                                    icon="pi pi-download" 
                                    [label]="'Download'"
                                    (onClick)="downloadFile(selectedRequest)"
                                    [outlined]="true"
                                    size="small">
                                </p-button>
                            </div>
                        </div>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.3dRequests.createdAt' | translate }}</label>
                        <span>{{ formatDate(selectedRequest.createdAt) }}</span>
                    </div>
                    <div class="detail-item" *ngIf="selectedRequest.estimatedPrice">
                        <label>{{ 'admin.3dRequests.estimatedPrice' | translate }}</label>
                        <span class="price">{{ selectedRequest.estimatedPrice | currency:'ILS' }}</span>
                    </div>
                    <div class="detail-item" *ngIf="selectedRequest.finalPrice">
                        <label>{{ 'admin.3dRequests.finalPrice' | translate }}</label>
                        <span class="price final">{{ selectedRequest.finalPrice | currency:'ILS' }}</span>
                    </div>
                </div>
            </div>
        </p-dialog>

        <!-- Edit Dialog -->
        <p-dialog 
            [(visible)]="showEditDialog" 
            [modal]="true"
            [style]="{ width: '500px', maxWidth: '95vw' }"
            [header]="editDialogHeader">
            <div class="edit-content" *ngIf="selectedRequest">
                <div class="form-field">
                    <label>{{ 'admin.3dRequests.status' | translate }}</label>
                    <p-select
                        [(ngModel)]="editStatus"
                        [options]="allStatusOptions"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-full">
                    </p-select>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.3dRequests.finalPrice' | translate }}</label>
                    <p-inputNumber 
                        [(ngModel)]="editFinalPrice"
                        mode="currency"
                        currency="ILS"
                        locale="en-IL"
                        styleClass="w-full">
                    </p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.3dRequests.adminNotes' | translate }}</label>
                    <textarea 
                        pTextarea
                        [(ngModel)]="editNotes"
                        rows="3"
                        class="w-full">
                    </textarea>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    (onClick)="showEditDialog = false"
                    [outlined]="true">
                </p-button>
                <p-button 
                    [label]="'common.save' | translate" 
                    (onClick)="saveRequest()"
                    [loading]="saving">
                </p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .requests-list-container { padding: 1.5rem; }
        .page-header { margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; color: #1e293b; }
        .subtitle { margin: 0.25rem 0 0; color: #64748b; font-size: 0.875rem; }
        
        .filters-card { margin-bottom: 1.5rem; }
        .filters-grid { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; }
        .filter-item { min-width: 200px; }
        .filter-label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
        .filter-actions { margin-left: auto; }

        .ref-number { font-family: monospace; font-weight: 600; color: #667eea; font-size: 0.85rem; }
        
        .customer-info { display: flex; flex-direction: column; gap: 0.125rem; }
        .customer-email { font-size: 0.8rem; color: #64748b; }
        .no-info { color: #94a3b8; }
        
        .action-buttons { display: flex; gap: 0.25rem; }
        
        .empty-message {
            text-align: center;
            padding: 3rem;
            color: #94a3b8;
        }
        .empty-message i { font-size: 2rem; display: block; margin-bottom: 0.5rem; }

        .view-content { padding: 0.5rem 0; }
        .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        .detail-item { display: flex; flex-direction: column; gap: 0.25rem; }
        .detail-item.full-width { grid-column: span 2; }
        .detail-item label { font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
        .detail-item span { font-size: 0.95rem; color: #1e293b; }
        .customer-details { display: flex; flex-direction: column; gap: 0.25rem; }
        .customer-details span { display: flex; align-items: center; gap: 0.5rem; }
        .customer-details i { color: #64748b; font-size: 0.875rem; }
        .notes-text { margin: 0; color: #475569; line-height: 1.5; }
        
        .file-preview { margin-top: 0.5rem; }
        .file-info-box {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%);
            border-radius: 10px;
            border: 1px solid rgba(102, 126, 234, 0.2);
        }
        .file-info-box > i {
            font-size: 2rem;
            color: #667eea;
        }
        .file-details {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            flex: 1;
        }
        .file-name {
            font-weight: 600;
            color: #1e293b;
        }
        .file-meta {
            font-size: 0.8rem;
            color: #64748b;
        }
        
        .price { font-weight: 600; color: #667eea; }
        .price.final { color: #764ba2; }

        .edit-content { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
    `]
})
export class ThreeDRequestsListComponent implements OnInit {
    private printingApiService = inject(PrintingApiService);
    private http = inject(HttpClient);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    languageService = inject(LanguageService);

    requests: Print3dRequest[] = [];
    loading = true;
    saving = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;

    selectedStatus: string = '';
    statusOptions = [
        { label: 'Pending', value: Print3dRequestStatus.Pending },
        { label: 'Under Review', value: Print3dRequestStatus.UnderReview },
        { label: 'Quoted', value: Print3dRequestStatus.Quoted },
        { label: 'Approved', value: Print3dRequestStatus.Approved },
        { label: 'Rejected', value: Print3dRequestStatus.Rejected },
        { label: 'Queued', value: Print3dRequestStatus.Queued },
        { label: 'Slicing', value: Print3dRequestStatus.Slicing },
        { label: 'Printing', value: Print3dRequestStatus.Printing },
        { label: 'Completed', value: Print3dRequestStatus.Completed },
        { label: 'Failed', value: Print3dRequestStatus.Failed },
        { label: 'Cancelled', value: Print3dRequestStatus.Cancelled }
    ];
    allStatusOptions = this.statusOptions;

    showViewDialog = false;
    showEditDialog = false;
    selectedRequest: Print3dRequest | null = null;

    editStatus: Print3dRequestStatus = Print3dRequestStatus.Pending;
    editFinalPrice: number | null = null;
    editNotes = '';

    get viewDialogHeader(): string {
        return this.translateService.instant('admin.3dRequests.viewDetails');
    }

    get editDialogHeader(): string {
        return this.translateService.instant('admin.3dRequests.updateRequest');
    }

    ngOnInit() {
        this.loadRequests();
    }

    loadRequests() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedStatus) {
            params.status = this.selectedStatus;
        }

        this.printingApiService.getRequests(params).subscribe({
            next: (response) => {
                this.requests = response.items || [];
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    viewRequest(request: Print3dRequest) {
        this.selectedRequest = request;
        this.showViewDialog = true;
    }

    editRequest(request: Print3dRequest) {
        this.selectedRequest = request;
        this.editStatus = request.status as Print3dRequestStatus;
        this.editFinalPrice = request.finalPrice || null;
        this.editNotes = '';
        this.showEditDialog = true;
    }

    saveRequest() {
        if (!this.selectedRequest) return;
        
        this.saving = true;
        this.printingApiService.updateRequestStatus(this.selectedRequest.id, {
            status: this.editStatus,
            notes: this.editNotes || undefined,
            finalPrice: this.editFinalPrice || undefined
        }).subscribe({
            next: () => {
                this.saving = false;
                this.showEditDialog = false;
                this.loadRequests();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.updated')
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    downloadFile(request: Print3dRequest) {
        this.http.get(`${environment.apiUrl}/3d-requests/${request.id}/file`, {
            responseType: 'blob'
        }).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = request.fileName || 'design.stl';
                link.click();
                window.URL.revokeObjectURL(url);
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: 'Failed to download file'
                });
            }
        });
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '-';
        const date = new Date(dateStr);
        return date.toLocaleDateString(this.languageService.language === 'ar' ? 'ar-EG' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    formatFileSize(bytes: number): string {
        if (!bytes) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    getStatusSeverity(status: Print3dRequestStatus | string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | null | undefined {
        const severityMap: Record<string, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            'Pending': 'warn',
            'UnderReview': 'info',
            'Quoted': 'info',
            'Approved': 'info',
            'Rejected': 'danger',
            'Queued': 'info',
            'Slicing': 'info',
            'Printing': 'info',
            'Completed': 'success',
            'Failed': 'danger',
            'Cancelled': 'secondary'
        };
        return severityMap[status] || 'secondary';
    }
}
