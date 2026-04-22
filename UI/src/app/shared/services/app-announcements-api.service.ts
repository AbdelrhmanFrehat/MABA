import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface AppAnnouncementDto {
    id: string;
    message: string;
    type?: string | null;
    isActive: boolean;
    displayOrder: number;
    targetPlatform: string;
    startsAt?: string | null;
    endsAt?: string | null;
    createdAt?: string;
    updatedAt?: string | null;
}

export interface CreateAppAnnouncementRequest {
    message: string;
    type?: string | null;
    isActive: boolean;
    displayOrder: number;
    targetPlatform: string;
    startsAt?: string | null;
    endsAt?: string | null;
}

export interface UpdateAppAnnouncementRequest {
    message: string;
    type?: string | null;
    isActive: boolean;
    displayOrder: number;
    targetPlatform: string;
    startsAt?: string | null;
    endsAt?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AppAnnouncementsApiService {
    constructor(private api: ApiService) {}

    getAdmin(): Observable<AppAnnouncementDto[]> {
        return this.api.get<AppAnnouncementDto[]>('app-announcements/admin');
    }

    create(req: CreateAppAnnouncementRequest): Observable<AppAnnouncementDto> {
        return this.api.post<AppAnnouncementDto>('app-announcements/admin', req);
    }

    update(id: string, req: UpdateAppAnnouncementRequest): Observable<AppAnnouncementDto> {
        return this.api.put<AppAnnouncementDto>('app-announcements/admin', id, req);
    }

    delete(id: string): Observable<void> {
        return this.api.delete<void>('app-announcements/admin', id);
    }

    toggle(id: string): Observable<AppAnnouncementDto> {
        return this.api.patch<AppAnnouncementDto>(`app-announcements/admin/${id}/toggle`, {});
    }
}
