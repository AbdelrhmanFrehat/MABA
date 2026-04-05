import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    PaymentVoucher, BizPaymentPlan,
    CreatePaymentVoucherRequest, CreateBizPaymentPlanRequest,
    RecordBizInstallmentPaymentRequest
} from '../models/biz-payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentVoucherApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Payment Vouchers ---

    getPaymentVouchers(params?: Record<string, any>): Observable<PaymentVoucher[]> {
        return this.http.get<PaymentVoucher[]>(`${this.baseUrl}/payment-vouchers`, { params: this.buildParams(params) });
    }

    getPaymentVoucherById(id: string): Observable<PaymentVoucher> {
        return this.http.get<PaymentVoucher>(`${this.baseUrl}/payment-vouchers/${id}`);
    }

    createPaymentVoucher(request: CreatePaymentVoucherRequest): Observable<PaymentVoucher> {
        return this.http.post<PaymentVoucher>(`${this.baseUrl}/payment-vouchers`, request);
    }

    confirmPaymentVoucher(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/payment-vouchers/${id}/confirm`, {});
    }

    voidPaymentVoucher(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/payment-vouchers/${id}/void`, {});
    }

    // --- Payment Plans ---

    getPaymentPlans(params?: Record<string, any>): Observable<BizPaymentPlan[]> {
        return this.http.get<BizPaymentPlan[]>(`${this.baseUrl}/payment-plans`, { params: this.buildParams(params) });
    }

    getPaymentPlanById(id: string): Observable<BizPaymentPlan> {
        return this.http.get<BizPaymentPlan>(`${this.baseUrl}/payment-plans/${id}`);
    }

    createPaymentPlan(request: CreateBizPaymentPlanRequest): Observable<BizPaymentPlan> {
        return this.http.post<BizPaymentPlan>(`${this.baseUrl}/payment-plans`, request);
    }

    recordInstallmentPayment(request: RecordBizInstallmentPaymentRequest): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/payment-plans/installment-payment`, request);
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
