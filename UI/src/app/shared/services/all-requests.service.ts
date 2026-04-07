import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AllRequestDetail, AllRequestsFilters, AllRequestsResponse, AllRequestType, UpdateAllRequestPayload } from '../models/all-requests.model';

@Injectable({ providedIn: 'root' })
export class AllRequestsService {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/all-requests`;

    getAll(filters: AllRequestsFilters = {}): Observable<AllRequestsResponse> {
        let params = new HttpParams();
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== null && value !== undefined && value !== '') {
                params = params.set(key, String(value));
            }
        });
        return this.http.get<AllRequestsResponse>(this.baseUrl, { params });
    }

    getById(requestType: AllRequestType, id: string): Observable<AllRequestDetail> {
        return this.http.get<AllRequestDetail>(`${this.baseUrl}/${requestType}/${id}`);
    }

    update(requestType: AllRequestType, id: string, payload: UpdateAllRequestPayload): Observable<AllRequestDetail> {
        return this.http.put<AllRequestDetail>(`${this.baseUrl}/${requestType}/${id}`, payload);
    }
}
