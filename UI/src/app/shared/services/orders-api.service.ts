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
        pageSize?: number;
        statusId?: string;
        userId?: string;
        dateFrom?: string;
        dateTo?: string;
        paymentStatus?: string;
    }): Observable<OrderListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                if (params[key as keyof typeof params] !== null && params[key as keyof typeof params] !== undefined && params[key as keyof typeof params] !== '') {
                    httpParams = httpParams.set(key, params[key as keyof typeof params]!.toString());
                }
            });
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
            Object.keys(params).forEach(key => {
                if (params[key as keyof typeof params] !== null && params[key as keyof typeof params] !== undefined && params[key as keyof typeof params] !== '') {
                    httpParams = httpParams.set(key, params[key as keyof typeof params]!.toString());
                }
            });
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

    downloadInvoice(id: string): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/orders/${id}/invoice`, { responseType: 'blob' });
    }

    getOrderStatuses(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}/orders/statuses`);
    }
}

