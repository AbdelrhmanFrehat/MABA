/** Design & CAD service request types (customer starting point). */
export type DesignRequestType =
    | 'IdeaOnly'
    | 'BrokenDesign'
    | 'PhysicalObject'
    | 'ExistingCAD'
    | 'TechnicalDrawings'
    | 'ImproveExistingProduct';

/** Intended use. */
export type IntendedUse = 'prototype' | 'final' | 'academic' | 'industrial';

/** Tolerance level. */
export type ToleranceLevel = 'standard' | 'precise' | 'unknown';

/** Status for admin pipeline. */
export type DesignRequestStatus =
    | 'New'
    | 'UnderReview'
    | 'Quoted'
    | 'Approved'
    | 'InProgress'
    | 'Delivered'
    | 'Closed'
    | 'Rejected'
    | 'Cancelled';

export const DESIGN_REQUEST_TYPES: DesignRequestType[] = [
    'IdeaOnly',
    'BrokenDesign',
    'PhysicalObject',
    'ExistingCAD',
    'TechnicalDrawings',
    'ImproveExistingProduct'
];

export interface DesignServiceRequestAttachmentDto {
    id: string;
    fileName: string;
    fileSizeBytes: number;
    contentType: string;
    uploadedAt: string;
}

export interface DesignServiceRequestDto {
    id: string;
    referenceNumber: string;
    /** From API list endpoint may omit full attachments; detail includes attachments. */
    attachmentCount?: number;
    userId?: string;
    requestType: DesignRequestType;
    title: string;
    description?: string;
    intendedUse?: string;
    materialPreference?: string;
    dimensionsNotes?: string;
    toleranceLevel?: ToleranceLevel;
    budgetRange?: string;
    timeline?: string;
    ipOwnershipConfirmed?: boolean;
    status: DesignRequestStatus;
    workflowStatus?: string;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
    adminNotes?: string;
    quotedPrice?: number;
    finalPrice?: number;
    deliveryNotes?: string;
    createdAt: string;
    updatedAt: string;
    reviewedAt?: string;
    quotedAt?: string;
    deliveredAt?: string;
    attachments?: DesignServiceRequestAttachmentDto[];
}

/** Payload for creating a design request (Phase 2 will use multipart/form-data). */
export interface CreateDesignRequestPayload {
    requestType: DesignRequestType;
    title: string;
    description?: string;
    intendedUse?: string;
    materialPreference?: string;
    dimensionsNotes?: string;
    toleranceLevel?: ToleranceLevel;
    budgetRange?: string;
    timeline?: string;
    ipOwnershipConfirmed?: boolean;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
    /** File references from upload step (mock: not sent until Phase 2). */
    attachmentFileNames?: string[];
}

export interface CreateDesignRequestResponse {
    id: string;
    referenceNumber: string;
}

export interface DesignRequestListResponse {
    items: DesignServiceRequestDto[];
    totalCount: number;
    page: number;
    pageSize: number;
}
