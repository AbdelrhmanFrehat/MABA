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
    color?: string;
    isActive: boolean;
    stockQuantity: number;
    availableColors: MaterialColor[];
    createdAt: string;
    updatedAt?: string;
}

export interface CreateMaterialRequest {
    nameEn: string;
    nameAr: string;
    pricePerGram: number;
    density: number;
    color?: string;
}

export interface UpdateMaterialRequest {
    id: string;
    nameEn: string;
    nameAr: string;
    pricePerGram: number;
    density: number;
    color?: string;
    isActive: boolean;
    stockQuantity: number;
}

export interface Print3dRequest {
    id: string;
    referenceNumber: string;
    status: Print3dRequestStatus;
    materialId?: string;
    materialName?: string;
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
}

export interface Print3dRequestListResponse {
    items: Print3dRequest[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

