import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Unit, UnitListResponse } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class UnitsService {
    private apiUrl = `${environment.apiUrl}/Units`;

    constructor(private http: HttpClient) {}

    /**
     * Get paginated list of units
     */
    getAll(params?: PaginationParams): Observable<PagedResponse<Unit>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
        }

        return this.http.get<PagedResponse<Unit>>(this.apiUrl, { params: httpParams });
    }

    /**
     * Get all units (for dropdowns - fetches all without pagination)
     */
    getAllForDropdown(): Observable<Unit[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Unit>>(this.apiUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    /**
     * Get unit by ID
     */
    getById(id: number): Observable<Unit> {
        return this.http.get<Unit>(`${this.apiUrl}/${id}`);
    }

    /**
     * Create a new unit
     */
    create(unit: Unit): Observable<number> {
        return this.http.post<number>(this.apiUrl, {
            nameAr: unit.nameAr,
            nameEn: unit.nameEn
        });
    }

    /**
     * Update an existing unit
     */
    update(id: number, unit: Unit): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, {
            nameAr: unit.nameAr,
            nameEn: unit.nameEn
        });
    }

    /**
     * Delete a unit (soft delete)
     */
    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
