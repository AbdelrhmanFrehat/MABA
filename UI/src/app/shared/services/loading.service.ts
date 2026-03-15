import { Injectable, signal } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class LoadingService {
    private loadingCount = 0;
    
    isLoading = signal(false);
    loadingMessage = signal('');

    show(message?: string): void {
        this.loadingCount++;
        this.isLoading.set(true);
        if (message) {
            this.loadingMessage.set(message);
        }
    }

    hide(): void {
        this.loadingCount--;
        if (this.loadingCount <= 0) {
            this.loadingCount = 0;
            this.isLoading.set(false);
            this.loadingMessage.set('');
        }
    }

    reset(): void {
        this.loadingCount = 0;
        this.isLoading.set(false);
        this.loadingMessage.set('');
    }
}
