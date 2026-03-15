import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, of, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    LaserMaterial,
    CreateLaserMaterialRequest,
    UpdateLaserMaterialRequest,
    LaserServiceRequestResult,
    LaserServiceRequest,
    LaserServiceRequestStatus
} from '../models/laser.model';

@Injectable({
    providedIn: 'root'
})
export class LaserApiService {
    private baseUrl = environment.apiUrl || '/api';

    private materialsCache = signal<LaserMaterial[]>([]);
    private cacheLoaded = signal<boolean>(false);
    private cacheLoading = signal<boolean>(false);

    readonly materials = computed(() => this.materialsCache());
    readonly isLoaded = computed(() => this.cacheLoaded());
    readonly isLoading = computed(() => this.cacheLoading());

    readonly activeMaterials = computed(() =>
        this.materialsCache().filter(m => m.isActive)
    );

    readonly nonMetalMaterials = computed(() =>
        this.materialsCache().filter(m => m.isActive && !m.isMetal)
    );

    constructor(private http: HttpClient) {}

    getMaterials(params?: { activeOnly?: boolean; type?: string }): Observable<LaserMaterial[]> {
        let httpParams = new HttpParams();
        if (params?.activeOnly !== undefined) {
            httpParams = httpParams.set('activeOnly', params.activeOnly.toString());
        }
        if (params?.type) {
            httpParams = httpParams.set('type', params.type);
        }
        return this.http.get<LaserMaterial[]>(`${this.baseUrl}/laser/materials`, { params: httpParams });
    }

    loadMaterialsIfNeeded(): Observable<LaserMaterial[]> {
        if (this.cacheLoaded() || this.cacheLoading()) {
            return of(this.materialsCache());
        }

        this.cacheLoading.set(true);
        return this.getMaterials({ activeOnly: true }).pipe(
            tap(materials => {
                this.materialsCache.set(materials);
                this.cacheLoaded.set(true);
                this.cacheLoading.set(false);
            }),
            catchError(err => {
                this.cacheLoading.set(false);
                throw err;
            })
        );
    }

    refreshCache(): Observable<LaserMaterial[]> {
        this.cacheLoading.set(true);
        return this.getMaterials({ activeOnly: true }).pipe(
            tap(materials => {
                this.materialsCache.set(materials);
                this.cacheLoaded.set(true);
                this.cacheLoading.set(false);
            }),
            catchError(err => {
                this.cacheLoading.set(false);
                throw err;
            })
        );
    }

    clearCache(): void {
        this.materialsCache.set([]);
        this.cacheLoaded.set(false);
    }

    getMaterialsByType(type: 'cut' | 'engrave'): LaserMaterial[] {
        return this.materialsCache().filter(m =>
            m.isActive && (m.type === type || m.type === 'both')
        );
    }

    getMaterialById(id: string): Observable<LaserMaterial> {
        return this.http.get<LaserMaterial>(`${this.baseUrl}/laser/materials/${id}`);
    }

    getAllMaterials(): Observable<LaserMaterial[]> {
        return this.http.get<LaserMaterial[]>(`${this.baseUrl}/laser/materials/admin?activeOnly=false`);
    }

    createMaterial(material: CreateLaserMaterialRequest): Observable<LaserMaterial> {
        return this.http.post<LaserMaterial>(`${this.baseUrl}/laser/materials`, material).pipe(
            tap(() => this.clearCache())
        );
    }

    updateMaterial(id: string, material: UpdateLaserMaterialRequest): Observable<LaserMaterial> {
        return this.http.put<LaserMaterial>(`${this.baseUrl}/laser/materials/${id}`, material).pipe(
            tap(() => this.clearCache())
        );
    }

    deleteMaterial(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/laser/materials/${id}`).pipe(
            tap(() => this.clearCache())
        );
    }

    getLocalizedName(material: LaserMaterial, lang: string): string {
        return lang === 'ar' && material.nameAr ? material.nameAr : material.nameEn;
    }

    getLocalizedNotes(material: LaserMaterial, lang: string): string | null {
        if (lang === 'ar' && material.notesAr) {
            return material.notesAr;
        }
        return material.notesEn;
    }

    getThicknessRange(material: LaserMaterial): string | null {
        if (material.minThicknessMm == null && material.maxThicknessMm == null) {
            return null;
        }
        if (material.minThicknessMm != null && material.maxThicknessMm != null) {
            return `${material.minThicknessMm} - ${material.maxThicknessMm} mm`;
        }
        if (material.minThicknessMm != null) {
            return `≥ ${material.minThicknessMm} mm`;
        }
        return `≤ ${material.maxThicknessMm} mm`;
    }

    submitServiceRequest(
        materialId: string,
        operationMode: 'cut' | 'engrave',
        image: File,
        dimensions?: {
            widthCm?: number;
            heightCm?: number;
        },
        customerInfo?: {
            name?: string;
            email?: string;
            phone?: string;
            notes?: string;
        }
    ): Observable<LaserServiceRequestResult> {
        const formData = new FormData();
        formData.append('materialId', materialId);
        formData.append('operationMode', operationMode);
        formData.append('image', image, image.name);
        
        if (dimensions?.widthCm) {
            formData.append('widthCm', dimensions.widthCm.toString());
        }
        if (dimensions?.heightCm) {
            formData.append('heightCm', dimensions.heightCm.toString());
        }
        
        if (customerInfo?.name) {
            formData.append('customerName', customerInfo.name);
        }
        if (customerInfo?.email) {
            formData.append('customerEmail', customerInfo.email);
        }
        if (customerInfo?.phone) {
            formData.append('customerPhone', customerInfo.phone);
        }
        if (customerInfo?.notes) {
            formData.append('customerNotes', customerInfo.notes);
        }

        return this.http.post<LaserServiceRequestResult>(
            `${this.baseUrl}/laser/requests`,
            formData
        );
    }

    getServiceRequests(params?: { status?: LaserServiceRequestStatus; limit?: number; offset?: number }): Observable<LaserServiceRequest[]> {
        let httpParams = new HttpParams();
        if (params?.status) {
            httpParams = httpParams.set('status', params.status);
        }
        if (params?.limit) {
            httpParams = httpParams.set('limit', params.limit.toString());
        }
        if (params?.offset) {
            httpParams = httpParams.set('offset', params.offset.toString());
        }
        return this.http.get<LaserServiceRequest[]>(`${this.baseUrl}/laser/requests`, { params: httpParams });
    }

    getServiceRequestById(id: string): Observable<LaserServiceRequest> {
        return this.http.get<LaserServiceRequest>(`${this.baseUrl}/laser/requests/${id}`);
    }

    updateServiceRequest(id: string, data: { status?: LaserServiceRequestStatus; adminNotes?: string; quotedPrice?: number }): Observable<LaserServiceRequest> {
        return this.http.put<LaserServiceRequest>(`${this.baseUrl}/laser/requests/${id}`, data);
    }

    getServiceRequestImageUrl(id: string): string {
        return `${this.baseUrl}/laser/requests/${id}/image`;
    }
}
