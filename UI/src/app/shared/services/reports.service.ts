import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
    SalesReportItem, 
    PurchaseReportItem, 
    ProfitReportItem, 
    StockReportItem, 
    LowStockReportItem 
} from '../models';

export interface ReportDateParams {
    fromDate?: string;
    toDate?: string;
}

export interface SalesReportParams extends ReportDateParams {
    customerId?: number;
    warehouseId?: number;
}

export interface PurchaseReportParams extends ReportDateParams {
    supplierId?: number;
    warehouseId?: number;
}

export interface StockReportParams {
    warehouseId?: number;
    itemId?: number;
}

export interface LowStockReportParams {
    warehouseId?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ReportsService {
    private apiUrl = `${environment.apiUrl}/Reports`;

    constructor(private http: HttpClient) {}

    getSalesReport(params?: SalesReportParams): Observable<SalesReportItem[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
            if (params.customerId) httpParams = httpParams.set('customerId', params.customerId.toString());
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
        }

        return this.http.get<SalesReportItem[]>(`${this.apiUrl}/Sales`, { params: httpParams });
    }

    getPurchasesReport(params?: PurchaseReportParams): Observable<PurchaseReportItem[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
            if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId.toString());
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
        }

        return this.http.get<PurchaseReportItem[]>(`${this.apiUrl}/Purchases`, { params: httpParams });
    }

    getProfitReport(params?: ReportDateParams): Observable<ProfitReportItem[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<ProfitReportItem[]>(`${this.apiUrl}/Profit`, { params: httpParams });
    }

    getStockReport(params?: StockReportParams): Observable<StockReportItem[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
            if (params.itemId) httpParams = httpParams.set('itemId', params.itemId.toString());
        }

        return this.http.get<StockReportItem[]>(`${this.apiUrl}/Stock`, { params: httpParams });
    }

    getLowStockReport(params?: LowStockReportParams): Observable<LowStockReportItem[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
        }

        return this.http.get<LowStockReportItem[]>(`${this.apiUrl}/LowStock`, { params: httpParams });
    }
}

