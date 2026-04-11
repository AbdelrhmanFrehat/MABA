import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Account, FiscalPeriod, JournalEntry,
    TrialBalanceRow, AccountLedgerEntry,
    CreateAccountRequest, UpdateAccountRequest,
    CreateJournalEntryRequest, CreateFiscalPeriodRequest
} from '../models/accounting.model';

@Injectable({ providedIn: 'root' })
export class AccountingApiService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    // --- Chart of Accounts ---

    getAccounts(params?: Record<string, any>): Observable<Account[]> {
        return this.http.get<Account[]>(`${this.baseUrl}/accounts`, { params: this.buildParams(params) });
    }

    getAccountTree(): Observable<Account[]> {
        return this.http.get<Account[]>(`${this.baseUrl}/accounts/tree`);
    }

    getAccountById(id: string): Observable<Account> {
        return this.http.get<Account>(`${this.baseUrl}/accounts/${id}`);
    }

    createAccount(request: CreateAccountRequest): Observable<Account> {
        return this.http.post<Account>(`${this.baseUrl}/accounts`, request);
    }

    updateAccount(id: string, request: UpdateAccountRequest): Observable<Account> {
        return this.http.put<Account>(`${this.baseUrl}/accounts/${id}`, request);
    }

    getAccountLedger(accountId: string, params?: Record<string, any>): Observable<any> {
        const p = { ...(params ?? {}), accountId };
        return this.http.get<any>(`${this.baseUrl}/reports/ledger`, { params: this.buildParams(p) });
    }

    // --- Journal Entries ---

    getJournalEntries(params?: Record<string, any>): Observable<any> {
        return this.http.get<any>(`${this.baseUrl}/journal-entries`, { params: this.buildParams(params) });
    }

    getJournalEntryById(id: string): Observable<JournalEntry> {
        return this.http.get<JournalEntry>(`${this.baseUrl}/journal-entries/${id}`);
    }

    createJournalEntry(request: CreateJournalEntryRequest): Observable<JournalEntry> {
        return this.http.post<JournalEntry>(`${this.baseUrl}/journal-entries`, request);
    }

    postJournalEntry(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/journal-entries/${id}/post`, {});
    }

    reverseJournalEntry(id: string): Observable<JournalEntry> {
        return this.http.post<JournalEntry>(`${this.baseUrl}/journal-entries/${id}/reverse`, {});
    }

    // --- Fiscal Periods ---

    getFiscalPeriods(): Observable<FiscalPeriod[]> {
        return this.http.get<FiscalPeriod[]>(`${this.baseUrl}/fiscal-periods`);
    }

    createFiscalPeriod(request: CreateFiscalPeriodRequest): Observable<FiscalPeriod> {
        return this.http.post<FiscalPeriod>(`${this.baseUrl}/fiscal-periods`, request);
    }

    closeFiscalPeriod(id: string): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/fiscal-periods/${id}/close`, {});
    }

    // --- Reports ---

    getTrialBalance(params?: Record<string, any>): Observable<any> {
        return this.http.get<any>(`${this.baseUrl}/reports/trial-balance`, { params: this.buildParams(params) });
    }

    getIncomeStatement(params?: Record<string, any>): Observable<any> {
        return this.http.get<any>(`${this.baseUrl}/reports/income-statement`, { params: this.buildParams(params) });
    }

    getBalanceSheet(params?: Record<string, any>): Observable<any> {
        return this.http.get<any>(`${this.baseUrl}/reports/balance-sheet`, { params: this.buildParams(params) });
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
