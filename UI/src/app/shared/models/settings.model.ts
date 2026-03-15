export interface Settings {
    id?: number;
    companyNameAr: string;
    companyNameEn: string;
    companyAddress?: string;
    companyPhone?: string;
    companyEmail?: string;
    defaultLanguage: 'en' | 'ar';
    dateFormat?: string;
    vatPercentage: number;
    defaultWarehouseId?: number;
    defaultWarehouseNameAr?: string;
    defaultWarehouseNameEn?: string;
    defaultPageSize: number;
}
