import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    PriceList, PriceListItem, WholesaleRule, SupplierItemPrice,
    TaxProfile, UnitOfMeasure, UnitConversion,
    CreatePriceListRequest, UpdatePriceListItemsRequest,
    CreateWholesaleRuleRequest, CreateSupplierItemPriceRequest,
    CreateTaxProfileRequest, CreateUnitOfMeasureRequest, CreateUnitConversionRequest
} from '../models/pricing.model';

@Injectable({ providedIn: 'root' })
export class PricingApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Price Lists ---

    getPriceLists(): Observable<PriceList[]> {
        return this.http.get<PriceList[]>(`${this.baseUrl}/price-lists`);
    }

    getPriceListById(id: string): Observable<PriceList> {
        return this.http.get<PriceList>(`${this.baseUrl}/price-lists/${id}`);
    }

    createPriceList(request: CreatePriceListRequest): Observable<PriceList> {
        return this.http.post<PriceList>(`${this.baseUrl}/price-lists`, request);
    }

    updatePriceList(id: string, request: CreatePriceListRequest): Observable<PriceList> {
        return this.http.put<PriceList>(`${this.baseUrl}/price-lists/${id}`, request);
    }

    getPriceListItems(priceListId: string): Observable<PriceListItem[]> {
        return this.http.get<PriceListItem[]>(`${this.baseUrl}/price-lists/${priceListId}/items`);
    }

    updatePriceListItems(priceListId: string, request: UpdatePriceListItemsRequest): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/price-lists/${priceListId}/items`, request);
    }

    resolvePrice(params: { itemId: string; customerId?: string; quantity?: number }): Observable<{ price: number; source: string }> {
        return this.http.get<{ price: number; source: string }>(`${this.baseUrl}/price-lists/resolve`, { params: this.buildParams(params) });
    }

    // --- Wholesale Rules ---

    getWholesaleRules(): Observable<WholesaleRule[]> {
        return this.http.get<WholesaleRule[]>(`${this.baseUrl}/wholesale-rules`);
    }

    createWholesaleRule(request: CreateWholesaleRuleRequest): Observable<WholesaleRule> {
        return this.http.post<WholesaleRule>(`${this.baseUrl}/wholesale-rules`, request);
    }

    deleteWholesaleRule(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/wholesale-rules/${id}`);
    }

    // --- Supplier Item Prices ---

    getSupplierItemPrices(params?: Record<string, any>): Observable<SupplierItemPrice[]> {
        return this.http.get<SupplierItemPrice[]>(`${this.baseUrl}/supplier-item-prices`, { params: this.buildParams(params) });
    }

    createSupplierItemPrice(request: CreateSupplierItemPriceRequest): Observable<SupplierItemPrice> {
        return this.http.post<SupplierItemPrice>(`${this.baseUrl}/supplier-item-prices`, request);
    }

    updateSupplierItemPrice(id: string, request: CreateSupplierItemPriceRequest): Observable<SupplierItemPrice> {
        return this.http.put<SupplierItemPrice>(`${this.baseUrl}/supplier-item-prices/${id}`, request);
    }

    deleteSupplierItemPrice(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/supplier-item-prices/${id}`);
    }

    // --- Tax Profiles ---

    getTaxProfiles(): Observable<TaxProfile[]> {
        return this.http.get<TaxProfile[]>(`${this.baseUrl}/tax-profiles`);
    }

    createTaxProfile(request: CreateTaxProfileRequest): Observable<TaxProfile> {
        return this.http.post<TaxProfile>(`${this.baseUrl}/tax-profiles`, request);
    }

    updateTaxProfile(id: string, request: CreateTaxProfileRequest): Observable<TaxProfile> {
        return this.http.put<TaxProfile>(`${this.baseUrl}/tax-profiles/${id}`, request);
    }

    // --- Units of Measure ---

    getUnitsOfMeasure(): Observable<UnitOfMeasure[]> {
        return this.http.get<UnitOfMeasure[]>(`${this.baseUrl}/units-of-measure`);
    }

    createUnitOfMeasure(request: CreateUnitOfMeasureRequest): Observable<UnitOfMeasure> {
        return this.http.post<UnitOfMeasure>(`${this.baseUrl}/units-of-measure`, request);
    }

    getUnitConversions(): Observable<UnitConversion[]> {
        return this.http.get<UnitConversion[]>(`${this.baseUrl}/unit-conversions`);
    }

    createUnitConversion(request: CreateUnitConversionRequest): Observable<UnitConversion> {
        return this.http.post<UnitConversion>(`${this.baseUrl}/unit-conversions`, request);
    }

    // --- Helpers ---

    private buildParams(params?: Record<string, any>): HttpParams {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return httpParams;
    }
}
