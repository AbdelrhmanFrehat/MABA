import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Cart, CartSummary, AddToCartRequest, UpdateCartItemRequest, ApplyCouponRequest } from '../models/cart.model';

@Injectable({
    providedIn: 'root'
})
export class CartApiService {
    constructor(private api: ApiService) {}

    getCart(): Observable<Cart> {
        return this.api.get<Cart>('cart');
    }

    addToCart(request: AddToCartRequest): Observable<Cart> {
        return this.api.post<Cart>('cart/items', request);
    }

    updateCartItem(itemId: string, request: UpdateCartItemRequest): Observable<Cart> {
        return this.api.put<Cart>('cart/items', itemId, request);
    }

    removeCartItem(itemId: string): Observable<Cart> {
        return this.api.delete<Cart>('cart/items', itemId);
    }

    clearCart(): Observable<void> {
        return this.api.delete<void>('cart', 'clear');
    }

    applyCoupon(request: ApplyCouponRequest): Observable<Cart> {
        return this.api.post<Cart>('cart/apply-coupon', request);
    }

    removeCoupon(): Observable<Cart> {
        return this.api.post<Cart>('cart/remove-coupon', {});
    }

    getCartSummary(): Observable<CartSummary> {
        return this.api.get<CartSummary>('cart/summary');
    }
}

