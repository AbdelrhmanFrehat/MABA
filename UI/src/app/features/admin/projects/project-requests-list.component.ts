import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { ProjectRequest, ProjectRequestStatus, ProjectRequestType } from '../../../shared/models/project.model';

@Component({
    selector: 'app-admin-project-requests-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TranslateModule,
        ButtonModule, TableModule, TagModule, SelectModule,
        ToastModule, DialogModule, TextareaModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        
        <div class="requests-admin">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'menu.projectRequests' | translate }}</h1>
                        <p>Manage incoming project requests and RFQs</p>
                    </div>
                </div>
                <div class="filters">
                    <p-select 
                        [(ngModel)]="statusFilter"
                        [options]="statusFilterOptions"
                        optionLabel="label"
                        optionValue="value"
                        placeholder="Filter by Status"
                        (onChange)="loadRequests()">
                    </p-select>
                </div>
            </div>

            <div class="table-section">
                <p-table 
                    [value]="requests()" 
                    [loading]="loading()"
                    [paginator]="true"
                    [rows]="10"
                    styleClass="p-datatable-sm">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>Reference</th>
                            <th>Contact</th>
                            <th>Type</th>
                            <th>Category/Project</th>
                            <th>Budget</th>
                            <th>Status</th>
                            <th>Date</th>
                            <th>Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-request>
                        <tr>
                            <td>
                                <strong class="ref-number">{{ request.referenceNumber }}</strong>
                            </td>
                            <td>
                                <div class="contact-info">
                                    <strong>{{ request.fullName }}</strong>
                                    <small>{{ request.email }}</small>
                                    @if (request.phone) {
                                        <small>{{ request.phone }}</small>
                                    }
                                </div>
                            </td>
                            <td>
                                <p-tag 
                                    [value]="request.requestType === 0 ? 'Similar' : 'Custom'"
                                    [severity]="request.requestType === 0 ? 'info' : 'secondary'">
                                </p-tag>
                            </td>
                            <td>
                                @if (request.requestType === 0 && request.projectTitle) {
                                    <span>{{ request.projectTitle }}</span>
                                } @else if (request.categoryName) {
                                    <span>{{ request.categoryName }}</span>
                                } @else {
                                    <span class="text-gray-400">—</span>
                                }
                            </td>
                            <td>{{ request.budgetRange || '—' }}</td>
                            <td>
                                <p-tag [value]="request.statusName" [severity]="getStatusSeverity(request.status)"></p-tag>
                            </td>
                            <td>{{ request.createdAt | date:'short' }}</td>
                            <td>
                                <p-button icon="pi pi-eye" [text]="true" (onClick)="viewRequest(request)" pTooltip="View Details"></p-button>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="8" class="text-center p-4">No project requests found</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>

        <!-- Detail Dialog -->
        <p-dialog 
            [(visible)]="dialogVisible" 
            header="Request Details"
            [modal]="true"
            [style]="{width: '600px'}">
            @if (selectedRequest) {
                <div class="request-detail">
                    <div class="detail-header">
                        <span class="ref-badge">{{ selectedRequest.referenceNumber }}</span>
                        <p-tag [value]="selectedRequest.statusName" [severity]="getStatusSeverity(selectedRequest.status)"></p-tag>
                    </div>

                    <div class="detail-section">
                        <h4>Contact Information</h4>
                        <div class="detail-row">
                            <span class="label">Name:</span>
                            <span class="value">{{ selectedRequest.fullName }}</span>
                        </div>
                        <div class="detail-row">
                            <span class="label">Email:</span>
                            <span class="value">{{ selectedRequest.email }}</span>
                        </div>
                        @if (selectedRequest.phone) {
                            <div class="detail-row">
                                <span class="label">Phone:</span>
                                <span class="value">{{ selectedRequest.phone }}</span>
                            </div>
                        }
                    </div>

                    <div class="detail-section">
                        <h4>Request Details</h4>
                        <div class="detail-row">
                            <span class="label">Type:</span>
                            <span class="value">{{ selectedRequest.requestType === 0 ? 'Similar to Existing' : 'Custom Project' }}</span>
                        </div>
                        @if (selectedRequest.projectTitle) {
                            <div class="detail-row">
                                <span class="label">Based On:</span>
                                <span class="value">{{ selectedRequest.projectTitle }}</span>
                            </div>
                        }
                        @if (selectedRequest.categoryName) {
                            <div class="detail-row">
                                <span class="label">Category:</span>
                                <span class="value">{{ selectedRequest.categoryName }}</span>
                            </div>
                        }
                        @if (selectedRequest.budgetRange) {
                            <div class="detail-row">
                                <span class="label">Budget:</span>
                                <span class="value">{{ selectedRequest.budgetRange }}</span>
                            </div>
                        }
                        @if (selectedRequest.timeline) {
                            <div class="detail-row">
                                <span class="label">Timeline:</span>
                                <span class="value">{{ selectedRequest.timeline }}</span>
                            </div>
                        }
                    </div>

                    @if (selectedRequest.description) {
                        <div class="detail-section">
                            <h4>Description</h4>
                            <p class="description">{{ selectedRequest.description }}</p>
                        </div>
                    }

                    <div class="detail-section">
                        <h4>Update Status</h4>
                        <div class="status-update">
                            <p-select 
                                [(ngModel)]="updateStatus"
                                [options]="statusOptions"
                                optionLabel="label"
                                optionValue="value"
                                placeholder="Select new status"
                                styleClass="w-full">
                            </p-select>
                        </div>
                        <div class="form-field mt-3">
                            <label>Admin Notes</label>
                            <textarea pTextarea [(ngModel)]="adminNotes" rows="3" class="w-full"></textarea>
                        </div>
                    </div>
                </div>
            }
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" [text]="true" (onClick)="dialogVisible = false"></p-button>
                <p-button label="Update" (onClick)="updateRequest()" [loading]="updating()"></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .requests-admin { padding: 1.5rem; }
        .page-header { margin-bottom: 1.5rem; }
        .header-content { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem; }
        .header-content h1 { margin: 0 0 0.25rem 0; font-size: 1.5rem; }
        .header-content p { margin: 0; color: #6b7280; }
        .filters { display: flex; gap: 1rem; }
        .ref-number { color: #667eea; font-family: monospace; }
        .contact-info { display: flex; flex-direction: column; gap: 0.125rem; }
        .contact-info small { color: #9ca3af; font-size: 0.75rem; }

        .request-detail { display: flex; flex-direction: column; gap: 1.5rem; }
        .detail-header { display: flex; justify-content: space-between; align-items: center; }
        .ref-badge { background: #f3f4f6; padding: 0.5rem 1rem; border-radius: 8px; font-family: monospace; font-weight: 600; color: #667eea; }
        .detail-section { }
        .detail-section h4 { margin: 0 0 0.75rem 0; font-size: 0.875rem; font-weight: 600; color: #374151; text-transform: uppercase; letter-spacing: 0.05em; }
        .detail-row { display: flex; gap: 1rem; padding: 0.5rem 0; border-bottom: 1px solid #f3f4f6; }
        .detail-row:last-child { border-bottom: none; }
        .detail-row .label { min-width: 100px; color: #6b7280; font-size: 0.875rem; }
        .detail-row .value { font-weight: 500; }
        .description { background: #f9fafb; padding: 1rem; border-radius: 8px; line-height: 1.6; margin: 0; }
        .status-update { margin-top: 0.5rem; }
        .form-field { display: flex; flex-direction: column; gap: 0.5rem; }
        .form-field label { font-size: 0.875rem; font-weight: 600; color: #374151; }
    `]
})
export class AdminProjectRequestsListComponent implements OnInit {
    private projectsApi = inject(ProjectsApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    loading = signal(false);
    updating = signal(false);
    requests = signal<ProjectRequest[]>([]);
    
    dialogVisible = false;
    selectedRequest: ProjectRequest | null = null;
    updateStatus: ProjectRequestStatus | null = null;
    adminNotes = '';
    statusFilter: ProjectRequestStatus | null = null;

    statusFilterOptions = [
        { value: null, label: 'All Statuses' },
        { value: ProjectRequestStatus.New, label: 'New' },
        { value: ProjectRequestStatus.InReview, label: 'In Review' },
        { value: ProjectRequestStatus.Quoted, label: 'Quoted' },
        { value: ProjectRequestStatus.InProgress, label: 'In Progress' },
        { value: ProjectRequestStatus.Closed, label: 'Closed' }
    ];

    statusOptions = [
        { value: ProjectRequestStatus.New, label: 'New' },
        { value: ProjectRequestStatus.InReview, label: 'In Review' },
        { value: ProjectRequestStatus.Quoted, label: 'Quoted' },
        { value: ProjectRequestStatus.InProgress, label: 'In Progress' },
        { value: ProjectRequestStatus.Closed, label: 'Closed' }
    ];

    ngOnInit() {
        this.loadRequests();
    }

    loadRequests() {
        this.loading.set(true);
        this.projectsApi.getProjectRequests({
            status: this.statusFilter ?? undefined,
            take: 100
        }).subscribe({
            next: (requests) => {
                this.requests.set(requests);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load requests' });
            }
        });
    }

    viewRequest(request: ProjectRequest) {
        this.selectedRequest = request;
        this.updateStatus = request.status;
        this.adminNotes = request.adminNotes || '';
        this.dialogVisible = true;
    }

    updateRequest() {
        if (!this.selectedRequest) return;

        this.updating.set(true);
        this.projectsApi.updateProjectRequest(this.selectedRequest.id, {
            status: this.updateStatus ?? undefined,
            adminNotes: this.adminNotes || undefined
        }).subscribe({
            next: () => {
                this.updating.set(false);
                this.dialogVisible = false;
                this.loadRequests();
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request updated successfully' });
            },
            error: () => {
                this.updating.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update request' });
            }
        });
    }

    getStatusSeverity(status: ProjectRequestStatus): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectRequestStatus.New]: 'info',
            [ProjectRequestStatus.InReview]: 'warn',
            [ProjectRequestStatus.Quoted]: 'secondary',
            [ProjectRequestStatus.InProgress]: 'contrast',
            [ProjectRequestStatus.Closed]: 'success'
        };
        return map[status] || 'secondary';
    }
}
