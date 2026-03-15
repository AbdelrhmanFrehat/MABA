import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Project,
    ProjectsListResponse,
    ProjectRequest,
    ProjectCategory,
    ProjectStatus,
    ProjectRequestStatus,
    ProjectRequestType,
    CreateProjectRequestPayload,
    CreateProjectPayload,
    UpdateProjectPayload
} from '../models/project.model';

@Injectable({ providedIn: 'root' })
export class ProjectsApiService {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/projects`;

    getProjects(params: {
        search?: string;
        category?: ProjectCategory;
        status?: ProjectStatus;
        isFeatured?: boolean;
        page?: number;
        pageSize?: number;
    } = {}): Observable<ProjectsListResponse> {
        let httpParams = new HttpParams();
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.category !== undefined) httpParams = httpParams.set('category', params.category.toString());
        if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
        if (params.isFeatured !== undefined) httpParams = httpParams.set('isFeatured', params.isFeatured.toString());
        if (params.page) httpParams = httpParams.set('page', params.page.toString());
        if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

        return this.http.get<ProjectsListResponse>(this.baseUrl, { params: httpParams });
    }

    getProjectBySlug(slug: string): Observable<Project> {
        return this.http.get<Project>(`${this.baseUrl}/${slug}`);
    }

    getCategories(): Observable<{ value: number; name: string }[]> {
        return this.http.get<{ value: number; name: string }[]>(`${this.baseUrl}/categories`);
    }

    getStatuses(): Observable<{ value: number; name: string }[]> {
        return this.http.get<{ value: number; name: string }[]>(`${this.baseUrl}/statuses`);
    }

    submitProjectRequest(payload: CreateProjectRequestPayload): Observable<{ success: boolean; referenceNumber: string; message: string }> {
        return this.http.post<{ success: boolean; referenceNumber: string; message: string }>(`${this.baseUrl}/request`, payload);
    }

    uploadAttachment(file: File): Observable<{ url: string; fileName: string }> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<{ url: string; fileName: string }>(`${this.baseUrl}/request/upload`, formData);
    }

    // Admin endpoints
    getProjectsAdmin(params: {
        search?: string;
        category?: ProjectCategory;
        status?: ProjectStatus;
        isFeatured?: boolean;
        isActive?: boolean;
        page?: number;
        pageSize?: number;
    } = {}): Observable<ProjectsListResponse> {
        let httpParams = new HttpParams();
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.category !== undefined) httpParams = httpParams.set('category', params.category.toString());
        if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
        if (params.isFeatured !== undefined) httpParams = httpParams.set('isFeatured', params.isFeatured.toString());
        if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
        if (params.page) httpParams = httpParams.set('page', params.page.toString());
        if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

        return this.http.get<ProjectsListResponse>(`${this.baseUrl}/admin`, { params: httpParams });
    }

    getProjectById(id: string): Observable<Project> {
        return this.http.get<Project>(`${this.baseUrl}/admin/${id}`);
    }

    createProject(payload: CreateProjectPayload): Observable<Project> {
        return this.http.post<Project>(`${this.baseUrl}/admin`, payload);
    }

    updateProject(id: string, payload: UpdateProjectPayload): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/admin/${id}`, payload);
    }

    deleteProject(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/admin/${id}`);
    }

    getProjectRequests(params: {
        status?: ProjectRequestStatus;
        requestType?: ProjectRequestType;
        search?: string;
        skip?: number;
        take?: number;
    } = {}): Observable<ProjectRequest[]> {
        let httpParams = new HttpParams();
        if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
        if (params.requestType !== undefined) httpParams = httpParams.set('requestType', params.requestType.toString());
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.skip !== undefined) httpParams = httpParams.set('skip', params.skip.toString());
        if (params.take !== undefined) httpParams = httpParams.set('take', params.take.toString());

        return this.http.get<ProjectRequest[]>(`${this.baseUrl}/admin/requests`, { params: httpParams });
    }

    updateProjectRequest(id: string, payload: { status?: ProjectRequestStatus; adminNotes?: string }): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/admin/requests/${id}`, { id, ...payload });
    }
}
