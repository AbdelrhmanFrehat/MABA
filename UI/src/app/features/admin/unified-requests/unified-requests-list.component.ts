import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { forkJoin, of } from 'rxjs';
import { AllRequestsService } from '../../../shared/services/all-requests.service';
import { AllRequestDetail, AllRequestListItem, AllRequestType, AllRequestWorkflowStatus, AllRequestsFilters, UpdateAllRequestPayload } from '../../../shared/models/all-requests.model';
import { ServiceRequestStatusBadgeComponent } from '../../../shared/components/service-request-status-badge/service-request-status-badge.component';
import { RequestCommercialPanelComponent } from '../../../shared/components/request-commercial-panel/request-commercial-panel.component';
import { getWorkflowOptions, ServiceWorkflowModule } from '../../../shared/utils/service-request-workflow';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { LaserApiService } from '../../../shared/services/laser-api.service';
import { CncApiService } from '../../../shared/services/cnc-api.service';
import { ProjectRequestCapability, ProjectRequestMainDomain, ProjectRequestProjectType, ProjectRequestStage } from '../../../shared/models/project.model';

@Component({
    selector: 'app-unified-requests-list',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        TranslateModule,
        ButtonModule,
        CardModule,
        DatePickerModule,
        DialogModule,
        InputNumberModule,
        InputTextModule,
        MultiSelectModule,
        SelectModule,
        TableModule,
        TagModule,
        TextareaModule,
        ToastModule,
        TooltipModule,
        ServiceRequestStatusBadgeComponent,
        RequestCommercialPanelComponent
    ],
    providers: [MessageService],
    templateUrl: './unified-requests-list.component.html',
    styleUrl: './unified-requests-list.component.scss'
})
export class UnifiedRequestsListComponent implements OnInit {
    private fb = inject(FormBuilder);
    private requestsApi = inject(AllRequestsService);
    private printingApi = inject(PrintingApiService);
    private laserApi = inject(LaserApiService);
    private cncApi = inject(CncApiService);
    private router = inject(Router);
    private translate = inject(TranslateService);
    private messageService = inject(MessageService);

    loading = signal(false);
    saving = signal(false);
    requests = signal<AllRequestListItem[]>([]);
    totalCount = signal(0);
    typeCounts = signal<{ requestType: string; count: number }[]>([]);
    statusCounts = signal<{ workflowStatus: string; count: number }[]>([]);
    selectedRequest = signal<AllRequestDetail | null>(null);
    dialogVisible = signal(false);

    materials3d = signal<{ label: string; value: string }[]>([]);
    profiles3d = signal<{ label: string; value: string }[]>([]);
    laserMaterials = signal<{ label: string; value: string }[]>([]);
    cncMaterials = signal<{ label: string; value: string }[]>([]);

    typeOptions = [
        { labelKey: 'admin.allRequests.types.all', value: null },
        { labelKey: 'admin.allRequests.types.project', value: 'project' },
        { labelKey: 'admin.allRequests.types.print3d', value: 'print3d' },
        { labelKey: 'admin.allRequests.types.design', value: 'design' },
        { labelKey: 'admin.allRequests.types.designCad', value: 'designCad' },
        { labelKey: 'admin.allRequests.types.laser', value: 'laser' },
        { labelKey: 'admin.allRequests.types.cnc', value: 'cnc' }
    ];

    projectTypeOptions = Object.values(ProjectRequestProjectType).map(value => ({ value, labelKey: `projects.request.projectTypeOptions.${value}` }));
    projectMainDomainOptions = Object.values(ProjectRequestMainDomain).map(value => ({ value, labelKey: `projects.request.mainDomainOptions.${value}` }));
    projectStageOptions = Object.values(ProjectRequestStage).map(value => ({ value, labelKey: `projects.request.projectStageOptions.${value}` }));
    projectCapabilityOptions = Object.values(ProjectRequestCapability).map(value => ({ value, labelKey: `projects.request.capabilityOptions.${value}` }));

    readonly filterForm = this.fb.group({
        requestType: this.fb.control<AllRequestType | null>(null),
        workflowStatus: this.fb.control<AllRequestWorkflowStatus | null>(null),
        createdFrom: this.fb.control<Date | null>(null),
        createdTo: this.fb.control<Date | null>(null),
        keyword: this.fb.control(''),
        customer: this.fb.control(''),
        reference: this.fb.control('')
    });

