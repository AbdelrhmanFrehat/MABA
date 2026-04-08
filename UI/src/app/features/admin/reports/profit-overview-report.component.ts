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
import { ProfitReportItem } from '../../../shared/models/report.model';

@Component({
    selector: 'app-profit-overview-report',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, DatePickerModule, CardModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.reports.profitOverview' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Compare sales, purchases, expenses, and profit by period to spot margin pressure quickly.</p>
            </div>
            <p-card styleClass="mb-4">
                <form [formGroup]="filtersForm" class="grid formgrid" (ngSubmit)="load()">
                    <div class="field col-12 md:col-4"><label class="block font-medium mb-2">From Date</label><p-datepicker formControlName="fromDate" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker></div>
                    <div class="field col-12 md:col-4"><label class="block font-medium mb-2">To Date</label><p-datepicker formControlName="toDate" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker></div>
                    <div class="field col-12 md:col-4 flex align-items-end"><div class="flex gap-2 w-full justify-content-end"><p-button label="Reset" severity="secondary" [outlined]="true" (onClick)="resetFilters()"></p-button><p-button label="Apply" type="submit"></p-button></div></div>
                </form>
            </p-card>
            <div class="grid mb-4">
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Sales</span><strong>{{ totalSales | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Purchases</span><strong>{{ totalPurchases | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Expenses</span><strong>{{ totalExpenses | currency:'ILS' }}</strong></div></div>
                <div class="col-12 md:col-3"><div class="summary-card"><span class="summary-label">Profit</span><strong>{{ totalProfit | currency:'ILS' }}</strong></div></div>
            </div>
            <app-data-table [data]="rows" [columns]="columns" [loading]="loading" [showAddButton]="false" [showDeleteSelected]="false" [title]="'Profit Overview'" [globalFilterFields]="['date']"></app-data-table>
        </div>
    `,
    styles: [`.summary-card { border: 1px solid var(--surface-border); background: var(--surface-card); border-radius: 12px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; min-height: 100%; } .summary-label { font-size: 0.875rem; color: var(--text-color-secondary); }`]
})
export class ProfitOverviewReportComponent implements OnInit {
    rows: ProfitReportItem[] = [];
    loading = false;
    totalSales = 0;
    totalPurchases = 0;
    totalExpenses = 0;
    totalProfit = 0;
    columns: TableColumn[] = [
        { field: 'date', header: 'Date', type: 'date', sortable: true },
        { field: 'salesAmount', header: 'Sales', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'purchasesAmount', header: 'Purchases', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'expensesAmount', header: 'Expenses', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'profit', header: 'Profit', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];
    filtersForm = inject(FormBuilder).group({ fromDate: [null as Date | null], toDate: [null as Date | null] });
    private reportsService = inject(ReportsService);
    private messageService = inject(MessageService);
    ngOnInit(): void { this.load(); }
    load(): void {
        const value = this.filtersForm.getRawValue();
        this.loading = true;
        this.reportsService.getProfitReport({ fromDate: value.fromDate?.toISOString(), toDate: value.toDate?.toISOString() }).subscribe({
            next: rows => { this.rows = rows ?? []; this.totalSales = this.rows.reduce((sum, row) => sum + (row.salesAmount || 0), 0); this.totalPurchases = this.rows.reduce((sum, row) => sum + (row.purchasesAmount || 0), 0); this.totalExpenses = this.rows.reduce((sum, row) => sum + (row.expensesAmount || 0), 0); this.totalProfit = this.rows.reduce((sum, row) => sum + (row.profit || 0), 0); this.loading = false; },
            error: err => { this.rows = []; this.totalSales = 0; this.totalPurchases = 0; this.totalExpenses = 0; this.totalProfit = 0; this.loading = false; this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load profit overview.' }); }
        });
    }
    resetFilters(): void { this.filtersForm.reset({ fromDate: null, toDate: null }); this.load(); }
}
