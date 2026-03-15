import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
    SoftwareProduct, 
    SoftwareRelease, 
    SoftwareFile,
    CreateSoftwareProductRequest,
    CreateSoftwareReleaseRequest 
} from '../../shared/models/software.model';

@Injectable({
    providedIn: 'root'
})
export class SoftwareApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/software`;

    // Public endpoints
    getProducts(search?: string, category?: string, os?: string): Observable<SoftwareProduct[]> {
        const params: any = {};
        if (search) params.search = search;
        if (category) params.category = category;
        if (os) params.os = os;
        return this.http.get<SoftwareProduct[]>(this.baseUrl, { params });
    }

    getCategories(): Observable<string[]> {
        return this.http.get<string[]>(`${this.baseUrl}/categories`);
    }

    getProductBySlug(slug: string): Observable<SoftwareProduct> {
        return this.http.get<SoftwareProduct>(`${this.baseUrl}/${slug}`);
    }

    downloadFile(fileId: string): Observable<Blob> {
        return this.http.post(`${this.baseUrl}/download/${fileId}`, {}, { responseType: 'blob' });
    }

    // Admin endpoints
    getAllProductsAdmin(): Observable<SoftwareProduct[]> {
        return this.http.get<SoftwareProduct[]>(`${this.baseUrl}/admin/products`);
    }

    createProduct(product: CreateSoftwareProductRequest): Observable<SoftwareProduct> {
        return this.http.post<SoftwareProduct>(`${this.baseUrl}/admin/products`, product);
    }

    updateProduct(id: string, product: CreateSoftwareProductRequest): Observable<SoftwareProduct> {
        return this.http.put<SoftwareProduct>(`${this.baseUrl}/admin/products/${id}`, product);
    }

    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/admin/products/${id}`);
    }

    getReleases(productId: string): Observable<SoftwareRelease[]> {
        return this.http.get<SoftwareRelease[]>(`${this.baseUrl}/admin/products/${productId}/releases`);
    }

    createRelease(productId: string, release: CreateSoftwareReleaseRequest): Observable<SoftwareRelease> {
        return this.http.post<SoftwareRelease>(`${this.baseUrl}/admin/products/${productId}/releases`, release);
    }

    updateRelease(id: string, release: CreateSoftwareReleaseRequest): Observable<SoftwareRelease> {
        return this.http.put<SoftwareRelease>(`${this.baseUrl}/admin/releases/${id}`, release);
    }

    deleteRelease(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/admin/releases/${id}`);
    }

    uploadFile(releaseId: string, file: File, os: string, arch: string, fileType: string): Observable<SoftwareFile> {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('os', os);
        formData.append('arch', arch);
        formData.append('fileType', fileType);
        return this.http.post<SoftwareFile>(`${this.baseUrl}/admin/releases/${releaseId}/files`, formData);
    }

    deleteFile(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/admin/files/${id}`);
    }
}
