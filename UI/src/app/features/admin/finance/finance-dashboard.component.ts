import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../shared/services/api.service';
import { OrdersApiService } from '../../../shared/services/orders-api.service';

@Component({
    selector: 'app-finance-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        CardModule,
        ChartModule,
        TableModule,
        TagModule,
        ProgressSpinnerModule,
        TranslateModule
    ],
    template: `
        <div class="finance-dashboard-container">
            <!-- Header -->
            <div class="page-header">
                <h1>{{ 'admin.finance.title' | translate }}</h1>
            </div>

            <!-- KPIs -->
            <div class="kpi-grid">
                <div class="kpi-card-wrapper">
                    <p-card>
                        <div class="kpi-content">
                            <div>
                                <div class="kpi-label">{{ 'admin.finance.totalRevenue' | translate }}</div>
                                <div class="kpi-value text-primary">{{ formatCurrency(kpis.totalRevenue) }}</div>
                            </div>
                            <i class="pi pi-dollar kpi-icon text-primary"></i>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card>
                        <div class="kpi-content">
                            <div>
                                <div class="kpi-label">{{ 'admin.finance.totalPaid' | translate }}</div>
                                <div class="kpi-value text-primary-500">{{ formatCurrency(kpis.totalPaid) }}</div>
                            </div>
                            <i class="pi pi-check-circle kpi-icon text-primary-500"></i>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card>
                        <div class="kpi-content">
                            <div>
                                <div class="kpi-label">{{ 'admin.finance.outstanding' | translate }}</div>
                                <div class="kpi-value text-orange-500">{{ formatCurrency(kpis.outstanding) }}</div>
                            </div>
                            <i class="pi pi-clock kpi-icon text-orange-500"></i>
                        </div>
                    </p-card>
                </div>
                <div class="kpi-card-wrapper">
                    <p-card>
                        <div class="kpi-content">
                            <div>
                                <div class="kpi-label">{{ 'admin.finance.refunds' | translate }}</div>
                                <div class="kpi-value text-red-500">{{ formatCurrency(kpis.refunds) }}</div>
                            </div>
                            <i class="pi pi-undo kpi-icon text-red-500"></i>
                        </div>
                    </p-card>
                </div>
            </div>

            <!-- Charts -->
            <div class="charts-grid">
                <div class="chart-wrapper">
                    <p-card [header]="'admin.finance.revenueByMonth' | translate">
                        <div class="chart-container">
                            <p-chart type="line" [data]="revenueChartData" [options]="chartOptions"></p-chart>
                        </div>
                    </p-card>
                </div>
                <div class="chart-wrapper">
                    <p-card [header]="'admin.finance.paymentMethods' | translate">
                        <div class="chart-container">
                            <p-chart type="doughnut" [data]="paymentMethodsChartData" [options]="chartOptions"></p-chart>
                        </div>
                    </p-card>
                </div>
            </div>

            <!-- Recent Payments -->
            <p-card [header]="'admin.finance.recentPayments' | translate">
                <div *ngIf="loading" class="flex justify-content-center p-6">
                    <p-progressSpinner></p-progressSpinner>
                </div>
                <p-table *ngIf="!loading" [value]="recentPayments" [tableStyle]="{'min-width': '50rem'}">
                    <ng-template #header>
                        <tr>
                            <th>{{ 'admin.finance.paymentId' | translate }}</th>
                            <th>{{ 'admin.finance.orderId' | translate }}</th>
                            <th>{{ 'admin.finance.amount' | translate }}</th>
                            <th>{{ 'admin.finance.method' | translate }}</th>
                            <th>{{ 'admin.finance.status' | translate }}</th>
                            <th>{{ 'admin.finance.date' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template #body let-payment>
                        <tr>
                            <td>{{ payment.id }}</td>
                            <td>{{ payment.orderId }}</td>
                            <td>{{ formatCurrency(payment.amount) }}</td>
                            <td>{{ payment.method }}</td>
                            <td>
                                <p-tag [value]="payment.status" [severity]="getStatusSeverity(payment.status)"></p-tag>
                            </td>
                            <td>{{ formatDate(payment.paymentDate) }}</td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>
    `,
    styles: [`
        .finance-dashboard-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .finance-dashboard-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .finance-dashboard-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
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

        @media (min-width: 1024px) {
            .kpi-grid {
                grid-template-columns: repeat(4, 1fr);
            }
        }

        .kpi-card-wrapper {
            width: 100%;
        }

        .kpi-content {
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .kpi-label {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
            margin-bottom: 0.5rem;
        }

        .kpi-value {
            font-size: 1.5rem;
            font-weight: bold;
        }

        @media (min-width: 768px) {
            .kpi-value {
                font-size: 2rem;
            }
        }

        .kpi-icon {
            font-size: 2.5rem;
        }

        @media (min-width: 768px) {
            .kpi-icon {
                font-size: 3rem;
            }
        }

        .charts-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        @media (min-width: 1024px) {
            .charts-grid {
                grid-template-columns: 2fr 1fr;
            }
        }

        .chart-wrapper {
            width: 100%;
        }

        .chart-container {
            position: relative;
            width: 100%;
            height: 300px;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .finance-dashboard-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }

            .kpi-value {
                font-size: 1.25rem;
            }

            .kpi-icon {
                font-size: 2rem;
            }

            .chart-container {
                height: 250px;
            }
        }
    `]
})
export class FinanceDashboardComponent implements OnInit {
    kpis = {
        totalRevenue: 0,
        totalPaid: 0,
        outstanding: 0,
        refunds: 0
    };
    recentPayments: any[] = [];
    loading = false;
    revenueChartData: any;
    paymentMethodsChartData: any;
    chartOptions: any;

