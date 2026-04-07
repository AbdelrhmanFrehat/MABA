import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { CardModule } from 'primeng/card';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { ReportsService } from '../../../shared/services/reports.service';
import { SalesReportItem } from '../../../shared/models/report.model';

@Component({
    selector: 'app-sales-report',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        DatePickerModule,
        CardModule,
        ToastModule,
        TranslateModule,
        DataTableComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.reports.sales' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Review invoice-level sales performance, discounts, paid amounts, and outstanding balances.</p>
            </div>

            <p-card styleClass="mb-4">
                <form [formGroup]="filtersForm" class="grid formgrid" (ngSubmit)="load()">
                    <div class="field col-12 md:col-4">
                        <label class="block font-medium mb-2">From Date</label>
                        <p-datepicker formControlName="fromDate" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker>
                    </div>

                    <div class="field col-12 md:col-4">
                        <label class="block font-medium mb-2">To Date</label>
                        <p-datepicker formControlName="toDate" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker>
                    </div>

                    <div class="field col-12 md:col-4 flex align-items-end">
                        <div class="flex gap-2 w-full justify-content-end">
                            <p-button label="Reset" severity="secondary" [outlined]="true" (onClick)="resetFilters()"></p-button>
                            <p-button label="Apply" type="submit"></p-button>
                        </div>
                    </div>
                </form>
            </p-card>

            <div class="grid mb-4">
                <div class="col-12 md:col-3">
                    <div class="summary-card">
                        <span class="summary-label">Invoices</span>
                        <strong>{{ rows.length }}</strong>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="summary-card">
                        <span class="summary-label">Total Sales</span>
                        <strong>{{ totalSales | currency:'ILS' }}</strong>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="summary-card">
                        <span class="summary-label">Collected</span>
                        <strong>{{ totalPaid | currency:'ILS' }}</strong>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="summary-card">
                        <span class="summary-label">Outstanding</span>
                        <strong>{{ totalOutstanding | currency:'ILS' }}</strong>
                    </div>
                </div>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Sales Report'"
                [globalFilterFields]="['invoiceNumber', 'customerNameEn', 'customerNameAr']"
            ></app-data-table>
        </div>
    `,
    styles: [`
        .summary-card {
            border: 1px solid var(--surface-border);
            background: var(--surface-card);
            border-radius: 12px;
            padding: 1rem;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            min-height: 100%;
        }

        .summary-label {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }
    `]
})
export class SalesReportComponent implements OnInit {
    rows: SalesReportItem[] = [];
    loading = false;
    totalSales = 0;
    totalPaid = 0;
    totalOutstanding = 0;

    columns: TableColumn[] = [
        { field: 'invoiceDate', header: 'Date', type: 'date', sortable: true },
        { field: 'invoiceNumber', header: 'Order / Invoice #', sortable: true },
        { field: 'customerNameEn', header: 'Customer', sortable: true },
        { field: 'totalAmount', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'discountAmount', header: 'Discount', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'netAmount', header: 'Net', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'paidAmount', header: 'Paid', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'remainingAmount', header: 'Remaining', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    filtersForm = inject(FormBuilder).group({
        fromDate: [null as Date | null],
        toDate: [null as Date | null]
    });

    private reportsService = inject(ReportsService);
    private messageService = inject(MessageService);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        const value = this.filtersForm.getRawValue();
        this.loading = true;

        this.reportsService.getSalesReport({
            fromDate: value.fromDate ? value.fromDate.toISOString() : undefined,
            toDate: value.toDate ? value.toDate.toISOString() : undefined
        }).subscribe({
            next: rows => {
                this.rows = rows ?? [];
                this.totalSales = this.rows.reduce((sum, row) => sum + (row.totalAmount || 0), 0);
                this.totalPaid = this.rows.reduce((sum, row) => sum + (row.paidAmount || 0), 0);
                this.totalOutstanding = this.rows.reduce((sum, row) => sum + (row.remainingAmount || 0), 0);
                this.loading = false;
            },
            error: err => {
                this.rows = [];
                this.totalSales = 0;
                this.totalPaid = 0;
                this.totalOutstanding = 0;
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.message || 'Failed to load sales report.'
                });
            }
        });
    }

    resetFilters(): void {
        this.filtersForm.reset({
            fromDate: null,
            toDate: null
        });
        this.load();
    }
}
