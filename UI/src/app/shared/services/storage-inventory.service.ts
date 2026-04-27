import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface StorageVariantDto {
    id: string;
    parentId: string;
    variantLabel?: string | null;
    sku: string;
    quantity: number;
    unit: string;
    package?: string | null;
    value?: string | null;
    valueUnit?: string | null;
    tolerance?: string | null;
    voltageRating?: string | null;
    currentRating?: string | null;
    powerRating?: string | null;
    notes?: string | null;
    imageUrl?: string | null;
    datasheetUrl?: string | null;
    isActive: boolean;
    isPublishedToShop: boolean;
    createdAt: string;
    updatedAt?: string | null;
}

export interface StorageParentDto {
    id: string;
    name: string;
    description?: string | null;
    category: string;
    manufacturer?: string | null;
    imageUrl?: string | null;
    datasheetUrl?: string | null;
    isPublishedToShop: boolean;
    isActive: boolean;
    sortOrder: number;
    variantCount: number;
    totalQuantity: number;
    createdAt: string;
    updatedAt?: string | null;
    variants?: StorageVariantDto[] | null;
}

export interface UpsertStorageParentRequest {
    name: string;
    description?: string | null;
    category: string;
    manufacturer?: string | null;
    imageUrl?: string | null;
    datasheetUrl?: string | null;
    isPublishedToShop: boolean;
    isActive: boolean;
    sortOrder: number;
}

export interface UpsertStorageVariantRequest {
    variantLabel?: string | null;
    sku: string;
    quantity: number;
    unit: string;
    package?: string | null;
    value?: string | null;
    valueUnit?: string | null;
    tolerance?: string | null;
    voltageRating?: string | null;
    currentRating?: string | null;
    powerRating?: string | null;
    notes?: string | null;
    imageUrl?: string | null;
    datasheetUrl?: string | null;
    isActive: boolean;
    isPublishedToShop: boolean;
}

@Injectable({ providedIn: 'root' })
export class StorageInventoryService {
    private base = `${environment.apiUrl}/storage-inventory`;
    private http = inject(HttpClient);

    getAll(params?: { search?: string; category?: string; publishedToShop?: boolean; lowStock?: boolean }): Observable<StorageParentDto[]> {
        let p = new HttpParams();
        if (params?.search) p = p.set('search', params.search);
        if (params?.category) p = p.set('category', params.category);
        if (params?.publishedToShop != null) p = p.set('publishedToShop', String(params.publishedToShop));
        if (params?.lowStock) p = p.set('lowStock', 'true');
        return this.http.get<StorageParentDto[]>(this.base, { params: p });
    }

    getById(id: string): Observable<StorageParentDto> {
        return this.http.get<StorageParentDto>(`${this.base}/${id}`);
    }

    create(req: UpsertStorageParentRequest): Observable<StorageParentDto> {
        return this.http.post<StorageParentDto>(this.base, req);
    }

    update(id: string, req: UpsertStorageParentRequest): Observable<StorageParentDto> {
        return this.http.put<StorageParentDto>(`${this.base}/${id}`, req);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }

    toggleShop(id: string): Observable<StorageParentDto> {
        return this.http.patch<StorageParentDto>(`${this.base}/${id}/toggle-shop`, {});
    }

    addVariant(parentId: string, req: UpsertStorageVariantRequest): Observable<StorageVariantDto> {
        return this.http.post<StorageVariantDto>(`${this.base}/${parentId}/variants`, req);
    }

    updateVariant(parentId: string, variantId: string, req: UpsertStorageVariantRequest): Observable<StorageVariantDto> {
        return this.http.put<StorageVariantDto>(`${this.base}/${parentId}/variants/${variantId}`, req);
    }

    deleteVariant(parentId: string, variantId: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/${parentId}/variants/${variantId}`);
    }

    setQuantity(parentId: string, variantId: string, quantity: number): Observable<StorageVariantDto> {
        return this.http.patch<StorageVariantDto>(`${this.base}/${parentId}/variants/${variantId}/quantity`, { quantity });
    }

    toggleVariantShop(parentId: string, variantId: string): Observable<StorageVariantDto> {
        return this.http.patch<StorageVariantDto>(`${this.base}/${parentId}/variants/${variantId}/toggle-shop`, {});
    }
}

export const STORAGE_CATEGORIES = [
    'Passive Components',
    'Semiconductors',
    'Microcontrollers & Dev Boards',
    'Sensors',
    'Power Management',
    'Connectors & Cables',
    'Displays',
    'Motors & Actuators',
    'RF & Wireless',
    'Tools & Accessories',
    'Mechanical Parts',
    'Other'
];
