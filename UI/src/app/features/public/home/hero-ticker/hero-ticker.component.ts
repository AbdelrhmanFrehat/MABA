import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface HeroTickerItem {
    id: string;
    title?: string;
    imageUrl: string;
    sortOrder: number;
}

/**
 * Hero Parallelogram Image Ticker — Phase 1: static demo.
 * Non-clickable, scrolls left→right above hero. No API; items passed in or use placeholders.
 */
@Component({
    selector: 'app-hero-ticker',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="hero-ticker" role="presentation" aria-hidden="true" [class.hero-ticker--visible]="displayItems.length > 0">
            <div class="hero-ticker__mask hero-ticker__mask--left"></div>
            <div class="hero-ticker__mask hero-ticker__mask--right"></div>
            <!-- Closed loop: one track with TWO identical sets; -50% translate = seamless loop -->
            <div class="hero-ticker__track">
                <!-- Set A -->
                <div class="hero-ticker__tile" *ngFor="let item of displayItems" [class.hero-ticker__tile--placeholder]="imageErrorIds.has(item.id)">
                    <div class="hero-ticker__tile__shape">
                        <img [src]="item.imageUrl" [alt]="item.title || ''" loading="lazy" (error)="onImageError(item.id)" />
                    </div>
                </div>
                <!-- Set B (duplicate for seamless loop) -->
                <div class="hero-ticker__tile" *ngFor="let item of displayItems" [class.hero-ticker__tile--placeholder]="imageErrorIds.has(item.id)">
                    <div class="hero-ticker__tile__shape">
                        <img [src]="item.imageUrl" [alt]="item.title || ''" loading="lazy" (error)="onImageError(item.id)" />
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .hero-ticker {
            position: absolute;
            /* Start just below fixed header (moved up to minimize gap under bar) */
            top: calc(var(--public-header-offset, 70px) - 32px);
            left: 0;
            right: 0;
            height: 140px;
            z-index: 5;
            overflow: hidden;
            pointer-events: none;
            direction: ltr;
            visibility: hidden;
        }
        .hero-ticker.hero-ticker--visible {
            visibility: visible;
        }

        .hero-ticker__mask {
            position: absolute;
            top: 0;
            bottom: 0;
            width: 80px;
            z-index: 2;
            pointer-events: none;
        }
        .hero-ticker__mask--left {
            left: 0;
            background: linear-gradient(to right, rgba(12, 20, 69, 0.95) 0%, transparent 100%);
        }
        .hero-ticker__mask--right {
            right: 0;
            left: auto;
            background: linear-gradient(to left, rgba(12, 20, 69, 0.95) 0%, transparent 100%);
        }

        /* Closed-loop track: holds two identical sets; -50% = one set width = seamless loop */
        .hero-ticker__track {
            position: absolute;
            left: 0;
            top: 0;
            height: 100%;
            display: flex;
            flex-wrap: nowrap;
            align-items: center;
            gap: 8px;
            padding: 0 24px;
            width: max-content;
            will-change: transform;
            animation: hero-ticker-scroll 45s linear infinite;
        }
        @media (prefers-reduced-motion: reduce) {
            .hero-ticker__track {
                animation: none;
            }
        }

        /* Parallelogram: radius + shadow on wrapper only; clip-path on inner (no radius on clipped shape) */
        .hero-ticker__tile {
            flex-shrink: 0;
            width: 260px;
            height: 130px;
            margin-right: -8px;
            border-radius: 16px;
            overflow: hidden;
            background: transparent;
            box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
        }
        .hero-ticker__tile__shape {
            width: 100%;
            height: 100%;
            overflow: hidden;
            border-radius: 0;
            clip-path: polygon(10% 0, 100% 0, 90% 100%, 0 100%);
        }
        .hero-ticker__tile__shape img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }

        /* Placeholder when image fails: no broken icon */
        .hero-ticker__tile--placeholder .hero-ticker__tile__shape {
            background: linear-gradient(135deg, rgba(40, 50, 90, 0.6) 0%, rgba(25, 35, 70, 0.8) 100%);
        }
        .hero-ticker__tile--placeholder .hero-ticker__tile__shape img {
            opacity: 0;
        }

        @keyframes hero-ticker-scroll {
            from { transform: translateX(0); }
            to   { transform: translateX(-50%); }
        }

        /* Tablet */
        @media (max-width: 1024px) {
            .hero-ticker { height: 120px; }
            .hero-ticker__tile { width: 220px; height: 110px; }
            .hero-ticker__mask { width: 60px; }
        }

        /* Mobile: hide ticker */
        @media (max-width: 768px) {
            .hero-ticker {
                display: none;
            }
        }
    `]
})
export class HeroTickerComponent {
    /** When not provided, uses static placeholder items for Phase 1 demo. */
    @Input() items: HeroTickerItem[] = [];

    /** IDs of items whose image failed to load; show placeholder instead of broken icon. */
    readonly imageErrorIds = new Set<string>();

    private readonly placeholderItems = this.getPlaceholderItems();

    get displayItems(): HeroTickerItem[] {
        return (this.items?.length ? this.items : this.placeholderItems) || [];
    }

    onImageError(itemId: string): void {
        this.imageErrorIds.add(itemId);
    }

    private getPlaceholderItems(): HeroTickerItem[] {
        const img = 'assets/img/defult.png';
        return [
            { id: 't1', imageUrl: img, sortOrder: 1 },
            { id: 't2', imageUrl: img, sortOrder: 2 },
            { id: 't3', imageUrl: img, sortOrder: 3 },
            { id: 't4', imageUrl: img, sortOrder: 4 },
            { id: 't5', imageUrl: img, sortOrder: 5 }
        ];
    }
}
