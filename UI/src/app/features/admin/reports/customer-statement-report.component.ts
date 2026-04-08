import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { ReportsService } from '../../../shared/services/reports.service';

interface CustomerStatementRow {
    customerName: string;
    invoices: number;
    totalAmount: number;
    paidAmount: number;
    remainingAmount: number;
}

@Component({
    selector: 'app-customer-statement-report',
    standalone: true,
    imports: [CommonModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.reports.customerStatement' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Summarize customer exposure by invoice count, collected cash, and remaining receivables.</p>
            </div>
            <app-data-table [data]="rows" [columns]="columns" [loading]="loading" [showAddButton]="false" [showDeleteSelected]="false" [title]="'Customer Statement'" [globalFilterFields]="['customerName']"></app-data-table>
        </div>
    `
})
export class CustomerStatementReportComponent implements OnInit {
    rows: CustomerStatementRow[] = [];
    loading = false;
    columns: TableColumn[] = [
        { field: 'customerName', header: 'Customer', sortable: true },
        { field: 'invoices', header: 'Invoices', sortable: true },
        { field: 'totalAmount', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'paidAmount', header: 'Paid', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'remainingAmount', header: 'Remaining', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];
    private reportsService = inject(ReportsService);
    private messageService = inject(MessageService);
    ngOnInit(): void { this.load(); }
    load(): void {
        this.loading = true;
        this.reportsService.getSalesReport().subscribe({
            next: rows => {
                const grouped = new Map<string, CustomerStatementRow>();
                (rows ?? []).forEach(row => {
                    const key = row.customerNameEn || row.customerNameAr || 'Unknown Customer';
                    const current = grouped.get(key) || { customerName: key, invoices: 0, totalAmount: 0, paidAmount: 0, remainingAmount: 0 };
                    current.invoices += 1;
                    current.totalAmount += row.totalAmount || 0;
                    current.paidAmount += row.paidAmount || 0;
                    current.remainingAmount += row.remainingAmount || 0;
                    grouped.set(key, current);
                });
                this.rows = Array.from(grouped.values());
                this.loading = false;
            },
            error: err => {
                this.rows = [];
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load customer statement.' });
            }
        });
    }
}
