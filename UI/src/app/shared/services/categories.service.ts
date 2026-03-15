import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class CategoriesService {
    private apiUrl = `${environment.apiUrl}/Categories`;

    constructor(private http: HttpClient) {}

    /**
     * Get paginated list of categories
     */
    getAll(params?: PaginationParams): Observable<PagedResponse<Category>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
        }

        return this.http.get<PagedResponse<Category>>(this.apiUrl, { params: httpParams });
    }

    /**
     * Get all categories (for dropdowns - fetches all without pagination)
     */
    getAllForDropdown(): Observable<Category[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Category>>(this.apiUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    /**
     * Get category by ID
     */
    getById(id: number): Observable<Category> {
        return this.http.get<Category>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create a new category
     */
    create(category: Category): Observable<number> {
        return this.http.post<number>(this.apiUrl, {
            nameAr: category.nameAr,
            nameEn: category.nameEn
        });
    }

    /**
     * Update an existing category
     */
    update(id: number, category: Category): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, {
            nameAr: category.nameAr,
            nameEn: category.nameEn
        });
    }

    /**
     * Delete a category (soft delete)
     */
    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
