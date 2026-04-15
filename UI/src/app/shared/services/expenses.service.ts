import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Expense, ExpenseCategory, CreateExpenseRequest } from '../models/expense.model';

export interface ExpenseQueryParams {
    expenseCategoryId?: string;
    fromDate?: string;
    toDate?: string;
}

export interface PaymentMethodOption {
    id: string;
    key: string;
    nameEn: string;
    nameAr: string;
}

@Injectable({
    providedIn: 'root'
})
export class ExpensesService {
    private categoriesUrl = `${environment.apiUrl}/ExpenseCategories`;
    private expensesUrl = `${environment.apiUrl}/Expenses`;
    private paymentMethodsUrl = `${environment.apiUrl}/PaymentMethods`;

    constructor(private http: HttpClient) {}

    getCategories(): Observable<ExpenseCategory[]> {
        return this.http.get<ExpenseCategory[]>(this.categoriesUrl);
    }

    getPaymentMethods(): Observable<PaymentMethodOption[]> {
        return this.http.get<PaymentMethodOption[]>(this.paymentMethodsUrl);
    }

    getExpenses(params?: ExpenseQueryParams): Observable<Expense[]> {
        let httpParams = new HttpParams();

        if (params?.expenseCategoryId) {
            httpParams = httpParams.set('expenseCategoryId', params.expenseCategoryId);
        }

        if (params?.fromDate) {
            httpParams = httpParams.set('fromDate', params.fromDate);
        }

        if (params?.toDate) {
            httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<Expense[]>(this.expensesUrl, { params: httpParams });
    }

    createExpense(request: CreateExpenseRequest): Observable<Expense> {
        return this.http.post<Expense>(this.expensesUrl, request);
    }
}
