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
import { ImageModule } from 'primeng/image';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LaserApiService } from '../../../shared/services/laser-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { ServiceRequestStatusBadgeComponent } from '../../../shared/components/service-request-status-badge/service-request-status-badge.component';
import { LaserServiceRequest, LaserServiceRequestStatus } from '../../../shared/models/laser.model';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import {
    ServiceWorkflowStatus,
    denormalizeLaserWorkflowStatus,
    getWorkflowOptions,
    normalizeLaserWorkflowStatus
} from '../../../shared/utils/service-request-workflow';

@Component({
    selector: 'app-laser-requests-list',
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
        ImageModule,
        ProgressSpinnerModule,
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
                        <h1>{{ 'admin.laserRequests.title' | translate }}</h1>
                        <p class="subtitle">{{ 'admin.laserRequests.subtitle' | translate }}</p>
                    </div>
                </div>
            </div>

            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.laserRequests.filterByStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStatus"
                            [options]="statusOptions"
                            [placeholder]="'admin.laserRequests.allStatuses' | translate"
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
                            <th>{{ 'admin.laserRequests.referenceNumber' | translate }}</th>
                            <th>{{ 'admin.laserRequests.material' | translate }}</th>
                            <th>{{ 'admin.laserRequests.mode' | translate }}</th>
                            <th>{{ 'admin.laserRequests.customer' | translate }}</th>
                            <th>{{ 'admin.laserRequests.status' | translate }}</th>
                            <th>{{ 'admin.laserRequests.createdAt' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    
                    <ng-template pTemplate="body" let-request>
                        <tr>
                            <td>
                                <span class="ref-number">{{ request.referenceNumber }}</span>
                            </td>
                            <td>{{ getMaterialName(request) }}</td>
                            <td>
                                <p-tag 
                                    [value]="request.operationMode === 'cut' ? 'Cut' : 'Engrave'"
                                    [severity]="request.operationMode === 'cut' ? 'danger' : 'success'">
                                </p-tag>
                            </td>
                            <td>
                                <div class="customer-info">
                                    <span *ngIf="request.customerName">{{ request.customerName }}</span>
                                    <span *ngIf="request.customerEmail" class="customer-email">{{ request.customerEmail }}</span>
                                    <span *ngIf="!request.customerName && !request.customerEmail" class="no-info">-</span>
                                </div>
                            </td>
                            <td>
                                <app-service-request-status-badge module="laser" [status]="request.workflowStatus || request.status"></app-service-request-status-badge>
                            </td>
                            <td>{{ formatDate(request.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button 
                                        icon="pi pi-eye" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="viewRequest(request)"
                                        [pTooltip]="'admin.laserRequests.viewDetails' | translate">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-pencil" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="editRequest(request)"
                                        [pTooltip]="'admin.serviceWorkflow.manageWorkflow' | translate">
                                    </p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="7" class="empty-message">
                                <i class="pi pi-inbox"></i>
                                <span>{{ 'admin.laserRequests.noRequests' | translate }}</span>
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
            [style]="{ width: '600px', maxWidth: '95vw' }"
            [header]="'admin.laserRequests.viewDetails' | translate">
            <div class="view-content" *ngIf="selectedRequest">
                <div class="detail-grid">
                    <div class="detail-item">
                        <label>{{ 'admin.laserRequests.referenceNumber' | translate }}</label>
                        <span class="ref-number">{{ selectedRequest.referenceNumber }}</span>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.laserRequests.status' | translate }}</label>
                        <app-service-request-status-badge module="laser" [status]="selectedRequest.workflowStatus || selectedRequest.status"></app-service-request-status-badge>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.laserRequests.material' | translate }}</label>
                        <span>{{ getMaterialName(selectedRequest) }}</span>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.laserRequests.mode' | translate }}</label>
                        <span>{{ selectedRequest.operationMode === 'cut' ? 'Cut' : 'Engrave' }}</span>
                    </div>
                    <div class="detail-item" *ngIf="selectedRequest.widthCm || selectedRequest.heightCm">
                        <label>{{ 'admin.laserRequests.dimensions' | translate }}</label>
                        <span class="dimensions">{{ selectedRequest.widthCm || '-' }} × {{ selectedRequest.heightCm || '-' }} cm</span>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.customerName || selectedRequest.customerEmail || selectedRequest.customerPhone">
                        <label>{{ 'admin.laserRequests.customer' | translate }}</label>
                        <div class="customer-details">
                            <span *ngIf="selectedRequest.customerName"><i class="pi pi-user"></i> {{ selectedRequest.customerName }}</span>
                            <span *ngIf="selectedRequest.customerEmail"><i class="pi pi-envelope"></i> {{ selectedRequest.customerEmail }}</span>
                            <span *ngIf="selectedRequest.customerPhone"><i class="pi pi-phone"></i> {{ selectedRequest.customerPhone }}</span>
                        </div>
                    </div>
                    <div class="detail-item full-width" *ngIf="selectedRequest.customerNotes">
                        <label>{{ 'admin.laserRequests.customerNotes' | translate }}</label>
                        <p class="notes-text">{{ selectedRequest.customerNotes }}</p>
                    </div>
                    <div class="detail-item full-width">
                        <label>{{ 'admin.laserRequests.uploadedImage' | translate }}</label>
                        <div class="image-preview-container">
                            <div *ngIf="loadingImage" class="image-loading">
                                <p-progressSpinner [style]="{width: '30px', height: '30px'}"></p-progressSpinner>
                                <span>Loading image...</span>
                            </div>
                            <div *ngIf="imageError" class="image-error">
                                <i class="pi pi-exclamation-triangle"></i>
                                <span>Failed to load image</span>
                                <p-button label="Retry" icon="pi pi-refresh" [text]="true" (onClick)="loadRequestImage(selectedRequest.id)"></p-button>
                            </div>
                            <div *ngIf="imageUrl && !loadingImage && !imageError" class="image-display">
                                <img [src]="imageUrl" [alt]="selectedRequest.imageFileName" class="request-image" />
                                <div class="image-filename">{{ selectedRequest.imageFileName }}</div>
                            </div>
                        </div>
                    </div>
                    <div class="detail-item">
                        <label>{{ 'admin.laserRequests.createdAt' | translate }}</label>
                        <span>{{ formatDate(selectedRequest.createdAt) }}</span>
                    </div>
                    <div class="detail-item" *ngIf="selectedRequest.quotedPrice">
                        <label>{{ 'admin.laserRequests.quotedPrice' | translate }}</label>
                        <span class="price">{{ selectedRequest.quotedPrice | currency:'ILS' }}</span>
                    </div>
                </div>
            </div>
        </p-dialog>

        <!-- Edit Dialog -->
        <p-dialog 
            [(visible)]="showEditDialog" 
            [modal]="true"
            [style]="{ width: '500px', maxWidth: '95vw' }"
            [header]="'admin.laserRequests.updateRequest' | translate">
            <div class="edit-content" *ngIf="selectedRequest">
                <div class="form-field">
                    <label>{{ 'admin.laserRequests.status' | translate }}</label>
                    <p-select
                        [(ngModel)]="editStatus"
                        [options]="allStatusOptions"
                        optionLabel="label"
                        optionValue="value"
                        styleClass="w-full">
                    </p-select>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.laserRequests.quotedPrice' | translate }}</label>
                    <p-inputNumber 
                        [(ngModel)]="editQuotedPrice"
                        mode="currency"
                        currency="ILS"
                        locale="en-IL"
                        styleClass="w-full">
                    </p-inputNumber>
                </div>
                <div class="form-field">
                    <label>{{ 'admin.laserRequests.adminNotes' | translate }}</label>
                    <textarea 
                        pTextarea
                        [(ngModel)]="editAdminNotes"
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

        .ref-number { font-family: monospace; font-weight: 600; color: #667eea; }
        
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
        .image-preview-container { margin-top: 0.5rem; }
        .image-loading, .image-error {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem;
            background: #f8fafc;
            border-radius: 8px;
            color: #64748b;
        }
        .image-error { color: #dc2626; }
        .image-error i { font-size: 1.25rem; }
        .image-display { text-align: center; }
        .request-image {
            max-width: 100%;
            max-height: 300px;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            object-fit: contain;
        }
        .image-filename {
            margin-top: 0.5rem;
            font-size: 0.8rem;
            color: #64748b;
        }
        .price { font-weight: 600; color: #667eea; }
        .dimensions { font-family: monospace; font-weight: 600; color: #667eea; }

        .edit-content { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
    `]
})
export class LaserRequestsListComponent implements OnInit {
    private laserApi = inject(LaserApiService);
    private http = inject(HttpClient);
    private sanitizer = inject(DomSanitizer);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    languageService = inject(LanguageService);

    requests: LaserServiceRequest[] = [];
    loading = true;
    saving = false;

    imageUrl: SafeUrl | null = null;
    loadingImage = false;
    imageError = false;

    selectedStatus: ServiceWorkflowStatus | null = null;
    statusOptions: { value: ServiceWorkflowStatus; label: string }[] = [];
    allStatusOptions = this.statusOptions;

    showViewDialog = false;
    showEditDialog = false;
    selectedRequest: LaserServiceRequest | null = null;

    editStatus: ServiceWorkflowStatus = 'New';
    editQuotedPrice: number | null = null;
    editAdminNotes = '';

    ngOnInit() {
        this.rebuildStatusOptions();
        this.translate.onLangChange.subscribe(() => this.rebuildStatusOptions());
        this.loadRequests();
    }

    rebuildStatusOptions() {
        this.statusOptions = getWorkflowOptions('laser', this.translate);
        this.allStatusOptions = this.statusOptions;
    }

    loadRequests() {
        this.loading = true;
        this.laserApi.getServiceRequests({ 
            status: this.selectedStatus ? denormalizeLaserWorkflowStatus(this.selectedStatus) : undefined 
        }).subscribe({
            next: (requests) => {
                this.requests = requests;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to load requests'
                });
            }
        });
    }

    getMaterialName(request: LaserServiceRequest): string {
        return this.languageService.language === 'ar' && request.materialNameAr 
            ? request.materialNameAr 
            : request.materialNameEn;
    }

    formatDate(dateStr: string): string {
        return new Date(dateStr).toLocaleDateString(
            this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL',
            { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }
        );
    }

    getImageUrl(id: string): string {
        return this.laserApi.getServiceRequestImageUrl(id);
    }

    viewRequest(request: LaserServiceRequest) {
        this.selectedRequest = request;
        this.showViewDialog = true;
        this.loadRequestImage(request.id);
    }

    loadRequestImage(requestId: string) {
        this.loadingImage = true;
        this.imageError = false;
        this.imageUrl = null;

        const url = this.laserApi.getServiceRequestImageUrl(requestId);
        this.http.get(url, { responseType: 'blob' }).subscribe({
            next: (blob) => {
                const objectUrl = URL.createObjectURL(blob);
                this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
                this.loadingImage = false;
            },
            error: () => {
                this.loadingImage = false;
                this.imageError = true;
            }
        });
    }

    editRequest(request: LaserServiceRequest) {
        this.selectedRequest = request;
        this.editStatus = normalizeLaserWorkflowStatus(request.workflowStatus || request.status);
        this.editQuotedPrice = request.quotedPrice || null;
        this.editAdminNotes = request.adminNotes || '';
        this.showEditDialog = true;
    }

    saveRequest() {
        if (!this.selectedRequest) return;

        this.saving = true;
        this.laserApi.updateServiceRequest(this.selectedRequest.id, {
            status: denormalizeLaserWorkflowStatus(this.editStatus),
            quotedPrice: this.editQuotedPrice || undefined,
            adminNotes: this.editAdminNotes || undefined
        }).subscribe({
            next: () => {
                this.saving = false;
                this.showEditDialog = false;
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Request updated successfully'
                });
                this.loadRequests();
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update request'
                });
            }
        });
    }
}
