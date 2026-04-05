export interface PriceList {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    currency: string;
    isDefault: boolean;
    isActive: boolean;
    effectiveFrom?: string;
    effectiveTo?: string;
    priority: number;
    items?: PriceListItem[];
    createdAt: string;
    updatedAt?: string;
}

export interface PriceListItem {
    id: string;
    priceListId: string;
    itemId: string;
    itemName?: string;
    itemSku?: string;
    price: number;
    minQuantity?: number;
    maxQuantity?: number;
    unitOfMeasureId?: string;
    unitOfMeasureName?: string;
}

export interface WholesaleRule {
    id: string;
    itemId?: string;
    itemName?: string;
    categoryId?: string;
    categoryName?: string;
    customerTypeLookupId?: string;
    customerTypeName?: string;
    minQuantity?: number;
    priceListId: string;
    priceListName?: string;
    priority: number;
    createdAt: string;
    updatedAt?: string;
}

export interface SupplierItemPrice {
    id: string;
    supplierId: string;
    supplierName?: string;
    itemId: string;
    itemName?: string;
    itemSku?: string;
    purchasePrice: number;
    currency: string;
    unitOfMeasureId?: string;
    unitOfMeasureName?: string;
    minQuantity?: number;
    effectiveFrom?: string;
    effectiveTo?: string;
    supplierSku?: string;
    leadTimeDays?: number;
    isPreferred: boolean;
}

export interface TaxProfile {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    rate: number;
    isDefault: boolean;
    isActive: boolean;
    taxAccountId?: string;
}

export interface UnitOfMeasure {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    isBase: boolean;
    isActive: boolean;
}

export interface UnitConversion {
    id: string;
    fromUnitId: string;
    fromUnitName?: string;
    toUnitId: string;
    toUnitName?: string;
    factor: number;
    itemId?: string;
    itemName?: string;
}

// --- Request DTOs ---

export interface CreatePriceListRequest {
    code: string;
    nameEn: string;
    nameAr: string;
    currency?: string;
    isDefault?: boolean;
    effectiveFrom?: string;
    effectiveTo?: string;
    priority?: number;
}

export interface UpdatePriceListItemsRequest {
    items: { itemId: string; price: number; minQuantity?: number; unitOfMeasureId?: string }[];
}

export interface CreateWholesaleRuleRequest {
    itemId?: string;
    categoryId?: string;
    customerTypeLookupId?: string;
    minQuantity?: number;
    priceListId: string;
    priority?: number;
}

export interface CreateSupplierItemPriceRequest {
    supplierId: string;
    itemId: string;
    purchasePrice: number;
    currency?: string;
    unitOfMeasureId?: string;
    minQuantity?: number;
    effectiveFrom?: string;
    effectiveTo?: string;
    supplierSku?: string;
    leadTimeDays?: number;
    isPreferred?: boolean;
}

export interface CreateTaxProfileRequest {
    code: string;
    nameEn: string;
    nameAr: string;
    rate: number;
    isDefault?: boolean;
    taxAccountId?: string;
}

export interface CreateUnitOfMeasureRequest {
    code: string;
    nameEn: string;
    nameAr: string;
    isBase?: boolean;
}

export interface CreateUnitConversionRequest {
    fromUnitId: string;
    toUnitId: string;
    factor: number;
    itemId?: string;
}
