import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterRequest, LoginRequest, AuthResponse, User } from '../models/auth.model';

@Injectable({
    providedIn: 'root'
})
export class AuthApiService {
    private baseUrl = `${environment.apiUrl}/auth`;

    constructor(private http: HttpClient) {}

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.baseUrl}/register`, request);
    }

    login(request: LoginRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request);
    }

    getCurrentUser(): Observable<User> {
        return this.http.get<User>(`${this.baseUrl}/me`);
    }

    forgotPassword(email: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.baseUrl}/forgot-password`, { email });
    }

    resetPassword(token: string, newPassword: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.baseUrl}/reset-password`, { token, newPassword });
    }
}


