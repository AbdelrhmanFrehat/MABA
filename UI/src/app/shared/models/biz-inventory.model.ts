export interface BizWarehouse {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    address?: string;
    isDefault: boolean;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface BizWarehouseItem {
    id: string;
    warehouseId: string;
    warehouseName?: string;
    itemId: string;
    itemName?: string;
    itemSku?: string;
    quantityOnHand: number;
    quantityReserved: number;
    quantityOnOrder: number;
    quantityAvailable: number;
    reorderLevel: number;
    costPerUnit: number;
    lastStockInAt?: string;
    lastStockOutAt?: string;
}

export interface BizInventoryMovement {
    id: string;
    warehouseItemId: string;
    itemName?: string;
    itemSku?: string;
    warehouseName?: string;
    movementTypeLookupId: string;
    movementTypeName?: string;
    quantity: number;
    costPerUnit: number;
    sourceDocumentType?: string;
    sourceDocumentId?: string;
    sourceDocumentNumber?: string;
    reason?: string;
    notes?: string;
    createdByUserId?: string;
    createdByName?: string;
    createdAt: string;
}

export interface StockValuation {
    itemId: string;
    itemName: string;
    itemSku: string;
    warehouseId?: string;
    warehouseName?: string;
    quantityOnHand: number;
    costPerUnit: number;
    totalValue: number;
}

// --- Request DTOs ---

export interface CreateBizWarehouseRequest {
    code: string;
    nameEn: string;
    nameAr: string;
    address?: string;
    isDefault?: boolean;
}

export interface UpdateBizWarehouseRequest {
    nameEn: string;
    nameAr: string;
    address?: string;
    isDefault: boolean;
    isActive: boolean;
}

export interface BizAdjustInventoryRequest {
    warehouseId: string;
    itemId: string;
    quantity: number;
    reason: string;
    notes?: string;
}

export interface BizTransferInventoryRequest {
    fromWarehouseId: string;
    toWarehouseId: string;
    itemId: string;
    quantity: number;
    notes?: string;
}