    readonly editForm = this.fb.group({
        workflowStatus: this.fb.control<AllRequestWorkflowStatus>('New'),
        title: this.fb.control(''),
        description: this.fb.control(''),
        customerName: this.fb.control(''),
        customerEmail: this.fb.control(''),
        customerPhone: this.fb.control(''),
        adminNotes: this.fb.control(''),
        internalNote: this.fb.control(''),
        budgetRange: this.fb.control(''),
        timeline: this.fb.control(''),
        deliveryNotes: this.fb.control(''),
        estimatedPrice: this.fb.control<number | null>(null),
        quotedPrice: this.fb.control<number | null>(null),
        finalPrice: this.fb.control<number | null>(null),
        materialId: this.fb.control<string | null>(null),
        materialColorId: this.fb.control<string | null>(null),
        profileId: this.fb.control<string | null>(null),
        estimatedPrintTimeHours: this.fb.control<number | null>(null),
        suggestedPrice: this.fb.control<number | null>(null),
        estimatedFilamentGrams: this.fb.control<number | null>(null),
        usedSpoolId: this.fb.control<string | null>(null),
        operationMode: this.fb.control(''),
        widthCm: this.fb.control<number | null>(null),
        heightCm: this.fb.control<number | null>(null),
        customerNotes: this.fb.control(''),
        serviceMode: this.fb.control(''),
        operationType: this.fb.control(''),
        widthMm: this.fb.control<number | null>(null),
        heightMm: this.fb.control<number | null>(null),
        thicknessMm: this.fb.control<number | null>(null),
        quantity: this.fb.control<number | null>(null),
        depthMode: this.fb.control(''),
        depthMm: this.fb.control<number | null>(null),
        designSourceType: this.fb.control(''),
        projectDescription: this.fb.control(''),
        pcbMaterial: this.fb.control(''),
        pcbThickness: this.fb.control<number | null>(null),
        pcbSide: this.fb.control(''),
        pcbOperation: this.fb.control(''),
        designNotes: this.fb.control(''),
        requestTypeName: this.fb.control(''),
        projectType: this.fb.control(''),
        mainDomain: this.fb.control(''),
        projectStage: this.fb.control(''),
        requiredCapabilities: this.fb.control<string[]>([]),
        intendedUse: this.fb.control(''),
        materialPreference: this.fb.control(''),
        dimensionsNotes: this.fb.control(''),
        toleranceLevel: this.fb.control(''),
        ipOwnershipConfirmed: this.fb.control(false),
        targetProcess: this.fb.control(''),
        materialNotes: this.fb.control(''),
        toleranceNotes: this.fb.control(''),
        whatNeedsChange: this.fb.control(''),
        criticalSurfaces: this.fb.control(''),
        fitmentRequirements: this.fb.control(''),
        purposeAndConstraints: this.fb.control(''),
        deadline: this.fb.control(''),
        hasPhysicalPart: this.fb.control(false),
        legalConfirmation: this.fb.control(false),
        canDeliverPhysicalPart: this.fb.control(false),
        projectId: this.fb.control(''),
        category: this.fb.control('')
    });

    ngOnInit(): void {
        this.loadRequests();
    }

    get workflowOptions() {
        return getWorkflowOptions(this.currentWorkflowModule, this.translate);
    }

    get currentWorkflowModule(): ServiceWorkflowModule {
        return this.selectedRequest()?.requestType === 'project'
            ? 'project'
            : (this.selectedRequest()?.requestType ?? 'design') as ServiceWorkflowModule;
    }

