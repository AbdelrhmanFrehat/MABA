export interface Warehouse {
    id?: number;
    code: string;
    nameAr: string;
    nameEn: string;
    isActive: boolean;
}

export interface WarehouseListResponse {
    items: Warehouse[];
    totalCount: number;
}
