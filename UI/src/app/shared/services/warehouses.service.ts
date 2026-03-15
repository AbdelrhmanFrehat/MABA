import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Warehouse } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

export interface WarehouseQueryParams extends PaginationParams {
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class WarehousesService {
    private apiUrl = `${environment.apiUrl}/Warehouses`;

    constructor(private http: HttpClient) {}

    getAll(params?: WarehouseQueryParams): Observable<PagedResponse<Warehouse>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
            if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        }

        return this.http.get<PagedResponse<Warehouse>>(this.apiUrl, { params: httpParams });
    }

    getAllForDropdown(): Observable<Warehouse[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Warehouse>>(this.apiUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    getById(id: number): Observable<Warehouse> {
        return this.http.get<Warehouse>(`${this.apiUrl}/${id}`);
    }

    create(warehouse: Warehouse): Observable<number> {
        return this.http.post<number>(this.apiUrl, warehouse);
    }

    update(id: number, warehouse: Warehouse): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, warehouse);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

