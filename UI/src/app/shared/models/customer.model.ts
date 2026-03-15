export interface Customer {
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

export interface CustomerListResponse {
    items: Customer[];
    totalCount: number;
}
