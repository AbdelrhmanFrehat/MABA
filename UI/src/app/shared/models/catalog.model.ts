// Catalog Models - Categories, Tags, Brands
export interface Category {
    id: string; // GUID
    parentId?: string;
    nameEn: string;
    nameAr: string;
    slug: string;
    sortOrder: number;
    isActive: boolean;
    imageUrl?: string;
    children?: Category[];
    createdAt: string;
    updatedAt?: string;
}

export interface CreateCategoryRequest {
    parentId?: string;
    nameEn: string;
    nameAr: string;
    slug: string;
    sortOrder: number;
    isActive: boolean;
}

export interface UpdateCategoryRequest {
    parentId?: string;
    nameEn: string;
    nameAr: string;
    slug: string;
    sortOrder: number;
    isActive: boolean;
}

export interface Tag {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    slug: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateTagRequest {
    nameEn: string;
    nameAr: string;
    slug: string;
    isActive: boolean;
}

export interface UpdateTagRequest {
    nameEn: string;
    nameAr: string;
    slug: string;
    isActive: boolean;
}

export interface Brand {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateBrandRequest {
    nameEn: string;
    nameAr: string;
    isActive: boolean;
}

export interface UpdateBrandRequest {
    nameEn: string;
    nameAr: string;
    isActive: boolean;
}


