import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
    Page,
    HomePageContent,
    CreatePageRequest,
    UpdatePageRequest,
    CreatePageSectionRequest
} from '../models/cms.model';

@Injectable({
    providedIn: 'root'
})
export class CmsApiService {
    constructor(private api: ApiService) {}

    getHomePage(): Observable<HomePageContent> {
        return this.api.get<HomePageContent>('home');
    }

    getPageBySlug(slug: string): Observable<Page> {
        return this.api.get<Page>(`pages/slug/${slug}`);
    }

    getPages(params?: {
        page?: number;
        pageSize?: number;
        isActive?: boolean;
    }): Observable<{ items: Page[]; totalCount: number }> {
        return this.api.get<{ items: Page[]; totalCount: number }>('pages', params);
    }

    getPageById(id: string): Observable<Page> {
        return this.api.getById<Page>('pages', id);
    }

    createPage(request: CreatePageRequest): Observable<Page> {
        return this.api.post<Page>('pages', request);
    }

    updatePage(id: string, request: UpdatePageRequest): Observable<Page> {
        return this.api.put<Page>('pages', id, request);
    }

    deletePage(id: string): Observable<void> {
        return this.api.delete<void>('pages', id);
    }

    publishPage(id: string): Observable<Page> {
        return this.api.post<Page>(`pages/${id}/publish`, {});
    }

    unpublishPage(id: string): Observable<Page> {
        return this.api.post<Page>(`pages/${id}/unpublish`, {});
    }

    addPageSection(pageId: string, request: CreatePageSectionRequest): Observable<Page> {
        return this.api.post<Page>(`pages/${pageId}/sections`, request);
    }

    updatePageSection(pageId: string, sectionId: string, request: Partial<CreatePageSectionRequest>): Observable<Page> {
        return this.api.put<Page>(`pages/${pageId}/sections`, sectionId, request);
    }

    deletePageSection(pageId: string, sectionId: string): Observable<Page> {
        return this.api.delete<Page>(`pages/${pageId}/sections`, sectionId);
    }
}

