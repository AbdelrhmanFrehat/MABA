export interface CncMaterial {
    id: string;
    nameEn: string;
    nameAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    type: 'routing' | 'pcb';
    
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
    
    // PCB-only flag
    isPcbOnly: boolean;
    
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
    type: 'routing' | 'pcb';
    minThicknessMm?: number;
    maxThicknessMm?: number;
    isMetal?: boolean;
    isActive?: boolean;
    sortOrder?: number;
    allowCut?: boolean;
    allowEngrave?: boolean;
    allowPocket?: boolean;
    allowDrill?: boolean;
    maxCutDepthMm?: number;
    maxEngraveDepthMm?: number;
    maxPocketDepthMm?: number;
    maxDrillDepthMm?: number;
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
    isPcbOnly?: boolean;
}

export interface UpdateCncMaterialRequest extends CreateCncMaterialRequest {
    id: string;
}
