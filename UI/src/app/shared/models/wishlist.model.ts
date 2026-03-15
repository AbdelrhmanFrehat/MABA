// Wishlist Models
export interface Wishlist {
    id: string; // GUID
    userId: string;
    items: WishlistItem[];
    createdAt: string;
    updatedAt?: string;
}

export interface WishlistItem {
    id: string; // GUID
    wishlistId: string;
    itemId: string;
    item: {
        id: string;
        nameEn: string;
        nameAr: string;
        sku: string;
        price: number;
        currency: string;
        averageRating: number;
        reviewsCount: number;
        mediaAssets?: Array<{
            id: string;
            fileUrl: string;
            altText?: string;
            isPrimary: boolean;
        }>;
    };
    addedAt: string;
}

export interface AddToWishlistRequest {
    itemId: string;
}