    loadRequests(page = 1): void {
        const filters = this.buildFilters(page);
        this.loading.set(true);
        this.requestsApi.getAll(filters).subscribe({
            next: (response) => {
                this.requests.set(response.items);
                this.totalCount.set(response.totalCount);
                this.typeCounts.set(response.typeCounts);
                this.statusCounts.set(response.statusCounts);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: this.translate.instant('admin.allRequests.messages.loadFailed') });
            }
        });
    }

    applyFilters(): void {
        this.loadRequests(1);
    }

    resetFilters(): void {
        this.filterForm.reset({
            requestType: null,
            workflowStatus: null,
            createdFrom: null,
            createdTo: null,
            keyword: '',
            customer: '',
            reference: ''
        });
        this.loadRequests(1);
    }

    openManage(request: AllRequestListItem): void {
        this.dialogVisible.set(true);
        this.selectedRequest.set(null);
        this.requestsApi.getById(request.requestType, request.id).subscribe({
            next: (detail) => {
                this.selectedRequest.set(detail);
                this.patchEditForm(detail);
                this.loadReferenceData(detail.requestType);
            },
            error: () => {
                this.dialogVisible.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: this.translate.instant('admin.allRequests.messages.detailFailed') });
            }
        });
    }

    openOriginalModule(request: AllRequestListItem | AllRequestDetail): void {
        this.router.navigateByUrl(request.originalModuleRoute);
    }

    save(): void {
        const selected = this.selectedRequest();
        if (!selected) return;

        this.saving.set(true);
        const payload = this.buildUpdatePayload(selected.requestType);
        this.requestsApi.update(selected.requestType, selected.id, payload).subscribe({
            next: (updated) => {
                this.selectedRequest.set(updated);
                this.patchEditForm(updated);
                this.loadRequests();
                this.saving.set(false);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: this.translate.instant('admin.allRequests.messages.saved') });
            },
            error: () => {
                this.saving.set(false);
                this.messageService.add({ severity: 'error', summary: 'Error', detail: this.translate.instant('admin.allRequests.messages.saveFailed') });
            }
        });
    }

    closeDialog(): void {
        this.dialogVisible.set(false);
        this.selectedRequest.set(null);
        this.editForm.reset();
    }

    onDialogVisibilityChange(visible: boolean): void {
        if (!visible) {
            this.closeDialog();
        }
    }

    translatedTypeLabel(type: string): string {
        return this.translate.instant(`admin.allRequests.types.${type}`);
    }

    private buildFilters(page: number): AllRequestsFilters {
        const value = this.filterForm.getRawValue();
        return {
            requestType: value.requestType,
            workflowStatus: value.workflowStatus,
            createdFrom: value.createdFrom ? value.createdFrom.toISOString() : null,
            createdTo: value.createdTo ? value.createdTo.toISOString() : null,
            keyword: value.keyword || null,
            customer: value.customer || null,
            reference: value.reference || null,
            page,
            pageSize: 20
        };
    }

    private patchEditForm(detail: AllRequestDetail): void {
        this.editForm.patchValue({
            workflowStatus: detail.workflowStatus,
            title: detail.title ?? '',
            description: detail.description ?? '',
            customerName: detail.customerName ?? '',
            customerEmail: detail.customerEmail ?? '',
            customerPhone: detail.customerPhone ?? '',
            adminNotes: detail.adminNotes ?? '',
            internalNote: '',
            budgetRange: detail.budgetRange ?? '',
            timeline: detail.timeline ?? '',
            deliveryNotes: detail.deliveryNotes ?? '',
            estimatedPrice: detail.estimatedPrice ?? null,
            quotedPrice: detail.quotedPrice ?? null,
            finalPrice: detail.finalPrice ?? null,
            materialId: detail.materialId ?? null,
            materialColorId: detail.materialColorId ?? null,
            profileId: detail.profileId ?? null,
            estimatedPrintTimeHours: detail.estimatedPrintTimeHours ?? null,
            suggestedPrice: detail.suggestedPrice ?? null,
            estimatedFilamentGrams: detail.estimatedFilamentGrams ?? null,
            usedSpoolId: detail.usedSpoolId ?? null,
            operationMode: detail.operationMode ?? '',
            widthCm: detail.widthCm ?? null,
            heightCm: detail.heightCm ?? null,
            customerNotes: detail.customerNotes ?? '',
            serviceMode: detail.serviceMode ?? '',
            operationType: detail.operationType ?? '',
            widthMm: detail.widthMm ?? null,
            heightMm: detail.heightMm ?? null,
            thicknessMm: detail.thicknessMm ?? null,
            quantity: detail.quantity ?? null,
            depthMode: detail.depthMode ?? '',
            depthMm: detail.depthMm ?? null,
            designSourceType: detail.designSourceType ?? '',
            projectDescription: detail.projectDescription ?? '',
            pcbMaterial: detail.pcbMaterial ?? '',
            pcbThickness: detail.pcbThickness ?? null,
            pcbSide: detail.pcbSide ?? '',
            pcbOperation: detail.pcbOperation ?? '',
            designNotes: detail.designNotes ?? '',
            requestTypeName: detail.requestTypeName ?? '',
            projectType: detail.projectType ?? '',
            mainDomain: detail.mainDomain ?? '',
            projectStage: detail.projectStage ?? '',
            requiredCapabilities: detail.requiredCapabilities ?? [],
            intendedUse: detail.intendedUse ?? '',
            materialPreference: detail.materialPreference ?? '',
            dimensionsNotes: detail.dimensionsNotes ?? '',
            toleranceLevel: detail.toleranceLevel ?? '',
            ipOwnershipConfirmed: !!detail.ipOwnershipConfirmed,
            targetProcess: detail.targetProcess ?? '',
            materialNotes: detail.materialNotes ?? '',
            toleranceNotes: detail.toleranceNotes ?? '',
            whatNeedsChange: detail.whatNeedsChange ?? '',
            criticalSurfaces: detail.criticalSurfaces ?? '',
            fitmentRequirements: detail.fitmentRequirements ?? '',
            purposeAndConstraints: detail.purposeAndConstraints ?? '',
            deadline: detail.deadline ?? '',
            hasPhysicalPart: !!detail.hasPhysicalPart,
            legalConfirmation: !!detail.legalConfirmation,
            canDeliverPhysicalPart: !!detail.canDeliverPhysicalPart,
            projectId: detail.projectId ?? '',
            category: detail.category ?? ''
        });
    }

    private buildUpdatePayload(requestType: AllRequestType): UpdateAllRequestPayload {
        const raw = this.editForm.getRawValue();
        const common: UpdateAllRequestPayload = {
            workflowStatus: raw.workflowStatus,
            customerName: raw.customerName,
            customerEmail: raw.customerEmail,
            customerPhone: raw.customerPhone,
            adminNotes: raw.adminNotes,
            internalNote: raw.internalNote
        };

        switch (requestType) {
            case 'project':
                return {
                    ...common,
                    requestTypeName: raw.requestTypeName,
                    projectType: raw.projectType,
                    mainDomain: raw.mainDomain,
                    projectStage: raw.projectStage,
                    requiredCapabilities: raw.requiredCapabilities ?? undefined,
                    description: raw.description,
                    budgetRange: raw.budgetRange,
                    timeline: raw.timeline,
                    category: raw.category,
                    projectId: raw.projectId
                };
            case 'print3d':
                return { ...common, customerNotes: raw.customerNotes, estimatedPrice: raw.estimatedPrice, finalPrice: raw.finalPrice, materialId: raw.materialId, profileId: raw.profileId, estimatedPrintTimeHours: raw.estimatedPrintTimeHours, suggestedPrice: raw.suggestedPrice, estimatedFilamentGrams: raw.estimatedFilamentGrams };
            case 'design':
                return { ...common, title: raw.title, description: raw.description, requestTypeName: raw.requestTypeName, intendedUse: raw.intendedUse, materialPreference: raw.materialPreference, dimensionsNotes: raw.dimensionsNotes, toleranceLevel: raw.toleranceLevel, budgetRange: raw.budgetRange, timeline: raw.timeline, quotedPrice: raw.quotedPrice, finalPrice: raw.finalPrice, deliveryNotes: raw.deliveryNotes, ipOwnershipConfirmed: raw.ipOwnershipConfirmed };
            case 'designCad':
                return { ...common, title: raw.title, description: raw.description, requestTypeName: raw.requestTypeName, targetProcess: raw.targetProcess, intendedUse: raw.intendedUse, materialNotes: raw.materialNotes, dimensionsNotes: raw.dimensionsNotes, toleranceNotes: raw.toleranceNotes, whatNeedsChange: raw.whatNeedsChange, criticalSurfaces: raw.criticalSurfaces, fitmentRequirements: raw.fitmentRequirements, purposeAndConstraints: raw.purposeAndConstraints, deadline: raw.deadline, customerNotes: raw.customerNotes, quotedPrice: raw.quotedPrice, finalPrice: raw.finalPrice, hasPhysicalPart: raw.hasPhysicalPart, legalConfirmation: raw.legalConfirmation, canDeliverPhysicalPart: raw.canDeliverPhysicalPart };
            case 'laser':
                return { ...common, customerNotes: raw.customerNotes, materialId: raw.materialId, operationMode: raw.operationMode, widthCm: raw.widthCm, heightCm: raw.heightCm, quotedPrice: raw.quotedPrice };
            case 'cnc':
                return { ...common, serviceMode: raw.serviceMode, materialId: raw.materialId, operationType: raw.operationType, widthMm: raw.widthMm, heightMm: raw.heightMm, thicknessMm: raw.thicknessMm, quantity: raw.quantity, depthMode: raw.depthMode, depthMm: raw.depthMm, designSourceType: raw.designSourceType, designNotes: raw.designNotes, projectDescription: raw.projectDescription, pcbMaterial: raw.pcbMaterial, pcbThickness: raw.pcbThickness, pcbSide: raw.pcbSide, pcbOperation: raw.pcbOperation, estimatedPrice: raw.estimatedPrice, finalPrice: raw.finalPrice };
        }
    }

    private loadReferenceData(requestType: AllRequestType): void {
        if (requestType === 'print3d') {
            forkJoin({
                materials: this.printingApi.getAllMaterials(),
                profiles: this.printingApi.getAllPrintQualityProfiles(true)
            }).subscribe(({ materials, profiles }) => {
                this.materials3d.set(materials.map(x => ({ label: x.nameEn, value: x.id })));
                this.profiles3d.set(profiles.map(x => ({ label: x.nameEn, value: x.id })));
            });
            return;
        }

        if (requestType === 'laser') {
            this.laserApi.getAllMaterials().subscribe(items => {
                this.laserMaterials.set(items.map(x => ({ label: x.nameEn, value: x.id })));
            });
            return;
        }

        if (requestType === 'cnc') {
            this.cncApi.getAllMaterials().subscribe(items => {
                this.cncMaterials.set(items.map(x => ({ label: x.nameEn, value: x.id })));
            });
            return;
        }

        of(null).subscribe();
    }
}
