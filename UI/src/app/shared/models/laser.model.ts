export interface LaserMaterial {
    id: string;
    nameEn: string;
    nameAr: string;
    type: 'cut' | 'engrave' | 'both';
    minThicknessMm: number | null;
    maxThicknessMm: number | null;
    notesEn: string | null;
    notesAr: string | null;
    isMetal: boolean;
    isActive: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt: string | null;
}

export interface CreateLaserMaterialRequest {
    nameEn: string;
    nameAr: string;
    type: 'cut' | 'engrave' | 'both';
    minThicknessMm?: number | null;
    maxThicknessMm?: number | null;
    notesEn?: string | null;
    notesAr?: string | null;
    isMetal?: boolean;
    isActive?: boolean;
    sortOrder?: number;
}

export interface UpdateLaserMaterialRequest {
    id: string;
    nameEn: string;
    nameAr: string;
    type: 'cut' | 'engrave' | 'both';
    minThicknessMm?: number | null;
    maxThicknessMm?: number | null;
    notesEn?: string | null;
    notesAr?: string | null;
    isMetal: boolean;
    isActive: boolean;
    sortOrder: number;
}

export interface LaserServiceRequestResult {
    id: string;
    referenceNumber: string;
    message: string;
}

export interface LaserServiceRequest {
    id: string;
    referenceNumber: string;
    materialId: string;
    materialNameEn: string;
    materialNameAr: string;
    operationMode: 'cut' | 'engrave';
    imagePath: string;
    imageFileName: string;
    widthCm?: number;
    heightCm?: number;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
    customerNotes?: string;
    adminNotes?: string;
    quotedPrice?: number;
    status: LaserServiceRequestStatus;
    statusName: string;
    createdAt: string;
    updatedAt?: string;
    reviewedAt?: string;
    completedAt?: string;
}

export type LaserServiceRequestStatus = 
    | 'Pending'
    | 'UnderReview'
    | 'Quoted'
    | 'Approved'
    | 'InProgress'
    | 'Completed'
    | 'Cancelled'
    | 'Rejected';
