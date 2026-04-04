import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Order,
    OrderListResponse,
    CreateOrderRequest,
    UpdateOrderStatusRequest,
    AddOrderNoteRequest
} from '../models/order.model';

@Injectable({
    providedIn: 'root'
})
export class OrdersApiService {
    private baseUrl = environment.apiUrl || '/api';

    constructor(private http: HttpClient) {}

    getOrders(params?: {
        page?: number;
        pageNumber?: number;
        pageSize?: number;
        statusId?: string;
        orderStatusId?: string;
        userId?: string;
        dateFrom?: string;
        dateTo?: string;
        searchTerm?: string;
        paymentStatus?: string;
        sortBy?: string;
        sortDescending?: boolean;
    }): Observable<OrderListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            const pageNum = params.pageNumber ?? params.page;
            if (pageNum !== undefined && pageNum !== null) {
                httpParams = httpParams.set('pageNumber', String(pageNum));
            }
            if (params.pageSize !== undefined && params.pageSize !== null) {
                httpParams = httpParams.set('pageSize', String(params.pageSize));
            }
            const orderStatusId = params.orderStatusId ?? params.statusId;
            if (orderStatusId) {
                httpParams = httpParams.set('orderStatusId', orderStatusId);
            }
            if (params.userId) {
                httpParams = httpParams.set('userId', params.userId);
            }
            if (params.dateFrom) {
                httpParams = httpParams.set('dateFrom', params.dateFrom);
            }
            if (params.dateTo) {
                httpParams = httpParams.set('dateTo', params.dateTo);
            }
            if (params.searchTerm) {
                httpParams = httpParams.set('searchTerm', params.searchTerm);
            }
            if (params.paymentStatus) {
                httpParams = httpParams.set('paymentStatus', params.paymentStatus);
            }
            if (params.sortBy) {
                httpParams = httpParams.set('sortBy', params.sortBy);
            }
            if (params.sortDescending !== undefined && params.sortDescending !== null) {
                httpParams = httpParams.set('sortDescending', String(params.sortDescending));
            }
        }
        return this.http.get<OrderListResponse>(`${this.baseUrl}/orders`, { params: httpParams });
    }

    getOrderById(id: string): Observable<Order> {
        return this.http.get<Order>(`${this.baseUrl}/orders/${id}`);
    }

    getUserOrders(userId: string, params?: {
        page?: number;
        pageSize?: number;
        statusId?: string;
    }): Observable<OrderListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            const pageNum = params.page;
            if (pageNum !== undefined && pageNum !== null) {
                httpParams = httpParams.set('pageNumber', String(pageNum));
            }
            if (params.pageSize !== undefined && params.pageSize !== null) {
                httpParams = httpParams.set('pageSize', String(params.pageSize));
            }
            if (params.statusId) {
                httpParams = httpParams.set('orderStatusId', params.statusId);
            }
        }
        return this.http.get<OrderListResponse>(`${this.baseUrl}/orders/user/${userId}`, { params: httpParams });
    }

    createOrder(request: CreateOrderRequest): Observable<Order> {
        return this.http.post<Order>(`${this.baseUrl}/orders`, request);
    }

    updateOrderStatus(id: string, request: UpdateOrderStatusRequest): Observable<Order> {
        return this.http.put<Order>(`${this.baseUrl}/orders/${id}/status`, request);
    }

    addOrderNote(id: string, request: AddOrderNoteRequest): Observable<Order> {
        return this.http.post<Order>(`${this.baseUrl}/orders/${id}/notes`, request);
    }

    /** Store invoices are generated in the browser (see MabaInvoicePdfService). No server /invoice route. */

    getOrderStatuses(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}/orders/statuses`);
    }
}

