import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, of, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    CncMaterial,
    CncOperationType,
    CncOperationAvailability,
    CncServiceRequestPayload,
    CncServiceRequestResult,
    CreateCncMaterialRequest,
    UpdateCncMaterialRequest,
    CncServiceRequestsListResponse,
    CncServiceRequestDto,
    CncAdminRequestsQuery,
    UpdateCncServiceRequestPayload
} from '../models/cnc.model';

@Injectable({
    providedIn: 'root'
})
export class CncApiService {
    private baseUrl = environment.apiUrl || '/api';

    private materialsCache = signal<CncMaterial[]>([]);
    private cacheLoaded = signal<boolean>(false);
    private cacheLoading = signal<boolean>(false);

    readonly materials = computed(() => this.materialsCache());
    readonly isLoaded = computed(() => this.cacheLoaded());
    readonly isLoading = computed(() => this.cacheLoading());

    readonly routingMaterials = computed(() =>
        this.materialsCache().filter(
            m => m.isActive && !m.isMetal && (m.type === 'routing' || m.type === 'both')
        )
    );

    readonly pcbMaterials = computed(() =>
        this.materialsCache().filter(
            m => m.isActive && !m.isMetal && (m.type === 'pcb' || m.type === 'both')
        )
    );

    constructor(private http: HttpClient) {}

    getMaterials(params?: { activeOnly?: boolean; type?: string }): Observable<CncMaterial[]> {
        let httpParams = new HttpParams();
        if (params?.activeOnly !== undefined) {
            httpParams = httpParams.set('activeOnly', params.activeOnly.toString());
        }
        if (params?.type) {
            httpParams = httpParams.set('type', params.type);
        }
        return this.http.get<CncMaterial[]>(`${this.baseUrl}/cnc/materials`, { params: httpParams });
    }

