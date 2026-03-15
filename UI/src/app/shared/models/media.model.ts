// Media Models
export interface MediaAsset {
    id: string; // GUID
    fileUrl: string;
    mimeType: string;
    fileName: string;
    fileExtension: string;
    fileSizeBytes: number;
    width?: number;
    height?: number;
    titleEn?: string;
    titleAr?: string;
    altEn?: string;
    altAr?: string;
    uploadedByUserId?: string;
    mediaTypeId: string;
    mediaTypeKey: string;
    createdAt: string;
    updatedAt?: string;
}

export interface UploadMediaRequest {
    file: File;
    mediaTypeId: string;
    titleEn?: string;
    titleAr?: string;
    altEn?: string;
    altAr?: string;
    uploadedByUserId?: string;
}

export interface UpdateMediaRequest {
    titleEn?: string;
    titleAr?: string;
    altEn?: string;
    altAr?: string;
}


