import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../../shared/components/data-table/data-table';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge';
import { SalesApiService } from '../../../../shared/services/sales-api.service';
import { SalesInvoice } from '../../../../shared/models/sales.model';

@Component({
    selector: 'app-sales-invoices-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.sales.invoices.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.sales.invoices.subtitle' | translate }}</p>
                </div>
                <p-button icon="pi pi-plus" [label]="'admin.sales.invoices.new' | translate" routerLink="/admin/sales/invoices/new"></p-button>
            </div>
            <app-data-table
                [data]="invoices"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [title]="'admin.sales.invoices.title' | translate"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    @if (column.field === 'statusName') {
                        <app-status-badge lookupTypeKey="sales_invoice_status" [valueId]="row.statusLookupId" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                    }
                    @if (column.field === 'isPostedLabel') {
                        <span [class]="row.isPosted ? 'text-green-600 font-semibold' : 'text-orange-500'">
                            {{ row.isPosted ? ('admin.accounting.posted' | translate) : ('admin.accounting.unposted' | translate) }}
                        </span>
                    }
                </ng-template>
            </app-data-table>
        </div>
    `
})
export class SalesInvoicesListComponent implements OnInit {
    invoices: SalesInvoice[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'invoiceNumber', headerKey: 'admin.sales.common.number', sortable: true },
        { field: 'customerName', headerKey: 'admin.sales.common.customer', sortable: true },
        { field: 'invoiceDate', headerKey: 'common.date', type: 'date', sortable: true },
        { field: 'statusName', headerKey: 'common.status', type: 'custom' },
        { field: 'total', headerKey: 'common.total', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'amountDue', headerKey: 'admin.payments.remainingAmount', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'isPostedLabel', headerKey: 'admin.accounting.posted', type: 'custom' }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-send', tooltipKey: 'admin.common.issue', action: 'issue', severity: 'success' },
        { icon: 'pi pi-wallet', tooltipKey: 'admin.sales.common.recordPayment', action: 'payment', severity: 'info' },
        { icon: 'pi pi-book', tooltipKey: 'admin.accounting.postToAccounting', action: 'post', severity: 'warn' }
    ];

    private salesApi = inject(SalesApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void {
        this.loadInvoices();
    }

    loadInvoices(): void {
        this.loading = true;
        this.salesApi.getSalesInvoices().subscribe({
            next: invoices => {
                this.invoices = invoices;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.loadError') });
            }
        });
    }

    handleAction(event: { action: string; data: SalesInvoice }): void {
        if (event.action === 'edit') {
            this.router.navigate(['/admin/sales/invoices', event.data.id, 'edit']);
            return;
        }

        if (event.action === 'payment') {
            this.router.navigate(['/admin/payments/new'], { queryParams: { salesInvoiceId: event.data.id } });
            return;
        }

        if (event.action === 'post') {
            if (event.data.isPosted) {
                this.messageService.add({ severity: 'info', summary: this.translateService.instant('messages.info'), detail: this.translateService.instant('admin.accounting.alreadyPosted') });
                return;
            }
            this.salesApi.postSalesInvoice(event.data.id).subscribe({
                next: (result) => {
                    this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: result?.journalEntryNumber ? `JE: ${result.journalEntryNumber}` : this.translateService.instant('admin.accounting.postSuccess') });
                    this.loadInvoices();
                },
                error: (err) => {
                    const detail = err?.error?.error ?? this.translateService.instant('messages.saveError');
                    this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail });
                }
            });
            return;
        }

        this.salesApi.issueSalesInvoice(event.data.id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.saved') });
                this.loadInvoices();
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.saveError') });
            }
        });
    }
}
