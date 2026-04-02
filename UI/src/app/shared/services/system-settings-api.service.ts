import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemSettingDto {
    id: string;
    key: string;
    value: string;
    descriptionEn?: string;
    descriptionAr?: string;
    category?: string;
    dataType?: string;
    isPublic: boolean;
    isEncrypted: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateSystemSettingRequest {
    key: string;
    value: string;
    descriptionEn?: string;
    descriptionAr?: string;
    category?: string;
    dataType?: string;
    isPublic?: boolean;
    isEncrypted?: boolean;
}

export interface UpdateSystemSettingRequest {
    value?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    category?: string;
    isPublic?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class SystemSettingsApiService {
    private baseUrl = `${environment.apiUrl}/settings`;

    constructor(private http: HttpClient) {}

    getByKey(key: string): Observable<SystemSettingDto> {
        return this.http.get<SystemSettingDto>(`${this.baseUrl}/key/${encodeURIComponent(key)}`);
    }

    getSettings(category?: string, isPublic?: boolean): Observable<SystemSettingDto[]> {
        const params: string[] = [];
        if (category) params.push(`category=${encodeURIComponent(category)}`);
        if (typeof isPublic === 'boolean') params.push(`isPublic=${isPublic}`);
        const query = params.length ? `?${params.join('&')}` : '';
        return this.http.get<SystemSettingDto[]>(`${this.baseUrl}${query}`);
    }

    create(request: CreateSystemSettingRequest): Observable<SystemSettingDto> {
        return this.http.post<SystemSettingDto>(this.baseUrl, request);
    }

    update(id: string, request: UpdateSystemSettingRequest): Observable<SystemSettingDto> {
        return this.http.put<SystemSettingDto>(`${this.baseUrl}/${id}`, request);
    }
}
