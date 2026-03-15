import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, Role } from '../models';
import { PaginationParams, PagedResponse } from '../models/api-response.model';

export interface UserQueryParams extends PaginationParams {
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class UsersService {
    private usersUrl = `${environment.apiUrl}/Users`;
    private rolesUrl = `${environment.apiUrl}/Roles`;

    constructor(private http: HttpClient) {}

    // Users
    getUsers(params?: UserQueryParams): Observable<PagedResponse<User>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        }

        return this.http.get<PagedResponse<User>>(this.usersUrl, { params: httpParams });
    }

    getUserById(id: number): Observable<User> {
        return this.http.get<User>(`${this.usersUrl}/${id}`);
    }

    createUser(user: User): Observable<number> {
        return this.http.post<number>(this.usersUrl, user);
    }

    updateUser(id: number, user: User): Observable<void> {
        return this.http.put<void>(`${this.usersUrl}/${id}`, user);
    }

    deleteUser(id: number): Observable<void> {
        return this.http.delete<void>(`${this.usersUrl}/${id}`);
    }

    // Roles
    getRoles(params?: PaginationParams): Observable<PagedResponse<Role>> {
        let httpParams = new HttpParams();
        
        if (params) {
            if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.search) httpParams = httpParams.set('search', params.search);
        }

        return this.http.get<PagedResponse<Role>>(this.rolesUrl, { params: httpParams });
    }

    getRolesForDropdown(): Observable<Role[]> {
        const params = new HttpParams()
            .set('pageNumber', '1')
            .set('pageSize', '1000');
        
        return this.http.get<PagedResponse<Role>>(this.rolesUrl, { params }).pipe(
            map(response => response.items)
        );
    }

    getRoleById(id: number): Observable<Role> {
        return this.http.get<Role>(`${this.rolesUrl}/${id}`);
    }

    createRole(role: Role): Observable<number> {
        return this.http.post<number>(this.rolesUrl, role);
    }

    updateRole(id: number, role: Role): Observable<void> {
        return this.http.put<void>(`${this.rolesUrl}/${id}`, role);
    }

    deleteRole(id: number): Observable<void> {
        return this.http.delete<void>(`${this.rolesUrl}/${id}`);
    }

    resetPassword(userId: number): Observable<void> {
        return this.http.post<void>(`${this.usersUrl}/${userId}/ResetPassword`, {});
    }
}

