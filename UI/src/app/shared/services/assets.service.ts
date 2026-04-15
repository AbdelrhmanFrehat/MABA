import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Asset, AssetCategory, AssetNumberingSetting,
    CreateAssetRequest, UpdateAssetRequest, CreateAssetCategoryRequest
} from '../models/asset.model';

@Injectable({ providedIn: 'root' })
export class AssetsService {
    private assetsUrl = `${environment.apiUrl}/Assets`;
    private categoriesUrl = `${environment.apiUrl}/AssetCategories`;
    private settingsUrl = `${environment.apiUrl}/asset-settings`;

    constructor(private http: HttpClient) {}

    getAssets(params?: { categoryId?: string; investorUserId?: string; statusId?: string; search?: string }): Observable<Asset[]> {
        let p = new HttpParams();
        if (params?.categoryId) p = p.set('categoryId', params.categoryId);
        if (params?.investorUserId) p = p.set('investorUserId', params.investorUserId);
        if (params?.statusId) p = p.set('statusId', params.statusId);
        if (params?.search) p = p.set('search', params.search);
        return this.http.get<Asset[]>(this.assetsUrl, { params: p });
    }

    getAsset(id: string): Observable<Asset> {
        return this.http.get<Asset>(`${this.assetsUrl}/${id}`);
    }

    getAssetByNumber(assetNumber: string): Observable<Asset> {
        return this.http.get<Asset>(`${this.assetsUrl}/by-number/${assetNumber}`);
    }

    create(req: CreateAssetRequest): Observable<Asset> {
        return this.http.post<Asset>(this.assetsUrl, req);
    }

    update(id: string, req: UpdateAssetRequest): Observable<Asset> {
        return this.http.put<Asset>(`${this.assetsUrl}/${id}`, req);
    }

    remove(id: string): Observable<void> {
        return this.http.delete<void>(`${this.assetsUrl}/${id}`);
    }

    // Categories
    getCategories(): Observable<AssetCategory[]> {
        return this.http.get<AssetCategory[]>(this.categoriesUrl);
    }
    createCategory(req: CreateAssetCategoryRequest): Observable<AssetCategory> {
        return this.http.post<AssetCategory>(this.categoriesUrl, req);
    }
    updateCategory(id: string, req: CreateAssetCategoryRequest): Observable<AssetCategory> {
        return this.http.put<AssetCategory>(`${this.categoriesUrl}/${id}`, req);
    }
    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
    }

    // Numbering
    getNumbering(): Observable<AssetNumberingSetting> {
        return this.http.get<AssetNumberingSetting>(`${this.settingsUrl}/numbering`);
    }
    updateNumbering(setting: AssetNumberingSetting): Observable<AssetNumberingSetting> {
        return this.http.put<AssetNumberingSetting>(`${this.settingsUrl}/numbering`, setting);
    }
}
