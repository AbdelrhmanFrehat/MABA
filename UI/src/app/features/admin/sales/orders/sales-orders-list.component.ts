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
import { SalesOrder } from '../../../../shared/models/sales.model';

@Component({
    selector: 'app-sales-orders-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent, StatusBadgeComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.sales.orders.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.sales.orders.subtitle' | translate }}</p>
                </div>
                <p-button icon="pi pi-plus" [label]="'admin.sales.orders.new' | translate" routerLink="/admin/sales/orders/new"></p-button>
            </div>

            <app-data-table
                [data]="orders"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [title]="'admin.sales.orders.title' | translate"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    @if (column.field === 'statusName') {
                        <app-status-badge lookupTypeKey="sales_order_status" [valueId]="row.statusLookupId" [label]="row.statusName" [color]="row.statusColor"></app-status-badge>
                    }
                </ng-template>
            </app-data-table>
        </div>
    `
})
export class SalesOrdersListComponent implements OnInit {
    orders: SalesOrder[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'orderNumber', headerKey: 'admin.sales.common.number', sortable: true },
        { field: 'customerName', headerKey: 'admin.sales.common.customer', sortable: true },
        { field: 'orderDate', headerKey: 'common.date', type: 'date', sortable: true },
        { field: 'statusName', headerKey: 'common.status', type: 'custom' },
        { field: 'total', headerKey: 'common.total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-pencil', tooltipKey: 'common.edit', action: 'edit' },
        { icon: 'pi pi-check', tooltipKey: 'admin.common.submitForApproval', action: 'submit', severity: 'secondary' },
        { icon: 'pi pi-verified', tooltipKey: 'admin.common.approve', action: 'approve', severity: 'success' },
        { icon: 'pi pi-file', tooltipKey: 'admin.sales.common.convertToInvoice', action: 'invoice', severity: 'info' }
    ];

    private salesApi = inject(SalesApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    private router = inject(Router);

    ngOnInit(): void {
        this.loadOrders();
    }

    loadOrders(): void {
        this.loading = true;
        this.salesApi.getSalesOrders().subscribe({
            next: orders => {
                this.orders = orders;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.loadError') });
            }
        });
    }

    handleAction(event: { action: string; data: SalesOrder }): void {
        if (event.action === 'edit') {
            this.router.navigate(['/admin/sales/orders', event.data.id, 'edit']);
            return;
        }

        if (event.action === 'submit') {
            this.salesApi.submitSalesOrderForApproval(event.data.id).subscribe({
                next: () => this.handleSuccess(),
                error: () => this.handleError()
            });
            return;
        }

        if (event.action === 'approve') {
            this.salesApi.approveSalesOrder(event.data.id).subscribe({
                next: () => this.handleSuccess(),
                error: () => this.handleError()
            });
            return;
        }

        if (event.action === 'invoice') {
            this.salesApi.convertSalesOrderToInvoice(event.data.id, { invoiceDate: new Date().toISOString(), warehouseId: event.data.warehouseId }).subscribe({
                next: () => this.handleSuccess(),
                error: () => this.handleError()
            });
        }
    }

    private handleSuccess(): void {
        this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.saved') });
        this.loadOrders();
    }

    private handleError(): void {
        this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail: this.translateService.instant('messages.saveError') });
    }
}
