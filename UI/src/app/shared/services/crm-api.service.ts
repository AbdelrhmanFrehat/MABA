import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Customer, Supplier,
    CreateCustomerRequest, UpdateCustomerRequest,
    CreateSupplierRequest, UpdateSupplierRequest,
    AccountStatement
} from '../models/crm.model';

@Injectable({ providedIn: 'root' })
export class CrmApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Customers ---

    getCustomers(params?: Record<string, any>): Observable<Customer[]> {
        return this.http.get<Customer[]>(`${this.baseUrl}/customers`, { params: this.buildParams(params) });
    }

    getCustomerById(id: string): Observable<Customer> {
        return this.http.get<Customer>(`${this.baseUrl}/customers/${id}`);
    }

    createCustomer(request: CreateCustomerRequest): Observable<Customer> {
        return this.http.post<Customer>(`${this.baseUrl}/customers`, request);
    }

    updateCustomer(id: string, request: UpdateCustomerRequest): Observable<Customer> {
        return this.http.put<Customer>(`${this.baseUrl}/customers/${id}`, request);
    }

    deleteCustomer(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/customers/${id}`);
    }

    getCustomerStatement(id: string, params?: Record<string, any>): Observable<AccountStatement> {
        return this.http.get<AccountStatement>(`${this.baseUrl}/customers/${id}/statement`, { params: this.buildParams(params) });
    }

    // --- Suppliers ---

    getSuppliers(params?: Record<string, any>): Observable<Supplier[]> {
        return this.http.get<Supplier[]>(`${this.baseUrl}/suppliers`, { params: this.buildParams(params) });
    }

    getSupplierById(id: string): Observable<Supplier> {
        return this.http.get<Supplier>(`${this.baseUrl}/suppliers/${id}`);
    }

    createSupplier(request: CreateSupplierRequest): Observable<Supplier> {
        return this.http.post<Supplier>(`${this.baseUrl}/suppliers`, request);
    }

    updateSupplier(id: string, request: UpdateSupplierRequest): Observable<Supplier> {
        return this.http.put<Supplier>(`${this.baseUrl}/suppliers/${id}`, request);
    }

    deleteSupplier(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/suppliers/${id}`);
    }

    getSupplierStatement(id: string, params?: Record<string, any>): Observable<AccountStatement> {
        return this.http.get<AccountStatement>(`${this.baseUrl}/suppliers/${id}/statement`, { params: this.buildParams(params) });
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
