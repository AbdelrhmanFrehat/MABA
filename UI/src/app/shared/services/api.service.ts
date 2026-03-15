import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private baseUrl = environment.apiUrl || '/api';

    constructor(private http: HttpClient) {}

    get<T>(endpoint: string, params?: any): Observable<T> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
                    httpParams = httpParams.set(key, params[key].toString());
                }
            });
        }
        return this.http.get<T>(`${this.baseUrl}/${endpoint}`, { params: httpParams });
    }

    getById<T>(endpoint: string, id: number | string): Observable<T> {
        return this.http.get<T>(`${this.baseUrl}/${endpoint}/${id}`);
    }

    post<T>(endpoint: string, data: any): Observable<T> {
        return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data);
    }

    put<T>(endpoint: string, id: number | string, data: any): Observable<T> {
        return this.http.put<T>(`${this.baseUrl}/${endpoint}/${id}`, data);
    }

    delete<T>(endpoint: string, id: number | string): Observable<T> {
        return this.http.delete<T>(`${this.baseUrl}/${endpoint}/${id}`);
    }

    patch<T>(url: string, data?: any): Observable<T> {
        const fullUrl = url.startsWith('http') ? url : `${this.baseUrl}/${url}`;
        return this.http.patch<T>(fullUrl, data ?? {});
    }
}

