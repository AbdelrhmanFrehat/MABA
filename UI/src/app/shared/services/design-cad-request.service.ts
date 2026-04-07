import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    CreateDesignCadRequestPayload,
    DesignCadRequestDto,
    DesignCadRequestListResponse
} from '../models/design-cad-request.model';

@Injectable({ providedIn: 'root' })
export class DesignCadRequestService {
    private baseUrl = `${environment.apiUrl || '/api/v1'}/design-cad-requests`;

    constructor(private http: HttpClient) {}

    /**
     * Create a Design CAD request with optional file uploads (multipart/form-data).
     * Requires auth; call only after user is logged in.
     */
    create(payload: CreateDesignCadRequestPayload, files: File[]): Observable<DesignCadRequestDto> {
        const formData = new FormData();
        formData.append('requestType', payload.requestType);
        formData.append('title', payload.title.trim());
        formData.append('hasPhysicalPart', String(!!payload.hasPhysicalPart));
        formData.append('legalConfirmation', String(!!payload.legalConfirmation));
        formData.append('canDeliverPhysicalPart', String(!!payload.canDeliverPhysicalPart));

        if (payload.scenario != null)
            formData.append('scenario', payload.scenario);
        if (payload.description)
            formData.append('description', payload.description);
        if (payload.targetProcess)
            formData.append('targetProcess', payload.targetProcess);
        if (payload.intendedUse)
            formData.append('intendedUse', payload.intendedUse);
        if (payload.materialNotes)
            formData.append('materialNotes', payload.materialNotes);
        if (payload.materialPreference)
            formData.append('materialPreference', payload.materialPreference);
        if (payload.approximateDimensions)
            formData.append('approximateDimensions', payload.approximateDimensions);
        if (payload.dimensionsNotes)
            formData.append('dimensionsNotes', payload.dimensionsNotes);
        if (payload.toleranceNotes)
            formData.append('toleranceNotes', payload.toleranceNotes);
        if (payload.whatNeedsChange)
            formData.append('whatNeedsChange', payload.whatNeedsChange);
        if (payload.changeRequests)
            formData.append('changeRequests', payload.changeRequests);
        if (payload.confirmOwnership != null)
            formData.append('confirmOwnership', String(!!payload.confirmOwnership));
        if (payload.canDeliverPhysicalItem != null)
            formData.append('canDeliverPhysicalItem', String(!!payload.canDeliverPhysicalItem));
        if (payload.additionalNotes)
            formData.append('additionalNotes', payload.additionalNotes);
        if (payload.criticalSurfaces)
            formData.append('criticalSurfaces', payload.criticalSurfaces);
        if (payload.fitmentRequirements)
            formData.append('fitmentRequirements', payload.fitmentRequirements);
        if (payload.purposeAndConstraints)
            formData.append('purposeAndConstraints', payload.purposeAndConstraints);
        if (payload.whatToModify)
            formData.append('whatToModify', payload.whatToModify);
        if (payload.referenceProductLink)
            formData.append('referenceProductLink', payload.referenceProductLink);
        if (payload.measurementsOrConstraints)
            formData.append('measurementsOrConstraints', payload.measurementsOrConstraints);
        if (payload.purposeOfAssembly)
            formData.append('purposeOfAssembly', payload.purposeOfAssembly);
        if (payload.numberOfParts)
            formData.append('numberOfParts', payload.numberOfParts);
        if (payload.movingPartsOrMechanisms)
            formData.append('movingPartsOrMechanisms', payload.movingPartsOrMechanisms);
        if (payload.spaceSizeConstraints)
            formData.append('spaceSizeConstraints', payload.spaceSizeConstraints);
        if (payload.deadline)
            formData.append('deadline', payload.deadline);
        if (payload.customerNotes)
            formData.append('customerNotes', payload.customerNotes);
        if (payload.additionalNotes)
            formData.append('additionalNotes', payload.additionalNotes);

        for (const file of files)
            formData.append('files', file, file.name);

        return this.http.post<DesignCadRequestDto>(this.baseUrl, formData);
    }

    getList(params?: {
        page?: number;
        pageSize?: number;
        status?: string;
        requestType?: string;
        search?: string;
    }): Observable<DesignCadRequestListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            Object.entries(params).forEach(([key, value]) => {
                if (value != null && value !== '')
                    httpParams = httpParams.set(key, String(value));
            });
        }
        return this.http.get<DesignCadRequestListResponse>(this.baseUrl, { params: httpParams });
    }

    getById(id: string): Observable<DesignCadRequestDto> {
        return this.http.get<DesignCadRequestDto>(`${this.baseUrl}/${id}`);
    }

    updateStatus(id: string, payload: { status: string; notes?: string }): Observable<DesignCadRequestDto> {
        return this.http.put<DesignCadRequestDto>(`${this.baseUrl}/${id}/status`, payload);
    }
}
