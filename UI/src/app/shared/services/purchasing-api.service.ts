import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    PurchaseQuotation, PurchaseOrder, PurchaseInvoice, PurchaseReturn,
    CreatePurchaseQuotationRequest, CreatePurchaseOrderRequest,
    CreatePurchaseInvoiceRequest, ConvertPurchaseOrderToInvoiceRequest,
    CreatePurchaseReturnRequest
} from '../models/purchasing.model';

@Injectable({ providedIn: 'root' })
export class PurchasingApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Purchase Quotations ---

    getPurchaseQuotations(params?: Record<string, any>): Observable<PurchaseQuotation[]> {
        return this.http.get<PurchaseQuotation[]>(`${this.baseUrl}/purchase-quotations`, { params: this.buildParams(params) });
    }

    getPurchaseQuotationById(id: string): Observable<PurchaseQuotation> {
        return this.http.get<PurchaseQuotation>(`${this.baseUrl}/purchase-quotations/${id}`);
    }

    createPurchaseQuotation(request: CreatePurchaseQuotationRequest): Observable<PurchaseQuotation> {
        return this.http.post<PurchaseQuotation>(`${this.baseUrl}/purchase-quotations`, request);
    }

    updatePurchaseQuotation(id: string, request: CreatePurchaseQuotationRequest): Observable<PurchaseQuotation> {
        return this.http.put<PurchaseQuotation>(`${this.baseUrl}/purchase-quotations/${id}`, request);
    }

    convertQuotationToOrder(id: string): Observable<PurchaseOrder> {
        return this.http.post<PurchaseOrder>(`${this.baseUrl}/purchase-quotations/${id}/convert-to-order`, {});
    }

    // --- Purchase Orders ---

    getPurchaseOrders(params?: Record<string, any>): Observable<PurchaseOrder[]> {
        return this.http.get<PurchaseOrder[]>(`${this.baseUrl}/purchase-orders`, { params: this.buildParams(params) });
    }

    getPurchaseOrderById(id: string): Observable<PurchaseOrder> {
        return this.http.get<PurchaseOrder>(`${this.baseUrl}/purchase-orders/${id}`);
    }

    createPurchaseOrder(request: CreatePurchaseOrderRequest): Observable<PurchaseOrder> {
        return this.http.post<PurchaseOrder>(`${this.baseUrl}/purchase-orders`, request);
    }

    updatePurchaseOrder(id: string, request: CreatePurchaseOrderRequest): Observable<PurchaseOrder> {
        return this.http.put<PurchaseOrder>(`${this.baseUrl}/purchase-orders/${id}`, request);
    }

    submitPurchaseOrderForApproval(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-orders/${id}/submit`, {});
    }

    approvePurchaseOrder(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-orders/${id}/approve`, {});
    }

    rejectPurchaseOrder(id: string, reason?: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-orders/${id}/reject`, { reason });
    }

    cancelPurchaseOrder(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-orders/${id}/cancel`, {});
    }

    convertPurchaseOrderToInvoice(id: string, request: ConvertPurchaseOrderToInvoiceRequest): Observable<PurchaseInvoice> {
        return this.http.post<PurchaseInvoice>(`${this.baseUrl}/purchase-orders/${id}/convert-to-invoice`, request);
    }

    // --- Purchase Invoices ---

    getPurchaseInvoices(params?: Record<string, any>): Observable<PurchaseInvoice[]> {
        return this.http.get<PurchaseInvoice[]>(`${this.baseUrl}/purchase-invoices`, { params: this.buildParams(params) });
    }

    getPurchaseInvoiceById(id: string): Observable<PurchaseInvoice> {
        return this.http.get<PurchaseInvoice>(`${this.baseUrl}/purchase-invoices/${id}`);
    }

    createPurchaseInvoice(request: CreatePurchaseInvoiceRequest): Observable<PurchaseInvoice> {
        return this.http.post<PurchaseInvoice>(`${this.baseUrl}/purchase-invoices`, request);
    }

    cancelPurchaseInvoice(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-invoices/${id}/cancel`, {});
    }

    // --- Purchase Returns ---

    getPurchaseReturns(params?: Record<string, any>): Observable<PurchaseReturn[]> {
        return this.http.get<PurchaseReturn[]>(`${this.baseUrl}/purchase-returns`, { params: this.buildParams(params) });
    }

    getPurchaseReturnById(id: string): Observable<PurchaseReturn> {
        return this.http.get<PurchaseReturn>(`${this.baseUrl}/purchase-returns/${id}`);
    }

    createPurchaseReturn(request: CreatePurchaseReturnRequest): Observable<PurchaseReturn> {
        return this.http.post<PurchaseReturn>(`${this.baseUrl}/purchase-returns`, request);
    }

    approvePurchaseReturn(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-returns/${id}/approve`, {});
    }

    completePurchaseReturn(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/purchase-returns/${id}/complete`, {});
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
