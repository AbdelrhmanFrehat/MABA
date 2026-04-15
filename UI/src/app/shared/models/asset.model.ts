export interface AssetCategory {
    id: string;
    nameEn: string;
    nameAr: string;
    numberingPrefix?: string;
    isActive: boolean;
    createdAt?: string;
    updatedAt?: string;
}

export interface Asset {
    id: string;
    assetNumber: string;
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;

    assetCategoryId: string;
    assetCategoryNameEn?: string;
    assetCategoryNameAr?: string;

    investorUserId: string;
    investorUserFullName?: string;

    acquisitionConditionId: string;
    acquisitionConditionKey?: string;
    acquisitionConditionNameEn?: string;
    acquisitionConditionNameAr?: string;
    conditionNotes?: string;

    approximatePrice: number;
    currency: string;
    acquiredAt: string;

    statusId: string;
    statusKey?: string;
    statusNameEn?: string;
    statusNameAr?: string;

    locationNotes?: string;
    photoMediaId?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface AssetNumberingSetting {
    id?: string;
    prefix: string;
    padWidth: number;
    nextNumber: number;
}

export interface CreateAssetRequest {
    nameEn: string;
    nameAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    assetCategoryId: string;
    investorUserId: string;
    acquisitionConditionId: string;
    conditionNotes?: string;
    approximatePrice: number;
    currency: string;
    acquiredAt: string;
    statusId: string;
    locationNotes?: string;
    photoMediaId?: string;
}

export interface UpdateAssetRequest extends CreateAssetRequest {}

export interface CreateAssetCategoryRequest {
    nameEn: string;
    nameAr: string;
    numberingPrefix?: string;
    isActive: boolean;
}
