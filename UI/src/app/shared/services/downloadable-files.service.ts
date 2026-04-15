import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DownloadableFileDto, UpdateDownloadableFileRequest } from '../models/downloadable-file.model';

@Injectable({ providedIn: 'root' })
export class DownloadableFilesService {
    private baseUrl = `${environment.apiUrl || '/api/v1'}/downloadable-files`;
    private http = inject(HttpClient);

    /** Admin: list all files (including inactive) for an entity. */
    listAdmin(entityType: string, entityId: string): Observable<DownloadableFileDto[]> {
        const params = new HttpParams()
            .set('entityType', entityType)
            .set('entityId', entityId);
        return this.http.get<DownloadableFileDto[]>(this.baseUrl, { params });
    }

    /** Public: list only active files for an entity. */
    listPublic(entityType: string, entityId: string): Observable<DownloadableFileDto[]> {
        const params = new HttpParams()
            .set('entityType', entityType)
            .set('entityId', entityId);
        return this.http.get<DownloadableFileDto[]>(`${this.baseUrl}/public`, { params });
    }

    /** Admin: upload a new file. */
    upload(
        entityType: string,
        entityId: string,
        file: File,
        title: string,
        category: string,
        description?: string,
        sortOrder = 0,
        isFeatured = false
    ): Observable<DownloadableFileDto> {
        const form = new FormData();
        form.append('file', file);
        form.append('entityType', entityType);
        form.append('entityId', entityId);
        form.append('title', title);
        form.append('category', category);
        if (description) form.append('description', description);
        form.append('sortOrder', String(sortOrder));
        form.append('isFeatured', String(isFeatured));
        return this.http.post<DownloadableFileDto>(this.baseUrl, form);
    }

    /** Admin: update file metadata. */
    update(id: string, request: UpdateDownloadableFileRequest): Observable<DownloadableFileDto> {
        return this.http.put<DownloadableFileDto>(`${this.baseUrl}/${id}`, request);
    }

    /** Admin: delete file. */
    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    /** Returns the direct download URL for a file. */
    downloadUrl(id: string): string {
        return `${this.baseUrl}/${id}/download`;
    }
}
