import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LanguageService } from '../../../shared/services/language.service';
import {
    PUBLISHED_UPDATES,
    HIGHLIGHTED_UPDATES,
    WhatsNewEntry,
    UpdateType
} from '../../../core/data/whats-new.data';

@Component({
    selector: 'app-whats-new',
    standalone: true,
    imports: [CommonModule, RouterModule],
    template: `
        <div class="wn-page" [dir]="lang.direction">

            <!-- ── Hero ─────────────────────────────────────── -->
            <section class="wn-hero">
                <div class="wn-hero-inner">
                    <span class="wn-hero-badge">
                        <i class="pi pi-sparkles"></i>
                        {{ isAr ? 'تحديثات المنصة' : 'Platform Updates' }}
                    </span>
                    <h1 class="wn-hero-title">
                        {{ isAr ? 'الجديد في MABA' : "What's New in MABA" }}
                    </h1>
                    <p class="wn-hero-sub">
                        {{ isAr
                            ? 'تابع التحسينات والميزات الجديدة والإصلاحات مع تطور MABA خلال مرحلة البيتا.'
                            : 'Track improvements, new features, and fixes as MABA evolves during the Beta phase.' }}
                    </p>
                </div>
            </section>

            <div class="wn-content">

                <!-- ── Highlighted ───────────────────────────── -->
                <section class="wn-section">
                    <div class="wn-section-label">
                        <i class="pi pi-star-fill"></i>
                        {{ isAr ? 'أبرز التحديثات' : 'Highlights' }}
                    </div>
                    <div class="wn-highlights-grid">
                        @for (entry of highlighted; track entry.id) {
                            <div class="wn-highlight-card">
                                <div class="wn-card-top">
                                    <span class="wn-type-chip" [class]="'chip-' + entry.type">
                                        {{ typeLabel(entry.type) }}
                                    </span>
                                    <span class="wn-card-date">{{ formatDate(entry.date) }}</span>
                                </div>
                                <h3 class="wn-card-title">{{ isAr ? entry.titleAr : entry.title }}</h3>
                                <p class="wn-card-summary">{{ isAr ? entry.summaryAr : entry.summary }}</p>
                                @if (entry.details) {
                                    <p class="wn-card-details">{{ isAr ? entry.detailsAr : entry.details }}</p>
                                }
                            </div>
                        }
                    </div>
                </section>

                <!-- ── Full Timeline ─────────────────────────── -->
                <section class="wn-section">
                    <div class="wn-section-label">
                        <i class="pi pi-list"></i>
                        {{ isAr ? 'جميع التحديثات' : 'All Updates' }}
                    </div>
                    <div class="wn-timeline">
                        @for (entry of all; track entry.id; let last = $last) {
                            <div class="wn-tl-item" [class.last]="last">
                                <div class="wn-tl-dot" [class]="'dot-' + entry.type"></div>
                                <div class="wn-tl-body">
                                    <div class="wn-tl-meta">
                                        <span class="wn-type-chip" [class]="'chip-' + entry.type">
                                            {{ typeLabel(entry.type) }}
                                        </span>
                                        <span class="wn-version-tag">{{ entry.versionTag }}</span>
                                        <span class="wn-tl-date">{{ formatDate(entry.date) }}</span>
                                    </div>
                                    <h4 class="wn-tl-title">{{ isAr ? entry.titleAr : entry.title }}</h4>
                                    <p class="wn-tl-summary">{{ isAr ? entry.summaryAr : entry.summary }}</p>
                                    @if (entry.details && isExpanded(entry.id)) {
                                        <p class="wn-tl-details">{{ isAr ? entry.detailsAr : entry.details }}</p>
                                    }
                                    @if (entry.details) {
                                        <button class="wn-expand-btn" (click)="toggle(entry.id)">
                                            {{ isExpanded(entry.id)
                                                ? (isAr ? 'عرض أقل' : 'Show less')
                                                : (isAr ? 'اقرأ المزيد' : 'Read more') }}
                                            <i [class]="isExpanded(entry.id) ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"></i>
                                        </button>
                                    }
                                </div>
                            </div>
                        }
                    </div>
                </section>

            </div>
        </div>
    `,
    styles: [`
        /* ── Page shell ─────────────────────────────────── */
        .wn-page {
            min-height: 100vh;
            background: #f8fafc;
            padding-top: 64px; /* header height */
        }

        /* ── Hero ───────────────────────────────────────── */
        .wn-hero {
            background: #0f172a;
            padding: 4rem 1.5rem 3.5rem;
            text-align: center;
        }
        .wn-hero-inner {
            max-width: 640px;
            margin: 0 auto;
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 1rem;
        }
        .wn-hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.4rem;
            font-size: 0.72rem;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: #a5b4fc;
            background: rgba(165,180,252,0.12);
            border: 1px solid rgba(165,180,252,0.25);
            padding: 0.3rem 0.75rem;
            border-radius: 999px;
        }
        .wn-hero-badge .pi { font-size: 0.7rem; }
        .wn-hero-title {
            margin: 0;
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 800;
            color: #f1f5f9;
            letter-spacing: -0.02em;
            line-height: 1.15;
        }
        .wn-hero-sub {
            margin: 0;
            font-size: 1rem;
            color: #94a3b8;
            line-height: 1.6;
        }

        /* ── Content wrapper ────────────────────────────── */
        .wn-content {
            max-width: 1040px;
            margin: 0 auto;
            padding: 3rem 1.5rem 5rem;
            display: flex;
            flex-direction: column;
            gap: 3.5rem;
        }

        /* ── Section label ──────────────────────────────── */
        .wn-section-label {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 0.72rem;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: #667eea;
            margin-bottom: 1.5rem;
        }
        .wn-section-label .pi { font-size: 0.75rem; }

        /* ── Type chips ─────────────────────────────────── */
        .wn-type-chip {
            display: inline-flex;
            align-items: center;
            font-size: 0.68rem;
            font-weight: 700;
            letter-spacing: 0.05em;
            text-transform: uppercase;
            padding: 0.2rem 0.55rem;
            border-radius: 4px;
        }
        .chip-feature      { background: #dbeafe; color: #1d4ed8; }
        .chip-improvement  { background: #d1fae5; color: #065f46; }
        .chip-fix          { background: #fef3c7; color: #92400e; }
        .chip-announcement { background: #ede9fe; color: #5b21b6; }

        /* ── Highlights grid ────────────────────────────── */
        .wn-highlights-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 1rem;
        }
        .wn-highlight-card {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 1.25rem 1.375rem 1.375rem;
            display: flex;
            flex-direction: column;
            gap: 0.6rem;
            transition: box-shadow 0.15s, border-color 0.15s;
        }
        .wn-highlight-card:hover {
            border-color: #c7d2fe;
            box-shadow: 0 4px 16px rgba(102,126,234,0.08);
        }
        .wn-card-top {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 0.5rem;
        }
        .wn-card-date {
            font-size: 0.72rem;
            color: #94a3b8;
            white-space: nowrap;
        }
        .wn-card-title {
            margin: 0;
            font-size: 0.975rem;
            font-weight: 700;
            color: #0f172a;
            line-height: 1.35;
        }
        .wn-card-summary {
            margin: 0;
            font-size: 0.84rem;
            color: #475569;
            line-height: 1.6;
        }
        .wn-card-details {
            margin: 0;
            font-size: 0.8rem;
            color: #64748b;
            line-height: 1.65;
            padding-top: 0.5rem;
            border-top: 1px solid #f1f5f9;
        }

        /* ── Timeline ───────────────────────────────────── */
        .wn-timeline {
            display: flex;
            flex-direction: column;
        }
        .wn-tl-item {
            display: flex;
            gap: 1.25rem;
            position: relative;
            padding-bottom: 2rem;
        }
        .wn-tl-item.last { padding-bottom: 0; }
        .wn-tl-item:not(.last)::before {
            content: '';
            position: absolute;
            left: 7px;
            top: 16px;
            bottom: 0;
            width: 1px;
            background: #e2e8f0;
        }
        [dir="rtl"] .wn-tl-item:not(.last)::before {
            left: auto;
            right: 7px;
        }
        .wn-tl-dot {
            flex-shrink: 0;
            width: 15px;
            height: 15px;
            border-radius: 50%;
            margin-top: 3px;
            border: 2px solid #fff;
            box-shadow: 0 0 0 1px #e2e8f0;
        }
        .dot-feature      { background: #3b82f6; }
        .dot-improvement  { background: #10b981; }
        .dot-fix          { background: #f59e0b; }
        .dot-announcement { background: #8b5cf6; }

        .wn-tl-body { flex: 1; min-width: 0; }
        .wn-tl-meta {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 0.4rem;
            flex-wrap: wrap;
        }
        .wn-version-tag {
            font-size: 0.68rem;
            font-weight: 600;
            color: #94a3b8;
            background: #f1f5f9;
            padding: 0.15rem 0.45rem;
            border-radius: 4px;
        }
        .wn-tl-date { font-size: 0.72rem; color: #94a3b8; }
        .wn-tl-title {
            margin: 0 0 0.35rem;
            font-size: 0.95rem;
            font-weight: 700;
            color: #0f172a;
        }
        .wn-tl-summary {
            margin: 0;
            font-size: 0.84rem;
            color: #475569;
            line-height: 1.6;
        }
        .wn-tl-details {
            margin: 0.625rem 0 0;
            font-size: 0.8rem;
            color: #64748b;
            line-height: 1.65;
            padding: 0.625rem 0.75rem;
            background: #f8fafc;
            border-left: 3px solid #c7d2fe;
            border-radius: 0 6px 6px 0;
        }
        [dir="rtl"] .wn-tl-details {
            border-left: none;
            border-right: 3px solid #c7d2fe;
            border-radius: 6px 0 0 6px;
        }
        .wn-expand-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.3rem;
            margin-top: 0.5rem;
            background: none;
            border: none;
            color: #667eea;
            font-size: 0.78rem;
            font-weight: 600;
            cursor: pointer;
            padding: 0;
            transition: opacity 0.15s;
        }
        .wn-expand-btn:hover { opacity: 0.75; }
        .wn-expand-btn .pi { font-size: 0.65rem; }

        /* ── Responsive ─────────────────────────────────── */
        @media (max-width: 640px) {
            .wn-hero { padding: 3rem 1rem 2.5rem; }
            .wn-content { padding: 2rem 1rem 4rem; gap: 2.5rem; }
            .wn-highlights-grid { grid-template-columns: 1fr; }
        }
    `]
})
export class WhatsNewComponent {
    lang = inject(LanguageService);

    readonly highlighted = HIGHLIGHTED_UPDATES;
    readonly all = PUBLISHED_UPDATES;

    private expanded = new Set<string>();

    get isAr(): boolean {
        return this.lang.language === 'ar';
    }

    isExpanded(id: string): boolean {
        return this.expanded.has(id);
    }

    toggle(id: string) {
        this.expanded.has(id) ? this.expanded.delete(id) : this.expanded.add(id);
    }

    typeLabel(type: UpdateType): string {
        if (this.isAr) {
            const map: Record<UpdateType, string> = {
                feature: 'ميزة',
                improvement: 'تحسين',
                fix: 'إصلاح',
                announcement: 'إعلان'
            };
            return map[type];
        }
        const map: Record<UpdateType, string> = {
            feature: 'Feature',
            improvement: 'Improvement',
            fix: 'Fix',
            announcement: 'Announcement'
        };
        return map[type];
    }

    formatDate(iso: string): string {
        const locale = this.isAr ? 'ar-SA' : 'en-GB';
        return new Date(iso).toLocaleDateString(locale, {
            day: 'numeric', month: 'short', year: 'numeric'
        });
    }
}
