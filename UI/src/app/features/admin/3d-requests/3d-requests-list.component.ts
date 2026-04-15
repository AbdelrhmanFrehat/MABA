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
import { ServiceRequestStatusBadgeComponent } from '../../../shared/components/service-request-status-badge/service-request-status-badge.component';
import {
    FilamentSpool,
    Print3dPricingSuggestionResponse,
    Print3dRequest,
    Print3dRequestStatus
} from '../../../shared/models/printing.model';
import { environment } from '../../../../environments/environment';
import {
    ServiceWorkflowStatus,
    denormalizePrint3dWorkflowStatus,
    getWorkflowOptions,
    normalizePrint3dWorkflowStatus
} from '../../../shared/utils/service-request-workflow';

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
                                <app-service-request-status-badge module="print3d" [status]="request.workflowStatus || request.status"></app-service-request-status-badge>
                            </td>
                            <td>{{ formatDate(request.createdAt) }}</td>
                            <td>
                                <div class="action-buttons">
                                    <p-button 
                                        icon="pi pi-eye" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="viewRequest(request)"
                                        pTooltip="{{ 'admin.3dRequests.viewDetails' | translate }}">
                                    </p-button>
                                    <p-button 
                                        icon="pi pi-pencil" 
                                        [rounded]="true" 
                                        [text]="true"
                                        (onClick)="editRequest(request)"
                                        pTooltip="{{ 'admin.serviceWorkflow.manageWorkflow' | translate }}">
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
                        <app-service-request-status-badge module="print3d" [status]="selectedRequest.workflowStatus || selectedRequest.status"></app-service-request-status-badge>
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
                                    [label]="'common.download' | translate"
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
                    <div
                        class="detail-item full-width pricing-view-section"
                        *ngIf="selectedRequest.estimatedPrintTimeHours != null || selectedRequest.suggestedPrice != null">
                        <label class="section-label">{{ 'admin.3dRequests.pricing' | translate }}</label>
                        <div class="detail-grid pricing-view-grid">
                            <div class="detail-item" *ngIf="selectedRequest.estimatedPrintTimeHours != null">
                                <label>{{ 'admin.3dRequests.estimatedPrintTimeHours' | translate }}</label>
                                <span>{{ selectedRequest.estimatedPrintTimeHours }} {{ 'admin.3dRequests.hoursAbbr' | translate }}</span>
                            </div>
                            <div class="detail-item" *ngIf="selectedRequest.suggestedPrice != null">
                                <label>{{ 'admin.3dRequests.suggestedPrice' | translate }}</label>
                                <span class="price suggested-view">{{ selectedRequest.suggestedPrice | currency:'ILS' }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="detail-item full-width production-section">
                        <label class="section-label">{{ 'admin.3dRequests.productionInfo' | translate }}</label>
                        <div class="production-grid production-grid--three">
                            <div class="detail-item">
                                <label>{{ 'admin.3dRequests.filamentSpool' | translate }}</label>
                                <span>{{ selectedRequest.usedSpoolName || '—' }}</span>
                            </div>
                            <div class="detail-item">
                                <label>{{ 'admin.3dRequests.estimatedFilamentGrams' | translate }}</label>
                                <span>{{ selectedRequest.estimatedFilamentGrams != null ? (selectedRequest.estimatedFilamentGrams + ' g') : '—' }}</span>
                            </div>
                            <div class="detail-item">
                                <label>{{ 'admin.3dRequests.filamentDeducted' | translate }}</label>
                                <p-tag
                                    [value]="(selectedRequest.isFilamentDeducted ? 'admin.3dRequests.yes' : 'admin.3dRequests.no') | translate"
                                    [severity]="selectedRequest.isFilamentDeducted ? 'success' : 'secondary'">
                                </p-tag>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </p-dialog>

        <!-- Edit Dialog -->
        <p-dialog 
            [(visible)]="showEditDialog" 
            [modal]="true"
            [style]="{ width: '640px', maxWidth: '95vw' }"
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

                <div class="edit-section-card production-edit-block">
                    <div class="section-label">{{ 'admin.3dRequests.productionInfo' | translate }}</div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.filamentSpool' | translate }}</label>
                        <p-select
                            [(ngModel)]="editUsedSpoolId"
                            [options]="sortedSpoolDropdownOptions"
                            optionLabel="label"
                            optionValue="value"
                            [showClear]="true"
                            [filter]="true"
                            filterBy="label"
                            [filterPlaceholder]="'common.search' | translate"
                            [disabled]="productionFieldsLocked"
                            [placeholder]="'admin.3dRequests.noSpoolSelected' | translate"
                            styleClass="w-full">
                        </p-select>
                        <p class="field-hint">{{ 'admin.3dRequests.spoolNamingHint' | translate }}</p>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.estimatedFilamentGrams' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="editEstimatedGrams"
                            [min]="0"
                            [useGrouping]="false"
                            [showButtons]="false"
                            [disabled]="productionFieldsLocked"
                            placeholder="—"
                            styleClass="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="warn-hints" *ngIf="!productionFieldsLocked && (showLowFilamentWarning || showExceedsWarning)">
                        <p class="warn-hint" *ngIf="showLowFilamentWarning">
                            <span aria-hidden="true">⚠️</span> {{ 'admin.3dRequests.warnLowFilament' | translate }}
                        </p>
                        <p class="warn-hint" *ngIf="showExceedsWarning">
                            <span aria-hidden="true">⚠️</span> {{ 'admin.3dRequests.warnMayExceed' | translate }}
                        </p>
                    </div>
                </div>

                <div class="edit-section-card pricing-edit-block">
                    <div class="section-label">{{ 'admin.3dRequests.pricing' | translate }}</div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.estimatedPrintTimeHours' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="editPrintTimeHours"
                            mode="decimal"
                            [min]="0"
                            [minFractionDigits]="0"
                            [maxFractionDigits]="2"
                            [useGrouping]="false"
                            [showButtons]="false"
                            placeholder="—"
                            styleClass="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.hourlyRate' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="editHourlyRate"
                            mode="currency"
                            currency="ILS"
                            locale="en-IL"
                            [min]="0"
                            styleClass="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.qualityMultiplier' | translate }}</label>
                        <input
                            pInputText
                            class="w-full readonly-input"
                            [value]="profileMultiplierDisplay"
                            readonly
                            tabindex="-1" />
                        <p class="field-hint">{{ 'admin.3dRequests.qualityMultiplierHint' | translate }}</p>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.profitMargin' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="editProfitMargin"
                            mode="decimal"
                            [min]="0.01"
                            [minFractionDigits]="0"
                            [maxFractionDigits]="3"
                            [useGrouping]="false"
                            styleClass="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.minimumPrice' | translate }}</label>
                        <p-inputNumber
                            [(ngModel)]="editMinimumPrice"
                            mode="currency"
                            currency="ILS"
                            locale="en-IL"
                            [min]="0"
                            styleClass="w-full">
                        </p-inputNumber>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.3dRequests.roundToNearest' | translate }}</label>
                        <p-select
                            [(ngModel)]="editRoundToNearest"
                            [options]="roundStepOptions"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                        <p class="field-hint">{{ 'admin.3dRequests.roundToNearestHint' | translate }}</p>
                    </div>
                    <div class="form-field pricing-actions">
                        <p-button
                            type="button"
                            [label]="'admin.3dRequests.calculatePrice' | translate"
                            icon="pi pi-calculator"
                            (onClick)="calculateSuggestedPrice()"
                            [loading]="calculatingPrice"
                            [outlined]="true">
                        </p-button>
                    </div>
                    <div class="suggested-price-box" *ngIf="editSuggestedPrice != null">
                        <label>{{ 'admin.3dRequests.suggestedPrice' | translate }}</label>
                        <span class="suggested-amount">{{ editSuggestedPrice | currency:'ILS' }}</span>
                        <p class="field-hint">{{ 'admin.3dRequests.suggestedPriceHint' | translate }}</p>
                    </div>
                    <div class="pricing-breakdown" *ngIf="pricingBreakdown">
                        <div class="breakdown-line">
                            <span class="bd-label">{{ 'admin.3dRequests.bdMaterial' | translate }}</span>
                            <span class="mono">{{ pricingBreakdown.grams }}g × {{ pricingBreakdown.costPerGram }} = {{ pricingBreakdown.materialCost | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line">
                            <span class="bd-label">{{ 'admin.3dRequests.bdMachine' | translate }}</span>
                            <span class="mono">{{ pricingBreakdown.printTimeHours }}h × {{ pricingBreakdown.hourlyRate }} = {{ pricingBreakdown.machineCost | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line">
                            <span class="bd-label">{{ 'admin.3dRequests.bdBase' | translate }}</span>
                            <span class="mono">{{ pricingBreakdown.baseCost | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line">
                            <span class="bd-label">{{ 'admin.3dRequests.bdAfterQuality' | translate }}</span>
                            <span class="mono">× {{ pricingBreakdown.qualityMultiplier }} → {{ pricingBreakdown.adjustedCost | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line">
                            <span class="bd-label">{{ 'admin.3dRequests.bdAfterMargin' | translate }}</span>
                            <span class="mono">× {{ pricingBreakdown.profitMargin }} → {{ pricingBreakdown.afterMargin | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line" *ngIf="pricingBreakdown.minimumApplied">
                            <span class="bd-label">{{ 'admin.3dRequests.bdMinimum' | translate }}</span>
                            <span class="mono">≥ {{ pricingBreakdown.minimumPrice | currency:'ILS' }} → {{ pricingBreakdown.afterMinimum | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line" *ngIf="pricingBreakdown.roundingApplied && pricingBreakdown.roundStep">
                            <span class="bd-label">{{ 'admin.3dRequests.bdRound' | translate }}</span>
                            <span class="mono">{{ 'admin.3dRequests.bdRoundStep' | translate: { step: pricingBreakdown.roundStep } }} → {{ pricingBreakdown.suggestedPrice | currency:'ILS' }}</span>
                        </div>
                        <div class="breakdown-line breakdown-total">
                            <span>{{ 'admin.3dRequests.breakdownFinalSuggested' | translate }}: {{ pricingBreakdown.suggestedPrice | currency:'ILS' }}</span>
                        </div>
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
                </div>

                <div class="edit-section-card system-status-block">
                    <div class="section-label">{{ 'admin.3dRequests.systemStatus' | translate }}</div>
                    <div class="system-status-row">
                        <span class="system-status-label">{{ 'admin.3dRequests.filamentDeducted' | translate }}</span>
                        <p-tag
                            [value]="(selectedRequest.isFilamentDeducted ? 'admin.3dRequests.yes' : 'admin.3dRequests.no') | translate"
                            [severity]="selectedRequest.isFilamentDeducted ? 'success' : 'secondary'">
                        </p-tag>
                    </div>
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

                <div class="form-field rejection-reason-block" *ngIf="editStatus === 'Rejected'">
                    <label class="rejection-label">{{ 'admin.requests.rejectionReason' | translate }} <span class="required-star">*</span></label>
                    <textarea
                        pTextarea
                        [(ngModel)]="editRejectionReason"
                        rows="3"
                        class="w-full"
                        [placeholder]="'admin.requests.rejectionReasonPlaceholder' | translate">
                    </textarea>
                    <small class="rejection-hint">{{ 'admin.requests.rejectionReasonHint' | translate }}</small>
                </div>

                <div class="notification-note" *ngIf="editStatus !== selectedRequest.workflowStatus && editStatus !== selectedRequest.status">
                    <i class="pi pi-envelope"></i>
                    <span>{{ 'admin.requests.customerWillBeNotified' | translate }}</span>
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

        .production-section .section-label,
        .production-edit-block .section-label {
            display: block;
            margin-bottom: 0.75rem;
            font-size: 0.8rem;
            font-weight: 600;
            color: #475569;
            text-transform: uppercase;
            letter-spacing: 0.04em;
        }
        .production-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
            margin-top: 0.25rem;
        }
        .production-grid--three {
            grid-template-columns: 1fr 1fr 1fr;
        }
        @media (max-width: 640px) {
            .production-grid--three { grid-template-columns: 1fr; }
        }
        @media (max-width: 500px) {
            .production-grid:not(.production-grid--three) { grid-template-columns: 1fr; }
        }

        .edit-content { padding: 0.5rem 0; }
        .form-field { margin-bottom: 1rem; }
        .form-field label { display: block; margin-bottom: 0.5rem; font-size: 0.875rem; font-weight: 500; color: #475569; }
        .rejection-reason-block { background: #fff5f5; border: 1px solid #fecaca; border-radius: 8px; padding: 0.75rem 1rem; }
        .rejection-label { color: #b91c1c !important; }
        .required-star { color: #ef4444; margin-left: 2px; }
        .rejection-hint { color: #6b7280; font-size: 0.75rem; margin-top: 0.25rem; display: block; }
        .notification-note { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8rem; color: #2563eb; background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 6px; padding: 0.5rem 0.75rem; margin-top: 0.75rem; }
        .edit-section-card {
            margin-bottom: 1rem;
            padding: 0.75rem 1rem;
            border-radius: 10px;
            border: 1px solid rgba(148, 163, 184, 0.35);
            background: rgba(248, 250, 252, 0.85);
        }
        .system-status-block .system-status-row {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            flex-wrap: wrap;
        }
        .system-status-label { font-size: 0.875rem; color: #475569; font-weight: 500; }
        .field-hint {
            margin: 0.35rem 0 0;
            font-size: 0.75rem;
            color: #64748b;
            line-height: 1.4;
        }
        .warn-hints { margin-top: 0.25rem; }
        .warn-hint {
            margin: 0.25rem 0 0;
            font-size: 0.8rem;
            color: #b45309;
            line-height: 1.4;
        }
        .production-edit-block { margin-bottom: 1rem; }
        .production-edit-block .form-field:last-of-type { margin-bottom: 0; }
        .pricing-edit-block .pricing-actions { margin-bottom: 0.75rem; }
        .readonly-input { cursor: default; opacity: 0.95; }
        .suggested-price-box {
            margin-bottom: 1rem;
            padding: 0.75rem 1rem;
            border-radius: 8px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.12) 0%, rgba(118, 75, 162, 0.1) 100%);
            border: 1px solid rgba(102, 126, 234, 0.25);
        }
        .suggested-price-box label { font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.04em; }
        .suggested-amount { display: block; font-size: 1.35rem; font-weight: 700; color: #4f46e5; margin-top: 0.25rem; }
        .pricing-breakdown {
            margin-bottom: 1rem;
            padding: 0.65rem 0.75rem;
            border-radius: 8px;
            background: rgba(15, 23, 42, 0.04);
            border: 1px solid rgba(148, 163, 184, 0.35);
        }
        .breakdown-line { font-size: 0.8rem; color: #334155; margin: 0.2rem 0; }
        .breakdown-line .mono { font-family: ui-monospace, monospace; }
        .breakdown-total { margin-top: 0.5rem; padding-top: 0.5rem; border-top: 1px solid rgba(148, 163, 184, 0.4); font-weight: 600; color: #1e293b; }
        .breakdown-line .bd-label { display: inline-block; min-width: 9.5rem; margin-right: 0.5rem; font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.03em; vertical-align: top; }
        .pricing-view-section { margin-top: 0.25rem; }
        .pricing-view-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-top: 0.35rem; }
        .price.suggested-view { color: #4f46e5; font-weight: 600; }
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

    selectedStatus: ServiceWorkflowStatus | '' = '';
    statusOptions: { value: ServiceWorkflowStatus; label: string }[] = [];
    allStatusOptions = this.statusOptions;

    showViewDialog = false;
    showEditDialog = false;
    selectedRequest: Print3dRequest | null = null;

    editStatus: ServiceWorkflowStatus = 'New';
    editFinalPrice: number | null = null;
    editNotes = '';
    editRejectionReason = '';
    editEstimatedGrams: number | null = null;
    /** Selected spool id, or undefined when cleared */
    editUsedSpoolId?: string | null;
    filamentSpools: FilamentSpool[] = [];

    editPrintTimeHours: number | null = null;
    editHourlyRate = environment.print3dDefaultHourlyRate;
    editProfitMargin = environment.print3dDefaultProfitMargin;
    editMinimumPrice = environment.print3dDefaultMinimumPrice;
    /** 0 = no rounding; otherwise step (e.g. 5). */
    editRoundToNearest = environment.print3dDefaultRoundToNearest;
    editSuggestedPrice: number | null = null;
    pricingBreakdown: Print3dPricingSuggestionResponse | null = null;
    calculatingPrice = false;

    roundStepOptions: { label: string; value: number }[] = [
        { label: '5', value: 5 },
        { label: '1', value: 1 },
        { label: '0.5', value: 0.5 },
        { label: '—', value: 0 }
    ];

    /** Spool / estimated locked after server-side filament deduction. */
    get productionFieldsLocked(): boolean {
        return !!this.selectedRequest?.isFilamentDeducted;
    }

    get sortedSpoolDropdownOptions(): { label: string; value: string }[] {
        const reqMat = this.selectedRequest?.materialId;
        const sorted = [...this.filamentSpools].sort((a, b) => {
            const aSame = reqMat && a.materialId === reqMat ? 1 : 0;
            const bSame = reqMat && b.materialId === reqMat ? 1 : 0;
            if (aSame !== bSame) {
                return bSame - aSame;
            }
            return b.remainingWeightGrams - a.remainingWeightGrams;
        });
        return sorted.map((s) => ({
            label: this.formatSpoolLabel(s),
            value: s.id
        }));
    }

    get selectedEditSpool(): FilamentSpool | undefined {
        if (!this.editUsedSpoolId) {
            return undefined;
        }
        return this.filamentSpools.find((s) => s.id === this.editUsedSpoolId);
    }

    get showLowFilamentWarning(): boolean {
        const s = this.selectedEditSpool;
        return !!s && s.remainingWeightGrams < 100;
    }

    get showExceedsWarning(): boolean {
        const s = this.selectedEditSpool;
        const est = this.editEstimatedGrams;
        if (!s || est == null) {
            return false;
        }
        return est > s.remainingWeightGrams;
    }

    get profileMultiplierDisplay(): string {
        const m = this.selectedRequest?.profilePriceMultiplier;
        if (m === undefined || m === null) {
            return '—';
        }
        return String(m);
    }

    calculateSuggestedPrice() {
        if (!this.selectedRequest) {
            return;
        }
        const hours = this.editPrintTimeHours;
        if (hours == null || hours <= 0) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translateService.instant('common.warning'),
                detail: this.translateService.instant('admin.3dRequests.pricingNeedTime')
            });
            return;
        }
        const grams = this.editEstimatedGrams ?? this.selectedRequest.estimatedFilamentGrams;
        if (grams == null || grams < 0) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translateService.instant('common.warning'),
                detail: this.translateService.instant('admin.3dRequests.pricingNeedGrams')
            });
            return;
        }

        this.calculatingPrice = true;
        this.printingApiService
            .suggestPrint3dPricing(this.selectedRequest.id, {
                estimatedPrintTimeHours: hours,
                hourlyRate: this.editHourlyRate,
                profileId: this.selectedRequest.profileId ?? null,
                estimatedFilamentGrams: grams,
                profitMargin: this.editProfitMargin,
                minimumPrice: this.editMinimumPrice,
                roundToNearest: this.editRoundToNearest
            })
            .subscribe({
                next: (res) => {
                    this.calculatingPrice = false;
                    this.editSuggestedPrice = res.suggestedPrice;
                    this.pricingBreakdown = res;
                },
                error: () => {
                    this.calculatingPrice = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('admin.3dRequests.pricingCalcError')
                    });
                }
            });
    }

    get viewDialogHeader(): string {
        return this.translateService.instant('admin.3dRequests.viewDetails');
    }

    get editDialogHeader(): string {
        return this.translateService.instant('admin.3dRequests.updateRequest');
    }

    ngOnInit() {
        this.rebuildStatusOptions();
        this.translateService.onLangChange.subscribe(() => this.rebuildStatusOptions());
        this.loadRequests();
        this.loadFilamentSpools();
    }

    rebuildStatusOptions() {
        this.statusOptions = getWorkflowOptions('print3d', this.translateService);
        this.allStatusOptions = this.statusOptions;
    }

    private loadFilamentSpools() {
        this.printingApiService.getFilamentSpools().subscribe({
            next: (rows) => {
                this.filamentSpools = rows ?? [];
            },
            error: () => {
                this.filamentSpools = [];
            }
        });
    }

    formatSpoolLabel(s: FilamentSpool): string {
        const material = s.materialNameEn || '?';
        const spoolName = s.name?.trim() ? s.name.trim() : '—';
        return `${material} — ${spoolName} (${s.remainingWeightGrams}g)`;
    }

    loadRequests() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedStatus) {
            params.status = denormalizePrint3dWorkflowStatus(this.selectedStatus);
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
        this.editStatus = normalizePrint3dWorkflowStatus(request.workflowStatus || request.status);
        this.editFinalPrice = request.finalPrice || null;
        this.editNotes = request.adminNotes ?? '';
        this.editRejectionReason = '';
        this.editEstimatedGrams = request.estimatedFilamentGrams ?? null;
        this.editUsedSpoolId = request.usedSpoolId ?? undefined;
        this.editPrintTimeHours = request.estimatedPrintTimeHours ?? null;
        this.editHourlyRate = environment.print3dDefaultHourlyRate;
        this.editProfitMargin = environment.print3dDefaultProfitMargin;
        this.editMinimumPrice = environment.print3dDefaultMinimumPrice;
        this.editRoundToNearest = environment.print3dDefaultRoundToNearest;
        this.editSuggestedPrice = request.suggestedPrice ?? null;
        this.pricingBreakdown = null;
        this.loadFilamentSpools();
        this.showEditDialog = true;
    }

    saveRequest() {
        if (!this.selectedRequest) return;

        if (this.editStatus === 'Rejected' && !this.editRejectionReason?.trim()) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translateService.instant('messages.validationError'),
                detail: this.translateService.instant('admin.requests.rejectionReasonRequired')
            });
            return;
        }

        this.saving = true;
        this.printingApiService.updateRequestStatus(this.selectedRequest.id, {
            status: denormalizePrint3dWorkflowStatus(this.editStatus),
            notes: this.editNotes || undefined,
            rejectionReason: this.editStatus === 'Rejected' ? this.editRejectionReason : undefined,
            finalPrice: this.editFinalPrice || undefined,
            usedSpoolId: this.editUsedSpoolId ?? null,
            estimatedFilamentGrams: this.editEstimatedGrams ?? null,
            estimatedPrintTimeHours: this.editPrintTimeHours ?? null,
            suggestedPrice: this.editSuggestedPrice ?? null
        }).subscribe({
            next: () => {
                this.saving = false;
                this.showEditDialog = false;
                this.loadRequests();
                this.loadFilamentSpools();
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

}
