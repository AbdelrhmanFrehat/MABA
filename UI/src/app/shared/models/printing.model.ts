// 3D Printing Models

// Material Color interface
export interface MaterialColor {
    id: string;
    materialId: string;
    nameEn: string;
    nameAr: string;
    hexCode: string;
    isActive: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateMaterialColorRequest {
    nameEn: string;
    nameAr: string;
    hexCode: string;
    isActive: boolean;
    sortOrder: number;
}

export interface UpdateMaterialColorRequest {
    id: string;
    nameEn: string;
    nameAr: string;
    hexCode: string;
    isActive: boolean;
    sortOrder: number;
}

// Admin Material interface (matches backend MaterialDto)
export interface Material {
    id: string;
    nameEn: string;
    nameAr: string;
    pricePerGram: number;
    density: number;
    isActive: boolean;
    totalStockGrams: number;
    availableColors: MaterialColor[];
    createdAt: string;
    updatedAt?: string;
}

export interface CreateMaterialRequest {
    nameEn: string;
    nameAr: string;
    pricePerGram: number;
    density: number;
}

export interface UpdateMaterialRequest {
    id: string;
    nameEn: string;
    nameAr: string;
    pricePerGram: number;
    density: number;
    isActive: boolean;
}

export interface Print3dRequest {
    id: string;
    referenceNumber: string;
    status: Print3dRequestStatus;
    workflowStatus?: string;
    materialId?: string;
    materialName?: string;
    materialColorId?: string | null;
    materialColorNameEn?: string | null;
    materialColorNameAr?: string | null;
    materialColorHexCode?: string | null;
    profileId?: string;
    profileName?: string;
    fileName?: string;
    fileSizeBytes: number;
    customerName?: string;
    customerEmail?: string;
    customerNotes?: string;
    adminNotes?: string;
    estimatedPrice?: number;
    finalPrice?: number;
    createdAt: string;
    reviewedAt?: string;
    approvedAt?: string;
    completedAt?: string;
    /** Linked filament spool (admin production tracking). */
    usedSpoolId?: string | null;
    estimatedFilamentGrams?: number | null;
    /** Reserved for future use (optional actual grams). */
    actualFilamentGrams?: number | null;
    /** Server-built label, e.g. "PLA Black — S1 (950g)". */
    usedSpoolName?: string | null;
    /** Set by server after one-time filament deduction on approval. */
    isFilamentDeducted?: boolean;
    estimatedPrintTimeHours?: number | null;
    suggestedPrice?: number | null;
    /** From material (admin pricing assistant). */
    materialPricePerGram?: number | null;
    /** From quality profile (machine cost multiplier). */
    profilePriceMultiplier?: number | null;
}

export enum Print3dRequestStatus {
    Pending = 'Pending',
    UnderReview = 'UnderReview',
    Quoted = 'Quoted',
    Approved = 'Approved',
    Rejected = 'Rejected',
    Queued = 'Queued',
    Slicing = 'Slicing',
    Printing = 'Printing',
    Completed = 'Completed',
    Failed = 'Failed',
    Cancelled = 'Cancelled'
}

export interface Print3dMaterial {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    code: string;
    type: string; // PLA, ABS, PETG, etc.
    color?: string;
    pricePerGram: number;
    currency: string;
    isActive: boolean;
}

export interface Print3dProfile {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    code: string;
    layerHeight: number;
    infillPercentage: number;
    supports: boolean;
    priceMultiplier: number;
    isActive: boolean;
}

// Print Quality Profile (matches backend PrintQualityProfileDto)
export interface PrintQualityProfile {
    id: string;
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    layerHeightMm: number;
    speedCategory: string; // Fast, Normal, Slow
    priceMultiplier: number;
    isDefault: boolean;
    isActive: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CreatePrintQualityProfileRequest {
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    layerHeightMm: number;
    speedCategory: string;
    priceMultiplier: number;
    isDefault: boolean;
    isActive: boolean;
    sortOrder: number;
}

export interface UpdatePrintQualityProfileRequest {
    id: string;
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    layerHeightMm: number;
    speedCategory: string;
    priceMultiplier: number;
    isDefault: boolean;
    isActive: boolean;
    sortOrder: number;
}

export interface Print3dEstimate {
    estimatedPrice: number;
    currency: string;
    estimatedTimeHours: number;
    materialGrams: number;
    breakdown: {
        materialCost: number;
        laborCost: number;
        overheadCost: number;
    };
}

export interface Print3dDesign {
    id: string;
    userId: string;
    userFullName?: string;
    title: string;
    notes?: string;
    isPublic: boolean;
    tags?: string;
    licenseType?: string;
    downloadCount: number;
    likeCount: number;
    files: DesignFile[];
    createdAt: string;
    updatedAt?: string;
}

export interface DesignFile {
    id: string;
    designId: string;
    mediaAssetId: string;
    fileUrl: string;
    originalFileUrl?: string;
    previewModelUrl?: string;
    previewFormat?: string;
    thumbnailUrl?: string;
    fileType?: string;
    isPreviewable?: boolean;
    fileName: string;
    format: string;
    fileSizeBytes: number;
    isPrimary: boolean;
    uploadedAt: string;
    createdAt: string;
}

export interface CreatePrint3dRequestRequest {
    materialId: string;
    materialColorId?: string;
    profileId?: string;
    comments?: string;
    designFile?: File;
}

export interface UpdatePrint3dRequestStatusRequest {
    status: Print3dRequestStatus;
    notes?: string;
    finalPrice?: number;
    usedSpoolId?: string | null;
    estimatedFilamentGrams?: number | null;
    estimatedPrintTimeHours?: number | null;
    suggestedPrice?: number | null;
    rejectionReason?: string;
}

/** POST .../3d-requests/{id}/pricing-suggestion — does not persist. */
export interface Print3dPricingSuggestionRequest {
    estimatedPrintTimeHours: number;
    hourlyRate?: number | null;
    profileId?: string | null;
    estimatedFilamentGrams?: number | null;
    profitMargin?: number | null;
    minimumPrice?: number | null;
    /** 0 = no rounding; omit = use server default. */
    roundToNearest?: number | null;
}

export interface Print3dPricingSuggestionResponse {
    suggestedPrice: number;
    materialCost: number;
    machineCost: number;
    baseCost: number;
    adjustedCost: number;
    afterMargin: number;
    afterMinimum: number;
    minimumApplied: boolean;
    roundingApplied: boolean;
    roundStep?: number | null;
    grams: number;
    costPerGram: number;
    printTimeHours: number;
    hourlyRate: number;
    qualityMultiplier: number;
    profitMargin: number;
    minimumPrice: number;
    defaultHourlyRate: number;
    defaultProfitMargin: number;
    defaultMinimumPrice: number;
}

export interface Print3dRequestListResponse {
    items: Print3dRequest[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

/** Filament spool inventory (admin; Phase 1 — not tied to print requests). */
export interface FilamentSpool {
    id: string;
    materialId: string;
    materialNameEn: string;
    materialNameAr?: string | null;
    materialColorId?: string | null;
    colorNameEn?: string | null;
    colorNameAr?: string | null;
    colorHexCode?: string | null;
    name?: string | null;
    initialWeightGrams: number;
    remainingWeightGrams: number;
    isActive: boolean;
    createdAt: string;
}

export interface CreateFilamentSpoolPayload {
    materialId: string;
    materialColorId: string;
    name?: string | null;
    initialWeightGrams: number;
}

export interface UpdateFilamentSpoolPayload {
    name?: string | null;
    remainingWeightGrams: number;
    isActive: boolean;
}

