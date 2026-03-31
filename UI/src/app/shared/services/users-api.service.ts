import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../models/auth.model';

export interface UpdateUserRequest {
    fullName: string;
    phone?: string;
}

@Injectable({
    providedIn: 'root'
})
export class UsersApiService {
    private baseUrl = `${environment.apiUrl}/users`;
    private authBaseUrl = `${environment.apiUrl}/auth`;

    constructor(private http: HttpClient) {}

    getAllUsers(isActive?: boolean): Observable<User[]> {
        let params = new HttpParams();
        if (isActive !== undefined) {
            params = params.set('isActive', isActive.toString());
        }
        return this.http.get<User[]>(this.baseUrl, { params });
    }

    getUserById(id: string): Observable<User> {
        return this.http.get<User>(`${this.baseUrl}/${id}`);
    }

    updateUser(id: string, request: UpdateUserRequest): Observable<User> {
        return this.http.put<User>(`${this.baseUrl}/${id}`, request);
    }

    getUsersCount(isActive?: boolean): Observable<number> {
        let params = new HttpParams();
        if (isActive !== undefined) {
            params = params.set('isActive', isActive.toString());
        }
        return this.http.get<number>(`${this.baseUrl}/count`, { params });
    }

    resendVerification(email: string): Observable<{ success: boolean; message: string }> {
        return this.http.post<{ success: boolean; message: string }>(`${this.authBaseUrl}/resend-verification`, { email });
    }
}


