import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Wishlist, WishlistItem, AddToWishlistRequest } from '../models/wishlist.model';

@Injectable({
    providedIn: 'root'
})
export class WishlistApiService {
    constructor(private api: ApiService) {}

    getWishlist(): Observable<Wishlist> {
        return this.api.get<Wishlist>('wishlist');
    }

    addToWishlist(request: AddToWishlistRequest): Observable<Wishlist> {
        return this.api.post<Wishlist>('wishlist/items', request);
    }

    removeFromWishlist(itemId: string): Observable<Wishlist> {
        return this.api.delete<Wishlist>('wishlist/items', itemId);
    }

    clearWishlist(): Observable<void> {
        return this.api.delete<void>('wishlist', 'clear');
    }

    isInWishlist(itemId: string): Observable<boolean> {
        return this.api.get<boolean>(`wishlist/items/${itemId}/check`);
    }
}

