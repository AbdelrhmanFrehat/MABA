export type AllRequestType = 'project' | 'print3d' | 'design' | 'designCad' | 'laser' | 'cnc';

export type AllRequestWorkflowStatus =
    | 'New'
    | 'UnderReview'
    | 'AwaitingCustomerConfirmation'
    | 'Approved'
    | 'InProgress'
    | 'ReadyForDelivery'
    | 'Completed'
    | 'Rejected'
    | 'Cancelled';

export interface AllRequestListItem {
    id: string;
    requestType: AllRequestType;
    referenceNumber: string;
    customerName?: string | null;
    customerEmail?: string | null;
    customerPhone?: string | null;
    title: string;
    summary?: string | null;
    workflowStatus: AllRequestWorkflowStatus;
    legacyStatus?: string | null;
    priority?: string | null;
    createdAt: string;
    originalModuleRoute: string;
    // Commercial pipeline
    customerId?: string | null;
    linkedQuotationId?: string | null;
}

export interface AllRequestHistoryItem {
    status: AllRequestWorkflowStatus;
    timestamp?: string | null;
    reached: boolean;
}

export interface AllRequestAttachment {
    id: string;
    fileName: string;
    url?: string | null;
    fileSizeBytes?: number | null;
    uploadedAt?: string | null;
}

export interface AllRequestDetail extends AllRequestListItem {
    description?: string | null;
    adminNotes?: string | null;
    internalNoteDraft?: string | null;
    requestTypeName?: string | null;
    fileName?: string | null;
    fileUrl?: string | null;
    downloadUrl?: string | null;
    imageUrl?: string | null;
    estimatedPrice?: number | null;
    quotedPrice?: number | null;
    finalPrice?: number | null;
    currency?: string | null;
    budgetRange?: string | null;
    timeline?: string | null;
    deliveryNotes?: string | null;
    reviewedAt?: string | null;
    approvedAt?: string | null;
    completedAt?: string | null;
    materialId?: string | null;
    materialName?: string | null;
    materialColorId?: string | null;
    materialColorName?: string | null;
    profileId?: string | null;
    profileName?: string | null;
    estimatedPrintTimeHours?: number | null;
    suggestedPrice?: number | null;
    estimatedFilamentGrams?: number | null;
    usedSpoolId?: string | null;
    usedSpoolName?: string | null;
    operationMode?: string | null;
    widthCm?: number | null;
    heightCm?: number | null;
    customerNotes?: string | null;
    serviceMode?: string | null;
    operationType?: string | null;
    widthMm?: number | null;
    heightMm?: number | null;
    thicknessMm?: number | null;
    quantity?: number | null;
    depthMode?: string | null;
    depthMm?: number | null;
    designSourceType?: string | null;
    projectDescription?: string | null;
    pcbMaterial?: string | null;
    pcbThickness?: number | null;
    pcbSide?: string | null;
    pcbOperation?: string | null;
    designNotes?: string | null;
    intendedUse?: string | null;
    materialPreference?: string | null;
    dimensionsNotes?: string | null;
    toleranceLevel?: string | null;
    ipOwnershipConfirmed?: boolean | null;
    targetProcess?: string | null;
    materialNotes?: string | null;
    toleranceNotes?: string | null;
    whatNeedsChange?: string | null;
    criticalSurfaces?: string | null;
    fitmentRequirements?: string | null;
    purposeAndConstraints?: string | null;
    deadline?: string | null;
    hasPhysicalPart?: boolean | null;
    legalConfirmation?: boolean | null;
    canDeliverPhysicalPart?: boolean | null;
    projectId?: string | null;
    projectTitle?: string | null;
    category?: string | null;
    projectType?: string | null;
    mainDomain?: string | null;
    projectStage?: string | null;
    requiredCapabilities: string[];
    history: AllRequestHistoryItem[];
    attachments: AllRequestAttachment[];
}

export interface AllRequestTypeCount {
    requestType: AllRequestType;
    count: number;
}

export interface AllRequestStatusCount {
    workflowStatus: AllRequestWorkflowStatus;
    count: number;
}

export interface AllRequestsResponse {
    items: AllRequestListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
    typeCounts: AllRequestTypeCount[];
    statusCounts: AllRequestStatusCount[];
}

export interface AllRequestsFilters {
    requestType?: AllRequestType | null;
    workflowStatus?: AllRequestWorkflowStatus | null;
    createdFrom?: string | null;
    createdTo?: string | null;
    keyword?: string | null;
    customer?: string | null;
    reference?: string | null;
    page?: number;
    pageSize?: number;
}

export interface UpdateAllRequestPayload {
    workflowStatus?: AllRequestWorkflowStatus | null;
    title?: string | null;
    description?: string | null;
    customerName?: string | null;
    customerEmail?: string | null;
    customerPhone?: string | null;
    adminNotes?: string | null;
    internalNote?: string | null;
    budgetRange?: string | null;
    timeline?: string | null;
    deliveryNotes?: string | null;
    estimatedPrice?: number | null;
    quotedPrice?: number | null;
    finalPrice?: number | null;
    materialId?: string | null;
    materialColorId?: string | null;
    profileId?: string | null;
    estimatedPrintTimeHours?: number | null;
    suggestedPrice?: number | null;
    estimatedFilamentGrams?: number | null;
    usedSpoolId?: string | null;
    operationMode?: string | null;
    widthCm?: number | null;
    heightCm?: number | null;
    customerNotes?: string | null;
    serviceMode?: string | null;
    operationType?: string | null;
    widthMm?: number | null;
    heightMm?: number | null;
    thicknessMm?: number | null;
    quantity?: number | null;
    depthMode?: string | null;
    depthMm?: number | null;
    designSourceType?: string | null;
    projectDescription?: string | null;
    pcbMaterial?: string | null;
    pcbThickness?: number | null;
    pcbSide?: string | null;
    pcbOperation?: string | null;
    designNotes?: string | null;
    requestTypeName?: string | null;
    intendedUse?: string | null;
    materialPreference?: string | null;
    dimensionsNotes?: string | null;
    toleranceLevel?: string | null;
    ipOwnershipConfirmed?: boolean | null;
    targetProcess?: string | null;
    materialNotes?: string | null;
    toleranceNotes?: string | null;
    whatNeedsChange?: string | null;
    criticalSurfaces?: string | null;
    fitmentRequirements?: string | null;
    purposeAndConstraints?: string | null;
    deadline?: string | null;
    hasPhysicalPart?: boolean | null;
    legalConfirmation?: boolean | null;
    canDeliverPhysicalPart?: boolean | null;
    projectId?: string | null;
    category?: string | null;
    projectType?: string | null;
    mainDomain?: string | null;
    projectStage?: string | null;
    requiredCapabilities?: string[];
}
