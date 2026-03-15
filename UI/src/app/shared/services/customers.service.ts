import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Customer } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

export interface CustomerQueryParams extends PaginationParams {
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class CustomersService {
    private apiUrl = `${environment.apiUrl}/Customers`;

    constructor(private http: HttpClient) {}

    getAll(params?: CustomerQueryParams): Observable<PagedResponse<Customer>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
            if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        }

        return this.http.get<PagedResponse<Customer>>(this.apiUrl, { params: httpParams });
    }

    getAllForDropdown(): Observable<Customer[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Customer>>(this.apiUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    getById(id: number): Observable<Customer> {
        return this.http.get<Customer>(`${this.apiUrl}/${id}`);
    }

    create(customer: Customer): Observable<number> {
        return this.http.post<number>(this.apiUrl, customer);
    }

    update(id: number, customer: Customer): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, customer);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

