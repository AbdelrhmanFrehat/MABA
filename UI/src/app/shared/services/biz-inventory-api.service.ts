import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    BizWarehouse, BizWarehouseItem, BizInventoryMovement, StockValuation,
    CreateBizWarehouseRequest, UpdateBizWarehouseRequest,
    BizAdjustInventoryRequest, BizTransferInventoryRequest
} from '../models/biz-inventory.model';

@Injectable({ providedIn: 'root' })
export class BizInventoryApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Warehouses ---

    getWarehouses(): Observable<BizWarehouse[]> {
        return this.http.get<BizWarehouse[]>(`${this.baseUrl}/warehouses`);
    }

    getWarehouseById(id: string): Observable<BizWarehouse> {
        return this.http.get<BizWarehouse>(`${this.baseUrl}/warehouses/${id}`);
    }

    createWarehouse(request: CreateBizWarehouseRequest): Observable<BizWarehouse> {
        return this.http.post<BizWarehouse>(`${this.baseUrl}/warehouses`, request);
    }

    updateWarehouse(id: string, request: UpdateBizWarehouseRequest): Observable<BizWarehouse> {
        return this.http.put<BizWarehouse>(`${this.baseUrl}/warehouses/${id}`, request);
    }

    // --- Stock ---

    getWarehouseStock(warehouseId: string, params?: Record<string, any>): Observable<BizWarehouseItem[]> {
        return this.http.get<BizWarehouseItem[]>(`${this.baseUrl}/warehouses/${warehouseId}/stock`, { params: this.buildParams(params) });
    }

    getItemStock(itemId: string): Observable<BizWarehouseItem[]> {
        return this.http.get<BizWarehouseItem[]>(`${this.baseUrl}/inventory/item/${itemId}/stock`);
    }

    getStockOverview(params?: Record<string, any>): Observable<BizWarehouseItem[]> {
        return this.http.get<BizWarehouseItem[]>(`${this.baseUrl}/inventory/stock`, { params: this.buildParams(params) });
    }

    adjustInventory(request: BizAdjustInventoryRequest): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/inventory/adjust`, request);
    }

    transferInventory(request: BizTransferInventoryRequest): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/inventory/transfer`, request);
    }

    // --- Movements ---

    getInventoryMovements(params?: Record<string, any>): Observable<BizInventoryMovement[]> {
        return this.http.get<BizInventoryMovement[]>(`${this.baseUrl}/inventory/movements`, { params: this.buildParams(params) });
    }

    // --- Valuation ---

    getStockValuation(params?: Record<string, any>): Observable<StockValuation[]> {
        return this.http.get<StockValuation[]>(`${this.baseUrl}/inventory/valuation`, { params: this.buildParams(params) });
    }

    // --- Helpers ---

    private buildParams(params?: Record<string, any>): HttpParams {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return httpParams;
    }
}
