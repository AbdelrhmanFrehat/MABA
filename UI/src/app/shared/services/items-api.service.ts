import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Item,
    ItemDetail,
    CreateItemRequest,
    UpdateItemRequest,
    ItemStatus,
    ItemMediaLink
} from '../models/item.model';

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface LinkMediaRequest {
    mediaAssetId: string;
    isPrimary: boolean;
    sortOrder: number;
}

@Injectable({
    providedIn: 'root'
})
export class ItemsApiService {
    private baseUrl = environment.apiUrl || '/api';

    constructor(private http: HttpClient) {}

    getAllItems(params?: {
        categoryId?: string;
        brandId?: string;
        itemStatusId?: string;
        tagId?: string;
        minPrice?: number;
        maxPrice?: number;
        page?: number;
        pageSize?: number;
        sortBy?: string;
        sortOrder?: string;
    }): Observable<Item[]> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = (params as any)[key];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return this.http.get<Item[]>(`${this.baseUrl}/items`, { params: httpParams });
    }

    searchItems(searchTerm?: string, params?: {
        categoryId?: string;
        brandId?: string;
        itemStatusId?: string;
        tagId?: string;
        minPrice?: number;
        maxPrice?: number;
        isFeatured?: boolean;
        isNew?: boolean;
        isOnSale?: boolean;
        page?: number;
        pageNumber?: number;
        pageSize?: number;
        sortBy?: string;
        sortOrder?: string;
        sortDescending?: boolean;
    }): Observable<PagedResult<Item>> {
        let httpParams = new HttpParams();
        if (searchTerm) {
            httpParams = httpParams.set('searchTerm', searchTerm);
        }
        if (params) {
            Object.keys(params).forEach(key => {
                const value = (params as any)[key];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return this.http.get<PagedResult<Item>>(`${this.baseUrl}/items/search`, { params: httpParams });
    }

    getItemById(id: string): Observable<Item> {
        return this.http.get<Item>(`${this.baseUrl}/items/${id}`);
    }

    getItemDetail(id: string): Observable<ItemDetail> {
        return this.http.get<ItemDetail>(`${this.baseUrl}/items/${id}/detail`);
    }

    createItem(request: CreateItemRequest): Observable<Item> {
        return this.http.post<Item>(`${this.baseUrl}/items`, request);
    }

    updateItem(id: string, request: UpdateItemRequest): Observable<Item> {
        return this.http.put<Item>(`${this.baseUrl}/items/${id}`, request);
    }

    deleteItem(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/items/${id}`);
    }

    getItemStatuses(): Observable<ItemStatus[]> {
        return this.http.get<ItemStatus[]>(`${this.baseUrl}/items/statuses`);
    }

    getFeaturedItems(): Observable<Item[]> {
        return this.http.get<Item[]>(`${this.baseUrl}/items/featured`);
    }

    getNewItems(): Observable<Item[]> {
        return this.http.get<Item[]>(`${this.baseUrl}/items/new`);
    }

    getOnSaleItems(): Observable<Item[]> {
        return this.http.get<Item[]>(`${this.baseUrl}/items/sale`);
    }

    getRelatedItems(id: string, limit?: number): Observable<Item[]> {
        let httpParams = new HttpParams();
        if (limit) {
            httpParams = httpParams.set('limit', limit.toString());
        }
        return this.http.get<Item[]>(`${this.baseUrl}/items/${id}/related`, { params: httpParams });
    }

    // ============ INVENTORY MANAGEMENT ============

    getInventoryByItemId(itemId: string): Observable<any> {
        return this.http.get<any>(`${this.baseUrl}/inventory/${itemId}`);
    }

    updateInventory(itemId: string, request: { quantityOnHand: number; reorderLevel: number }): Observable<any> {
        return this.http.put<any>(`${this.baseUrl}/inventory/${itemId}`, request);
    }

    // ============ ITEM MEDIA MANAGEMENT ============

    getItemMedia(itemId: string): Observable<ItemMediaLink[]> {
        return this.http.get<ItemMediaLink[]>(`${this.baseUrl}/items/${itemId}/media`);
    }

    linkMediaToItem(itemId: string, request: LinkMediaRequest): Observable<ItemMediaLink> {
        return this.http.post<ItemMediaLink>(`${this.baseUrl}/items/${itemId}/media`, request);
    }

    unlinkMediaFromItem(itemId: string, mediaLinkId: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/items/${itemId}/media/${mediaLinkId}`);
    }

    setPrimaryMedia(itemId: string, mediaLinkId: string): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/items/${itemId}/media/${mediaLinkId}/primary`, {});
    }
}
