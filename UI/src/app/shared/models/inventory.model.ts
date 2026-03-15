import { MovementType, ReferenceType } from './api-response.model';

export interface InventoryAdjustmentLine {
    id?: number;
    itemId: number;
    itemNameAr?: string;
    itemNameEn?: string;
    oldQuantity?: number;
    newQuantity: number;
    differenceQuantity?: number;
}

export interface InventoryAdjustment {
    id?: number;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    adjustmentDate: string;
    reason: string;
    notes?: string;
    lines: InventoryAdjustmentLine[];
}

export interface StockMovement {
    id?: number;
    movementDate: string;
    itemId: number;
    itemNameAr?: string;
    itemNameEn?: string;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    movementType: MovementType;
    quantityChange: number;
    beforeQuantity: number;
    afterQuantity: number;
    referenceType?: ReferenceType;
    referenceId?: number;
}

export interface InventoryAdjustmentListResponse {
    items: InventoryAdjustment[];
    totalCount: number;
}

export interface StockMovementListResponse {
    items: StockMovement[];
    totalCount: number;
}
