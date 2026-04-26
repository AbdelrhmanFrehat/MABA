import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import {
    ProjectRequest,
    ProjectRequestActivity,
    ProjectRequestAttachment,
    ProjectRequestCapability,
    ProjectRequestComplexity,
    ProjectRequestFeasibility,
    ProjectRequestMainDomain,
    ProjectRequestPriority,
    ProjectRequestProjectType,
    ProjectRequestStage,
    ProjectRequestStatus,
    ProjectWorkflowStatus
} from '../../../shared/models/project.model';

@Component({
    selector: 'app-admin-project-requests-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TranslateModule,
        ButtonModule, TableModule, TagModule, SelectModule, MultiSelectModule,
        InputTextModule, InputNumberModule, ToastModule, DialogModule,
        TextareaModule, TooltipModule, DividerModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>

        <div class="requests-admin">
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.projectRequests.title' | translate }}</h1>
                        <p>{{ 'admin.projectRequests.subtitle' | translate }}</p>
                    </div>
                </div>
                <div class="filters">
                    <p-select [(ngModel)]="workflowStatusFilter" [options]="workflowStatusFilterOptions" optionLabel="label" optionValue="value" [placeholder]="'admin.projectRequests.filterByStatus' | translate" (onChange)="loadRequests()"></p-select>
                    <p-select [(ngModel)]="projectTypeFilter" [options]="projectTypeOptions" optionLabel="label" optionValue="value" [placeholder]="'admin.projectRequests.filterByProjectType' | translate" (onChange)="loadRequests()"></p-select>
                    <p-select [(ngModel)]="mainDomainFilter" [options]="mainDomainOptions" optionLabel="label" optionValue="value" [placeholder]="'admin.projectRequests.filterByMainDomain' | translate" (onChange)="loadRequests()"></p-select>
                    <p-select [(ngModel)]="projectStageFilter" [options]="projectStageOptions" optionLabel="label" optionValue="value" [placeholder]="'admin.projectRequests.filterByProjectStage' | translate" (onChange)="loadRequests()"></p-select>
                    <input pInputText [(ngModel)]="searchTerm" [placeholder]="'admin.projectRequests.searchPlaceholder' | translate" (keyup.enter)="loadRequests()" />
                    <p-button icon="pi pi-search" [text]="true" (onClick)="loadRequests()"></p-button>
                </div>
            </div>

            <div class="table-section">
                <p-table [value]="requests()" [loading]="loading()" [paginator]="true" [rows]="15" styleClass="p-datatable-sm">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.projectRequests.reference' | translate }}</th>
                            <th>{{ 'admin.projectRequests.customer' | translate }}</th>
                            <th>{{ 'admin.projectRequests.projectType' | translate }}</th>
                            <th>{{ 'admin.projectRequests.mainDomain' | translate }}</th>
                            <th>{{ 'admin.projectRequests.requiredCapabilities' | translate }}</th>
                            <th>{{ 'admin.projectRequests.assignedTo' | translate }}</th>
                            <th>{{ 'admin.projectRequests.priority' | translate }}</th>
                            <th>{{ 'admin.projectRequests.workflowStatus' | translate }}</th>
                            <th>{{ 'admin.projectRequests.date' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-request>
                        <tr>
                            <td><strong class="ref-number">{{ request.referenceNumber }}</strong></td>
                            <td>
                                <div class="contact-info">
                                    <strong>{{ request.fullName }}</strong>
                                    <small>{{ request.email }}</small>
                                </div>
                            </td>
                            <td><p-tag [value]="getProjectTypeLabel(request.projectType, request.requestType)" severity="secondary"></p-tag></td>
                            <td>{{ getMainDomainLabel(request.mainDomain, request.categoryName) }}</td>
                            <td>
                                <div class="capability-list">
                                    @for (cap of (request.requiredCapabilities || []).slice(0, 2); track cap) {
                                        <p-tag [value]="getCapabilityLabel(cap)" severity="info"></p-tag>
                                    }
                                    @if ((request.requiredCapabilities || []).length > 2) {
                                        <span class="more-count">+{{ request.requiredCapabilities.length - 2 }}</span>
                                    }
                                </div>
                            </td>
                            <td>
                                @if (request.assignedToName) {
                                    <span class="assignee-badge"><i class="pi pi-user"></i> {{ request.assignedToName }}</span>
                                } @else {
                                    <span class="unassigned">—</span>
                                }
                            </td>
                            <td>
                                @if (request.priority) {
                                    <p-tag [value]="getPriorityLabel(request.priority)" [severity]="getPrioritySeverity(request.priority)"></p-tag>
                                } @else { <span class="unassigned">—</span> }
                            </td>
                            <td>
                                <div class="status-cell">
                                    <p-tag [value]="getWorkflowLabel(request.workflowStatus)" [severity]="getWorkflowSeverity(request.workflowStatus)"></p-tag>
                                    <p-select
                                        [ngModel]="request.workflowStatus"
                                        [options]="workflowStatusEditOptions"
                                        optionLabel="label" optionValue="value"
                                        (ngModelChange)="quickChangeStatus(request, $event)"
                                        styleClass="quick-status-select"
                                        [pTooltip]="'admin.projectRequests.quickChangeStatus' | translate">
                                    </p-select>
                                </div>
                            </td>
                            <td>{{ request.createdAt | date:'shortDate' }}</td>
                            <td>
                                <p-button icon="pi pi-pencil" [text]="true" (onClick)="viewRequest(request)" [pTooltip]="'admin.projectRequests.viewDetails' | translate"></p-button>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr><td colspan="10" class="text-center p-4">{{ 'admin.projectRequests.noRequests' | translate }}</td></tr>
                    </ng-template>
                </p-table>
            </div>
        </div>

        <!-- Details / Edit Dialog -->
        <p-dialog [(visible)]="dialogVisible" [modal]="true" [style]="{width: '820px', maxWidth: '96vw'}" [closeOnEscape]="true">
            <ng-template pTemplate="header">
                <div class="dlg-header">
                    <span class="dlg-label">Projects</span>
                    <span class="dlg-title">{{ 'admin.projectRequests.requestDetails' | translate }}</span>
                </div>
            </ng-template>
            @if (selectedRequest) {
                <div class="request-detail">

                    <!-- Header -->
                    <div class="detail-header">
                        <span class="ref-badge">{{ selectedRequest.referenceNumber }}</span>
                        <p-tag [value]="getWorkflowLabel(selectedRequest.workflowStatus)" [severity]="getWorkflowSeverity(selectedRequest.workflowStatus)"></p-tag>
                        @if (selectedRequest.priority) {
                            <p-tag [value]="getPriorityLabel(selectedRequest.priority)" [severity]="getPrioritySeverity(selectedRequest.priority)" styleClass="ml-1"></p-tag>
                        }
                    </div>

                    <!-- Section 1: Customer Contact -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.contactSection' | translate }}</h4>
                        <div class="detail-grid">
                            <div class="form-field"><label>{{ 'admin.projectRequests.customer' | translate }}</label><input pInputText [(ngModel)]="editable.fullName" /></div>
                            <div class="form-field"><label>{{ 'admin.projectRequests.email' | translate }}</label><input pInputText [(ngModel)]="editable.email" /></div>
                            <div class="form-field"><label>{{ 'admin.projectRequests.phone' | translate }}</label><input pInputText [(ngModel)]="editable.phone" /></div>
                        </div>
                    </div>

                    <!-- Section 2: Project Scope -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.requestSection' | translate }}</h4>
                        <div class="detail-grid">
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.projectType' | translate }}</label>
                                <p-select [(ngModel)]="editable.projectType" [options]="projectTypeEditOptions" optionLabel="label" optionValue="value"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.mainDomain' | translate }}</label>
                                <p-select [(ngModel)]="editable.mainDomain" [options]="mainDomainEditOptions" optionLabel="label" optionValue="value"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.projectStage' | translate }}</label>
                                <p-select [(ngModel)]="editable.projectStage" [options]="projectStageEditOptions" optionLabel="label" optionValue="value"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.linkedProject' | translate }}</label>
                                <input pInputText [ngModel]="selectedRequest.projectTitle || '-'" [ngModelOptions]="{standalone: true}" readonly />
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.budget' | translate }}</label>
                                <p-select [(ngModel)]="editable.budgetRange" [options]="budgetOptions" optionLabel="label" optionValue="value" [showClear]="true"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.timeline' | translate }}</label>
                                <p-select [(ngModel)]="editable.timeline" [options]="timelineOptions" optionLabel="label" optionValue="value" [showClear]="true"></p-select>
                            </div>
                            <div class="form-field span-2">
                                <label>{{ 'admin.projectRequests.requiredCapabilities' | translate }}</label>
                                <p-multiselect [(ngModel)]="editable.requiredCapabilities" [options]="capabilityOptions" optionLabel="label" optionValue="value" display="chip" [showToggleAll]="false"></p-multiselect>
                            </div>
                            <div class="form-field span-2">
                                <label>{{ 'admin.projectRequests.description' | translate }}</label>
                                <textarea pTextarea [(ngModel)]="editable.projectDescription" rows="4"></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Section 3: Workflow & Assignment -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.workflowSection' | translate }}</h4>
                        <div class="detail-grid">
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.workflowStatus' | translate }}</label>
                                <p-select [(ngModel)]="editable.workflowStatus" [options]="workflowStatusEditOptions" optionLabel="label" optionValue="value"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.assignedTo' | translate }}</label>
                                <input pInputText [(ngModel)]="editable.assignedToName" [placeholder]="'admin.projectRequests.assignedToPlaceholder' | translate" />
                            </div>
                        </div>
                    </div>

                    <!-- Section 4: Internal Review -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.internalReview' | translate }}</h4>
                        <div class="detail-grid">
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.priority' | translate }}</label>
                                <p-select [(ngModel)]="editable.priority" [options]="priorityOptions" optionLabel="label" optionValue="value" [showClear]="true"></p-select>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.complexityLevel' | translate }}</label>
                                <p-select [(ngModel)]="editable.complexityLevel" [options]="complexityOptions" optionLabel="label" optionValue="value" [showClear]="true"></p-select>
                            </div>
                            <div class="form-field span-2">
                                <label>{{ 'admin.projectRequests.technicalFeasibility' | translate }}</label>
                                <p-select [(ngModel)]="editable.technicalFeasibility" [options]="feasibilityOptions" optionLabel="label" optionValue="value" [showClear]="true"></p-select>
                            </div>
                            <div class="form-field span-2">
                                <label>{{ 'admin.projectRequests.internalNotes' | translate }}</label>
                                <textarea pTextarea [(ngModel)]="editable.internalNotes" rows="3" [placeholder]="'admin.projectRequests.internalNotesPlaceholder' | translate"></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Section 5: Commercial Evaluation -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.commercialEvaluation' | translate }}</h4>
                        <div class="detail-grid">
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.estimatedCost' | translate }}</label>
                                <p-inputnumber [(ngModel)]="editable.estimatedCost" mode="decimal" [minFractionDigits]="2" [maxFractionDigits]="2" prefix="₪ " [placeholder]="'admin.projectRequests.estimatedCostPlaceholder' | translate"></p-inputnumber>
                                <small class="hint">{{ 'admin.projectRequests.estimatedCostHint' | translate }}</small>
                            </div>
                            <div class="form-field">
                                <label>{{ 'admin.projectRequests.estimatedTimeline' | translate }}</label>
                                <input pInputText [(ngModel)]="editable.estimatedTimeline" [placeholder]="'admin.projectRequests.estimatedTimelinePlaceholder' | translate" />
                            </div>
                        </div>
                    </div>

                    <!-- Section 6: Attachments -->
                    @if ((selectedRequest.attachments || []).length || selectedRequest.attachmentUrl) {
                        <div class="detail-section">
                            <h4>{{ 'admin.projectRequests.attachments' | translate }}</h4>
                            <div class="attachment-list">
                                @for (attachment of getAttachments(selectedRequest); track attachment.url) {
                                    <a [href]="attachment.url" target="_blank" rel="noreferrer">
                                        <i class="pi pi-file"></i> {{ attachment.fileName }}
                                    </a>
                                }
                            </div>
                        </div>
                    }

                    <!-- Section 7: Admin Notes -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.adminNotes' | translate }}</h4>
                        <textarea pTextarea [(ngModel)]="adminNotes" rows="3" class="w-full" [placeholder]="'admin.projectRequests.notesPlaceholder' | translate"></textarea>
                    </div>

                    <!-- Section 8: Activity Log -->
                    <div class="detail-section">
                        <h4>{{ 'admin.projectRequests.activityLog' | translate }}</h4>
                        @if (activitiesLoading()) {
                            <div class="activity-loading"><i class="pi pi-spin pi-spinner"></i></div>
                        } @else if (activities().length === 0) {
                            <p class="no-activity">{{ 'admin.projectRequests.noActivity' | translate }}</p>
                        } @else {
                            <div class="activity-timeline">
                                @for (activity of activities(); track activity.id) {
                                    <div class="activity-item">
                                        <div class="activity-icon" [class]="'action-' + activity.actionType.toLowerCase()">
                                            <i [class]="getActivityIcon(activity.actionType)"></i>
                                        </div>
                                        <div class="activity-body">
                                            <span class="activity-description">{{ activity.description }}</span>
                                            <div class="activity-meta">
                                                @if (activity.createdBy) { <span>{{ activity.createdBy }}</span> }
                                                <span>{{ activity.createdAt | date:'short' }}</span>
                                            </div>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                    </div>

                </div>
            }
            <ng-template pTemplate="footer">
                <p-button [label]="'common.cancel' | translate" [text]="true" (onClick)="dialogVisible = false"></p-button>
                <p-button [label]="'common.save' | translate" (onClick)="updateRequest()" [loading]="updating()"></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .requests-admin { padding: 1.5rem; }
        .page-header { margin-bottom: 1.5rem; }
        .header-content { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem; }
        .header-content h1 { margin: 0 0 0.25rem 0; font-size: 1.5rem; }
        .header-content p { margin: 0; color: #6b7280; }
        .filters { display: flex; gap: 0.75rem; flex-wrap: wrap; }
        .filters > * { min-width: 11rem; }
        .ref-number { color: #667eea; font-family: monospace; }
        .contact-info { display: flex; flex-direction: column; gap: 0.125rem; }
        .contact-info small { color: #9ca3af; font-size: 0.75rem; }
        .capability-list { display: flex; flex-wrap: wrap; gap: 0.35rem; align-items: center; }
        .more-count { color: #64748b; font-size: 0.78rem; }
        .assignee-badge { font-size: 0.82rem; color: #374151; display: flex; align-items: center; gap: 0.25rem; }
        .unassigned { color: #d1d5db; }
        .status-cell { display: flex; flex-direction: column; gap: 0.35rem; }
        :host ::ng-deep .quick-status-select { font-size: 0.75rem !important; }
        :host ::ng-deep .quick-status-select .p-select { height: 1.75rem; font-size: 0.75rem; }

        /* Dialog styles */
        .request-detail { display: flex; flex-direction: column; gap: 1.25rem; }
        .detail-header { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
        .ref-badge { background: #f3f4f6; padding: 0.5rem 1rem; border-radius: 8px; font-family: monospace; font-weight: 600; color: #667eea; }
        .detail-section { border: 1px solid #e5e7eb; border-radius: 10px; padding: 1.25rem; }
        .detail-section h4 { margin: 0 0 1rem 0; font-size: 0.85rem; font-weight: 700; color: #374151; text-transform: uppercase; letter-spacing: 0.05em; }
        .detail-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; }
        .form-field { display: flex; flex-direction: column; gap: 0.4rem; }
        .form-field.span-2 { grid-column: span 2; }
        .form-field label { font-size: 0.82rem; font-weight: 600; color: #374151; }
        .form-field .hint { font-size: 0.75rem; color: #9ca3af; margin-top: 0.1rem; }
        .attachment-list { display: flex; flex-wrap: wrap; gap: 0.75rem; }
        .attachment-list a { color: var(--primary-color); text-decoration: none; font-weight: 600; display: flex; align-items: center; gap: 0.35rem; }
        .attachment-list a:hover { text-decoration: underline; }

        /* Activity log */
        .activity-loading, .no-activity { text-align: center; color: #9ca3af; padding: 1rem 0; font-size: 0.9rem; }
        .activity-timeline { display: flex; flex-direction: column; gap: 0; }
        .activity-item { display: flex; gap: 0.85rem; padding: 0.65rem 0; border-bottom: 1px solid #f3f4f6; }
        .activity-item:last-child { border-bottom: none; }
        .activity-icon { width: 2rem; height: 2rem; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; background: #f3f4f6; }
        .activity-icon i { font-size: 0.8rem; color: #6b7280; }
        .activity-icon.action-created { background: #dbeafe; } .activity-icon.action-created i { color: #3b82f6; }
        .activity-icon.action-statuschanged { background: #fef3c7; } .activity-icon.action-statuschanged i { color: #d97706; }
        .activity-icon.action-assigned { background: #d1fae5; } .activity-icon.action-assigned i { color: #059669; }
        .activity-icon.action-noteadded { background: #ede9fe; } .activity-icon.action-noteadded i { color: #7c3aed; }
        .activity-body { display: flex; flex-direction: column; gap: 0.15rem; flex: 1; }
        .activity-description { font-size: 0.875rem; color: #374151; }
        .activity-meta { display: flex; gap: 0.75rem; font-size: 0.75rem; color: #9ca3af; }
        @media (max-width: 768px) { .detail-grid { grid-template-columns: 1fr; } .form-field.span-2 { grid-column: span 1; } }
    `]
})
export class AdminProjectRequestsListComponent implements OnInit {
    private projectsApi = inject(ProjectsApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    loading = signal(false);
    updating = signal(false);
    activitiesLoading = signal(false);
    requests = signal<ProjectRequest[]>([]);
    activities = signal<ProjectRequestActivity[]>([]);

    dialogVisible = false;
    selectedRequest: ProjectRequest | null = null;
    adminNotes = '';
    searchTerm = '';
    workflowStatusFilter: string | null = null;
    projectTypeFilter: string | null = null;
    mainDomainFilter: string | null = null;
    projectStageFilter: string | null = null;
    editable = this.emptyEditable();

    workflowStatusFilterOptions: { value: string | null; label: string }[] = [];
    workflowStatusEditOptions: { value: string; label: string }[] = [];

    projectTypeOptions: { value: string | null; label: string }[] = [
        { value: null, label: '' },
        { value: ProjectRequestProjectType.CustomProject, label: '' },
        { value: ProjectRequestProjectType.BasedOnExistingProduct, label: '' },
        { value: ProjectRequestProjectType.UpgradeModification, label: '' },
        { value: ProjectRequestProjectType.ReverseEngineeringRepair, label: '' }
    ];

    mainDomainOptions: { value: string | null; label: string }[] = [
        { value: null, label: '' },
        { value: ProjectRequestMainDomain.Robotics, label: '' },
        { value: ProjectRequestMainDomain.EmbeddedSystems, label: '' },
        { value: ProjectRequestMainDomain.MedicalDevices, label: '' },
        { value: ProjectRequestMainDomain.IoTSystems, label: '' },
        { value: ProjectRequestMainDomain.IndustrialAutomation, label: '' },
        { value: ProjectRequestMainDomain.MechanicalSystem, label: '' },
        { value: ProjectRequestMainDomain.SoftwareSystem, label: '' },
        { value: ProjectRequestMainDomain.MixedSystem, label: '' }
    ];

    projectStageOptions: { value: string | null; label: string }[] = [
        { value: null, label: '' },
        { value: ProjectRequestStage.Idea, label: '' },
        { value: ProjectRequestStage.ConceptReady, label: '' },
        { value: ProjectRequestStage.PrototypeExists, label: '' },
        { value: ProjectRequestStage.NeedsManufacturing, label: '' },
        { value: ProjectRequestStage.NeedsImprovement, label: '' }
    ];

    capabilityOptions: { value: string; label: string }[] = Object.values(ProjectRequestCapability).map(v => ({ value: v, label: '' }));
    priorityOptions: { value: string; label: string }[] = [];
    feasibilityOptions: { value: string; label: string }[] = [];
    complexityOptions: { value: string; label: string }[] = [];
    budgetOptions: { value: string; label: string }[] = [];
    timelineOptions: { value: string; label: string }[] = [];

    get projectTypeEditOptions() { return this.projectTypeOptions.filter(o => o.value !== null) as { value: string; label: string }[]; }
    get mainDomainEditOptions() { return this.mainDomainOptions.filter(o => o.value !== null) as { value: string; label: string }[]; }
    get projectStageEditOptions() { return this.projectStageOptions.filter(o => o.value !== null) as { value: string; label: string }[]; }

    ngOnInit() {
        this.buildLabels();
        this.translate.onLangChange.subscribe(() => this.buildLabels());
        this.loadRequests();
    }

    private buildLabels(): void {
        const t = (key: string) => this.translate.instant(key);

        this.workflowStatusFilterOptions = [
            { value: null, label: t('admin.projectRequests.allStatuses') },
            { value: ProjectWorkflowStatus.New, label: t('admin.projectRequests.workflow.New') },
            { value: ProjectWorkflowStatus.UnderReview, label: t('admin.projectRequests.workflow.UnderReview') },
            { value: ProjectWorkflowStatus.WaitingForInfo, label: t('admin.projectRequests.workflow.WaitingForInfo') },
            { value: ProjectWorkflowStatus.TechnicalReview, label: t('admin.projectRequests.workflow.TechnicalReview') },
            { value: ProjectWorkflowStatus.QuotationPreparation, label: t('admin.projectRequests.workflow.QuotationPreparation') },
            { value: ProjectWorkflowStatus.QuoteSent, label: t('admin.projectRequests.workflow.QuoteSent') },
            { value: ProjectWorkflowStatus.Approved, label: t('admin.projectRequests.workflow.Approved') },
            { value: ProjectWorkflowStatus.InExecution, label: t('admin.projectRequests.workflow.InExecution') },
            { value: ProjectWorkflowStatus.Completed, label: t('admin.projectRequests.workflow.Completed') },
            { value: ProjectWorkflowStatus.Rejected, label: t('admin.projectRequests.workflow.Rejected') }
        ];
        this.workflowStatusEditOptions = this.workflowStatusFilterOptions
            .filter(o => o.value !== null) as { value: string; label: string }[];

        this.projectTypeOptions = this.projectTypeOptions.map(o => ({
            ...o,
            label: o.value ? t(`projects.request.projectTypeOptions.${o.value}`) : t('admin.projectRequests.allProjectTypes')
        }));
        this.mainDomainOptions = this.mainDomainOptions.map(o => ({
            ...o,
            label: o.value ? t(`projects.request.mainDomainOptions.${o.value}`) : t('admin.projectRequests.allMainDomains')
        }));
        this.projectStageOptions = this.projectStageOptions.map(o => ({
            ...o,
            label: o.value ? t(`projects.request.projectStageOptions.${o.value}`) : t('admin.projectRequests.allProjectStages')
        }));
        this.capabilityOptions = this.capabilityOptions.map(o => ({
            ...o,
            label: t(`projects.request.capabilityOptions.${o.value}`)
        }));
        this.priorityOptions = Object.values(ProjectRequestPriority).map(v => ({
            value: v, label: t(`admin.projectRequests.priorities.${v}`)
        }));
        this.feasibilityOptions = Object.values(ProjectRequestFeasibility).map(v => ({
            value: v, label: t(`admin.projectRequests.feasibility.${v}`)
        }));
        this.complexityOptions = Object.values(ProjectRequestComplexity).map(v => ({
            value: v, label: t(`admin.projectRequests.complexity.${v}`)
        }));
        this.budgetOptions = [
            { value: 'Under100Ils', label: t('projects.request.budgetOptions.Under100Ils') },
            { value: 'Between100And500Ils', label: t('projects.request.budgetOptions.Between100And500Ils') },
            { value: 'Between500And1000Ils', label: t('projects.request.budgetOptions.Between500And1000Ils') },
            { value: 'Between1000And5000Ils', label: t('projects.request.budgetOptions.Between1000And5000Ils') },
            { value: 'Between5000And10000Ils', label: t('projects.request.budgetOptions.Between5000And10000Ils') },
            { value: 'Over10000Ils', label: t('projects.request.budgetOptions.Over10000Ils') },
            { value: 'NotSureYet', label: t('projects.request.budgetOptions.NotSureYet') }
        ];
        this.timelineOptions = [
            { value: 'AsapUrgent', label: t('projects.request.timelineOptions.AsapUrgent') },
            { value: 'OneToTwoWeeks', label: t('projects.request.timelineOptions.OneToTwoWeeks') },
            { value: 'TwoToFourWeeks', label: t('projects.request.timelineOptions.TwoToFourWeeks') },
            { value: 'OneToTwoMonths', label: t('projects.request.timelineOptions.OneToTwoMonths') },
            { value: 'TwoToSixMonths', label: t('projects.request.timelineOptions.TwoToSixMonths') },
            { value: 'Flexible', label: t('projects.request.timelineOptions.Flexible') }
        ];
    }

    loadRequests() {
        this.loading.set(true);
        this.projectsApi.getProjectRequests({
            workflowStatus: this.workflowStatusFilter ?? undefined,
            projectType: this.projectTypeFilter ?? undefined,
            mainDomain: this.mainDomainFilter ?? undefined,
            projectStage: this.projectStageFilter ?? undefined,
            search: this.searchTerm || undefined,
            take: 200
        }).subscribe({
            next: (requests) => { this.requests.set(requests); this.loading.set(false); },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('admin.projectRequests.loadFailed') });
            }
        });
    }

    viewRequest(request: ProjectRequest) {
        this.selectedRequest = request;
        this.adminNotes = request.adminNotes || '';
        this.editable = {
            fullName: request.fullName,
            email: request.email,
            phone: request.phone || '',
            projectType: request.projectType || this.defaultProjectTypeFromLegacy(request.requestType),
            mainDomain: request.mainDomain || '',
            requiredCapabilities: [...(request.requiredCapabilities || [])],
            projectStage: request.projectStage || '',
            budgetRange: request.budgetRange || '',
            timeline: request.timeline || '',
            projectDescription: request.projectDescription || request.description || '',
            workflowStatus: request.workflowStatus || ProjectWorkflowStatus.New,
            assignedToName: request.assignedToName || '',
            assignedToUserId: request.assignedToUserId || '',
            priority: request.priority || '',
            technicalFeasibility: request.technicalFeasibility || '',
            estimatedCost: request.estimatedCost ?? null,
            estimatedTimeline: request.estimatedTimeline || '',
            complexityLevel: request.complexityLevel || '',
            internalNotes: request.internalNotes || ''
        };
        this.dialogVisible = true;
        this.loadActivities(request.id);
    }

    private loadActivities(id: string) {
        this.activitiesLoading.set(true);
        this.activities.set([]);
        this.projectsApi.getProjectRequestActivities(id).subscribe({
            next: (acts) => { this.activities.set(acts); this.activitiesLoading.set(false); },
            error: () => this.activitiesLoading.set(false)
        });
    }

    quickChangeStatus(request: ProjectRequest, newStatus: string) {
        if (newStatus === request.workflowStatus) return;
        this.projectsApi.updateProjectRequest(request.id, { workflowStatus: newStatus }).subscribe({
            next: () => {
                request.workflowStatus = newStatus;
                this.requests.update(list => list.map(r => r.id === request.id ? { ...r, workflowStatus: newStatus } : r));
                this.messageService.add({ severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('admin.projectRequests.statusUpdated') });
            },
            error: (err) => this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: err.error?.message || this.translate.instant('admin.projectRequests.updateFailed') })
        });
    }

    updateRequest() {
        if (!this.selectedRequest) return;
        this.updating.set(true);
        this.projectsApi.updateProjectRequest(this.selectedRequest.id, {
            workflowStatus: this.editable.workflowStatus || undefined,
            adminNotes: this.adminNotes || undefined,
            fullName: this.editable.fullName || undefined,
            email: this.editable.email || undefined,
            phone: this.editable.phone || undefined,
            projectType: this.editable.projectType || undefined,
            mainDomain: this.editable.mainDomain || undefined,
            requiredCapabilities: this.editable.requiredCapabilities,
            projectStage: this.editable.projectStage || undefined,
            budgetRange: this.editable.budgetRange || undefined,
            timeline: this.editable.timeline || undefined,
            description: this.editable.projectDescription || undefined,
            projectDescription: this.editable.projectDescription || undefined,
            assignedToName: this.editable.assignedToName ?? undefined,
            assignedToUserId: this.editable.assignedToUserId || undefined,
            priority: this.editable.priority || undefined,
            technicalFeasibility: this.editable.technicalFeasibility || undefined,
            estimatedCost: this.editable.estimatedCost ?? undefined,
            estimatedTimeline: this.editable.estimatedTimeline || undefined,
            complexityLevel: this.editable.complexityLevel || undefined,
            internalNotes: this.editable.internalNotes ?? undefined
        }).subscribe({
            next: () => {
                this.updating.set(false);
                this.dialogVisible = false;
                this.loadRequests();
                this.messageService.add({ severity: 'success', summary: this.translate.instant('messages.success'), detail: this.translate.instant('admin.projectRequests.statusUpdated') });
            },
            error: (err) => {
                this.updating.set(false);
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: err.error?.message || this.translate.instant('admin.projectRequests.updateFailed') });
            }
        });
    }

    getAttachments(request: ProjectRequest): ProjectRequestAttachment[] {
        if (request.attachments?.length) return request.attachments;
        if (request.attachmentUrl && request.attachmentFileName)
            return [{ url: request.attachmentUrl, fileName: request.attachmentFileName }];
        return [];
    }

    getWorkflowLabel(status: string | null | undefined): string {
        const s = status || ProjectWorkflowStatus.New;
        return this.translate.instant(`admin.projectRequests.workflow.${s}`);
    }

    getWorkflowSeverity(status: string | null | undefined): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' | undefined {
        const map: Record<string, 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast'> = {
            [ProjectWorkflowStatus.New]: 'info',
            [ProjectWorkflowStatus.UnderReview]: 'warn',
            [ProjectWorkflowStatus.WaitingForInfo]: 'warn',
            [ProjectWorkflowStatus.TechnicalReview]: 'secondary',
            [ProjectWorkflowStatus.QuotationPreparation]: 'secondary',
            [ProjectWorkflowStatus.QuoteSent]: 'contrast',
            [ProjectWorkflowStatus.Approved]: 'success',
            [ProjectWorkflowStatus.InExecution]: 'success',
            [ProjectWorkflowStatus.Completed]: 'success',
            [ProjectWorkflowStatus.Rejected]: 'danger'
        };
        return map[status || 'New'] || 'secondary';
    }

    getPriorityLabel(priority: string): string {
        return this.translate.instant(`admin.projectRequests.priorities.${priority}`);
    }

    getPrioritySeverity(priority: string): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' | undefined {
        const map: Record<string, 'info' | 'warn' | 'danger' | 'secondary'> = {
            Low: 'secondary', Medium: 'info', High: 'warn', Critical: 'danger'
        };
        return map[priority] || 'secondary';
    }

    getProjectTypeLabel(projectType?: string | null, requestType?: number): string {
        return projectType
            ? this.translate.instant(`projects.request.projectTypeOptions.${projectType}`)
            : this.translate.instant(requestType === 0 ? 'projects.request.projectTypeOptions.BasedOnExistingProduct' : 'projects.request.projectTypeOptions.CustomProject');
    }

    getMainDomainLabel(mainDomain?: string | null, categoryName?: string | null): string {
        return mainDomain ? this.translate.instant(`projects.request.mainDomainOptions.${mainDomain}`) : (categoryName || '—');
    }

    getCapabilityLabel(capability: string): string {
        return this.translate.instant(`projects.request.capabilityOptions.${capability}`);
    }

    getBudgetLabel(budget?: string | null): string {
        if (!budget) return '—';
        const t = this.translate.instant(`projects.request.budgetOptions.${budget}`);
        return t === `projects.request.budgetOptions.${budget}` ? budget : t;
    }

    getTimelineLabel(timeline?: string | null): string {
        if (!timeline) return '—';
        const t = this.translate.instant(`projects.request.timelineOptions.${timeline}`);
        return t === `projects.request.timelineOptions.${timeline}` ? timeline : t;
    }

    getActivityIcon(actionType: string): string {
        const icons: Record<string, string> = {
            Created: 'pi pi-plus',
            StatusChanged: 'pi pi-sync',
            Assigned: 'pi pi-user',
            NoteAdded: 'pi pi-pencil'
        };
        return icons[actionType] || 'pi pi-circle';
    }

    private defaultProjectTypeFromLegacy(requestType: number): string {
        return requestType === 0 ? ProjectRequestProjectType.BasedOnExistingProduct : ProjectRequestProjectType.CustomProject;
    }

    private emptyEditable() {
        return {
            fullName: '',
            email: '',
            phone: '',
            projectType: '' as string,
            mainDomain: '' as string,
            requiredCapabilities: [] as string[],
            projectStage: '' as string,
            budgetRange: '',
            timeline: '',
            projectDescription: '',
            workflowStatus: ProjectWorkflowStatus.New as string,
            assignedToName: '',
            assignedToUserId: '',
            priority: '',
            technicalFeasibility: '',
            estimatedCost: null as number | null,
            estimatedTimeline: '',
            complexityLevel: '',
            internalNotes: ''
        };
    }
}
