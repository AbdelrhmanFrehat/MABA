import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Print3dRequest,
    Print3dRequestListResponse,
    Print3dMaterial,
    Print3dProfile,
    Print3dEstimate,
    Print3dDesign,
    CreatePrint3dRequestRequest,
    UpdatePrint3dRequestStatusRequest,
    Material,
    MaterialColor,
    CreateMaterialRequest,
    UpdateMaterialRequest,
    CreateMaterialColorRequest,
    UpdateMaterialColorRequest,
    PrintQualityProfile,
    CreatePrintQualityProfileRequest,
    UpdatePrintQualityProfileRequest
} from '../models/printing.model';

@Injectable({
    providedIn: 'root'
})
export class PrintingApiService {
    private baseUrl = environment.apiUrl || '/api';

    constructor(private http: HttpClient) {}

    // Public method for 3D materials (uses the main materials endpoint)
    getMaterials(): Observable<Material[]> {
        return this.http.get<Material[]>(`${this.baseUrl}/materials`);
    }

    // Admin Materials CRUD
    getAllMaterials(): Observable<Material[]> {
        return this.http.get<Material[]>(`${this.baseUrl}/materials`);
    }

    getMaterialById(id: string): Observable<Material> {
        return this.http.get<Material>(`${this.baseUrl}/materials/${id}`);
    }

    createMaterial(material: CreateMaterialRequest): Observable<Material> {
        return this.http.post<Material>(`${this.baseUrl}/materials`, material);
    }

    updateMaterial(id: string, material: UpdateMaterialRequest): Observable<Material> {
        return this.http.put<Material>(`${this.baseUrl}/materials/${id}`, material);
    }

    deleteMaterial(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/materials/${id}`);
    }

    // Material Colors
    getMaterialColors(materialId: string): Observable<MaterialColor[]> {
        return this.http.get<MaterialColor[]>(`${this.baseUrl}/materials/${materialId}/colors`);
    }

    getAllMaterialColors(materialId: string): Observable<MaterialColor[]> {
        return this.http.get<MaterialColor[]>(`${this.baseUrl}/materials/${materialId}/colors/all`);
    }

    createMaterialColor(materialId: string, color: CreateMaterialColorRequest): Observable<MaterialColor> {
        return this.http.post<MaterialColor>(`${this.baseUrl}/materials/${materialId}/colors`, color);
    }

    updateMaterialColor(materialId: string, colorId: string, color: UpdateMaterialColorRequest): Observable<MaterialColor> {
        return this.http.put<MaterialColor>(`${this.baseUrl}/materials/${materialId}/colors/${colorId}`, color);
    }

    deleteMaterialColor(materialId: string, colorId: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/materials/${materialId}/colors/${colorId}`);
    }

    getProfiles(): Observable<PrintQualityProfile[]> {
        return this.http.get<PrintQualityProfile[]>(`${this.baseUrl}/printqualityprofiles?activeOnly=true`);
    }

    // Print Quality Profiles CRUD
    getAllPrintQualityProfiles(activeOnly: boolean = false): Observable<PrintQualityProfile[]> {
        return this.http.get<PrintQualityProfile[]>(`${this.baseUrl}/printqualityprofiles?activeOnly=${activeOnly}`);
    }

    getPrintQualityProfileById(id: string): Observable<PrintQualityProfile> {
        return this.http.get<PrintQualityProfile>(`${this.baseUrl}/printqualityprofiles/${id}`);
    }

    createPrintQualityProfile(profile: CreatePrintQualityProfileRequest): Observable<PrintQualityProfile> {
        return this.http.post<PrintQualityProfile>(`${this.baseUrl}/printqualityprofiles`, profile);
    }

    updatePrintQualityProfile(id: string, profile: UpdatePrintQualityProfileRequest): Observable<PrintQualityProfile> {
        return this.http.put<PrintQualityProfile>(`${this.baseUrl}/printqualityprofiles/${id}`, profile);
    }

    deletePrintQualityProfile(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/printqualityprofiles/${id}`);
    }

    getRequests(params?: {
        page?: number;
        pageSize?: number;
        status?: string;
        userId?: string;
    }): Observable<Print3dRequestListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                if (params[key as keyof typeof params] !== null && params[key as keyof typeof params] !== undefined && params[key as keyof typeof params] !== '') {
                    httpParams = httpParams.set(key, params[key as keyof typeof params]!.toString());
                }
            });
        }
        return this.http.get<Print3dRequestListResponse>(`${this.baseUrl}/3d-requests`, { params: httpParams });
    }

    getRequestById(id: string): Observable<Print3dRequest> {
        return this.http.get<Print3dRequest>(`${this.baseUrl}/3d-requests/${id}`);
    }

    createRequest(request: CreatePrint3dRequestRequest): Observable<Print3dRequest> {
        const formData = new FormData();
        formData.append('materialId', request.materialId);
        if (request.materialColorId) {
            formData.append('materialColorId', request.materialColorId);
        }
        if (request.profileId) {
            formData.append('profileId', request.profileId);
        }
        if (request.comments) {
            formData.append('comments', request.comments);
        }
        if (request.designFile) {
            formData.append('file', request.designFile);
        }
        return this.http.post<Print3dRequest>(`${this.baseUrl}/3d-requests`, formData);
    }

    updateRequestStatus(id: string, request: UpdatePrint3dRequestStatusRequest): Observable<Print3dRequest> {
        return this.http.put<Print3dRequest>(`${this.baseUrl}/3d-requests/${id}/status`, request);
    }

    getEstimate(requestId: string): Observable<Print3dEstimate> {
        return this.http.get<Print3dEstimate>(`${this.baseUrl}/3d-requests/${requestId}/estimate`);
    }

    uploadDesign(file: File): Observable<Print3dDesign> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<Print3dDesign>(`${this.baseUrl}/designs`, formData);
    }

    getDesigns(params?: {
        page?: number;
        pageSize?: number;
        userId?: string;
    }): Observable<Print3dDesign[]> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                if (params[key as keyof typeof params] !== null && params[key as keyof typeof params] !== undefined && params[key as keyof typeof params] !== '') {
                    httpParams = httpParams.set(key, params[key as keyof typeof params]!.toString());
                }
            });
        }
        return this.http.get<Print3dDesign[]>(`${this.baseUrl}/designs`, { params: httpParams });
    }

    getDesignDetail(id: string): Observable<Print3dDesign> {
        return this.http.get<Print3dDesign>(`${this.baseUrl}/designs/${id}/detail`);
    }

    deleteDesign(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/designs/${id}/delete`, {});
    }
}

