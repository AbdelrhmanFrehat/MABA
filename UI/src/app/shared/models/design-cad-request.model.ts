export type DesignCadRequestType = 'IdeaOnly' | 'ExistingFiles' | 'ReverseEngineering' | 'PhysicalItem' | 'ModifyProduct' | 'MechanicalAssembly';
export type DesignCadTargetProcess = 'General' | 'Print3d' | 'Cnc' | 'Injection' | 'Other';

/** Front-end scenario key for the 6 request-type cards; maps to DesignCadRequestType for API. */
export type CadRequestScenario = 'ideaOnly' | 'existingCad' | 'reverseEngineering' | 'physicalObject' | 'modifyProduct' | 'mechanicalAssembly';

export const SCENARIO_TO_REQUEST_TYPE: Record<CadRequestScenario, DesignCadRequestType> = {
    ideaOnly: 'IdeaOnly',
    existingCad: 'ExistingFiles',
    reverseEngineering: 'ReverseEngineering',
    physicalObject: 'PhysicalItem',
    modifyProduct: 'ModifyProduct',
    mechanicalAssembly: 'MechanicalAssembly'
};

export interface CreateDesignCadRequestPayload {
    requestType: DesignCadRequestType;
    /** Optional; sent when backend supports it. */
    scenario?: CadRequestScenario;
    title: string;
    description?: string;
    targetProcess?: DesignCadTargetProcess;
    intendedUse?: string;
    materialNotes?: string;
    materialPreference?: string;
    approximateDimensions?: string;
    dimensionsNotes?: string;
    toleranceNotes?: string;
    whatNeedsChange?: string;
    changeRequests?: string;
    criticalSurfaces?: string;
    fitmentRequirements?: string;
    purposeAndConstraints?: string;
    whatToModify?: string;
    referenceProductLink?: string;
    measurementsOrConstraints?: string;
    purposeOfAssembly?: string;
    numberOfParts?: string;
    movingPartsOrMechanisms?: string;
    spaceSizeConstraints?: string;
    deadline?: string;
    additionalNotes?: string;
    hasPhysicalPart: boolean;
    legalConfirmation: boolean;
    confirmOwnership?: boolean;
    canDeliverPhysicalPart: boolean;
    canDeliverPhysicalItem?: boolean;
    customerNotes?: string;
}

export interface DesignCadRequestDto {
    id: string;
    referenceNumber: string;
    requestType: string;
    title: string;
    description?: string;
    status: string;
    workflowStatus?: string;
    createdAt: string;
    reviewedAt?: string;
    completedAt?: string;
    attachmentCount?: number;
    attachments?: { id: string; fileName: string; fileSizeBytes: number; uploadedAt: string }[];
    [key: string]: unknown;
}

export interface DesignCadRequestListResponse {
    items: DesignCadRequestDto[];
    totalCount: number;
    page: number;
    pageSize: number;
}
