import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MediaAsset, UpdateMediaRequest } from '../models/media.model';

@Injectable({
    providedIn: 'root'
})
export class MediaApiService {
    private baseUrl = `${environment.apiUrl}/media`;

    constructor(private http: HttpClient) {}

    uploadMedia(formData: FormData): Observable<MediaAsset> {
        return this.http.post<MediaAsset>(`${this.baseUrl}/upload`, formData);
    }

    getAllMedia(mediaTypeId?: string, uploadedByUserId?: string): Observable<MediaAsset[]> {
        let params = new HttpParams();
        if (mediaTypeId) {
            params = params.set('mediaTypeId', mediaTypeId);
        }
        if (uploadedByUserId) {
            params = params.set('uploadedByUserId', uploadedByUserId);
        }
        return this.http.get<MediaAsset[]>(this.baseUrl, { params });
    }

    getMediaById(id: string): Observable<MediaAsset> {
        return this.http.get<MediaAsset>(`${this.baseUrl}/${id}`);
    }

    updateMedia(id: string, request: UpdateMediaRequest): Observable<MediaAsset> {
        return this.http.put<MediaAsset>(`${this.baseUrl}/${id}`, request);
    }

    deleteMedia(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}


