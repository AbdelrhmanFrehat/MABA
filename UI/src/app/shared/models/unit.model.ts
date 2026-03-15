export interface Unit {
    id?: number;
    nameAr: string;
    nameEn: string;
}

export interface UnitListResponse {
    items: Unit[];
    totalCount: number;
}
