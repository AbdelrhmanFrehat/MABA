import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Item } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

export interface ItemQueryParams extends PaginationParams {
    categoryId?: number;
    warehouseId?: number;
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ItemsService {
    private apiUrl = `${environment.apiUrl}/Items`;

    constructor(private http: HttpClient) {}

    getAll(params?: ItemQueryParams): Observable<PagedResponse<Item>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
            if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId.toString());
            if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId.toString());
            if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        }

        return this.http.get<PagedResponse<Item>>(this.apiUrl, { params: httpParams });
    }

    getAllForDropdown(): Observable<Item[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Item>>(this.apiUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    getById(id: number): Observable<Item> {
        return this.http.get<Item>(`${this.apiUrl}/${id}`);
    }

    create(item: Item): Observable<number> {
        return this.http.post<number>(this.apiUrl, item);
    }

    update(id: number, item: Item): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, item);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

