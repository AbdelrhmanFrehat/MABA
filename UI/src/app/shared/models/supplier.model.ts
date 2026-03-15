export interface Supplier {
    id?: number;
    nameAr: string;
    nameEn: string;
    phone?: string;
    email?: string;
    address?: string;
    openingBalance: number;
    notes?: string;
    isActive: boolean;
}

export interface SupplierListResponse {
    items: Supplier[];
    totalCount: number;
}
