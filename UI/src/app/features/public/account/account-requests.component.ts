import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { LaserApiService } from '../../../shared/services/laser-api.service';
import { LanguageService } from '../../../shared/services/language.service';

interface ServiceRequest {
    id: string;
    referenceNumber: string;
    type: 'print3d' | 'laser';
    typeLabel: string;
    status: string;
    statusSeverity: 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';
    materialName?: string;
    fileName?: string;
    createdAt: string;
    estimatedPrice?: number;
    finalPrice?: number;
    notes?: string;
}

@Component({
    selector: 'app-account-requests',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        CardModule,
        ButtonModule,
        TagModule,
        TableModule,
        SelectModule,
        ProgressSpinnerModule,
        TooltipModule,
        DialogModule
    ],
    template: `
        <div class="requests-page" [dir]="languageService.direction">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div class="header-text">
                        <h1>
                            <i class="pi pi-list"></i>
                            {{ 'requests.title' | translate }}
                        </h1>
                        <p>{{ 'requests.subtitle' | translate }}</p>
                    </div>
                    <div class="header-actions">
                        <a routerLink="/3d-print" class="action-btn primary">
                            <i class="pi pi-plus"></i>
                            {{ 'requests.new3dPrint' | translate }}
                        </a>
                        <a routerLink="/laser" class="action-btn">
                            <i class="pi pi-plus"></i>
                            {{ 'requests.newLaser' | translate }}
                        </a>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <div class="filters-section">
                <div class="filter-group">
                    <label>{{ 'requests.filterType' | translate }}</label>
                    <p-select 
                        [(ngModel)]="selectedType" 
                        [options]="typeOptions" 
                        optionLabel="label" 
                        optionValue="value"
                        (onChange)="filterRequests()"
                        [placeholder]="'requests.allTypes' | translate">
                    </p-select>
                </div>
                <div class="filter-group">
                    <label>{{ 'requests.filterStatus' | translate }}</label>
                    <p-select 
                        [(ngModel)]="selectedStatus" 
                        [options]="statusOptions" 
                        optionLabel="label" 
                        optionValue="value"
                        (onChange)="filterRequests()"
                        [placeholder]="'requests.allStatuses' | translate">
                    </p-select>
                </div>
            </div>

            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
                <p-progressSpinner strokeWidth="3" animationDuration="1s"></p-progressSpinner>
                <p>{{ 'requests.loading' | translate }}</p>
            </div>

            <!-- Empty State -->
            <div *ngIf="!loading && filteredRequests.length === 0" class="empty-state">
                <div class="empty-icon">
                    <i class="pi pi-inbox"></i>
                </div>
                <h3>{{ 'requests.noRequests' | translate }}</h3>
                <p>{{ 'requests.noRequestsDesc' | translate }}</p>
                <a routerLink="/3d-print" class="start-btn">
                    <i class="pi pi-plus"></i>
                    {{ 'requests.startFirst' | translate }}
                </a>
            </div>

            <!-- Requests Table -->
            <div *ngIf="!loading && filteredRequests.length > 0" class="requests-table-container">
                <p-table [value]="filteredRequests" [paginator]="true" [rows]="10" styleClass="p-datatable-sm">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'requests.reference' | translate }}</th>
                            <th>{{ 'requests.type' | translate }}</th>
                            <th>{{ 'requests.material' | translate }}</th>
                            <th>{{ 'requests.status' | translate }}</th>
                            <th>{{ 'requests.date' | translate }}</th>
                            <th>{{ 'requests.price' | translate }}</th>
                            <th>{{ 'requests.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-request>
                        <tr>
                            <td>
                                <span class="ref-number">{{ request.referenceNumber }}</span>
                            </td>
                            <td>
                                <p-tag 
                                    [value]="request.typeLabel" 
                                    [severity]="request.type === 'print3d' ? 'info' : 'warn'"
                                    [rounded]="true">
                                </p-tag>
                            </td>
                            <td>{{ request.materialName || '-' }}</td>
                            <td>
                                <p-tag 
                                    [value]="request.status" 
                                    [severity]="request.statusSeverity"
                                    [rounded]="true">
                                </p-tag>
                            </td>
                            <td>{{ formatDate(request.createdAt) }}</td>
                            <td>
                                <span *ngIf="request.finalPrice" class="price final">₪{{ request.finalPrice }}</span>
                                <span *ngIf="!request.finalPrice && request.estimatedPrice" class="price estimated">~₪{{ request.estimatedPrice }}</span>
                                <span *ngIf="!request.finalPrice && !request.estimatedPrice">-</span>
                            </td>
                            <td>
                                <p-button 
                                    icon="pi pi-eye" 
                                    [rounded]="true" 
                                    [text]="true"
                                    pTooltip="{{ 'requests.viewDetails' | translate }}"
                                    (click)="viewRequest(request)">
                                </p-button>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>

            <!-- View Details Dialog -->
            <p-dialog 
                [(visible)]="showViewDialog" 
                [modal]="true" 
                [style]="{width: '500px'}"
                [header]="'requests.requestDetails' | translate">
                <div class="view-content" *ngIf="selectedRequest">
                    <div class="detail-grid">
                        <div class="detail-item">
                            <span class="detail-label">{{ 'requests.reference' | translate }}</span>
                            <span class="detail-value ref">{{ selectedRequest.referenceNumber }}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">{{ 'requests.type' | translate }}</span>
                            <p-tag [value]="selectedRequest.typeLabel" [severity]="selectedRequest.type === 'print3d' ? 'info' : 'warn'"></p-tag>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">{{ 'requests.status' | translate }}</span>
                            <p-tag [value]="selectedRequest.status" [severity]="selectedRequest.statusSeverity"></p-tag>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">{{ 'requests.material' | translate }}</span>
                            <span class="detail-value">{{ selectedRequest.materialName || '-' }}</span>
                        </div>
                        <div class="detail-item" *ngIf="selectedRequest.fileName">
                            <span class="detail-label">{{ 'requests.file' | translate }}</span>
                            <span class="detail-value file">{{ selectedRequest.fileName }}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">{{ 'requests.date' | translate }}</span>
                            <span class="detail-value">{{ formatDate(selectedRequest.createdAt) }}</span>
                        </div>
                        <div class="detail-item" *ngIf="selectedRequest.estimatedPrice">
                            <span class="detail-label">{{ 'requests.estimatedPrice' | translate }}</span>
                            <span class="detail-value">₪{{ selectedRequest.estimatedPrice }}</span>
                        </div>
                        <div class="detail-item" *ngIf="selectedRequest.finalPrice">
                            <span class="detail-label">{{ 'requests.finalPrice' | translate }}</span>
                            <span class="detail-value price">₪{{ selectedRequest.finalPrice }}</span>
                        </div>
                        <div class="detail-item full-width" *ngIf="selectedRequest.notes">
                            <span class="detail-label">{{ 'requests.notes' | translate }}</span>
                            <span class="detail-value">{{ selectedRequest.notes }}</span>
                        </div>
                    </div>
                </div>
                <ng-template pTemplate="footer">
                    <p-button 
                        [label]="'common.close' | translate" 
                        [outlined]="true"
                        (click)="showViewDialog = false">
                    </p-button>
                </ng-template>
            </p-dialog>
        </div>
    `,
    styles: [`
        .requests-page {
            min-height: 100vh;
            background: var(--surface-ground);
            padding-bottom: 2rem;
        }

        .page-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 2rem;
            color: white;
            margin-bottom: 1.5rem;
        }

        .header-content {
            max-width: 1200px;
            margin: 0 auto;
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .header-text h1 {
            font-size: 1.75rem;
            font-weight: 700;
            margin: 0 0 0.5rem 0;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .header-text p {
            margin: 0;
            opacity: 0.9;
        }

        .header-actions {
            display: flex;
            gap: 0.75rem;
        }

        .action-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.25rem;
            border-radius: 8px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.2s ease;
            background: rgba(255,255,255,0.2);
            color: white;
            border: 1px solid rgba(255,255,255,0.3);
        }

        .action-btn.primary {
            background: white;
            color: #667eea;
            border: none;
        }

        .action-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }

        .filters-section {
            max-width: 1200px;
            margin: 0 auto 1.5rem;
            padding: 0 1rem;
            display: flex;
            gap: 1rem;
            flex-wrap: wrap;
        }

        .filter-group {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .filter-group label {
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--text-color-secondary);
        }

        :host ::ng-deep .filter-group .p-select {
            min-width: 180px;
        }

        .loading-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 300px;
            gap: 1rem;
            color: var(--text-color-secondary);
        }

        .empty-state {
            max-width: 400px;
            margin: 3rem auto;
            text-align: center;
            padding: 2rem;
        }

        .empty-icon {
            width: 100px;
            height: 100px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea22 0%, #764ba222 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .empty-icon i {
            font-size: 2.5rem;
            color: #667eea;
        }

        .empty-state h3 {
            margin: 0 0 0.5rem 0;
            color: var(--text-color);
        }

        .empty-state p {
            color: var(--text-color-secondary);
            margin-bottom: 1.5rem;
        }

        .start-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            transition: all 0.2s ease;
        }

        .start-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
        }

        .requests-table-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        :host ::ng-deep .p-datatable {
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }

        .ref-number {
            font-family: monospace;
            font-weight: 600;
            color: #667eea;
        }

        .price {
            font-weight: 600;
        }

        .price.final {
            color: #667eea;
        }

        .price.estimated {
            color: var(--text-color-secondary);
        }

        .view-content {
            padding: 1rem 0;
        }

        .detail-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1.25rem;
        }

        .detail-item {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }

        .detail-item.full-width {
            grid-column: 1 / -1;
        }

        .detail-label {
            font-size: 0.8rem;
            color: var(--text-color-secondary);
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .detail-value {
            font-weight: 500;
            color: var(--text-color);
        }

        .detail-value.ref {
            font-family: monospace;
            color: #667eea;
        }

        .detail-value.file {
            font-family: monospace;
            font-size: 0.9rem;
        }

        .detail-value.price {
            font-size: 1.1rem;
            color: #667eea;
        }

        @media (max-width: 768px) {
            .header-content {
                flex-direction: column;
                text-align: center;
            }

            .filters-section {
                flex-direction: column;
            }

            :host ::ng-deep .filter-group .p-select {
                width: 100%;
            }

            .detail-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class AccountRequestsComponent implements OnInit {
    loading = false;
    allRequests: ServiceRequest[] = [];
    filteredRequests: ServiceRequest[] = [];

    selectedType: string | null = null;
    selectedStatus: string | null = null;

    showViewDialog = false;
    selectedRequest: ServiceRequest | null = null;

    typeOptions = [
        { label: 'All Types', value: null },
        { label: '3D Print', value: 'print3d' },
        { label: 'Laser', value: 'laser' }
    ];

    statusOptions = [
        { label: 'All Statuses', value: null },
        { label: 'Pending', value: 'Pending' },
        { label: 'Under Review', value: 'UnderReview' },
        { label: 'Approved', value: 'Approved' },
        { label: 'In Progress', value: 'InProgress' },
        { label: 'Completed', value: 'Completed' },
        { label: 'Cancelled', value: 'Cancelled' }
    ];

    private printingApiService = inject(PrintingApiService);
    private laserApiService = inject(LaserApiService);
    private translateService = inject(TranslateService);
    private route = inject(ActivatedRoute);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            if (params['type']) {
                this.selectedType = params['type'];
            }
            if (params['status']) {
                this.selectedStatus = params['status'];
            }
        });
        this.loadRequests();
        this.updateLabels();
    }

    updateLabels() {
        if (this.languageService.language === 'ar') {
            this.typeOptions = [
                { label: 'جميع الأنواع', value: null },
                { label: 'طباعة 3D', value: 'print3d' },
                { label: 'ليزر', value: 'laser' }
            ];
            this.statusOptions = [
                { label: 'جميع الحالات', value: null },
                { label: 'قيد الانتظار', value: 'Pending' },
                { label: 'قيد المراجعة', value: 'UnderReview' },
                { label: 'موافق عليه', value: 'Approved' },
                { label: 'قيد التنفيذ', value: 'InProgress' },
                { label: 'مكتمل', value: 'Completed' },
                { label: 'ملغي', value: 'Cancelled' }
            ];
        }
    }

    loadRequests() {
        this.loading = true;
        this.allRequests = [];
        let completedCalls = 0;
        const totalCalls = 2;

        const checkComplete = () => {
            completedCalls++;
            if (completedCalls >= totalCalls) {
                this.allRequests.sort((a, b) => 
                    new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                );
                this.filterRequests();
                this.loading = false;
            }
        };

        // Load 3D print requests
        this.printingApiService.getRequests({ page: 1, pageSize: 100 }).subscribe({
            next: (response) => {
                (response.items || []).forEach((r: any) => {
                    this.allRequests.push({
                        id: r.id,
                        referenceNumber: r.referenceNumber || r.id?.slice(0, 8),
                        type: 'print3d',
                        typeLabel: this.languageService.language === 'ar' ? 'طباعة 3D' : '3D Print',
                        status: r.status,
                        statusSeverity: this.getStatusSeverity(r.status),
                        materialName: r.materialName,
                        fileName: r.fileName,
                        createdAt: r.createdAt,
                        estimatedPrice: r.estimatedPrice,
                        finalPrice: r.finalPrice,
                        notes: r.customerNotes
                    });
                });
                checkComplete();
            },
            error: () => checkComplete()
        });

        // Load laser requests
        this.laserApiService.getServiceRequests({ limit: 100 }).subscribe({
            next: (laserRequests) => {
                (laserRequests || []).forEach((r: any) => {
                    this.allRequests.push({
                        id: r.id,
                        referenceNumber: r.referenceNumber || r.id?.slice(0, 8),
                        type: 'laser',
                        typeLabel: this.languageService.language === 'ar' ? 'ليزر' : 'Laser',
                        status: r.status || r.statusName,
                        statusSeverity: this.getStatusSeverity(r.status || r.statusName),
                        materialName: r.materialName,
                        fileName: r.fileName,
                        createdAt: r.createdAt,
                        estimatedPrice: r.estimatedPrice,
                        finalPrice: r.finalPrice,
                        notes: r.notes
                    });
                });
                checkComplete();
            },
            error: () => checkComplete()
        });
    }

    filterRequests() {
        this.filteredRequests = this.allRequests.filter(r => {
            if (this.selectedType && r.type !== this.selectedType) return false;
            if (this.selectedStatus && r.status !== this.selectedStatus) return false;
            return true;
        });
    }

    viewRequest(request: ServiceRequest) {
        this.selectedRequest = request;
        this.showViewDialog = true;
    }

    getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
        const statusLower = (status || '').toLowerCase();
        if (statusLower.includes('completed') || statusLower.includes('approved')) return 'success';
        if (statusLower.includes('pending')) return 'warn';
        if (statusLower.includes('review') || statusLower.includes('progress')) return 'info';
        if (statusLower.includes('cancelled') || statusLower.includes('rejected') || statusLower.includes('failed')) return 'danger';
        return 'secondary';
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '-';
        const date = new Date(dateStr);
        return date.toLocaleDateString(this.languageService.language === 'ar' ? 'ar-EG' : 'en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
    }
}
