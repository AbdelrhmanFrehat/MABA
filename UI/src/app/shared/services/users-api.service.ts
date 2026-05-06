import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User } from '../models/auth.model';

export interface UpdateUserRequest {
    fullName: string;
    phone?: string;
}

export interface UserSearchResult {
    id: string;
    fullName: string;
    email: string;
    phone?: string | null;
    isActive: boolean;
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

    searchUsers(term: string, limit = 20): Observable<UserSearchResult[]> {
        const params = new HttpParams()
            .set('searchTerm', term)
            .set('pageSize', limit.toString())
            .set('isActive', 'true');
        return this.http.get<any>(`${this.baseUrl}/search`, { params }).pipe(
            map(res => {
                // Backend returns PagedResult<UserDto> with an items array
                const items: any[] = Array.isArray(res) ? res : (res?.items ?? res?.data ?? []);
                return items.map((u: any) => ({
                    id: u.id,
                    fullName: u.fullName ?? u.name ?? '',
                    email: u.email ?? '',
                    phone: u.phone ?? null,
                    isActive: u.isActive ?? true
                } as UserSearchResult));
            })
        );
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


