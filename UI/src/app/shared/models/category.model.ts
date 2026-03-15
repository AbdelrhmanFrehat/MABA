export interface Category {
    id?: number;
    nameAr: string;
    nameEn: string;
}

export interface CategoryListResponse {
    items: Category[];
    totalCount: number;
}
