import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PurchaseInvoice } from '../models';
import { PaginationParams, PagedResponse, InvoiceStatus } from '../models/api-response.model';

export interface PurchaseInvoiceQueryParams extends PaginationParams {
    supplierId?: number;
    warehouseId?: number;
    status?: InvoiceStatus;
    fromDate?: string;
    toDate?: string;
}

@Injectable({
    providedIn: 'root'
})
export class PurchaseInvoicesService {
    private apiUrl = `${environment.apiUrl}/PurchaseInvoices`;

    constructor(private http: HttpClient) {}

    getAll(params?: PurchaseInvoiceQueryParams): Observable<PagedResponse<PurchaseInvoice>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
            if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId.toString());
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
            if (params.status) httpParams = httpParams.set('status', params.status.toString());
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<PagedResponse<PurchaseInvoice>>(this.apiUrl, { params: httpParams });
    }

    getById(id: number): Observable<PurchaseInvoice> {
        return this.http.get<PurchaseInvoice>(`${this.apiUrl}/${id}`);
    }

    create(invoice: PurchaseInvoice): Observable<number> {
        return this.http.post<number>(this.apiUrl, invoice);
    }

    update(id: number, invoice: PurchaseInvoice): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, invoice);
    }

    confirm(id: number): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${id}/Confirm`, {});
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

