import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Role, Permission, CreateRoleRequest, UpdateRoleRequest } from '../models/role.model';

@Injectable({
    providedIn: 'root'
})
export class RolesApiService {
    private baseUrl = `${environment.apiUrl}/roles`;

    constructor(private http: HttpClient) {}

    getAllRoles(): Observable<Role[]> {
        return this.http.get<Role[]>(this.baseUrl);
    }

    getRoleById(id: string): Observable<Role> {
        return this.http.get<Role>(`${this.baseUrl}/${id}`);
    }

    createRole(request: CreateRoleRequest): Observable<Role> {
        return this.http.post<Role>(this.baseUrl, request);
    }

    updateRole(id: string, request: UpdateRoleRequest): Observable<Role> {
        return this.http.put<Role>(`${this.baseUrl}/${id}`, request);
    }

    deleteRole(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    getAllPermissions(): Observable<Permission[]> {
        return this.http.get<Permission[]>(`${environment.apiUrl}/permissions`);
    }

    getUserRoles(userId: string): Observable<Role[]> {
        return this.http.get<Role[]>(`${environment.apiUrl}/users/${userId}/roles`);
    }

    assignRoleToUser(roleId: string, userId: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.baseUrl}/${roleId}/users/${userId}`, {});
    }

    removeRoleFromUser(roleId: string, userId: string): Observable<{ message: string }> {
        return this.http.delete<{ message: string }>(`${this.baseUrl}/${roleId}/users/${userId}`);
    }
}


