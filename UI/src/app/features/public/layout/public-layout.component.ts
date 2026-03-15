import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PublicHeaderComponent } from './public-header.component';
import { PublicFooterComponent } from './public-footer.component';
import { LanguageService } from '../../../shared/services/language.service';
import { AuthService } from '../../../shared/services/auth.service';
import { DOCUMENT } from '@angular/common';

const BETA_BANNER_STORAGE_KEY = 'maba_beta_banner_dismissed';

@Component({
    selector: 'app-public-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, RouterOutlet, TranslateModule, PublicHeaderComponent, PublicFooterComponent],
    template: `
        <div class="public-layout" [dir]="languageService.direction" [class.with-beta-banner]="!bannerDismissed">
            <div class="public-top-fixed">
                <!-- Beta notice banner (dismissible, above navbar) -->
                <div class="beta-banner" *ngIf="!bannerDismissed">
                    <span class="beta-banner-text">{{ 'beta.banner.message' | translate }}</span>
                    <a routerLink="/contact" class="beta-banner-link">{{ 'beta.banner.contact' | translate }}</a>
                    <button type="button" class="beta-banner-close" (click)="dismissBanner()" [attr.aria-label]="'common.close' | translate">
                        <i class="pi pi-times"></i>
                    </button>
                </div>
                <app-public-header></app-public-header>
            </div>
            <main class="public-main-content">
                <router-outlet></router-outlet>
            </main>
            <app-public-footer></app-public-footer>
        </div>
    `,
    styles: [`
        .public-layout {
            display: block;
            min-height: 100vh;
            --public-header-offset: 70px;
        }
        .public-layout.with-beta-banner {
            --public-header-offset: 106px;
        }
        .public-top-fixed {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            z-index: 1000;
        }
        .public-top-fixed .public-header {
            position: relative;
            top: auto;
        }
        .beta-banner {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            height: 36px;
            padding-inline: 2.5rem 0.5rem;
            background: rgba(0, 0, 0, 0.04);
            border-bottom: 1px solid rgba(0, 0, 0, 0.06);
            font-size: 0.8125rem;
            color: #4b5563;
        }
        .beta-banner-text {
            flex: 1;
            text-align: center;
        }
        .beta-banner-link {
            color: #667eea;
            font-weight: 600;
            text-decoration: none;
            white-space: nowrap;
        }
        .beta-banner-link:hover {
            text-decoration: underline;
        }
        .beta-banner-close {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 28px;
            height: 28px;
            padding: 0;
            border: none;
            background: transparent;
            color: #6b7280;
            border-radius: 6px;
            cursor: pointer;
            transition: background 0.2s, color 0.2s;
            margin-inline-start: auto;
            flex-shrink: 0;
        }
        .beta-banner-close:hover {
            background: rgba(0, 0, 0, 0.06);
            color: #374151;
        }
        .public-main-content {
            display: block;
            width: 100%;
            padding-top: var(--public-header-offset);
        }
    `]
})
export class PublicLayoutComponent implements OnInit, OnDestroy {
    bannerDismissed = false;

    constructor(
        public languageService: LanguageService,
        public authService: AuthService,
        @Inject(DOCUMENT) private document: Document,
        @Inject(PLATFORM_ID) private platformId: Object
    ) {}

    ngOnInit(): void {
        if (isPlatformBrowser(this.platformId)) {
            try {
                this.bannerDismissed = localStorage.getItem(BETA_BANNER_STORAGE_KEY) === 'true';
            } catch {
                this.bannerDismissed = false;
            }
            const token = this.authService.token;
            if (token && !this.authService.user) {
                this.authService.getCurrentUser().subscribe();
            }
        } else {
            this.bannerDismissed = true;
        }
    }

    dismissBanner(): void {
        this.bannerDismissed = true;
        if (isPlatformBrowser(this.platformId)) {
            try {
                localStorage.setItem(BETA_BANNER_STORAGE_KEY, 'true');
            } catch {}
        }
    }

    ngOnDestroy(): void {}
}
