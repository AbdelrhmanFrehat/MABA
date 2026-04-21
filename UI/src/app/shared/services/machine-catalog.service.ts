import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    MachineCategory, MachineFamily,
    MachineDefinitionSummary, MachineDefinition, CapabilitiesSection
} from '../models/machine-catalog.model';

@Injectable({ providedIn: 'root' })
export class MachineCatalogService {
    private base = environment.apiUrl || '/api/v1';

    constructor(private http: HttpClient) {}

    // ── Categories ────────────────────────────────────────────────────────

    getCategories(isActive?: boolean): Observable<MachineCategory[]> {
        let params = new HttpParams();
        if (isActive !== undefined) params = params.set('isActive', String(isActive));
        return this.http.get<MachineCategory[]>(`${this.base}/machine-categories`, { params });
    }

    getCategoryById(id: string): Observable<MachineCategory> {
        return this.http.get<MachineCategory>(`${this.base}/machine-categories/${id}`);
    }

    createCategory(payload: Partial<MachineCategory>): Observable<MachineCategory> {
        return this.http.post<MachineCategory>(`${this.base}/machine-categories`, payload);
    }

    updateCategory(id: string, payload: Partial<MachineCategory>): Observable<MachineCategory> {
        return this.http.put<MachineCategory>(`${this.base}/machine-categories/${id}`, { ...payload, id });
    }

    toggleCategoryActive(id: string): Observable<void> {
        return this.http.patch<void>(`${this.base}/machine-categories/${id}/toggle-active`, {});
    }

    // ── Families ──────────────────────────────────────────────────────────

    getFamilies(categoryId?: string, isActive?: boolean): Observable<MachineFamily[]> {
        let params = new HttpParams();
        if (categoryId) params = params.set('categoryId', categoryId);
        if (isActive !== undefined) params = params.set('isActive', String(isActive));
        return this.http.get<MachineFamily[]>(`${this.base}/machine-families`, { params });
    }

    getFamilyById(id: string): Observable<MachineFamily> {
        return this.http.get<MachineFamily>(`${this.base}/machine-families/${id}`);
    }

    createFamily(payload: Partial<MachineFamily>): Observable<MachineFamily> {
        return this.http.post<MachineFamily>(`${this.base}/machine-families`, payload);
    }

    updateFamily(id: string, payload: Partial<MachineFamily>): Observable<MachineFamily> {
        return this.http.put<MachineFamily>(`${this.base}/machine-families/${id}`, { ...payload, id });
    }

    toggleFamilyActive(id: string): Observable<void> {
        return this.http.patch<void>(`${this.base}/machine-families/${id}/toggle-active`, {});
    }

    // ── Definitions ───────────────────────────────────────────────────────

    getDefinitions(params: {
        categoryId?: string;
        familyId?: string;
        activeOnly?: boolean;
        includeDeprecated?: boolean;
        search?: string;
    }): Observable<MachineDefinitionSummary[]> {
        let p = new HttpParams();
        if (params.categoryId) p = p.set('categoryId', params.categoryId);
        if (params.familyId) p = p.set('familyId', params.familyId);
        if (params.activeOnly !== undefined) p = p.set('activeOnly', String(params.activeOnly));
        if (params.includeDeprecated !== undefined) p = p.set('includeDeprecated', String(params.includeDeprecated));
        if (params.search) p = p.set('search', params.search);
        return this.http.get<MachineDefinitionSummary[]>(`${this.base}/machine-definitions`, { params: p });
    }

    getDefinitionById(id: string): Observable<MachineDefinition> {
        return this.http.get<MachineDefinition>(`${this.base}/machine-definitions/${id}`);
    }

    getCapabilities(id: string): Observable<CapabilitiesSection> {
        return this.http.get<CapabilitiesSection>(`${this.base}/machine-definitions/${id}/capabilities`);
    }

    createDefinition(payload: Partial<MachineDefinition>): Observable<MachineDefinition> {
        return this.http.post<MachineDefinition>(`${this.base}/machine-definitions`, payload);
    }

    updateDefinition(id: string, payload: Partial<MachineDefinition>): Observable<MachineDefinition> {
        return this.http.put<MachineDefinition>(`${this.base}/machine-definitions/${id}`, { ...payload, id });
    }

    patchStatus(id: string, patch: { isActive?: boolean; isDeprecated?: boolean; isPublic?: boolean; deprecationNote?: string }): Observable<void> {
        return this.http.patch<void>(`${this.base}/machine-definitions/${id}/status`, patch);
    }

    deleteDefinition(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/machine-definitions/${id}`);
    }

    // ── Image upload ──────────────────────────────────────────────────────

    uploadImage(file: File): Observable<string> {
        const IMAGE_TYPE_ID = '00000000-0000-0000-0000-000000000001';
        const fd = new FormData();
        fd.append('file', file);
        fd.append('mediaTypeId', IMAGE_TYPE_ID);
        fd.append('titleEn', file.name);
        return this.http.post<{ fileUrl: string }>(`${this.base}/media/upload`, fd).pipe(
            map(r => r.fileUrl)
        );
    }
}
