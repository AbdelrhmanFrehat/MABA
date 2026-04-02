export interface CncMaterial {
    id: string;
    nameEn: string;
    nameAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    type: 'routing' | 'pcb' | 'both';
    
    // Core Constraints
    minThicknessMm: number | null;
    maxThicknessMm: number | null;
    isMetal: boolean;
    isActive: boolean;
    sortOrder: number;
    
    // Operation Flags
    allowCut: boolean;
    allowEngrave: boolean;
    allowPocket: boolean;
    allowDrill: boolean;
    
    // Depth Constraints
    maxCutDepthMm: number | null;
    maxEngraveDepthMm: number | null;
    maxPocketDepthMm: number | null;
    maxDrillDepthMm: number | null;
    
    // Operation-specific Notes
    cutNotesEn?: string;
    cutNotesAr?: string;
    engraveNotesEn?: string;
    engraveNotesAr?: string;
    pocketNotesEn?: string;
    pocketNotesAr?: string;
    drillNotesEn?: string;
    drillNotesAr?: string;
    
    // General Notes
    notesEn?: string;
    notesAr?: string;
    
    /** Legacy: true when type is pcb (exclusive PCB mode). */
    isPcbOnly: boolean;

    pcbMaterialType?: string | null;
    supportedBoardThicknesses?: string | null;
    supportsSingleSided: boolean;
    supportsDoubleSided: boolean;

    createdAt: string;
    updatedAt?: string;
}

export type CncOperationType = 'cut' | 'engrave' | 'pocket' | 'drill';
export type CncDepthMode = 'through' | 'custom';
export type CncDesignSourceType = 'production' | 'image';
export type CncServiceMode = 'routing' | 'pcb';
export type PcbMaterialType = 'FR4' | 'FR1';
export type PcbThickness = 1.0 | 1.2 | 1.6 | 2.0;

export interface PcbMaterialOption {
    value: PcbMaterialType;
    labelEn: string;
    labelAr: string;
    descriptionEn: string;
    descriptionAr: string;
    minThickness: number;
    maxThickness: number;
    availableThicknesses: PcbThickness[];
}

export interface PcbThicknessOption {
    value: PcbThickness;
    label: string;
}

export interface CncOperationAvailability {
    operation: CncOperationType;
    available: boolean;
    reason?: string;
    maxDepth?: number;
}

export interface CncServiceRequestPayload {
    serviceMode: CncServiceMode;
    // Routing mode fields
    materialId?: string;
    operationType?: CncOperationType;
    widthMm?: number;
    heightMm?: number;
    thicknessMm?: number;
    depthMode?: CncDepthMode;
    depthMm?: number;
    // PCB mode fields
    pcbMaterial?: PcbMaterialType;
    pcbThickness?: PcbThickness;
    pcbSide?: 'single' | 'double';
    pcbOperation?: string;
    // Common fields
    quantity: number;
    designSourceType: CncDesignSourceType;
    designNotes?: string;
    customerName: string;
    customerEmail: string;
    customerPhone?: string;
    projectDescription?: string;
}

export interface CncServiceRequestResult {
    id: string;
    referenceNumber: string;
    message: string;
}

export interface CreateCncMaterialRequest {
    nameEn: string;
    nameAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    type: 'routing' | 'pcb' | 'both';
    minThicknessMm?: number | null;
    maxThicknessMm?: number | null;
    isMetal?: boolean;
    isActive?: boolean;
    sortOrder?: number;
    allowCut?: boolean;
    allowEngrave?: boolean;
    allowPocket?: boolean;
    allowDrill?: boolean;
    maxCutDepthMm?: number | null;
    maxEngraveDepthMm?: number | null;
    maxPocketDepthMm?: number | null;
    maxDrillDepthMm?: number | null;
    cutNotesEn?: string;
    cutNotesAr?: string;
    engraveNotesEn?: string;
    engraveNotesAr?: string;
    pocketNotesEn?: string;
    pocketNotesAr?: string;
    drillNotesEn?: string;
    drillNotesAr?: string;
    notesEn?: string;
    notesAr?: string;
    pcbMaterialType?: string | null;
    supportedBoardThicknesses?: string | null;
    supportsSingleSided?: boolean;
    supportsDoubleSided?: boolean;
}

export interface UpdateCncMaterialRequest extends CreateCncMaterialRequest {
    id: string;
}

/** Matches API CncServiceRequestStatus (numeric). */
export enum CncServiceRequestStatus {
    Pending = 0,
    InReview = 1,
    Quoted = 2,
    Accepted = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6
}

/** Admin list/detail DTO (camelCase JSON). */
export interface CncServiceRequestDto {
    id: string;
    referenceNumber: string;
    serviceMode: string;
    materialId?: string | null;
    materialNameEn?: string | null;
    materialNameAr?: string | null;
    pcbMaterial?: string | null;
    pcbThickness?: number | null;
    pcbSide?: string | null;
    pcbOperation?: string | null;
    operationType?: string | null;
    widthMm?: number | null;
    heightMm?: number | null;
    thicknessMm?: number | null;
    quantity: number;
    depthMode?: string | null;
    depthMm?: number | null;
    designSourceType: string;
    filePath?: string | null;
    fileName?: string | null;
    designNotes?: string | null;
    customerName: string;
    customerEmail: string;
    customerPhone?: string | null;
    projectDescription?: string | null;
    adminNotes?: string | null;
    estimatedPrice?: number | null;
    finalPrice?: number | null;
    status: CncServiceRequestStatus;
    statusName?: string;
    createdAt: string;
    updatedAt?: string | null;
    reviewedAt?: string | null;
    completedAt?: string | null;
}

export interface CncServiceRequestsListResponse {
    items: CncServiceRequestDto[];
    totalCount: number;
}

export interface CncAdminRequestsQuery {
    status?: CncServiceRequestStatus | null;
    serviceMode?: string | null;
    search?: string | null;
    createdFrom?: string | null;
    createdTo?: string | null;
    skip?: number;
    take?: number;
}
