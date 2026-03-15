// Item Models
import { MediaAsset } from './media.model';

export interface Item {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    sku: string;
    generalDescriptionEn?: string;
    generalDescriptionAr?: string;
    itemStatusId: string;
    itemStatusKey: string;
    price: number;
    currency: string;
    brandId?: string;
    brandNameEn?: string;
    brandNameAr?: string;
    categoryId?: string;
    categoryNameEn?: string;
    categoryNameAr?: string;
    averageRating: number;
    reviewsCount: number;
    viewsCount: number;
    tagIds: string[];
    inventory?: Inventory;
    primaryImageUrl?: string;
    mediaAssets?: (MediaAsset | ItemMediaLink)[];
    isFeatured?: boolean;
    isNew?: boolean;
    isOnSale?: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateItemRequest {
    nameEn: string;
    nameAr: string;
    sku: string;
    generalDescriptionEn?: string;
    generalDescriptionAr?: string;
    itemStatusId: string;
    price: number;
    currency: string;
    brandId?: string;
    categoryId?: string;
    tagIds?: string[];
    initialQuantity?: number;
    reorderLevel?: number;
}

export interface UpdateItemRequest {
    nameEn: string;
    nameAr: string;
    sku: string;
    generalDescriptionEn?: string;
    generalDescriptionAr?: string;
    itemStatusId: string;
    price: number;
    currency: string;
    brandId?: string;
    categoryId?: string;
    tagIds?: string[];
    isFeatured?: boolean;
    isNew?: boolean;
    isOnSale?: boolean;
}

export interface Inventory {
    id: string; // GUID
    itemId: string;
    quantityOnHand: number;
    reorderLevel: number;
    lastStockInAt?: string;
}

export interface UpdateInventoryRequest {
    quantityOnHand: number;
    reorderLevel: number;
}

export interface ItemSection {
    id: string; // GUID
    itemId: string;
    titleEn: string;
    titleAr: string;
    contentEn?: string;
    contentAr?: string;
    order: number;
    features: ItemSectionFeature[];
    createdAt: string;
}

export interface ItemSectionFeature {
    id: string; // GUID
    sectionId: string;
    labelEn: string;
    labelAr: string;
    valueEn: string;
    valueAr: string;
    order: number;
}

export interface ItemDocument {
    id: string; // GUID
    itemId: string;
    nameEn: string;
    nameAr: string;
    fileUrl: string;
    fileType: string; // PDF, DOC, etc.
    fileSize: number;
    documentType: DocumentType; // Datasheet, Manual, etc.
    order: number;
    createdAt: string;
}

export enum DocumentType {
    Datasheet = 'Datasheet',
    Manual = 'Manual',
    Specification = 'Specification',
    Certificate = 'Certificate',
    Other = 'Other'
}

export interface ItemMediaLink {
    id: string; // GUID
    itemId: string;
    mediaAssetId: string;
    /** URL from API (prefer this for display) */
    fileUrl: string;
    /** @deprecated use fileUrl - kept for backward compatibility */
    mediaAssetUrl?: string;
    titleEn?: string | null;
    titleAr?: string | null;
    altTextEn?: string;
    altTextAr?: string;
    isPrimary: boolean;
    sortOrder: number;
    order?: number; // alias for sortOrder
    mediaType?: MediaType;
}

export enum MediaType {
    Image = 'Image',
    Video = 'Video',
    Document = 'Document',
    Model3D = 'Model3D'
}

export interface ItemDetail {
    item: Item;
    sections: ItemSection[];
    documents: ItemDocument[];
    mediaAssets: ItemMediaLink[];
    compatibleMachines: CompatibleMachine[];
    reviews: any[]; // Review[] - imported separately to avoid circular dependency
    relatedItems: Item[];
}

export interface CompatibleMachine {
    machineId: string;
    machineNameEn: string;
    machineNameAr: string;
    compatibilityNotes?: string;
}

export interface ItemStatus {
    id: string; // GUID
    key: string;
    nameEn: string;
    nameAr: string;
    color?: string;
    order: number;
    isActive: boolean;
}
