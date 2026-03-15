import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Category,
    CreateCategoryRequest,
    UpdateCategoryRequest,
    Tag,
    CreateTagRequest,
    UpdateTagRequest,
    Brand,
    CreateBrandRequest,
    UpdateBrandRequest
} from '../models/catalog.model';

@Injectable({
    providedIn: 'root'
})
export class CatalogApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // Categories
    getAllCategories(isActive?: boolean, includeChildren?: boolean): Observable<Category[]> {
        let params = new HttpParams();
        if (isActive !== undefined) {
            params = params.set('isActive', isActive.toString());
        }
        if (includeChildren !== undefined) {
            params = params.set('includeChildren', includeChildren.toString());
        }
        return this.http.get<Category[]>(`${this.baseUrl}/categories`, { params });
    }

    getCategoryById(id: string): Observable<Category> {
        return this.http.get<Category>(`${this.baseUrl}/categories/${id}`);
    }

    createCategory(request: CreateCategoryRequest): Observable<Category> {
        return this.http.post<Category>(`${this.baseUrl}/categories`, request);
    }

    updateCategory(id: string, request: UpdateCategoryRequest): Observable<Category> {
        return this.http.put<Category>(`${this.baseUrl}/categories/${id}`, request);
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/categories/${id}`);
    }

    // Tags
    getAllTags(isActive?: boolean): Observable<Tag[]> {
        let params = new HttpParams();
        if (isActive !== undefined) {
            params = params.set('isActive', isActive.toString());
        }
        return this.http.get<Tag[]>(`${this.baseUrl}/tags`, { params });
    }

    getTagById(id: string): Observable<Tag> {
        return this.http.get<Tag>(`${this.baseUrl}/tags/${id}`);
    }

    createTag(request: CreateTagRequest): Observable<Tag> {
        return this.http.post<Tag>(`${this.baseUrl}/tags`, request);
    }

    updateTag(id: string, request: UpdateTagRequest): Observable<Tag> {
        return this.http.put<Tag>(`${this.baseUrl}/tags/${id}`, request);
    }

    deleteTag(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/tags/${id}`);
    }

    // Brands
    getAllBrands(isActive?: boolean): Observable<Brand[]> {
        let params = new HttpParams();
        if (isActive !== undefined) {
            params = params.set('isActive', isActive.toString());
        }
        return this.http.get<Brand[]>(`${this.baseUrl}/brands`, { params });
    }

    getBrandById(id: string): Observable<Brand> {
        return this.http.get<Brand>(`${this.baseUrl}/brands/${id}`);
    }

    createBrand(request: CreateBrandRequest): Observable<Brand> {
        return this.http.post<Brand>(`${this.baseUrl}/brands`, request);
    }

    updateBrand(id: string, request: UpdateBrandRequest): Observable<Brand> {
        return this.http.put<Brand>(`${this.baseUrl}/brands/${id}`, request);
    }

    deleteBrand(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/brands/${id}`);
    }
}