    loadMaterialsIfNeeded(): Observable<CncMaterial[]> {
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

    refreshCache(): Observable<CncMaterial[]> {
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

    getMaterialById(id: string): Observable<CncMaterial> {
        return this.http.get<CncMaterial>(`${this.baseUrl}/cnc/materials/${id}`);
    }

    getAllMaterials(): Observable<CncMaterial[]> {
        return this.http.get<CncMaterial[]>(`${this.baseUrl}/cnc/materials/admin?activeOnly=false`);
    }

    createMaterial(material: CreateCncMaterialRequest): Observable<CncMaterial> {
        return this.http.post<CncMaterial>(`${this.baseUrl}/cnc/materials`, material).pipe(
            tap(() => this.clearCache())
        );
    }

    updateMaterial(id: string, material: UpdateCncMaterialRequest): Observable<CncMaterial> {
        return this.http.put<CncMaterial>(`${this.baseUrl}/cnc/materials/${id}`, material).pipe(
            tap(() => this.clearCache())
        );
    }

    deleteMaterial(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/cnc/materials/${id}`).pipe(
            tap(() => this.clearCache())
        );
    }

    // Rules Engine Methods
    getAvailableOperations(material: CncMaterial | null, thickness: number): CncOperationAvailability[] {
        const operations: CncOperationType[] = ['cut', 'engrave', 'pocket', 'drill'];
        
        if (!material) {
            return operations.map(op => ({
                operation: op,
                available: true,
                maxDepth: thickness
            }));
        }

        return operations.map(op => {
            const available = this.isOperationAllowed(material, op);
            let reason: string | undefined;
            let maxDepth: number | undefined;

            if (!available) {
                reason = this.getOperationDisabledReason(material, op);
            } else {
                maxDepth = this.getMaxDepthForOperation(material, op, thickness);
            }

            return { operation: op, available, reason, maxDepth };
        });
    }

    isOperationAllowed(material: CncMaterial, operation: CncOperationType): boolean {
        switch (operation) {
            case 'cut': return material.allowCut;
            case 'engrave': return material.allowEngrave;
            case 'pocket': return material.allowPocket;
            case 'drill': return material.allowDrill;
            default: return false;
        }
    }

    getOperationDisabledReason(material: CncMaterial, operation: CncOperationType, lang: string = 'en'): string {
        const isAr = lang === 'ar';
        const materialName = isAr ? (material.nameAr || material.nameEn) : material.nameEn;
        
        const operationNotes = this.getOperationNotes(material, operation, lang);
        if (operationNotes) {
            return operationNotes;
        }

        switch (operation) {
            case 'cut':
                return isAr 
                    ? `القطع غير متاح لمادة ${materialName}` 
                    : `Cut not available for ${materialName}`;
            case 'engrave':
                return isAr 
                    ? `النقش غير متاح لمادة ${materialName}` 
                    : `Engrave not available for ${materialName}`;
            case 'pocket':
                return isAr 
                    ? `التجويف غير متاح لمادة ${materialName}` 
                    : `Pocket not available for ${materialName}`;
            case 'drill':
                return isAr 
                    ? `الحفر غير متاح لمادة ${materialName}` 
                    : `Drill not available for ${materialName}`;
            default:
                return isAr 
                    ? `العملية غير متاحة` 
                    : `Operation not available`;
        }
    }

    getOperationNotes(material: CncMaterial, operation: CncOperationType, lang: string = 'en'): string | undefined {
        const isAr = lang === 'ar';
        switch (operation) {
            case 'cut':
                return isAr ? material.cutNotesAr : material.cutNotesEn;
            case 'engrave':
                return isAr ? material.engraveNotesAr : material.engraveNotesEn;
            case 'pocket':
                return isAr ? material.pocketNotesAr : material.pocketNotesEn;
            case 'drill':
                return isAr ? material.drillNotesAr : material.drillNotesEn;
            default:
                return undefined;
        }
    }

    getMaxDepthForOperation(material: CncMaterial, operation: CncOperationType, thickness: number): number {
        let maxMaterialDepth: number | null = null;
        
        switch (operation) {
            case 'cut':
                maxMaterialDepth = material.maxCutDepthMm;
                break;
            case 'engrave':
                maxMaterialDepth = material.maxEngraveDepthMm;
                break;
            case 'pocket':
                maxMaterialDepth = material.maxPocketDepthMm;
                break;
            case 'drill':
                maxMaterialDepth = material.maxDrillDepthMm;
                break;
        }

        if (maxMaterialDepth !== null && maxMaterialDepth < thickness) {
            return maxMaterialDepth;
        }
        return thickness;
    }

    clampThickness(material: CncMaterial | null, thickness: number): number {
        if (!material) return thickness;
        
        let clamped = thickness;
        if (material.minThicknessMm !== null && thickness < material.minThicknessMm) {
            clamped = material.minThicknessMm;
        }
        if (material.maxThicknessMm !== null && thickness > material.maxThicknessMm) {
            clamped = material.maxThicknessMm;
        }
        return clamped;
    }

    getFirstAvailableOperation(material: CncMaterial | null, thickness: number): CncOperationType {
        const availabilities = this.getAvailableOperations(material, thickness);
        const firstAvailable = availabilities.find(a => a.available);
        return firstAvailable?.operation || 'cut';
    }

    validateDepth(material: CncMaterial | null, operation: CncOperationType, depth: number, thickness: number): { valid: boolean; error?: string } {
        if (depth > thickness) {
            return { valid: false, error: 'Depth cannot exceed material thickness' };
        }
        
        if (material) {
            const maxDepth = this.getMaxDepthForOperation(material, operation, thickness);
            if (depth > maxDepth) {
                return { valid: false, error: `Maximum depth for this operation is ${maxDepth}mm` };
            }
        }
        
        return { valid: true };
    }

    getLocalizedName(material: CncMaterial, lang: string): string {
        return lang === 'ar' && material.nameAr ? material.nameAr : material.nameEn;
    }

    getLocalizedNotes(material: CncMaterial, lang: string): string | undefined {
        if (lang === 'ar' && material.notesAr) {
            return material.notesAr;
        }
        return material.notesEn;
    }

    getThicknessRange(material: CncMaterial): string | null {
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
        payload: CncServiceRequestPayload,
        files: File[]
    ): Observable<CncServiceRequestResult> {
        const formData = new FormData();
        
        formData.append('serviceMode', payload.serviceMode);
        if (payload.materialId) formData.append('materialId', payload.materialId);
        if (payload.operationType) formData.append('operationType', payload.operationType);
        if (payload.pcbSide) formData.append('pcbSide', payload.pcbSide);
        if (payload.pcbOperation) formData.append('pcbOperation', payload.pcbOperation);
        if (payload.pcbMaterial) formData.append('pcbMaterial', payload.pcbMaterial);
        if (payload.pcbThickness !== undefined) formData.append('pcbThickness', payload.pcbThickness.toString());
        if (payload.widthMm !== undefined) formData.append('widthMm', payload.widthMm.toString());
        if (payload.heightMm !== undefined) formData.append('heightMm', payload.heightMm.toString());
        if (payload.thicknessMm !== undefined) formData.append('thicknessMm', payload.thicknessMm.toString());
        formData.append('quantity', payload.quantity.toString());
        if (payload.depthMode) formData.append('depthMode', payload.depthMode);
        if (payload.depthMm !== undefined) formData.append('depthMm', payload.depthMm.toString());
        formData.append('designSourceType', payload.designSourceType);
        if (payload.designNotes) formData.append('designNotes', payload.designNotes);
        formData.append('customerName', payload.customerName);
        formData.append('customerEmail', payload.customerEmail);
        if (payload.customerPhone) formData.append('customerPhone', payload.customerPhone);
        if (payload.projectDescription) formData.append('projectDescription', payload.projectDescription);
        
        files.forEach((file, index) => {
            formData.append(`files`, file, file.name);
        });

        return this.http.post<CncServiceRequestResult>(`${this.baseUrl}/cnc/requests`, formData);
    }

    getAdminRequests(query: CncAdminRequestsQuery): Observable<CncServiceRequestsListResponse> {
        let params = new HttpParams();
        if (query.status !== undefined && query.status !== null) {
            params = params.set('status', String(query.status));
        }
        if (query.serviceMode) {
            params = params.set('serviceMode', query.serviceMode);
        }
        if (query.search) {
            params = params.set('search', query.search);
        }
        if (query.createdFrom) {
            params = params.set('createdFrom', query.createdFrom);
        }
        if (query.createdTo) {
            params = params.set('createdTo', query.createdTo);
        }
        const skip = query.skip ?? 0;
        const take = query.take ?? 25;
        params = params.set('skip', String(skip));
        params = params.set('take', String(take));
        return this.http.get<CncServiceRequestsListResponse>(`${this.baseUrl}/cnc/requests`, { params });
    }

    getAdminRequestById(id: string): Observable<CncServiceRequestDto> {
        return this.http.get<CncServiceRequestDto>(`${this.baseUrl}/cnc/requests/${id}`);
    }

    updateAdminRequest(id: string, payload: UpdateCncServiceRequestPayload): Observable<CncServiceRequestDto> {
        return this.http.put<CncServiceRequestDto>(`${this.baseUrl}/cnc/requests/${id}`, payload);
    }

    downloadRequestFile(id: string): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/cnc/requests/${id}/file`, { responseType: 'blob' });
    }
}
