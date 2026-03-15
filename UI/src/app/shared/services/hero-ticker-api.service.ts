import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

/** Response from GET hero-ticker/public */
export interface HeroTickerPublicDto {
    id: string;
    title?: string;
    imageUrl: string;
    sortOrder: number;
}

/** Admin DTO from GET hero-ticker/admin */
export interface HeroTickerItemDto {
    id: string;
    title?: string;
    imageUrl: string;
    sortOrder: number;
    isActive: boolean;
    createdAt?: string;
    updatedAt?: string;
}

export interface CreateHeroTickerItemRequest {
    title?: string;
    imageUrl: string;
    sortOrder: number;
    isActive: boolean;
}

export interface UpdateHeroTickerItemRequest {
    id: string;
    title?: string;
    imageUrl?: string;
    sortOrder?: number;
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class HeroTickerApiService {
    constructor(private api: ApiService) {}

    /** Public endpoint: active items ordered by SortOrder (for home hero ticker). */
    getPublic(): Observable<HeroTickerPublicDto[]> {
        return this.api.get<HeroTickerPublicDto[]>('hero-ticker/public');
    }

    /** Admin: all items ordered by SortOrder. */
    getAdmin(): Observable<HeroTickerItemDto[]> {
        return this.api.get<HeroTickerItemDto[]>('hero-ticker/admin');
    }

    create(request: CreateHeroTickerItemRequest): Observable<HeroTickerItemDto> {
        return this.api.post<HeroTickerItemDto>('hero-ticker/admin', request);
    }

    update(id: string, request: UpdateHeroTickerItemRequest): Observable<HeroTickerItemDto | null> {
        return this.api.put<HeroTickerItemDto | null>('hero-ticker/admin', id, request);
    }

    delete(id: string): Observable<void> {
        return this.api.delete<void>('hero-ticker/admin', id);
    }

    reorder(items: { id: string; sortOrder: number }[]): Observable<void> {
        return this.api.patch<void>('hero-ticker/admin/reorder', { items });
    }
}
