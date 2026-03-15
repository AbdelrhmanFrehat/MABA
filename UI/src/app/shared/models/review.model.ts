// Review Models
export interface Review {
    id: string; // GUID
    itemId: string;
    itemNameEn?: string;
    itemNameAr?: string;
    userId: string;
    userName: string;
    userEmail?: string;
    rating: number;
    title?: string;
    comment?: string;
    status: ReviewStatus;
    isVerifiedPurchase: boolean;
    helpfulCount: number;
    replies: ReviewReply[];
    createdAt: string;
    updatedAt?: string;
}

export interface ReviewReply {
    id: string; // GUID
    reviewId: string;
    userId: string;
    userName: string;
    comment: string;
    isAdminReply: boolean;
    createdAt: string;
}

export enum ReviewStatus {
    Pending = 'Pending',
    Approved = 'Approved',
    Rejected = 'Rejected'
}

export interface CreateReviewRequest {
    itemId: string;
    rating: number;
    title?: string;
    comment?: string;
}

export interface UpdateReviewRequest {
    rating?: number;
    title?: string;
    comment?: string;
}

export interface ReviewRating {
    itemId: string;
    averageRating: number;
    totalReviews: number;
    ratingDistribution: {
        five: number;
        four: number;
        three: number;
        two: number;
        one: number;
    };
}

export interface ReviewListResponse {
    items: Review[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    averageRating: number;
}

