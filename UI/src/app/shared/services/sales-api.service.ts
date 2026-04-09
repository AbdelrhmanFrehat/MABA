import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    SalesQuotation, SalesOrder, SalesInvoice, SalesReturn,
    CreateSalesQuotationRequest, CreateSalesOrderRequest,
    CreateSalesInvoiceRequest, ConvertToInvoiceRequest,
    CreateSalesReturnRequest,
    RequestCommercialLinks, RequestCommercialDraft, CreateQuotationFromRequestRequest
} from '../models/sales.model';

@Injectable({ providedIn: 'root' })
export class SalesApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Sales Quotations ---

    getSalesQuotations(params?: Record<string, any>): Observable<SalesQuotation[]> {
        return this.http.get<SalesQuotation[]>(`${this.baseUrl}/sales-quotations`, { params: this.buildParams(params) });
    }

    getSalesQuotationById(id: string): Observable<SalesQuotation> {
        return this.http.get<SalesQuotation>(`${this.baseUrl}/sales-quotations/${id}`);
    }

    createSalesQuotation(request: CreateSalesQuotationRequest): Observable<SalesQuotation> {
        return this.http.post<SalesQuotation>(`${this.baseUrl}/sales-quotations`, request);
    }

    updateSalesQuotation(id: string, request: CreateSalesQuotationRequest): Observable<SalesQuotation> {
        return this.http.put<SalesQuotation>(`${this.baseUrl}/sales-quotations/${id}`, request);
    }

    sendSalesQuotation(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-quotations/${id}/send`, {});
    }

    acceptSalesQuotation(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-quotations/${id}/accept`, {});
    }

    rejectSalesQuotation(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-quotations/${id}/reject`, {});
    }

    convertQuotationToOrder(id: string): Observable<SalesOrder> {
        return this.http.post<SalesOrder>(`${this.baseUrl}/sales-quotations/${id}/convert-to-order`, {});
    }

    createQuotationFromRequest(request: CreateQuotationFromRequestRequest): Observable<SalesQuotation> {
        return this.http.post<SalesQuotation>(`${this.baseUrl}/sales-quotations/from-request`, request);
    }

    getCommercialLinks(requestType: string, requestId: string): Observable<RequestCommercialLinks> {
        return this.http.get<RequestCommercialLinks>(`${this.baseUrl}/sales-quotations/commercial-links/${requestType}/${requestId}`);
    }

    getCommercialDraft(requestType: string, requestId: string): Observable<RequestCommercialDraft> {
        return this.http.get<RequestCommercialDraft>(`${this.baseUrl}/sales-quotations/commercial-draft/${requestType}/${requestId}`);
    }

    // --- Sales Orders ---

    getSalesOrders(params?: Record<string, any>): Observable<SalesOrder[]> {
        return this.http.get<SalesOrder[]>(`${this.baseUrl}/sales-orders`, { params: this.buildParams(params) });
    }

    getSalesOrderById(id: string): Observable<SalesOrder> {
        return this.http.get<SalesOrder>(`${this.baseUrl}/sales-orders/${id}`);
    }

    createSalesOrder(request: CreateSalesOrderRequest): Observable<SalesOrder> {
        return this.http.post<SalesOrder>(`${this.baseUrl}/sales-orders`, request);
    }

    updateSalesOrder(id: string, request: CreateSalesOrderRequest): Observable<SalesOrder> {
        return this.http.put<SalesOrder>(`${this.baseUrl}/sales-orders/${id}`, request);
    }

    submitSalesOrderForApproval(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-orders/${id}/submit`, {});
    }

    approveSalesOrder(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-orders/${id}/approve`, {});
    }

    rejectSalesOrder(id: string, reason?: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-orders/${id}/reject`, { reason });
    }

    cancelSalesOrder(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-orders/${id}/cancel`, {});
    }

    convertSalesOrderToInvoice(id: string, request: ConvertToInvoiceRequest): Observable<SalesInvoice> {
        return this.http.post<SalesInvoice>(`${this.baseUrl}/sales-orders/${id}/convert-to-invoice`, request);
    }

    // --- Sales Invoices ---

    getSalesInvoices(params?: Record<string, any>): Observable<SalesInvoice[]> {
        return this.http.get<SalesInvoice[]>(`${this.baseUrl}/sales-invoices`, { params: this.buildParams(params) });
    }

    getSalesInvoiceById(id: string): Observable<SalesInvoice> {
        return this.http.get<SalesInvoice>(`${this.baseUrl}/sales-invoices/${id}`);
    }

    createSalesInvoice(request: CreateSalesInvoiceRequest): Observable<SalesInvoice> {
        return this.http.post<SalesInvoice>(`${this.baseUrl}/sales-invoices`, request);
    }

    issueSalesInvoice(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-invoices/${id}/issue`, {});
    }

    cancelSalesInvoice(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-invoices/${id}/cancel`, {});
    }

    // --- Sales Returns ---

    getSalesReturns(params?: Record<string, any>): Observable<SalesReturn[]> {
        return this.http.get<SalesReturn[]>(`${this.baseUrl}/sales-returns`, { params: this.buildParams(params) });
    }

    getSalesReturnById(id: string): Observable<SalesReturn> {
        return this.http.get<SalesReturn>(`${this.baseUrl}/sales-returns/${id}`);
    }

    createSalesReturn(request: CreateSalesReturnRequest): Observable<SalesReturn> {
        return this.http.post<SalesReturn>(`${this.baseUrl}/sales-returns`, request);
    }

    approveSalesReturn(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-returns/${id}/approve`, {});
    }

    completeSalesReturn(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/sales-returns/${id}/complete`, {});
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
