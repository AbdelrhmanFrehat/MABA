import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of, switchMap, tap, catchError, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { LocalCartService } from './local-cart.service';
import { Cart, CartItem, AddToCartRequest, UpdateCartItemRequest } from '../models/cart.model';

@Injectable({
    providedIn: 'root'
})
export class CartService {
    private baseUrl = environment.apiUrl || '/api';
    private cartSubject = new BehaviorSubject<Cart | null>(null);
    
    cart$ = this.cartSubject.asObservable();

    private http = inject(HttpClient);
    private authService = inject(AuthService);
    private localCartService = inject(LocalCartService);

    constructor() {
        // Initial load
        this.loadCart();
    }

    private getHeaders(): HttpHeaders {
        let headers = new HttpHeaders();
        if (!this.authService.authenticated) {
            headers = headers.set('X-Session-Id', this.localCartService.getSessionId());
        }
        return headers;
    }

    loadCart(): void {
        if (this.authService.authenticated) {
            // Fetch from API
            this.http.get<Cart>(`${this.baseUrl}/cart`, { headers: this.getHeaders() })
                .pipe(
                    catchError(() => of(this.createEmptyCart()))
                )
                .subscribe(cart => this.cartSubject.next(cart));
        } else {
            // Use local cart
            const localCart = this.localCartService.getCurrentCart();
            this.cartSubject.next(localCart || this.createEmptyCart());
        }
    }

    getCart(): Observable<Cart> {
        const current = this.cartSubject.value;
        if (current) {
            return of(current);
        }

        if (this.authService.authenticated) {
            return this.http.get<Cart>(`${this.baseUrl}/cart`, { headers: this.getHeaders() })
                .pipe(
                    tap(cart => this.cartSubject.next(cart)),
                    catchError(() => of(this.createEmptyCart()))
                );
        } else {
            const localCart = this.localCartService.getCurrentCart() || this.createEmptyCart();
            this.cartSubject.next(localCart);
            return of(localCart);
        }
    }

    addToCart(request: AddToCartRequest, itemDetails?: { nameEn: string; nameAr: string; sku: string; price: number; imageUrl?: string }): Observable<Cart> {
        if (this.authService.authenticated) {
            return this.http.post<Cart>(`${this.baseUrl}/cart/items`, request, { headers: this.getHeaders() })
                .pipe(tap(cart => this.cartSubject.next(cart)));
        } else {
            if (!itemDetails) {
                throw new Error('Item details required for guest cart');
            }
            return this.localCartService.addToCart(request, itemDetails)
                .pipe(tap(cart => this.cartSubject.next(cart)));
        }
    }

    updateCartItem(itemId: string, request: UpdateCartItemRequest): Observable<Cart> {
        if (this.authService.authenticated) {
            return this.http.put<Cart>(`${this.baseUrl}/cart/items/${itemId}`, request, { headers: this.getHeaders() })
                .pipe(tap(cart => this.cartSubject.next(cart)));
        } else {
            return this.localCartService.updateCartItem(itemId, request)
                .pipe(tap(cart => this.cartSubject.next(cart)));
        }
    }

    removeCartItem(itemId: string): Observable<Cart> {
        if (this.authService.authenticated) {
            return this.http.delete<Cart>(`${this.baseUrl}/cart/items/${itemId}`, { headers: this.getHeaders() })
                .pipe(tap(cart => this.cartSubject.next(cart)));
        } else {
            return this.localCartService.removeCartItem(itemId)
                .pipe(tap(cart => this.cartSubject.next(cart)));
        }
    }

    clearCart(): Observable<void> {
        if (this.authService.authenticated) {
            return this.http.delete<void>(`${this.baseUrl}/cart`, { headers: this.getHeaders() })
                .pipe(tap(() => this.cartSubject.next(this.createEmptyCart())));
        } else {
            return this.localCartService.clearCart()
                .pipe(tap(() => this.cartSubject.next(this.createEmptyCart())));
        }
    }

    checkout(checkoutData: {
        shippingAddress: any;
        billingAddress: any;
        paymentMethod: string;
        shippingMethod: string;
        notes?: string;
    }): Observable<any> {
        if (!this.authService.authenticated) {
            throw new Error('Authentication required for checkout');
        }

        return this.http.post<any>(`${this.baseUrl}/cart/checkout`, checkoutData, { headers: this.getHeaders() })
            .pipe(
                tap(() => {
                    this.cartSubject.next(this.createEmptyCart());
                    this.localCartService.clearLocalCart();
                })
            );
    }

    private mergeGuestCart(): void {
        const localCart = this.localCartService.getCartForMerge();
        const sessionId = this.localCartService.getSessionId();

        if (localCart && localCart.items.length > 0) {
            // Call merge API
            this.http.post<Cart>(`${this.baseUrl}/cart/merge`, { sessionId }, { headers: this.getHeaders() })
                .pipe(
                    tap(cart => {
                        this.cartSubject.next(cart);
                        this.localCartService.clearLocalCart();
                        this.localCartService.clearSessionId();
                    }),
                    catchError(() => {
                        // If merge fails, just load user's cart
                        return this.http.get<Cart>(`${this.baseUrl}/cart`, { headers: this.getHeaders() });
                    })
                )
                .subscribe(cart => this.cartSubject.next(cart));
        } else {
            // No local cart, just load user's cart
            this.loadCart();
        }
    }

    getCartItemCount(): Observable<number> {
        return this.cart$.pipe(
            map(cart => cart?.items?.reduce((sum, item) => sum + item.quantity, 0) || 0)
        );
    }

    private createEmptyCart(): Cart {
        return {
            id: '',
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
}
