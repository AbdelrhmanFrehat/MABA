import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment } from '../models';
import { PaginationParams, PagedResponse, PartyType, PaymentDirection, PaymentMethod } from '../models/api-response.model';

export interface PaymentQueryParams extends PaginationParams {
    partyType?: PartyType;
    partyId?: number;
    fromDate?: string;
    toDate?: string;
    direction?: PaymentDirection;
    method?: PaymentMethod;
}

@Injectable({
    providedIn: 'root'
})
export class PaymentsService {
    private apiUrl = `${environment.apiUrl}/Payments`;

    constructor(private http: HttpClient) {}

    getAll(params?: PaymentQueryParams): Observable<PagedResponse<Payment>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.partyType) httpParams = httpParams.set('partyType', params.partyType.toString());
            if (params.partyId) httpParams = httpParams.set('partyId', params.partyId.toString());
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
            if (params.direction) httpParams = httpParams.set('direction', params.direction.toString());
            if (params.method) httpParams = httpParams.set('method', params.method.toString());
        }

        return this.http.get<PagedResponse<Payment>>(this.apiUrl, { params: httpParams });
    }

    getById(id: number): Observable<Payment> {
        return this.http.get<Payment>(`${this.apiUrl}/${id}`);
    }

    create(payment: Payment): Observable<number> {
        return this.http.post<number>(this.apiUrl, payment);
    }

    update(id: number, payment: Payment): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, payment);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

