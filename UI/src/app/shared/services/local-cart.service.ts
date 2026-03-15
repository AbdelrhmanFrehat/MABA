import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { Cart, CartItem, AddToCartRequest, UpdateCartItemRequest } from '../models/cart.model';

const CART_STORAGE_KEY = 'maba_guest_cart';
const SESSION_ID_KEY = 'maba_session_id';

@Injectable({
    providedIn: 'root'
})
export class LocalCartService {
    private cartSubject = new BehaviorSubject<Cart | null>(null);
    
    constructor() {
        this.loadFromStorage();
    }

    private loadFromStorage(): void {
        try {
            const stored = localStorage.getItem(CART_STORAGE_KEY);
            if (stored) {
                const cart = JSON.parse(stored) as Cart;
                this.cartSubject.next(cart);
            }
        } catch {
            console.error('Failed to load cart from storage');
        }
    }

    private saveToStorage(cart: Cart): void {
        try {
            localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart));
            this.cartSubject.next(cart);
        } catch {
            console.error('Failed to save cart to storage');
        }
    }

    getSessionId(): string {
        let sessionId = localStorage.getItem(SESSION_ID_KEY);
        if (!sessionId) {
            sessionId = 'guest_' + crypto.randomUUID();
            localStorage.setItem(SESSION_ID_KEY, sessionId);
        }
        return sessionId;
    }

    clearSessionId(): void {
        localStorage.removeItem(SESSION_ID_KEY);
    }

    getCart(): Observable<Cart | null> {
        return this.cartSubject.asObservable();
    }

    getCurrentCart(): Cart | null {
        return this.cartSubject.value;
    }

    addToCart(request: AddToCartRequest, itemDetails: { nameEn: string; nameAr: string; sku: string; price: number; imageUrl?: string }): Observable<Cart> {
        let cart = this.cartSubject.value;
        
        if (!cart) {
            cart = this.createEmptyCart();
        }

        const existingItem = cart.items.find(i => i.itemId === request.itemId);
        
        if (existingItem) {
            existingItem.quantity += request.quantity;
            existingItem.subtotal = existingItem.quantity * existingItem.unitPrice;
        } else {
            const newItem: CartItem = {
                id: crypto.randomUUID(),
                cartId: cart.id,
                itemId: request.itemId,
                itemNameEn: itemDetails.nameEn,
                itemNameAr: itemDetails.nameAr,
                itemSku: itemDetails.sku,
                quantity: request.quantity,
                unitPrice: itemDetails.price,
                subtotal: request.quantity * itemDetails.price,
                mediaAssetUrl: itemDetails.imageUrl
            };
            cart.items.push(newItem);
        }

        this.recalculateTotals(cart);
        this.saveToStorage(cart);
        
        return of(cart);
    }

    updateCartItem(itemId: string, request: UpdateCartItemRequest): Observable<Cart> {
        const cart = this.cartSubject.value;
        
        if (!cart) {
            throw new Error('Cart not found');
        }

        if (request.quantity <= 0) {
            return this.removeCartItem(itemId);
        }

        const item = cart.items.find(i => i.id === itemId);
        if (item) {
            item.quantity = request.quantity;
            item.subtotal = item.quantity * item.unitPrice;
        }

        this.recalculateTotals(cart);
        this.saveToStorage(cart);
        
        return of(cart);
    }

    removeCartItem(itemId: string): Observable<Cart> {
        const cart = this.cartSubject.value;
        
        if (!cart) {
            throw new Error('Cart not found');
        }

        cart.items = cart.items.filter(i => i.id !== itemId);
        
        this.recalculateTotals(cart);
        this.saveToStorage(cart);
        
        return of(cart);
    }

    clearCart(): Observable<void> {
        localStorage.removeItem(CART_STORAGE_KEY);
        this.cartSubject.next(null);
        return of(undefined);
    }

    getCartForMerge(): Cart | null {
        return this.cartSubject.value;
    }

    clearLocalCart(): void {
        localStorage.removeItem(CART_STORAGE_KEY);
        this.cartSubject.next(null);
    }

    private createEmptyCart(): Cart {
        return {
            id: crypto.randomUUID(),
            sessionId: this.getSessionId(),
            items: [],
            subtotal: 0,
            taxAmount: 0,
            shippingAmount: 0,
            discountAmount: 0,
            total: 0,
            currency: 'ILS',
            createdAt: new Date().toISOString()
        };
    }

    private recalculateTotals(cart: Cart): void {
        // Ensure each item subtotal is quantity × unitPrice (in case of stale data)
        cart.items.forEach(item => {
            item.subtotal = Math.round(item.quantity * item.unitPrice * 100) / 100;
        });
        cart.subtotal = Math.round(cart.items.reduce((sum, item) => sum + item.subtotal, 0) * 100) / 100;
        cart.taxAmount = Math.round(cart.subtotal * 0.15 * 100) / 100; // 15% VAT
        cart.shippingAmount = cart.subtotal > 200 ? 0 : 25; // Free shipping over ₪200
        cart.discountAmount = cart.couponDiscount ?? 0;
        cart.total = Math.round((cart.subtotal + cart.taxAmount + cart.shippingAmount - cart.discountAmount) * 100) / 100;
        cart.updatedAt = new Date().toISOString();
    }
}
