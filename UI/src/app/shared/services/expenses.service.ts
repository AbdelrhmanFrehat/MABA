import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Expense, ExpenseCategory } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

export interface ExpenseQueryParams extends PaginationParams {
    expenseCategoryId?: number;
    fromDate?: string;
    toDate?: string;
}

@Injectable({
    providedIn: 'root'
})
export class ExpensesService {
    private categoriesUrl = `${environment.apiUrl}/ExpenseCategories`;
    private expensesUrl = `${environment.apiUrl}/Expenses`;

    constructor(private http: HttpClient) {}

    // Expense Categories
    getCategories(params?: PaginationParams): Observable<PagedResponse<ExpenseCategory>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
        }

        return this.http.get<PagedResponse<ExpenseCategory>>(this.categoriesUrl, { params: httpParams });
    }

    getCategoriesForDropdown(): Observable<ExpenseCategory[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<ExpenseCategory>>(this.categoriesUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    getCategoryById(id: number): Observable<ExpenseCategory> {
        return this.http.get<ExpenseCategory>(`${this.categoriesUrl}/${id}`);
    }

    createCategory(category: ExpenseCategory): Observable<number> {
        return this.http.post<number>(this.categoriesUrl, category);
    }

    updateCategory(id: number, category: ExpenseCategory): Observable<void> {
        return this.http.put<void>(`${this.categoriesUrl}/${id}`, category);
    }

    deleteCategory(id: number): Observable<void> {
        return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
    }

    // Expenses
    getExpenses(params?: ExpenseQueryParams): Observable<PagedResponse<Expense>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.expenseCategoryId) httpParams = httpParams.set('expenseCategoryId', params.expenseCategoryId.toString());
            if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
            if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
        }

        return this.http.get<PagedResponse<Expense>>(this.expensesUrl, { params: httpParams });
    }

    getExpenseById(id: number): Observable<Expense> {
        return this.http.get<Expense>(`${this.expensesUrl}/${id}`);
    }

    createExpense(expense: Expense): Observable<number> {
        return this.http.post<number>(this.expensesUrl, expense);
    }

    updateExpense(id: number, expense: Expense): Observable<void> {
        return this.http.put<void>(`${this.expensesUrl}/${id}`, expense);
    }

    deleteExpense(id: number): Observable<void> {
        return this.http.delete<void>(`${this.expensesUrl}/${id}`);
    }
}

