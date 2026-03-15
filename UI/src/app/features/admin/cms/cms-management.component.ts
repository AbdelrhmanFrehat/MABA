import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-cms-management',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        CardModule,
        ButtonModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="cms-management-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.cms.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'menu.heroTicker' | translate" 
                            icon="pi pi-images"
                            [routerLink]="['/admin/hero-ticker']">
                        </p-button>
                        <p-button 
                            [label]="'admin.cms.managePages' | translate" 
                            icon="pi pi-file"
                            [outlined]="true"
                            [routerLink]="['/admin/cms/pages']">
                        </p-button>
                    </div>
                </div>
            </div>

            <!-- Cards Grid: Hero Ticker first so it's always visible -->
            <div class="cards-grid">
                <div class="card-wrapper">
                    <p-card class="cms-card cursor-pointer hover:shadow-4 transition-all" [routerLink]="['/admin/hero-ticker']">
                        <div class="card-content">
                            <i class="pi pi-images card-icon text-primary"></i>
                            <h3 class="card-title">{{ 'menu.heroTicker' | translate }}</h3>
                            <p class="card-description">{{ 'admin.heroTicker.cardDescription' | translate }}</p>
                            <p-button 
                                [label]="'admin.heroTicker.addImage' | translate"
                                [outlined]="true"
                                [routerLink]="['/admin/hero-ticker']"
                            ></p-button>
                        </div>
                    </p-card>
                </div>
                <div class="card-wrapper">
                    <p-card class="cms-card cursor-pointer hover:shadow-4 transition-all" (click)="navigateToPages()">
                        <div class="card-content">
                            <i class="pi pi-file card-icon text-primary"></i>
                            <h3 class="card-title">{{ 'admin.cms.pages' | translate }}</h3>
                            <p class="card-description">{{ 'admin.cms.pagesDescription' | translate }}</p>
                            <p-button 
                                [label]="'admin.cms.managePages' | translate"
                                [outlined]="true"
                                (click)="navigateToPages(); $event.stopPropagation()"
                            ></p-button>
                        </div>
                    </p-card>
                </div>
                <div class="card-wrapper">
                    <p-card class="cms-card cursor-pointer hover:shadow-4 transition-all" [routerLink]="['/admin/media']">
                        <div class="card-content">
                            <i class="pi pi-image card-icon text-primary"></i>
                            <h3 class="card-title">{{ 'admin.media.title' | translate }}</h3>
                            <p class="card-description">{{ 'admin.cms.mediaDescription' | translate }}</p>
                            <p-button 
                                [label]="'admin.media.manage' | translate"
                                [outlined]="true"
                                [routerLink]="['/admin/media']"
                            ></p-button>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .cms-management-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .cms-management-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .cms-management-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .page-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0;
        }

        @media (min-width: 768px) {
            .page-header h1 {
                font-size: 2rem;
            }
        }

        .header-actions {
            display: flex;
            gap: 0.5rem;
        }

        .cards-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .cards-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (min-width: 1024px) {
            .cards-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        .card-wrapper {
            width: 100%;
        }

        .cms-card {
            height: 100%;
        }

        .card-content {
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            padding: 1.5rem;
        }

        .card-icon {
            font-size: 3rem;
            margin-bottom: 1rem;
        }

        @media (min-width: 768px) {
            .card-icon {
                font-size: 4rem;
            }
        }

        .card-title {
            font-size: 1.25rem;
            font-weight: bold;
            margin-bottom: 0.5rem;
        }

        @media (min-width: 768px) {
            .card-title {
                font-size: 1.5rem;
            }
        }

        .card-description {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
            margin-bottom: 1rem;
        }

        @media (min-width: 768px) {
            .card-description {
                font-size: 1rem;
            }
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .cms-management-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }

            .card-icon {
                font-size: 2.5rem;
            }
        }
    `]
})
export class CmsManagementComponent implements OnInit {
    private router = inject(Router);

    ngOnInit() {
        // Component initialized
    }

    navigateToPages() {
        this.router.navigate(['/admin/cms/pages']);
    }
}
