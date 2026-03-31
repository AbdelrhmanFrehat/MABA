import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { InventoryAdjustment, StockMovement } from '../models';
import { PaginationParams, PagedResponse, MovementType, ReferenceType } from '../models/api-response.model';
import { UpdateInventoryRequest } from '../models/item.model';

export interface InventoryAdjustmentQueryParams extends PaginationParams {
    warehouseId?: number;
    fromDate?: string;
    toDate?: string;
}

export interface StockMovementQueryParams extends PaginationParams {
    itemId?: number;
    warehouseId?: number;
    movementType?: MovementType;
    fromDate?: string;
    toDate?: string;
    referenceType?: ReferenceType;
    referenceId?: number;
}

@Injectable({
    providedIn: 'root'
})
export class InventoryService {
    private adjustmentsUrl = `${environment.apiUrl}/InventoryAdjustments`;
    private movementsUrl = `${environment.apiUrl}/StockMovements`;

    constructor(private http: HttpClient) {}
    
    // Inventory Adjustments
    getAdjustments(params?: InventoryAdjustmentQueryParams): Observable<PagedResponse<InventoryAdjustment>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<PagedResponse<InventoryAdjustment>>(this.adjustmentsUrl, { params: httpParams });
    }

    getAdjustmentById(id: number): Observable<InventoryAdjustment> {
        return this.http.get<InventoryAdjustment>(`${this.adjustmentsUrl}/${id}`);
    }

    createAdjustment(adjustment: InventoryAdjustment): Observable<number> {
        return this.http.post<number>(this.adjustmentsUrl, adjustment);
    }

    updateAdjustment(id: number, adjustment: InventoryAdjustment): Observable<void> {
        return this.http.put<void>(`${this.adjustmentsUrl}/${id}`, adjustment);
    }

    deleteAdjustment(id: number): Observable<void> {
        return this.http.delete<void>(`${this.adjustmentsUrl}/${id}`);
    }

    // Stock Movements (read-only)
    getMovements(params?: StockMovementQueryParams): Observable<PagedResponse<StockMovement>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.itemId) httpParams = httpParams.set('itemId', params.itemId.toString());
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
            if (params.movementType) httpParams = httpParams.set('movementType', params.movementType.toString());
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
            if (params.referenceType) httpParams = httpParams.set('referenceType', params.referenceType.toString());
            if (params.referenceId) httpParams = httpParams.set('referenceId', params.referenceId.toString());
        }

        return this.http.get<PagedResponse<StockMovement>>(this.movementsUrl, { params: httpParams });
    }

    getMovementById(id: number): Observable<StockMovement> {
        return this.http.get<StockMovement>(`${this.movementsUrl}/${id}`);
    }

    // Inventory List (items with inventory data)
    getInventoryList(params?: {
        page?: number;
        pageSize?: number;
        categoryId?: string;
        stockStatus?: string;
        search?: string;
    }): Observable<{ items: any[]; totalCount: number; page: number; pageSize: number; totalPages: number }> {
        let httpParams = new HttpParams();
        const page = params?.page ?? 1;
        const pageSize = params?.pageSize ?? 10;
        const stockStatus = params?.stockStatus ?? '';

        httpParams = httpParams
            .set('pageNumber', page.toString())
            .set('pageSize', pageSize.toString());

        if (params?.categoryId) {
            httpParams = httpParams.set('categoryId', params.categoryId);
        }
        if (params?.search) {
            httpParams = httpParams.set('searchTerm', params.search);
        }
        if (stockStatus === 'inStock') {
            httpParams = httpParams.set('inStock', 'true');
        }

        // Inventory list is sourced from searchable items + embedded inventory payload.
        return this.http.get<{ items: any[]; totalCount: number; pageNumber: number; pageSize: number; totalPages: number }>(
            `${environment.apiUrl}/items/search`,
            { params: httpParams }
        ).pipe(
            map((response) => {
                const sourceItems = response?.items || [];
                let mappedItems = sourceItems.map((item: any) => {
                    const inventory = item.inventory || {};
                    const quantityOnHand = Number(inventory.quantityOnHand ?? 0);
                    const reorderLevel = Number(inventory.reorderLevel ?? 0);
                    return {
                        id: inventory.id || item.id,
                        itemId: item.id,
                        quantityOnHand,
                        reorderLevel,
                        lastStockInAt: inventory.lastStockInAt,
                        item
                    };
                });

                // "lowStock" and "outOfStock" are derived from item inventory values.
                if (stockStatus === 'lowStock') {
                    mappedItems = mappedItems.filter((x: any) => x.quantityOnHand > 0 && x.quantityOnHand <= x.reorderLevel);
                } else if (stockStatus === 'outOfStock') {
                    mappedItems = mappedItems.filter((x: any) => x.quantityOnHand <= 0);
                }

                return {
                    items: mappedItems,
                    totalCount: stockStatus ? mappedItems.length : (response?.totalCount ?? mappedItems.length),
                    page,
                    pageSize,
                    totalPages: stockStatus
                        ? Math.max(1, Math.ceil(mappedItems.length / pageSize))
                        : (response?.totalPages ?? 1)
                };
            })
        );
    }

    adjustInventory(itemId: string, request: UpdateInventoryRequest): Observable<any> {
        return this.http.put<any>(`${environment.apiUrl}/inventory/item/${itemId}`, request);
    }
}

