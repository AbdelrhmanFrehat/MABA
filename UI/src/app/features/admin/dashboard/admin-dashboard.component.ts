import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ChartModule } from 'primeng/chart';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DashboardService, SalesOverTimeData, OrdersByStatusData } from '../../../shared/services/dashboard.service';
import { OrdersApiService } from '../../../shared/services/orders-api.service';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { ReviewsApiService } from '../../../shared/services/reviews-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Router } from '@angular/router';
import { Subject, forkJoin, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        ChartModule,
        ProgressSpinnerModule,
        SkeletonModule,
        ToastModule,
        SelectModule,
        InputTextModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="dashboard-container">
            <!-- Header -->
            <div class="dashboard-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.dashboard.title' | translate }}</h1>
                        <p>{{ 'admin.dashboard.welcome' | translate }}</p>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            icon="pi pi-refresh" 
                            [text]="true"
                            [rounded]="true"
                            [loading]="refreshing"
                            (onClick)="refreshData()"
                            [pTooltip]="'admin.dashboard.refresh' | translate"
                            tooltipPosition="bottom"
                        ></p-button>
                    </div>
                </div>
                <div class="date-filter-section">
                    <div class="date-filter-item">
                        <label>{{ 'admin.dashboard.dateRange' | translate }}</label>
                        <p-select 
                            [options]="dateRangeOptions" 
                            [(ngModel)]="selectedDateRange"
                            (onChange)="onDateRangeChange()"
                            [placeholder]="'admin.dashboard.selectDateRange' | translate"
                            optionLabel="label"
                            optionValue="value"
                            styleClass="w-full"
                        ></p-select>
                    </div>
                    <div class="date-filter-item" *ngIf="selectedDateRange === 'custom'">
                        <label>{{ 'admin.dashboard.fromDate' | translate }}</label>
                        <input 
                            type="date"
                            pInputText
                            [(ngModel)]="fromDateString"
                            (change)="onCustomDateChange()"
                            [max]="toDateString || getTodayString()"
                            class="w-full"
                        />
                    </div>
                    <div class="date-filter-item" *ngIf="selectedDateRange === 'custom'">
                        <label>{{ 'admin.dashboard.toDate' | translate }}</label>
                        <input 
                            type="date"
                            pInputText
                            [(ngModel)]="toDateString"
                            (change)="onCustomDateChange()"
                            [max]="getTodayString()"
                            [min]="fromDateString"
                            class="w-full"
                        />
                    </div>
                </div>
            </div>

            <!-- KPI Cards -->
            <div class="kpi-grid">
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-primary'" (click)="navigateToOrders()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-primary-100">
                                <i class="pi pi-shopping-cart text-primary"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.totalOrders' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-primary">{{ summary?.totalOrders || 0 }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-info'" (click)="navigateToAllRequests()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-blue-100">
                                <i class="pi pi-inbox text-blue-700"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.totalRequests' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-blue-700">{{ summary?.totalRequests || 0 }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-primary'" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-primary-100">
                                <i class="pi pi-dollar text-primary-500"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.totalRevenue' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="4rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-primary-500">{{ formatCurrency(summary?.totalRevenue || 0) }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-info'" (click)="navigateToUsers()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-blue-100">
                                <i class="pi pi-users text-blue-500"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.totalCustomers' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-blue-500">{{ summary?.totalCustomers ?? 0 }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-warning'" (click)="navigateTo3DRequests()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-orange-100">
                                <i class="pi pi-print text-orange-500"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.active3DJobs' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-orange-500">{{ active3DJobs }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-danger'" (click)="navigateToInventory()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-red-100">
                                <i class="pi pi-exclamation-triangle text-red-500"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.lowStockItems' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-red-500">{{ summary?.lowStockItemsCount || 0 }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card [styleClass]="'kpi-card kpi-card-purple'" (click)="navigateToReviews()" class="cursor-pointer">
                        <div class="kpi-content">
                            <div class="kpi-icon-wrapper bg-purple-100">
                                <i class="pi pi-star text-purple-500"></i>
                            </div>
                            <div class="kpi-text">
                                <div class="kpi-label">{{ 'admin.dashboard.pendingReviews' | translate }}</div>
                                <div *ngIf="loadingSummary" class="kpi-skeleton">
                                    <p-skeleton width="3rem" height="1.5rem" styleClass="mb-0"></p-skeleton>
                                </div>
                                <div *ngIf="!loadingSummary" class="kpi-value text-purple-500">{{ pendingReviews }}</div>
                            </div>
                        </div>
                    </p-card>
                </div>
            </div>

            <!-- Charts -->
            <div class="charts-grid">
                <div class="chart-wrapper">
                    <p-card [header]="'admin.dashboard.salesOverTime' | translate" [styleClass]="'chart-card'">
                        <div *ngIf="loadingCharts" class="flex justify-content-center align-items-center chart-container">
                            <p-progressSpinner></p-progressSpinner>
                        </div>
                        <div *ngIf="!loadingCharts && salesChartData?.labels?.length === 0" class="flex justify-content-center align-items-center text-500 chart-container">
                            <div class="text-center">
                                <i class="pi pi-chart-line text-4xl mb-2 block"></i>
                                <p>{{ 'common.noDataFound' | translate }}</p>
                            </div>
                        </div>
                        <div *ngIf="!loadingCharts && salesChartData?.labels?.length > 0" class="chart-container">
                            <p-chart 
                                type="line" 
                                [data]="salesChartData" 
                                [options]="chartOptions" 
                                class="chart-responsive"
                            ></p-chart>
                        </div>
                    </p-card>
                </div>
                <div class="chart-wrapper">
                    <p-card [header]="'admin.dashboard.ordersByStatus' | translate" [styleClass]="'chart-card'">
                        <div *ngIf="loadingCharts" class="flex justify-content-center align-items-center chart-container">
                            <p-progressSpinner></p-progressSpinner>
                        </div>
                        <div *ngIf="!loadingCharts && ordersStatusChartData?.labels?.length === 0" class="flex justify-content-center align-items-center text-500 chart-container">
                            <div class="text-center">
                                <i class="pi pi-chart-pie text-4xl mb-2 block"></i>
                                <p>{{ 'common.noDataFound' | translate }}</p>
                            </div>
                        </div>
                        <div *ngIf="!loadingCharts && ordersStatusChartData?.labels?.length > 0" class="chart-container">
                            <p-chart 
                                type="doughnut" 
                                [data]="ordersStatusChartData" 
                                [options]="doughnutChartOptions" 
                                class="chart-responsive"
                            ></p-chart>
                        </div>
                    </p-card>
                </div>
            </div>

            <!-- Quick Actions -->
            <div class="quick-actions-section">
                <p-card [header]="'admin.dashboard.quickActions' | translate" [styleClass]="'actions-card'">
                    <div class="actions-grid">
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'menu.heroTicker' | translate" 
                                icon="pi pi-images" 
                                [outlined]="true"
                                [routerLink]="['/admin/hero-ticker']"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.manageProjects' | translate" 
                                icon="pi pi-briefcase" 
                                [outlined]="true"
                                [routerLink]="['/admin/projects']"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.manageCatalog' | translate" 
                                icon="pi pi-sitemap" 
                                [outlined]="true"
                                [routerLink]="['/admin/categories']"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.createItem' | translate" 
                                icon="pi pi-plus" 
                                [outlined]="true"
                                [routerLink]="['/admin/items']"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.viewLowStock' | translate" 
                                icon="pi pi-exclamation-triangle" 
                                [outlined]="true"
                                [routerLink]="['/admin/inventory']"
                                [queryParams]="{lowStock: true}"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.viewPendingOrders' | translate" 
                                icon="pi pi-shopping-cart" 
                                [outlined]="true"
                                [routerLink]="['/admin/orders']"
                                [queryParams]="{status: 'pending'}"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                        <div class="action-button-wrapper">
                            <p-button 
                                [label]="'admin.dashboard.viewPendingReviews' | translate" 
                                icon="pi pi-star" 
                                [outlined]="true"
                                [routerLink]="['/admin/reviews']"
                                [queryParams]="{status: 'pending'}"
                                styleClass="w-full"
                            ></p-button>
                        </div>
                    </div>
                </p-card>
            </div>

            <!-- Recent Activities -->
            <div class="activities-grid">
                <div class="activity-wrapper">
                    <p-card [header]="'admin.dashboard.recentOrders' | translate" [styleClass]="'activity-card'">
                        <div *ngIf="loadingOrders" class="flex justify-content-center p-4">
                            <p-progressSpinner></p-progressSpinner>
                        </div>
                        <div *ngIf="!loadingOrders && recentOrders.length === 0" class="text-center p-4 text-500">
                            <i class="pi pi-inbox text-3xl md:text-4xl mb-3 block"></i>
                            <p class="text-sm md:text-base">{{ 'common.noDataFound' | translate }}</p>
                        </div>
                        <div *ngIf="!loadingOrders && recentOrders.length > 0" class="list-none p-0 m-0">
                            <div 
                                *ngFor="let order of recentOrders" 
                                class="flex align-items-center justify-content-between p-2 md:p-3 border-bottom-1 surface-border hover:bg-surface-50 transition-colors cursor-pointer"
                                (click)="navigateToOrderDetail(order.id)"
                            >
                                <div class="flex-1 min-w-0 pr-2">
                                    <div class="font-medium text-sm md:text-base truncate">{{ order.orderNumber }}</div>
                                    <div class="text-500 text-xs md:text-sm truncate">{{ order.customerName || order.userFullName || 'N/A' }}</div>
                                    <div class="text-500 text-xs">{{ formatDate(order.createdAt) }}</div>
                                </div>
                                <div class="text-right flex-shrink-0">
                                    <div class="font-bold mb-1 text-xs md:text-sm lg:text-base">{{ formatCurrency(order.total || order.totalAmount || 0) }}</div>
                                    <span *ngIf="order.status" [class]="'order-status-badge status-' + order.status.key" class="block text-xs mt-1">{{ getStatusName(order.status) }}</span>
                                    <span *ngIf="order.orderStatusKey && !order.status" [class]="'order-status-badge status-' + order.orderStatusKey" class="block text-xs mt-1">{{ order.orderStatusKey }}</span>
                                </div>
                            </div>
                        </div>
                        <div *ngIf="!loadingOrders && recentOrders.length > 0" class="mt-2 md:mt-3 pt-2 md:pt-3 border-top-1">
                            <p-button 
                                [label]="'admin.dashboard.viewAllOrders' | translate" 
                                [text]="true"
                                icon="pi pi-arrow-right"
                                iconPos="right"
                                [routerLink]="['/admin/orders']"
                                styleClass="w-full"
                                [style]="{'font-size': '0.875rem'}"
                            ></p-button>
                        </div>
                    </p-card>
                </div>
                <div class="activity-wrapper">
                    <p-card [header]="'admin.dashboard.recent3DRequests' | translate" [styleClass]="'activity-card'">
                        <div *ngIf="loading3DRequests" class="flex justify-content-center p-4">
                            <p-progressSpinner></p-progressSpinner>
                        </div>
                        <div *ngIf="!loading3DRequests && recent3DRequests.length === 0" class="text-center p-4 text-500">
                            <i class="pi pi-inbox text-3xl md:text-4xl mb-3 block"></i>
                            <p class="text-sm md:text-base">{{ 'common.noDataFound' | translate }}</p>
                        </div>
                        <div *ngIf="!loading3DRequests && recent3DRequests.length > 0" class="list-none p-0 m-0">
                            <div 
                                *ngFor="let request of recent3DRequests" 
                                class="flex align-items-center justify-content-between p-2 md:p-3 border-bottom-1 surface-border hover:bg-surface-50 transition-colors cursor-pointer"
                                (click)="navigateTo3DRequestDetail(request.id)"
                            >
                                <div class="flex-1 min-w-0 pr-2">
                                    <div class="font-medium text-sm md:text-base truncate">{{ request.id?.substring(0, 8) || 'N/A' }}</div>
                                    <div class="text-500 text-xs md:text-sm truncate">{{ request.userName || request.userEmail || 'N/A' }}</div>
                                    <div class="text-500 text-xs">{{ formatDate(request.createdAt) }}</div>
                                </div>
                                <div class="text-right flex-shrink-0">
                                    <div class="font-bold mb-1 text-xs md:text-sm lg:text-base">{{ formatCurrency(request.estimatedPrice || 0) }}</div>
                                    <span [class]="'request-status-badge status-' + (request.status || 'unknown')" class="block text-xs mt-1">{{ request.status || 'Unknown' }}</span>
                                </div>
                            </div>
                        </div>
                        <div *ngIf="!loading3DRequests && recent3DRequests.length > 0" class="mt-2 md:mt-3 pt-2 md:pt-3 border-top-1">
                            <p-button 
                                [label]="'admin.dashboard.viewAll3DRequests' | translate" 
                                [text]="true"
                                icon="pi pi-arrow-right"
                                iconPos="right"
                                [routerLink]="['/admin/3d-requests']"
                                styleClass="w-full"
                                [style]="{'font-size': '0.875rem'}"
                            ></p-button>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .dashboard-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .dashboard-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .dashboard-container {
                padding: 1.5rem;
            }
        }

        .dashboard-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 1rem;
        }

        .dashboard-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0 0 0.5rem 0;
        }

        .dashboard-header p {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
            margin: 0;
        }

        .header-actions {
            display: flex;
            gap: 0.5rem;
        }

        .date-filter-section {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
            padding: 1rem;
            background: var(--surface-50);
            border-radius: 8px;
            margin-top: 1rem;
        }

        @media (min-width: 768px) {
            .date-filter-section {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        .date-filter-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .date-filter-item label {
            font-size: 0.875rem;
            font-weight: 500;
            color: var(--text-color);
        }

        @media (min-width: 768px) {
            .dashboard-header h1 {
                font-size: 2rem;
            }
            .dashboard-header p {
                font-size: 1rem;
            }
        }

        /* KPI Grid */
        .kpi-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        @media (min-width: 576px) {
            .kpi-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (min-width: 768px) {
            .kpi-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        @media (min-width: 1024px) {
            .kpi-grid {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        @media (min-width: 1400px) {
            .kpi-grid {
                grid-template-columns: repeat(6, 1fr);
            }
        }

        .kpi-card-wrapper {
            width: 100%;
        }

        .kpi-card {
            transition: all 0.3s ease;
            height: 100%;
            min-height: 100px;
            cursor: pointer;
        }

        .kpi-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        :host ::ng-deep .p-card.kpi-card-primary {
            border-inline-start: 3px solid var(--p-primary-500);
        }

        .kpi-content {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .kpi-icon-wrapper {
            width: 2.5rem;
            height: 2.5rem;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .kpi-icon-wrapper i {
            font-size: 1.25rem;
        }

        @media (min-width: 768px) {
            .kpi-icon-wrapper {
                width: 3rem;
                height: 3rem;
            }
            .kpi-icon-wrapper i {
                font-size: 1.5rem;
            }
        }

        .kpi-text {
            flex: 1;
            min-width: 0;
        }

        .kpi-label {
            color: var(--text-color-secondary);
            font-size: 0.75rem;
            font-weight: 500;
            margin-bottom: 0.25rem;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        @media (min-width: 768px) {
            .kpi-label {
                font-size: 0.875rem;
            }
        }

        .kpi-value {
            font-size: 1.25rem;
            font-weight: bold;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        @media (min-width: 768px) {
            .kpi-value {
                font-size: 1.5rem;
            }
        }

        @media (min-width: 1024px) {
            .kpi-value {
                font-size: 1.75rem;
            }
        }

        @media (min-width: 1400px) {
            .kpi-value {
                font-size: 2rem;
            }
        }

        .kpi-skeleton {
            display: flex;
            align-items: center;
        }

        /* Charts Grid */
        .charts-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        @media (min-width: 1024px) {
            .charts-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .chart-wrapper {
            width: 100%;
        }

        .chart-card {
            height: 100%;
            min-height: 350px;
        }

        .chart-container {
            position: relative;
            width: 100%;
            height: 250px;
            min-height: 250px;
        }

        @media (min-width: 768px) {
            .chart-container {
                height: 300px;
                min-height: 300px;
            }
        }

        @media (min-width: 1024px) {
            .chart-container {
                height: 350px;
                min-height: 350px;
            }
        }

        .chart-responsive {
            width: 100% !important;
            height: 100% !important;
            max-height: 100% !important;
        }

        /* Quick Actions */
        .quick-actions-section {
            width: 100%;
            margin-bottom: 1.5rem;
        }

        .actions-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 0.75rem;
        }

        @media (min-width: 576px) {
            .actions-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (min-width: 768px) {
            .actions-grid {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        .action-button-wrapper {
            width: 100%;
        }

        /* Activities Grid */
        .activities-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 1024px) {
            .activities-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        .activity-wrapper {
            width: 100%;
        }

        .activity-card {
            height: 100%;
            min-height: 300px;
        }

        /* Status Badges */
        .order-status-badge, .request-status-badge {
            display: inline-block;
            padding: 0.2rem 0.4rem;
            border-radius: 4px;
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: capitalize;
            white-space: nowrap;
        }

        @media (min-width: 768px) {
            .order-status-badge, .request-status-badge {
                padding: 0.25rem 0.5rem;
                font-size: 0.75rem;
            }
        }

        .order-status-badge.status-Pending,
        .request-status-badge.status-Pending {
            background-color: var(--yellow-100);
            color: var(--yellow-800);
        }

        .order-status-badge.status-Processing,
        .request-status-badge.status-Processing {
            background-color: var(--blue-100);
            color: var(--blue-800);
        }

        .order-status-badge.status-Completed,
        .request-status-badge.status-Completed {
            background-color: var(--primary-100);
            color: var(--primary-800);
        }

        .order-status-badge.status-Cancelled,
        .request-status-badge.status-Cancelled {
            background-color: var(--red-100);
            color: var(--red-800);
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .dashboard-container {
                padding: 0.5rem;
            }

            .kpi-card {
                min-height: 90px;
            }

            .kpi-icon-wrapper {
                width: 2rem;
                height: 2rem;
            }

            .kpi-icon-wrapper i {
                font-size: 1rem;
            }

            .kpi-value {
                font-size: 1.125rem;
            }

            .chart-container {
                height: 200px;
                min-height: 200px;
            }

            .activity-card {
                min-height: 250px;
            }
        }
    `]
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
    summary: any = null;
    loadingSummary = false;
    loadingOrders = false;
    loading3DRequests = false;
    loadingCharts = false;
    refreshing = false;
    active3DJobs = 0;
    pendingReviews = 0;
    totalItems = 0;
    recentOrders: any[] = [];
    recent3DRequests: any[] = [];
    
    salesChartData: any;
    ordersStatusChartData: any;
    chartOptions: any;
    doughnutChartOptions: any;

    // Date range filters
    selectedDateRange: string = 'last30days';
    fromDate: Date | null = null;
    toDate: Date | null = null;
    fromDateString: string = '';
    toDateString: string = '';
    dateRangeOptions: any[] = [];

    private destroy$ = new Subject<void>();
    private autoRefreshInterval: any;

    private dashboardService = inject(DashboardService);
    private ordersApiService = inject(OrdersApiService);
    private itemsApiService = inject(ItemsApiService);
    private printingApiService = inject(PrintingApiService);
    private reviewsApiService = inject(ReviewsApiService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);
    private router = inject(Router);
    private messageService = inject(MessageService);

    ngOnInit() {
        this.initCharts();
        this.initDateRangeOptions();
        this.setDefaultDateRange();
        this.loadDashboardData();
        this.startAutoRefresh();
    }

    initDateRangeOptions() {
        this.dateRangeOptions = [
            { label: this.translateService.instant('admin.dashboard.today'), value: 'today' },
            { label: this.translateService.instant('admin.dashboard.last7days'), value: 'last7days' },
            { label: this.translateService.instant('admin.dashboard.last30days'), value: 'last30days' },
            { label: this.translateService.instant('admin.dashboard.last90days'), value: 'last90days' },
            { label: this.translateService.instant('admin.dashboard.thisMonth'), value: 'thisMonth' },
            { label: this.translateService.instant('admin.dashboard.lastMonth'), value: 'lastMonth' },
            { label: this.translateService.instant('admin.dashboard.thisYear'), value: 'thisYear' },
            { label: this.translateService.instant('admin.dashboard.custom'), value: 'custom' }
        ];
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
        }
    }

    setDefaultDateRange() {
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        this.toDate = today;
        this.toDateString = this.formatDateForInput(today);
        
        const last30Days = new Date();
        last30Days.setDate(last30Days.getDate() - 30);
        last30Days.setHours(0, 0, 0, 0);
        this.fromDate = last30Days;
        this.fromDateString = this.formatDateForInput(last30Days);
    }

    formatDateForInput(date: Date): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    /** HTML date inputs yield yyyy-MM-dd; parse as local calendar day (avoid UTC-midnight shift from `new Date(string)`). */
    private parseLocalDateFromInput(dateStr: string, boundary: 'start' | 'end'): Date {
        const [y, m, d] = dateStr.split('-').map(Number);
        if (boundary === 'start') {
            return new Date(y, m - 1, d, 0, 0, 0, 0);
        }
        return new Date(y, m - 1, d, 23, 59, 59, 999);
    }

    getTodayString(): string {
        return this.formatDateForInput(new Date());
    }

    onDateRangeChange() {
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        this.toDate = today;
        this.toDateString = this.formatDateForInput(today);

        switch (this.selectedDateRange) {
            case 'today':
                this.fromDate = new Date(today);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'last7days':
                this.fromDate = new Date(today);
                this.fromDate.setDate(this.fromDate.getDate() - 7);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'last30days':
                this.fromDate = new Date(today);
                this.fromDate.setDate(this.fromDate.getDate() - 30);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'last90days':
                this.fromDate = new Date(today);
                this.fromDate.setDate(this.fromDate.getDate() - 90);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'thisMonth':
                this.fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'lastMonth':
                const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
                this.fromDate = lastMonth;
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                const lastDayOfLastMonth = new Date(today.getFullYear(), today.getMonth(), 0);
                this.toDate = new Date(lastDayOfLastMonth);
                this.toDate.setHours(23, 59, 59, 999);
                this.toDateString = this.formatDateForInput(this.toDate);
                break;
            case 'thisYear':
                this.fromDate = new Date(today.getFullYear(), 0, 1);
                this.fromDate.setHours(0, 0, 0, 0);
                this.fromDateString = this.formatDateForInput(this.fromDate);
                break;
            case 'custom':
                // Keep current dates
                break;
        }

        if (this.selectedDateRange !== 'custom') {
            this.loadDashboardData();
        }
    }

    onCustomDateChange() {
        if (this.fromDateString) {
            this.fromDate = this.parseLocalDateFromInput(this.fromDateString, 'start');
        }
        if (this.toDateString) {
            this.toDate = this.parseLocalDateFromInput(this.toDateString, 'end');
        }
        if (this.fromDate && this.toDate && this.fromDate <= this.toDate) {
            this.loadDashboardData();
        }
    }

    refreshData() {
        this.refreshing = true;
        this.loadDashboardData();
        setTimeout(() => {
            this.refreshing = false;
        }, 1000);
    }

    startAutoRefresh() {
        // Auto refresh every 5 minutes
        this.autoRefreshInterval = setInterval(() => {
            this.loadDashboardData();
        }, 5 * 60 * 1000);
    }

    loadDashboardData() {
        this.loadingSummary = true;
        this.loadingOrders = true;
        this.loading3DRequests = true;
        this.loadingCharts = true;
        
        // Prepare date params
        const dateParams = this.getDateParams();
        
        // Load dashboard summary with date range
        this.dashboardService.getSummary(dateParams)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    this.summary = data;
                    this.active3DJobs = data.active3DJobs || 0;
                    this.pendingReviews = data.pendingReviews || 0;
                    this.loadingSummary = false;
                },
                error: (error) => {
                    this.loadingSummary = false;
                    this.messageService.add({
                        severity: 'warn',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.translateService.instant('messages.errorLoadingDashboard'),
                        life: 3000
                    });
                }
            });

        // Load recent orders
        this.ordersApiService.getOrders({ page: 1, pageSize: 5 })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.recentOrders = response.items || [];
                    this.loadingOrders = false;
                },
                error: () => {
                    this.loadingOrders = false;
                    // Silently handle error - recent orders are not critical
                }
            });

        // Load recent 3D requests
        this.printingApiService.getRequests({ page: 1, pageSize: 5 })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.recent3DRequests = response.items || [];
                    this.loading3DRequests = false;
                },
                error: () => {
                    this.loading3DRequests = false;
                    // Silently handle error - recent 3D requests are not critical
                }
            });

        // Load total items count
        this.itemsApiService.getAllItems()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (items) => {
                    this.totalItems = items.length;
                },
                error: () => {
                    // Silently handle error
                }
            });

        // Load charts data in parallel with date range
        forkJoin({
            sales: this.dashboardService.getSalesOverTime({ 
                ...dateParams, 
                periods: this.getPeriodsForDateRange() 
            }),
            ordersByStatus: this.dashboardService.getOrdersByStatus(dateParams)
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: ({ sales, ordersByStatus }) => {
                    this.updateSalesChart(sales);
                    this.updateOrdersStatusChart(ordersByStatus);
                    this.loadingCharts = false;
                },
                error: () => {
                    this.loadingCharts = false;
                    // Silently handle error - chart data is not critical
                }
            });
    }

    getDateParams(): { fromDate?: string; toDate?: string } {
        if (!this.fromDate || !this.toDate) {
            return {};
        }
        return {
            fromDate: this.fromDate.toISOString(),
            toDate: this.toDate.toISOString()
        };
    }

    getPeriodsForDateRange(): number {
        if (!this.fromDate || !this.toDate) {
            return 6;
        }
        const daysDiff = Math.ceil((this.toDate.getTime() - this.fromDate.getTime()) / (1000 * 60 * 60 * 24));
        if (daysDiff <= 7) return 7;
        if (daysDiff <= 30) return 30;
        if (daysDiff <= 90) return 12;
        return 12;
    }

    initCharts() {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = documentStyle.getPropertyValue('--text-color');
        const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
        const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

        // Initialize empty charts - will be populated with real data
        this.salesChartData = {
            labels: [],
            datasets: [
                {
                    label: 'Sales',
                    data: [],
                    fill: false,
                    borderColor: documentStyle.getPropertyValue('--primary-color'),
                    tension: 0.4
                }
            ]
        };

        this.ordersStatusChartData = {
            labels: [],
            datasets: [
                {
                    data: [],
                    backgroundColor: []
                }
            ]
        };

        this.chartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        usePointStyle: true
                    },
                    position: 'top'
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    ticks: {
                        color: textColorSecondary,
                        maxRotation: 45,
                        minRotation: 0
                    },
                    grid: {
                        color: surfaceBorder
                    }
                },
                y: {
                    ticks: {
                        color: textColorSecondary,
                        callback: function(value: any) {
                            return '₪' + value.toLocaleString();
                        }
                    },
                    grid: {
                        color: surfaceBorder
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        };

        this.doughnutChartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        usePointStyle: true
                    },
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context: any) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        };
    }

    updateSalesChart(data: SalesOverTimeData[]) {
        const documentStyle = getComputedStyle(document.documentElement);
        this.salesChartData = {
            labels: data.map(d => d.period),
            datasets: [
                {
                    label: 'Sales',
                    data: data.map(d => d.sales),
                    fill: false,
                    borderColor: documentStyle.getPropertyValue('--primary-color'),
                    tension: 0.4
                }
            ]
        };
    }

    updateOrdersStatusChart(data: OrdersByStatusData[]) {
        const documentStyle = getComputedStyle(document.documentElement);
        const colors = [
            documentStyle.getPropertyValue('--p-orange-500').trim() || '#FF6B35',
            documentStyle.getPropertyValue('--p-primary-500').trim() || '#667eea',
            documentStyle.getPropertyValue('--p-primary-600').trim() || '#5a67d8',
            documentStyle.getPropertyValue('--p-primary-400').trim() || '#818CF8',
            documentStyle.getPropertyValue('--p-red-500').trim() || '#F44336',
            documentStyle.getPropertyValue('--p-primary-700').trim() || '#4c51bf',
            documentStyle.getPropertyValue('--p-cyan-500').trim() || '#00BCD4'
        ];

        this.ordersStatusChartData = {
            labels: data.map(d => d.status),
            datasets: [
                {
                    data: data.map(d => d.count),
                    backgroundColor: data.map((_, index) => colors[index % colors.length])
                }
            ]
        };
    }

    formatCurrency(value: number, currency = 'ILS'): string {
        return new Intl.NumberFormat('he-IL', {
            style: 'currency',
            currency: currency
        }).format(value);
    }

    formatDate(date: string | Date): string {
        if (!date) return '';
        const dateObj = typeof date === 'string' ? new Date(date) : date;
        if (isNaN(dateObj.getTime())) return '';
        return dateObj.toLocaleDateString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    getStatusName(status: any): string {
        const lang = this.languageService.language;
        return lang === 'ar' ? status.nameAr : status.nameEn;
    }

    navigateToOrders() {
        this.router.navigate(['/admin/orders']);
    }

    navigateToAllRequests() {
        this.router.navigate(['/admin/requests']);
    }

    navigateToUsers() {
        this.router.navigate(['/admin/users']);
    }

    navigateTo3DRequests() {
        this.router.navigate(['/admin/3d-requests']);
    }

    navigateToInventory() {
        this.router.navigate(['/admin/inventory'], { queryParams: { lowStock: true } });
    }

    navigateToReviews() {
        this.router.navigate(['/admin/reviews'], { queryParams: { status: 'pending' } });
    }

    navigateToOrderDetail(orderId: string) {
        if (orderId) {
            this.router.navigate(['/admin/orders', orderId]);
        }
    }

    navigateTo3DRequestDetail(requestId: string) {
        if (requestId) {
            this.router.navigate(['/admin/3d-requests', requestId]);
        }
    }
}