    private apiService = inject(ApiService);
    private ordersApiService = inject(OrdersApiService);

    ngOnInit() {
        this.setupCharts();
        this.loadData();
    }

    setupCharts() {
        this.chartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top'
                }
            }
        };

        this.revenueChartData = {
            labels: [],
            datasets: [{
                label: 'Revenue',
                data: [],
                fill: false,
                borderColor: '#42A5F5',
                tension: 0.4
            }]
        };

        this.paymentMethodsChartData = {
            labels: [],
            datasets: [{
                data: [],
                backgroundColor: ['#42A5F5', '#66BB6A', '#FFA726', '#EF5350']
            }]
        };
    }

    loadData() {
        this.loading = true;
        
        // Load KPIs
        this.apiService.get('/finance/kpis').subscribe({
            next: (kpis: any) => {
                this.kpis = kpis;
            }
        });

        // Load recent payments fallback from orders (there is no /payments endpoint in API).
        this.ordersApiService.getOrders({ page: 1, pageSize: 10 }).subscribe({
            next: (response: any) => {
                const orders = response?.items || [];
                this.recentPayments = orders.map((order: any) => ({
                    id: order.id,
                    orderId: order.id,
                    amount: order.total ?? 0,
                    method: order.paymentMethodName || order.paymentMethod || '-',
                    status: order.paymentStatus || order.statusName || order.status || 'Pending',
                    paymentDate: order.createdAt || order.updatedAt || new Date().toISOString()
                }));
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });

        // Load revenue chart data
        this.apiService.get('/finance/revenue-by-month').subscribe({
            next: (data: any) => {
                this.revenueChartData = {
                    labels: data.labels || [],
                    datasets: [{
                        label: 'Revenue',
                        data: data.values || [],
                        fill: false,
                        borderColor: '#42A5F5',
                        tension: 0.4
                    }]
                };
            }
        });

        // Load payment methods distribution
        this.apiService.get('/finance/payment-methods-distribution').subscribe({
            next: (data: any) => {
                this.paymentMethodsChartData = {
                    labels: data.labels || [],
                    datasets: [{
                        data: data.values || [],
                        backgroundColor: ['#42A5F5', '#66BB6A', '#FFA726', '#EF5350']
                    }]
                };
            }
        });
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(value);
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString();
    }

    getStatusSeverity(status: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | null | undefined {
        const severityMap: Record<string, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            'Paid': 'success',
            'Pending': 'warn',
            'Failed': 'danger',
            'Refunded': 'info'
        };
        return severityMap[status] || 'secondary';
    }
}
