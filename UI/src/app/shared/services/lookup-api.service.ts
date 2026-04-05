import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    LookupType,
    LookupValue,
    CreateLookupTypeRequest,
    UpdateLookupTypeRequest,
    CreateLookupValueRequest,
    UpdateLookupValueRequest
} from '../models/lookup.model';

@Injectable({ providedIn: 'root' })
export class LookupApiService {
    private baseUrl = `${environment.apiUrl}/lookups`;
    private cache = new Map<string, LookupValue[]>();

    constructor(private http: HttpClient) {}

    // --- Lookup Types ---

    getLookupTypes(): Observable<LookupType[]> {
        return this.http.get<LookupType[]>(this.baseUrl);
    }

    getLookupTypeByKey(key: string): Observable<LookupType> {
        return this.http.get<LookupType>(`${this.baseUrl}/types/${key}`);
    }

    createLookupType(request: CreateLookupTypeRequest): Observable<LookupType> {
        return this.http.post<LookupType>(this.baseUrl, request);
    }

    updateLookupType(id: string, request: UpdateLookupTypeRequest): Observable<LookupType> {
        return this.http.put<LookupType>(`${this.baseUrl}/${id}`, request);
    }

    deleteLookupType(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    // --- Lookup Values ---

    /**
     * Get lookup values by type key.
     * Uses in-memory cache to avoid redundant API calls per session.
     * All dropdowns, statuses, types, categories MUST use this method.
     */
    getValues(typeKey: string, forceRefresh = false): Observable<LookupValue[]> {
        if (!forceRefresh && this.cache.has(typeKey)) {
            return of(this.cache.get(typeKey)!);
        }
        return this.http.get<LookupValue[]>(`${this.baseUrl}/${typeKey}/values`).pipe(
            tap(values => this.cache.set(typeKey, values))
        );
    }

    getValueById(id: string): Observable<LookupValue> {
        return this.http.get<LookupValue>(`${this.baseUrl}/values/${id}`);
    }

    createLookupValue(request: CreateLookupValueRequest): Observable<LookupValue> {
        const result = this.http.post<LookupValue>(`${this.baseUrl}/values`, request);
        // Invalidate cache for the parent type
        return result.pipe(tap(() => this.invalidateCacheForType(request.lookupTypeId)));
    }

    updateLookupValue(id: string, request: UpdateLookupValueRequest): Observable<LookupValue> {
        return this.http.put<LookupValue>(`${this.baseUrl}/values/${id}`, request).pipe(
            tap(() => this.cache.clear()) // Clear all cache on update
        );
    }

    deleteLookupValue(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/values/${id}`).pipe(
            tap(() => this.cache.clear())
        );
    }

    // --- Cache Management ---

    clearCache(): void {
        this.cache.clear();
    }

    private invalidateCacheForType(typeId: string): void {
        // Since we cache by key not by id, clear all
        this.cache.clear();
    }

    /**
     * Preload multiple lookup types at once.
     * Useful for pages that need many lookup dropdowns.
     */
    preload(typeKeys: string[]): void {
        typeKeys.forEach(key => {
            if (!this.cache.has(key)) {
                this.getValues(key).subscribe();
            }
        });
    }
}
