import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardSummary } from '../models';

export interface DashboardParams {
    fromDate?: string;
    toDate?: string;
}

export interface SalesOverTimeData {
    period: string;
    sales: number;
}

export interface OrdersByStatusData {
    status: string;
    count: number;
}

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private apiUrl = `${environment.apiUrl}/Dashboard`;

    constructor(private http: HttpClient) {}

    getSummary(params?: DashboardParams): Observable<DashboardSummary> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<DashboardSummary>(`${this.apiUrl}/Summary`, { params: httpParams });
    }

    getSalesOverTime(params?: DashboardParams & { periods?: number }): Observable<SalesOverTimeData[]> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
            if (params.periods) httpParams = httpParams.set('periods', params.periods.toString());
        }

        return this.http.get<SalesOverTimeData[]>(`${this.apiUrl}/SalesOverTime`, { params: httpParams });
    }

    getOrdersByStatus(params?: DashboardParams): Observable<OrdersByStatusData[]> {
        let httpParams = new HttpParams();
        if (params) {
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }
        return this.http.get<OrdersByStatusData[]>(`${this.apiUrl}/OrdersByStatus`, { params: httpParams });
    }
}

