export interface LookupType {
    id: string;
    key: string;
    nameEn: string;
    nameAr: string;
    description?: string;
    isSystem: boolean;
    isActive: boolean;
    values?: LookupValue[];
    createdAt: string;
    updatedAt?: string;
}

export interface LookupValue {
    id: string;
    lookupTypeId: string;
    key: string;
    nameEn: string;
    nameAr: string;
    description?: string;
    sortOrder: number;
    isDefault: boolean;
    isSystem: boolean;
    isActive: boolean;
    color?: string;
    icon?: string;
    metaJson?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateLookupTypeRequest {
    key: string;
    nameEn: string;
    nameAr: string;
    description?: string;
}

export interface UpdateLookupTypeRequest {
    nameEn: string;
    nameAr: string;
    description?: string;
    isActive: boolean;
}

export interface CreateLookupValueRequest {
    lookupTypeId: string;
    key: string;
    nameEn: string;
    nameAr: string;
    description?: string;
    sortOrder?: number;
    isDefault?: boolean;
    color?: string;
    icon?: string;
    metaJson?: string;
}

export interface UpdateLookupValueRequest {
    nameEn: string;
    nameAr: string;
    description?: string;
    sortOrder?: number;
    isDefault?: boolean;
    isActive?: boolean;
    color?: string;
    icon?: string;
    metaJson?: string;
}
