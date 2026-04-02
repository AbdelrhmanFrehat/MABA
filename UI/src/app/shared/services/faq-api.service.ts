import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { FaqItem, CreateFaqPayload, ReorderFaqPayload, FaqCategory } from '../models/faq.model';

@Injectable({ providedIn: 'root' })
export class FaqApiService {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/faq`;
    private adminBaseUrl = `${environment.apiUrl}/admin/faq`;

    getPublicFaq(params: { search?: string; category?: FaqCategory; featured?: boolean } = {}): Observable<FaqItem[]> {
        let httpParams = new HttpParams();
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.category) httpParams = httpParams.set('category', params.category);
        if (params.featured !== undefined) httpParams = httpParams.set('featured', params.featured.toString());
        return this.http.get<FaqItem[]>(this.baseUrl, { params: httpParams });
    }

    getCategories(): Observable<{ value: number; name: string }[]> {
        return this.http.get<{ value: number; name: string }[]>(`${this.baseUrl}/categories`);
    }

    getById(id: string): Observable<FaqItem | null> {
        return this.http.get<FaqItem>(`${this.adminBaseUrl}/${id}`).pipe(
            catchError(() => of(null))
        );
    }

    getAdminFaq(params: { search?: string; category?: FaqCategory; isActive?: boolean } = {}): Observable<FaqItem[]> {
        let httpParams = new HttpParams();
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.category) httpParams = httpParams.set('category', params.category);
        if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        return this.http.get<FaqItem[]>(this.adminBaseUrl, { params: httpParams });
    }

    create(payload: CreateFaqPayload): Observable<FaqItem> {
        const body = {
            category: payload.category,
            questionEn: payload.questionEn,
            questionAr: payload.questionAr || null,
            answerEn: payload.answerEn,
            answerAr: payload.answerAr || null,
            isActive: payload.isActive,
            isFeatured: payload.isFeatured,
            sortOrder: payload.sortOrder
        };
        return this.http.post<FaqItem>(this.adminBaseUrl, body);
    }

    update(id: string, payload: CreateFaqPayload): Observable<void> {
        const body = {
            id,
            category: payload.category,
            questionEn: payload.questionEn,
            questionAr: payload.questionAr || null,
            answerEn: payload.answerEn,
            answerAr: payload.answerAr || null,
            isActive: payload.isActive,
            isFeatured: payload.isFeatured,
            sortOrder: payload.sortOrder
        };
        return this.http.put<void>(`${this.adminBaseUrl}/${id}`, body);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.adminBaseUrl}/${id}`);
    }

    toggleActive(id: string): Observable<void> {
        return this.http.patch<void>(`${this.adminBaseUrl}/${id}/active`, {});
    }

    reorder(payload: ReorderFaqPayload): Observable<void> {
        const body = { items: payload.items };
        return this.http.patch<void>(`${this.adminBaseUrl}/reorder`, body);
    }
}
