import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { RatingModule } from 'primeng/rating';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ReviewsApiService } from '../../../shared/services/reviews-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Review } from '../../../shared/models/review.model';
import { DataTableComponent } from '../../../shared/components/data-table/data-table';

@Component({
    selector: 'app-reviews-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TableModule,
        SelectModule,
        TagModule,
        RatingModule,
        InputTextModule,
        ToastModule,
        DialogModule,
        TranslateModule,
        DataTableComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="reviews-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.reviews.title' | translate }}</h1>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <p-card class="filters-card">
                <div class="filters-grid">
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.reviews.filterByStatus' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedStatus"
                            [options]="statusOptions"
                            [placeholder]="'admin.reviews.allStatuses' | translate"
                            (onChange)="loadReviews()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                    <div class="filter-item">
                        <label class="filter-label">{{ 'admin.reviews.filterByRating' | translate }}</label>
                        <p-select
                            [(ngModel)]="selectedRating"
                            [options]="ratingOptions"
                            [placeholder]="'admin.reviews.allRatings' | translate"
                            (onChange)="loadReviews()"
                            [showClear]="true"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full">
                        </p-select>
                    </div>
                </div>
            </p-card>

            <!-- Reviews Table -->
            <div class="table-section">
                <p-card>
                    <app-data-table
                        [data]="reviews"
                        [columns]="columns"
                        [loading]="loading"
                        [paginator]="true"
                        [rows]="pageSize"
                        [lazy]="true"
                        [totalRecords]="totalRecords"
                        (onLazyLoad)="onLazyLoad($event)"
                        (onAction)="handleAction($event)">
                    </app-data-table>
                </p-card>
            </div>

            <!-- Review Detail Dialog -->
            <p-dialog 
                [header]="'admin.reviews.reviewDetails' | translate"
                [(visible)]="showDetailDialog"
                [modal]="true"
                [style]="{width: '90vw', maxWidth: '600px'}"
                [breakpoints]="{'960px': '90vw', '640px': '95vw'}"
                (onHide)="closeDetailDialog()">
                <div class="dialog-content" *ngIf="selectedReview">
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.item' | translate }}:</strong> {{ getItemName(selectedReview) }}
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.customer' | translate }}:</strong> {{ selectedReview.userName }}
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.rating' | translate }}:</strong>
                        <p-rating [ngModel]="selectedReview.rating" [readonly]="true" [stars]="5"></p-rating>
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.title' | translate }}:</strong> {{ getReviewTitle(selectedReview) }}
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.comment' | translate }}:</strong>
                        <p>{{ getReviewComment(selectedReview) }}</p>
                    </div>
                    <div class="dialog-field">
                        <strong>{{ 'admin.reviews.status' | translate }}:</strong>
                        <p-tag [value]="getStatusName(selectedReview.status)" [severity]="getStatusSeverity(selectedReview.status)"></p-tag>
                    </div>
                    <div class="dialog-actions">
                        <p-button 
                            *ngIf="selectedReview.status === 'Pending'"
                            [label]="'admin.reviews.approve' | translate" 
                            icon="pi pi-check"
                            severity="success"
                            (click)="approveReview()"
                            styleClass="w-full md:w-auto">
                        </p-button>
                        <p-button 
                            *ngIf="selectedReview.status === 'Pending'"
                            [label]="'admin.reviews.reject' | translate" 
                            icon="pi pi-times"
                            severity="danger"
                            [outlined]="true"
                            (click)="rejectReview()"
                            styleClass="w-full md:w-auto">
                        </p-button>
                        <p-button 
                            [label]="'common.close' | translate" 
                            [outlined]="true"
                            (click)="closeDetailDialog()"
                            styleClass="w-full md:w-auto">
                        </p-button>
                    </div>
                </div>
            </p-dialog>
        </div>
    `,
    styles: [`
        .reviews-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .reviews-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .reviews-list-container {
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

        .filters-card {
            margin-bottom: 1.5rem;
        }

        .filters-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .filters-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .filter-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .filter-label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .reviews-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }

        /* Dialog styles */
        .dialog-content {
            padding: 0.5rem 0;
        }

        @media (min-width: 768px) {
            .dialog-content {
                padding: 1rem 0;
            }
        }

        .dialog-field {
            margin-bottom: 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 1px solid var(--surface-border);
        }

        .dialog-field:last-of-type {
            border-bottom: none;
        }

        .dialog-field strong {
            display: block;
            margin-bottom: 0.25rem;
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }

        .dialog-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            justify-content: flex-end;
            margin-top: 1.5rem;
        }

        @media (max-width: 767px) {
            .dialog-actions {
                flex-direction: column;
            }

            .dialog-actions p-button {
                width: 100%;
            }
        }
    `]
})
export class ReviewsListComponent implements OnInit {
    reviews: Review[] = [];
    loading = false;
    currentPage = 1;
    pageSize = 10;
    totalRecords = 0;
    selectedStatus: string = '';
    selectedRating: number | null = null;
    showDetailDialog = false;
    selectedReview: Review | null = null;

    statusOptions: any[] = [
        { label: 'All Statuses', value: '' },
        { label: 'Pending', value: 'Pending' },
        { label: 'Approved', value: 'Approved' },
        { label: 'Rejected', value: 'Rejected' }
    ];

    ratingOptions: any[] = [
        { label: 'All Ratings', value: null },
        { label: '5 Stars', value: 5 },
        { label: '4 Stars', value: 4 },
        { label: '3 Stars', value: 3 },
        { label: '2 Stars', value: 2 },
        { label: '1 Star', value: 1 }
    ];

    columns: any[] = [];

    private reviewsApiService = inject(ReviewsApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);

    ngOnInit() {
        this.setupColumns();
        this.loadReviews();
    }

    setupColumns() {
        const lang = this.languageService.language;
        this.columns = [
            { field: 'itemName', header: lang === 'ar' ? 'المنتج' : 'Item', sortable: true },
            { field: 'userName', header: lang === 'ar' ? 'العميل' : 'Customer', sortable: true },
            { field: 'rating', header: lang === 'ar' ? 'التقييم' : 'Rating', sortable: true },
            { field: 'title', header: lang === 'ar' ? 'العنوان' : 'Title', sortable: true },
            { field: 'status', header: lang === 'ar' ? 'الحالة' : 'Status', sortable: true },
            { field: 'createdAt', header: lang === 'ar' ? 'تاريخ الإنشاء' : 'Created At', sortable: true },
            { field: 'actions', header: lang === 'ar' ? 'الإجراءات' : 'Actions', sortable: false }
        ];
    }

    loadReviews() {
        this.loading = true;
        const params: any = {
            page: this.currentPage,
            pageSize: this.pageSize
        };

        if (this.selectedStatus) {
            params.status = this.selectedStatus;
        }
        if (this.selectedRating) {
            params.rating = this.selectedRating;
        }

        this.reviewsApiService.getPendingReviews(params).subscribe({
            next: (response) => {
                this.reviews = response.items || [];
                this.totalRecords = response.totalCount || 0;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    onLazyLoad(event: any) {
        this.currentPage = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.loadReviews();
    }

    handleAction(event: { action: string; data: any }) {
        const review = event.data as Review;
        switch (event.action) {
            case 'view':
                this.selectedReview = review;
                this.showDetailDialog = true;
                break;
            case 'approve':
                this.approveReview(review.id);
                break;
            case 'reject':
                this.rejectReview(review.id);
                break;
        }
    }

    approveReview(id?: string) {
        const reviewId = id || this.selectedReview?.id;
        if (!reviewId) return;

        this.reviewsApiService.approveReview(reviewId).subscribe({
            next: () => {
                this.loadReviews();
                this.closeDetailDialog();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.reviews.approved')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    rejectReview(id?: string) {
        const reviewId = id || this.selectedReview?.id;
        if (!reviewId) return;

        this.reviewsApiService.rejectReview(reviewId).subscribe({
            next: () => {
                this.loadReviews();
                this.closeDetailDialog();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('admin.reviews.rejected')
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    closeDetailDialog() {
        this.showDetailDialog = false;
        this.selectedReview = null;
    }

    getItemName(review: Review): string {
        return this.languageService.language === 'ar' ? (review.itemNameAr || '') : (review.itemNameEn || '');
    }

    getReviewTitle(review: Review): string {
        return review.title || '';
    }

    getReviewComment(review: Review): string {
        return review.comment || '';
    }

    getStatusName(status: string): string {
        return status;
    }

    getStatusSeverity(status: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | null | undefined {
        const severityMap: Record<string, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            'Pending': 'warn',
            'Approved': 'success',
            'Rejected': 'danger'
        };
        return severityMap[status] || 'secondary';
    }
}
