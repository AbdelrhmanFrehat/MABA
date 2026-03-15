import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    CreateDesignRequestPayload,
    CreateDesignRequestResponse,
    DesignServiceRequestDto,
    DesignRequestListResponse
} from '../models/design-request.model';

@Injectable({ providedIn: 'root' })
export class DesignRequestService {
    private baseUrl = `${environment.apiUrl || '/api/v1'}/design-requests`;

    constructor(private http: HttpClient) {}

    /**
     * Create a design request with optional file uploads (multipart/form-data).
     * Requires auth; call only after checking user is logged in.
     */
    createRequest(payload: CreateDesignRequestPayload, files: File[]): Observable<DesignServiceRequestDto> {
        const formData = new FormData();
        formData.append('requestType', payload.requestType);
        formData.append('title', payload.title);
        formData.append('ipOwnershipConfirmed', String(!!payload.ipOwnershipConfirmed));

        if (payload.description)
            formData.append('description', payload.description);
        if (payload.intendedUse)
            formData.append('intendedUse', payload.intendedUse);
        if (payload.materialPreference)
            formData.append('materialPreference', payload.materialPreference);
        if (payload.dimensionsNotes)
            formData.append('dimensionsNotes', payload.dimensionsNotes);
        if (payload.toleranceLevel)
            formData.append('toleranceLevel', payload.toleranceLevel);
        if (payload.budgetRange)
            formData.append('budgetRange', payload.budgetRange);
        if (payload.timeline)
            formData.append('timeline', payload.timeline);

        for (const file of files)
            formData.append('files', file, file.name);

        return this.http.post<DesignServiceRequestDto>(this.baseUrl, formData);
    }

    getMyRequests(params?: {
        page?: number;
        pageSize?: number;
        status?: string;
        requestType?: string;
        search?: string;
    }): Observable<DesignRequestListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            Object.entries(params).forEach(([key, value]) => {
                if (value != null && value !== '')
                    httpParams = httpParams.set(key, String(value));
            });
        }
        return this.http.get<DesignRequestListResponse>(this.baseUrl, { params: httpParams });
    }

    getById(id: string): Observable<DesignServiceRequestDto> {
        return this.http.get<DesignServiceRequestDto>(`${this.baseUrl}/${id}`);
    }

    getAttachmentDownloadUrl(requestId: string, attachmentId: string): string {
        return `${this.baseUrl}/${requestId}/attachments/${attachmentId}`;
    }

    /** Admin: update status (and optional notes). */
    updateStatus(id: string, payload: { status: string; notes?: string }): Observable<DesignServiceRequestDto> {
        return this.http.put<DesignServiceRequestDto>(`${this.baseUrl}/${id}/status`, payload);
    }

    /** Admin: update admin notes, quoted price, final price, delivery notes. */
    updateAdmin(id: string, payload: { adminNotes?: string; quotedPrice?: number; finalPrice?: number; deliveryNotes?: string }): Observable<DesignServiceRequestDto> {
        return this.http.put<DesignServiceRequestDto>(`${this.baseUrl}/${id}`, payload);
    }

    /** Get URL to download all attachments as zip (use with auth header via HttpClient get responseType blob). */
    getDownloadAllUrl(id: string): string {
        return `${this.baseUrl}/${id}/download-all`;
    }
}
