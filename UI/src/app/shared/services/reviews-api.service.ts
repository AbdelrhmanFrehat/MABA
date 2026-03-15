import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
    Review,
    ReviewListResponse,
    ReviewRating,
    CreateReviewRequest,
    UpdateReviewRequest
} from '../models/review.model';

@Injectable({
    providedIn: 'root'
})
export class ReviewsApiService {
    constructor(private api: ApiService) {}

    getReviews(itemId: string, params?: {
        page?: number;
        pageSize?: number;
        status?: string;
        rating?: number;
    }): Observable<ReviewListResponse> {
        return this.api.get<ReviewListResponse>(`items/${itemId}/reviews`, params);
    }

    getReviewRating(itemId: string): Observable<ReviewRating> {
        return this.api.get<ReviewRating>(`items/${itemId}/reviews/rating`);
    }

    createReview(request: CreateReviewRequest): Observable<Review> {
        return this.api.post<Review>('reviews', request);
    }

    updateReview(id: string, request: UpdateReviewRequest): Observable<Review> {
        return this.api.put<Review>('reviews', id, request);
    }

    deleteReview(id: string): Observable<void> {
        return this.api.delete<void>('reviews', id);
    }

    // Admin endpoints
    getPendingReviews(params?: {
        page?: number;
        pageSize?: number;
    }): Observable<ReviewListResponse> {
        return this.api.get<ReviewListResponse>('reviews/pending', params);
    }

    approveReview(id: string): Observable<Review> {
        return this.api.post<Review>(`reviews/${id}/approve`, {});
    }

    rejectReview(id: string, reason?: string): Observable<Review> {
        return this.api.post<Review>(`reviews/${id}/reject`, { reason });
    }

    addReviewReply(reviewId: string, comment: string): Observable<Review> {
        return this.api.post<Review>(`reviews/${reviewId}/replies`, { comment });
    }
}

