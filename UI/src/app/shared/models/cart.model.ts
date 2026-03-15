// Cart Models
export interface Cart {
    id: string; // GUID
    userId?: string;
    sessionId?: string;
    items: CartItem[];
    subtotal: number;
    taxAmount: number;
    shippingAmount: number;
    discountAmount: number;
    total: number;
    currency: string;
    couponCode?: string;
    couponDiscount?: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CartItem {
    id: string; // GUID
    cartId: string;
    itemId: string;
    itemNameEn: string;
    itemNameAr: string;
    itemSku: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
    mediaAssetId?: string;
    mediaAssetUrl?: string;
}

export interface CartSummary {
    itemCount: number;
    subtotal: number;
    taxAmount: number;
    shippingAmount: number;
    discountAmount: number;
    total: number;
    currency: string;
}

export interface AddToCartRequest {
    itemId: string;
    quantity: number;
}

export interface UpdateCartItemRequest {
    quantity: number;
}

export interface ApplyCouponRequest {
    couponCode: string;
}

