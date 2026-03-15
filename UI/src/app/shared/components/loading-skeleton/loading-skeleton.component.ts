import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-loading-skeleton',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="skeleton-container" [class]="containerClass">
            <div *ngFor="let item of items" class="skeleton-item" [style.width]="itemWidth" [style.height]="itemHeight">
                <div class="skeleton-shimmer"></div>
            </div>
        </div>
    `,
    styles: [`
        .skeleton-container {
            display: flex;
            flex-wrap: wrap;
            gap: 1rem;
        }
        
        .skeleton-item {
            background: var(--surface-ground);
            border-radius: 8px;
            overflow: hidden;
            position: relative;
        }
        
        .skeleton-shimmer {
            width: 100%;
            height: 100%;
            background: linear-gradient(
                90deg,
                var(--surface-ground) 0%,
                var(--surface-100) 50%,
                var(--surface-ground) 100%
            );
            background-size: 200% 100%;
            animation: shimmer 1.5s infinite;
        }
        
        @keyframes shimmer {
            0% {
                background-position: -200% 0;
            }
            100% {
                background-position: 200% 0;
            }
        }
    `]
})
export class LoadingSkeletonComponent {
    @Input() count: number = 6;
    @Input() itemWidth: string = '100%';
    @Input() itemHeight: string = '200px';
    @Input() containerClass: string = '';

    get items(): number[] {
        return Array(this.count).fill(0);
    }
}

