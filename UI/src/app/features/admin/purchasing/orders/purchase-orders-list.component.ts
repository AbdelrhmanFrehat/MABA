import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataTableComponent, TableAction, TableColumn } from '../../../../shared/components/data-table/data-table';
import { FormDialogComponent } from '../../../../shared/components/form-dialog/form-dialog';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge';
import { DocumentLinesTableComponent, DocLine } from '../../../../shared/components/document-lines-table/document-lines-table';
import { PurchasingApiService } from '../../../../shared/services/purchasing-api.service';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { Supplier } from '../../../../shared/models/crm.model';
import { CreatePurchaseOrderRequest, PurchaseOrder, PurchaseOrderLine } from '../../../../shared/models/purchasing.model';

type PurchaseOrderStatusKey =
    | 'draft'
    | 'pending_approval'
    | 'approved'
    | 'partially_received'
    | 'fully_received'
    | 'cancelled';

interface PurchaseOrderStatusOption {
    key: PurchaseOrderStatusKey;
    name: string;
    color: string;
}

interface PurchaseOrderTotals {
    subTotal: number;
    discountAmount: number;
    discountPercent?: number;
    taxAmount: number;
    total: number;
}

@Component({
    selector: 'app-purchase-orders-list',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        SelectModule,
        TableModule,
        ToastModule,
        TranslateModule,
        DataTableComponent,
        FormDialogComponent,
        StatusBadgeComponent,
        DocumentLinesTableComponent
    ],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4 purchase-orders-page">
            <div class="flex justify-between items-start gap-3 mb-4 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">Purchase Orders</h1>
                    <p class="text-600 mt-2 mb-0">Track supplier orders, approvals, delivery timing, and receiving readiness.</p>
                </div>
                <p-button
                    icon="pi pi-plus"
                    label="New Purchase Order"
                    (onClick)="openCreateDialog()"
                ></p-button>
            </div>

            <app-data-table
                [data]="orders"
                [columns]="columns"
                [actions]="actions"
                [loading]="loading"
                [showAddButton]="false"
                [title]="'Purchase Orders'"
                [globalFilterFields]="['orderNumber', 'supplierName', 'statusName', 'sourceQuotationNumber']"
                (onAction)="handleAction($event)"
            >
                <ng-template #customColumn let-row let-column="column">
                    <ng-container [ngSwitch]="column.field">
                        <app-status-badge
                            *ngSwitchCase="'statusName'"
                            [label]="row.statusName"
                            [color]="row.statusColor"
                        ></app-status-badge>
                        <span *ngSwitchCase="'supplierName'" class="font-medium">
                            {{ row.supplierName || '-' }}
                        </span>
                        <span *ngSwitchCase="'expectedDeliveryDate'">
                            {{ row.expectedDeliveryDate ? (row.expectedDeliveryDate | date:'mediumDate') : '-' }}
                        </span>
                    </ng-container>
                </ng-template>
            </app-data-table>
        </div>

        <app-form-dialog
            [(visible)]="showFormDialog"
            [header]="editingOrder ? 'Edit Purchase Order' : 'New Purchase Order'"
            [dialogWidth]="'1050px'"
            [saving]="saving"
            [saveDisabled]="orderForm.invalid || orderLines.length === 0"
            cancelLabel="Cancel"
            saveLabel="Save Purchase Order"
            (onSave)="saveOrder()"
            (onCancel)="closeFormDialog()"
            (onHide)="closeFormDialog()"
        >
            <form [formGroup]="orderForm" class="grid formgrid">
                <div class="field col-12 md:col-6">
                    <label class="block font-medium mb-2">Supplier</label>
                    <p-select
                        formControlName="supplierId"
                        [options]="suppliers"
                        optionLabel="nameEn"
                        optionValue="id"
                        placeholder="Select supplier"
                        [filter]="true"
                        filterBy="nameEn,nameAr,code"
                        styleClass="w-full"
                    ></p-select>
                    <small class="p-error" *ngIf="isInvalid('supplierId')">Supplier is required.</small>
                </div>

                <div class="field col-12 md:col-3">
                    <label class="block font-medium mb-2">Order Date</label>
                    <input pInputText type="date" class="w-full" formControlName="orderDate" />
                    <small class="p-error" *ngIf="isInvalid('orderDate')">Order date is required.</small>
                </div>

                <div class="field col-12 md:col-3">
                    <label class="block font-medium mb-2">Expected Delivery</label>
                    <input pInputText type="date" class="w-full" formControlName="expectedDeliveryDate" />
                </div>

                <div class="field col-12 md:col-4">
                    <label class="block font-medium mb-2">Source Quotation</label>
                    <input pInputText class="w-full" formControlName="sourceQuotationNumber" placeholder="Optional reference" />
                </div>

                <div class="field col-12 md:col-4">
                    <label class="block font-medium mb-2">Status</label>
                    <p-select
                        formControlName="statusKey"
                        [options]="statusOptions"
                        optionLabel="name"
                        optionValue="key"
                        styleClass="w-full"
                    ></p-select>
                </div>

                <div class="field col-12 md:col-4">
                    <label class="block font-medium mb-2">Order Number</label>
                    <input pInputText class="w-full" [value]="editingOrder?.orderNumber || nextOrderNumberPreview" readonly />
                </div>

                <div class="field col-12">
                    <label class="block font-medium mb-2">Notes</label>
                    <textarea pTextarea rows="3" class="w-full" formControlName="notes" placeholder="Internal notes, receiving instructions, or supplier context"></textarea>
                </div>
            </form>

            <div class="mt-4">
                <app-document-lines-table
                    [lines]="orderLines"
                    [showTax]="true"
                    [showDocumentDiscount]="true"
                    [defaultTaxPercent]="17"
                    (linesChange)="onLinesChanged($event)"
                    (totalsChange)="onTotalsChanged($event)"
                ></app-document-lines-table>
            </div>
        </app-form-dialog>

        <app-form-dialog
            [(visible)]="showViewDialog"
            [header]="selectedOrder?.orderNumber ? ('Purchase Order ' + selectedOrder?.orderNumber) : 'Purchase Order Details'"
            [dialogWidth]="'980px'"
            [showSaveButton]="false"
            cancelLabel="Close"
            (onCancel)="closeViewDialog()"
            (onHide)="closeViewDialog()"
        >
            <div *ngIf="selectedOrder" class="grid">
                <div class="col-12 md:col-3">
                    <div class="detail-card">
                        <span class="detail-label">Supplier</span>
                        <strong>{{ selectedOrder.supplierName || '-' }}</strong>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="detail-card">
                        <span class="detail-label">Status</span>
                        <app-status-badge [label]="selectedOrder.statusName" [color]="selectedOrder.statusColor"></app-status-badge>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="detail-card">
                        <span class="detail-label">Order Date</span>
                        <strong>{{ selectedOrder.orderDate | date:'mediumDate' }}</strong>
                    </div>
                </div>
                <div class="col-12 md:col-3">
                    <div class="detail-card">
                        <span class="detail-label">Total</span>
                        <strong>{{ selectedOrder.total | currency:selectedOrder.currency }}</strong>
                    </div>
                </div>
            </div>

            <div class="surface-card border-1 surface-border border-round p-3 mt-3" *ngIf="selectedOrder?.notes">
                <div class="font-semibold mb-2">Notes</div>
                <div class="text-700 white-space-pre-line">{{ selectedOrder?.notes }}</div>
            </div>

            <div class="mt-4">
                <p-table [value]="selectedOrder?.lines || []" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '52rem' }">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>#</th>
                            <th>Item</th>
                            <th>Qty</th>
                            <th>Unit Price</th>
                            <th>Discount</th>
                            <th>Tax</th>
                            <th>Total</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-line let-ri="rowIndex">
                        <tr>
                            <td>{{ ri + 1 }}</td>
                            <td>{{ line.itemName || line.itemId }}</td>
                            <td>{{ line.quantity }}</td>
                            <td>{{ line.unitPrice | number:'1.2-2' }}</td>
                            <td>{{ line.discountAmount | number:'1.2-2' }}</td>
                            <td>{{ line.taxAmount | number:'1.2-2' }}</td>
                            <td class="font-semibold">{{ line.lineTotal | number:'1.2-2' }}</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </app-form-dialog>
    `,
    styles: [`
        .purchase-orders-page :host ::ng-deep .p-toolbar {
            border-radius: 1rem;
        }

        .field {
            margin-bottom: 1rem;
        }

        .detail-card {
            border: 1px solid var(--surface-border);
            background: var(--surface-card);
            border-radius: 12px;
            padding: 1rem;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            min-height: 100%;
        }

        .detail-label {
            font-size: 0.875rem;
            color: var(--text-color-secondary);
        }
    `]
})
export class PurchaseOrdersListComponent implements OnInit {
    orders: PurchaseOrder[] = [];
    suppliers: Supplier[] = [];
    loading = false;
    saving = false;
    showFormDialog = false;
    showViewDialog = false;
    selectedOrder: PurchaseOrder | null = null;
    editingOrder: PurchaseOrder | null = null;
    orderLines: DocLine[] = [];
    nextOrderNumberPreview = this.generateOrderNumber();
    private localOrders: PurchaseOrder[] = [];
    private latestTotals: PurchaseOrderTotals = {
        subTotal: 0,
        discountAmount: 0,
        discountPercent: undefined as number | undefined,
        taxAmount: 0,
        total: 0
    };

    readonly statusOptions: PurchaseOrderStatusOption[] = [
        { key: 'draft', name: 'Draft', color: '#64748b' },
        { key: 'pending_approval', name: 'Pending Approval', color: '#f59e0b' },
        { key: 'approved', name: 'Approved', color: '#22c55e' },
        { key: 'partially_received', name: 'Partially Received', color: '#0ea5e9' },
        { key: 'fully_received', name: 'Fully Received', color: '#10b981' },
        { key: 'cancelled', name: 'Cancelled', color: '#ef4444' }
    ];

    columns: TableColumn[] = [
        { field: 'orderNumber', header: 'Number', sortable: true },
        { field: 'supplierName', header: 'Supplier', sortable: true, type: 'custom' },
        { field: 'orderDate', header: 'Date', type: 'date', sortable: true },
        { field: 'expectedDeliveryDate', header: 'Expected Delivery', type: 'custom' },
        { field: 'statusName', header: 'Status', type: 'custom' },
        { field: 'total', header: 'Total', type: 'currency', currencyCode: 'ILS', sortable: true }
    ];

    actions: TableAction[] = [
        { icon: 'pi pi-eye', tooltip: 'View', action: 'view', severity: 'secondary' },
        { icon: 'pi pi-pencil', tooltip: 'Edit', action: 'edit' },
        { icon: 'pi pi-send', tooltip: 'Submit for Approval', action: 'submit', severity: 'info', visible: row => this.getStatusKey(row) === 'draft' },
        { icon: 'pi pi-check', tooltip: 'Approve', action: 'approve', severity: 'success', visible: row => this.getStatusKey(row) === 'pending_approval' },
        { icon: 'pi pi-times', tooltip: 'Cancel', action: 'cancel', severity: 'danger', visible: row => !['cancelled', 'fully_received'].includes(this.getStatusKey(row)) }
    ];

    orderForm = inject(FormBuilder).group({
        supplierId: ['', Validators.required],
        orderDate: [this.today(), Validators.required],
        expectedDeliveryDate: [''],
        sourceQuotationNumber: [''],
        statusKey: ['draft' as PurchaseOrderStatusKey, Validators.required],
        notes: ['']
    });

    private purchasingApi = inject(PurchasingApiService);
    private crmApi = inject(CrmApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void {
        this.loadSuppliers();
        this.loadOrders();
    }

    openCreateDialog(): void {
        this.editingOrder = null;
        this.selectedOrder = null;
        this.nextOrderNumberPreview = this.generateOrderNumber();
        this.orderForm.reset({
            supplierId: '',
            orderDate: this.today(),
            expectedDeliveryDate: '',
            sourceQuotationNumber: '',
            statusKey: 'draft',
            notes: ''
        });
        this.orderLines = [];
        this.latestTotals = { subTotal: 0, discountAmount: 0, discountPercent: undefined, taxAmount: 0, total: 0 };
        this.showFormDialog = true;
    }

    closeFormDialog(): void {
        this.showFormDialog = false;
        this.editingOrder = null;
    }

    closeViewDialog(): void {
        this.showViewDialog = false;
        this.selectedOrder = null;
    }

    loadOrders(): void {
        this.loading = true;
        this.purchasingApi.getPurchaseOrders().subscribe({
            next: orders => {
                this.orders = orders ?? [];
                this.localOrders = [...this.orders];
                this.loading = false;
                this.nextOrderNumberPreview = this.generateOrderNumber();
            },
            error: () => {
                this.orders = [...this.localOrders];
                this.loading = false;
            }
        });
    }

    loadSuppliers(): void {
        this.crmApi.getSuppliers().subscribe({
            next: suppliers => {
                this.suppliers = suppliers ?? [];
            },
            error: () => {
                this.suppliers = [];
            }
        });
    }

    handleAction(event: { action: string; data: PurchaseOrder }): void {
        if (event.action === 'view') {
            this.selectedOrder = event.data;
            this.showViewDialog = true;
            return;
        }

        if (event.action === 'edit') {
            this.editingOrder = event.data;
            this.nextOrderNumberPreview = event.data.orderNumber;
            this.orderForm.reset({
                supplierId: event.data.supplierId,
                orderDate: this.toDateInput(event.data.orderDate),
                expectedDeliveryDate: this.toDateInput(event.data.expectedDeliveryDate),
                sourceQuotationNumber: event.data.sourceQuotationNumber || '',
                statusKey: this.getStatusKey(event.data),
                notes: event.data.notes || ''
            });
            this.orderLines = this.mapLinesToEditor(event.data.lines);
            this.latestTotals = {
                subTotal: event.data.subTotal,
                discountAmount: event.data.discountAmount,
                discountPercent: event.data.discountPercent,
                taxAmount: event.data.taxAmount,
                total: event.data.total
            };
            this.showFormDialog = true;
            return;
        }

        if (event.action === 'submit') {
            this.updateStatus(event.data, 'pending_approval');
            return;
        }

        if (event.action === 'approve') {
            this.updateStatus(event.data, 'approved');
            return;
        }

        if (event.action === 'cancel') {
            this.updateStatus(event.data, 'cancelled');
        }
    }

    saveOrder(): void {
        if (this.orderForm.invalid || this.orderLines.length === 0) {
            this.orderForm.markAllAsTouched();
            this.showWarn('Please complete the required fields and add at least one line item.');
            return;
        }

        const value = this.orderForm.getRawValue();
        const request: CreatePurchaseOrderRequest = {
            supplierId: value.supplierId || '',
            orderDate: this.toIso(value.orderDate || this.today()),
            expectedDeliveryDate: value.expectedDeliveryDate ? this.toIso(value.expectedDeliveryDate) : undefined,
            currency: 'ILS',
            discountAmount: this.latestTotals.discountAmount,
            discountPercent: this.latestTotals.discountPercent,
            notes: value.notes || undefined,
            lines: this.orderLines.map(line => ({
                itemId: line.itemId,
                unitOfMeasureId: line.unitOfMeasureId,
                quantity: line.quantity,
                unitPrice: line.unitPrice,
                discountPercent: line.discountPercent,
                discountAmount: line.discountAmount,
                taxPercent: line.taxPercent,
                notes: line.notes
            }))
        };

        const localOrder = this.buildOrderFromForm(request, value.statusKey || 'draft');
        this.saving = true;

        const save$ = this.editingOrder
            ? this.purchasingApi.updatePurchaseOrder(this.editingOrder.id, request)
            : this.purchasingApi.createPurchaseOrder(request);

        save$.subscribe({
            next: saved => {
                this.upsertOrder(saved);
                this.finishSave('Purchase order saved.');
            },
            error: () => {
                this.upsertOrder(localOrder);
                this.finishSave('Purchase order saved locally. Backend integration is not fully available yet.', 'warn');
            }
        });
    }

    onLinesChanged(lines: DocLine[]): void {
        this.orderLines = [...lines];
    }

    onTotalsChanged(totals: PurchaseOrderTotals): void {
        this.latestTotals = totals;
    }

    isInvalid(controlName: string): boolean {
        const control = this.orderForm.get(controlName);
        return !!control && control.invalid && (control.touched || control.dirty);
    }

    private updateStatus(order: PurchaseOrder, statusKey: PurchaseOrderStatusKey): void {
        const status = this.statusOptions.find(x => x.key === statusKey)!;
        const updated: PurchaseOrder = {
            ...order,
            statusLookupId: status.key,
            statusName: status.name,
            statusColor: status.color,
            approvedAt: statusKey === 'approved' ? new Date().toISOString() : order.approvedAt
        };

        let request$;
        if (statusKey === 'pending_approval') {
            request$ = this.purchasingApi.submitPurchaseOrderForApproval(order.id);
        } else if (statusKey === 'approved') {
            request$ = this.purchasingApi.approvePurchaseOrder(order.id);
        } else if (statusKey === 'cancelled') {
            request$ = this.purchasingApi.cancelPurchaseOrder(order.id);
        } else {
            this.upsertOrder(updated);
            return;
        }

        request$.subscribe({
            next: () => {
                this.upsertOrder(updated);
                this.showSuccess(`Purchase order ${status.name.toLowerCase()}.`);
            },
            error: () => {
                this.upsertOrder(updated);
                this.showWarn(`Purchase order marked as ${status.name.toLowerCase()} locally. Backend action is not fully available yet.`);
            }
        });
    }

    private buildOrderFromForm(request: CreatePurchaseOrderRequest, statusKey: PurchaseOrderStatusKey): PurchaseOrder {
        const supplier = this.suppliers.find(x => x.id === request.supplierId);
        const status = this.statusOptions.find(x => x.key === statusKey)!;
        const now = new Date().toISOString();
        const existingId = this.editingOrder?.id || this.generateId();

        return {
            id: existingId,
            orderNumber: this.editingOrder?.orderNumber || this.generateOrderNumber(),
            supplierId: request.supplierId,
            supplierName: supplier?.nameEn || 'Unknown Supplier',
            statusLookupId: status.key,
            statusName: status.name,
            statusColor: status.color,
            sourceQuotationId: undefined,
            sourceQuotationNumber: this.orderForm.get('sourceQuotationNumber')?.value || undefined,
            orderDate: request.orderDate,
            expectedDeliveryDate: request.expectedDeliveryDate,
            currency: request.currency || 'ILS',
            subTotal: this.latestTotals.subTotal,
            discountAmount: this.latestTotals.discountAmount,
            discountPercent: this.latestTotals.discountPercent,
            taxAmount: this.latestTotals.taxAmount,
            total: this.latestTotals.total,
            notes: request.notes,
            approvedByUserId: this.editingOrder?.approvedByUserId,
            approvedByName: this.editingOrder?.approvedByName,
            approvedAt: statusKey === 'approved' ? now : this.editingOrder?.approvedAt,
            warehouseId: request.warehouseId,
            createdByUserId: this.editingOrder?.createdByUserId || 'local-admin',
            lines: this.mapLinesToModel(this.orderLines, existingId),
            createdAt: this.editingOrder?.createdAt || now,
            updatedAt: now
        };
    }

    private mapLinesToEditor(lines: PurchaseOrderLine[] | undefined): DocLine[] {
        return (lines || []).map((line, index) => ({
            lineNumber: line.lineNumber || index + 1,
            itemId: line.itemId,
            itemName: line.itemName,
            itemSku: line.itemSku,
            unitOfMeasureId: line.unitOfMeasureId,
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            discountPercent: line.discountPercent,
            discountAmount: line.discountAmount,
            taxPercent: line.taxPercent,
            taxAmount: line.taxAmount,
            lineTotal: line.lineTotal,
            notes: line.notes
        }));
    }

    private mapLinesToModel(lines: DocLine[], purchaseOrderId: string): PurchaseOrderLine[] {
        return lines.map((line, index) => ({
            purchaseOrderId,
            lineNumber: line.lineNumber || index + 1,
            itemId: line.itemId,
            itemName: line.itemName,
            itemSku: line.itemSku,
            unitOfMeasureId: line.unitOfMeasureId,
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            discountPercent: line.discountPercent,
            discountAmount: line.discountAmount,
            taxPercent: line.taxPercent,
            taxAmount: line.taxAmount,
            lineTotal: line.lineTotal,
            notes: line.notes,
            quantityReceived: 0,
            quantityReturned: 0
        }));
    }

    private upsertOrder(order: PurchaseOrder): void {
        const next = [...this.orders];
        const index = next.findIndex(x => x.id === order.id);
        if (index >= 0) {
            next[index] = order;
        } else {
            next.unshift(order);
        }
        this.orders = next;
        this.localOrders = [...next];
        this.nextOrderNumberPreview = this.generateOrderNumber();
    }

    private finishSave(message: string, severity: 'success' | 'warn' = 'success'): void {
        this.saving = false;
        this.showFormDialog = false;
        this.editingOrder = null;
        if (severity === 'success') {
            this.showSuccess(message);
        } else {
            this.showWarn(message);
        }
    }

    private getStatusKey(order: PurchaseOrder): PurchaseOrderStatusKey {
        const key = (order.statusLookupId || '').toLowerCase().replace(/-/g, '_') as PurchaseOrderStatusKey;
        return this.statusOptions.some(x => x.key === key) ? key : 'draft';
    }

    private generateOrderNumber(): string {
        const year = new Date().getFullYear();
        const allNumbers = [...this.orders, ...this.localOrders]
            .map(x => x.orderNumber)
            .filter(Boolean)
            .map(x => {
                const match = x.match(/(\d{4})$/);
                return match ? Number(match[1]) : 0;
            });
        const next = (allNumbers.length ? Math.max(...allNumbers) : 0) + 1;
        return `PO-${year}-${next.toString().padStart(4, '0')}`;
    }

    private toDateInput(value?: string): string {
        if (!value) return '';
        return new Date(value).toISOString().slice(0, 10);
    }

    private toIso(value: string): string {
        return new Date(value).toISOString();
    }

    private today(): string {
        return new Date().toISOString().slice(0, 10);
    }

    private generateId(): string {
        return `local-${Date.now()}-${Math.random().toString(16).slice(2)}`;
    }

    private showSuccess(detail: string): void {
        this.messageService.add({
            severity: 'success',
            summary: this.translateService.instant('messages.success'),
            detail
        });
    }

    private showWarn(detail: string): void {
        this.messageService.add({
            severity: 'warn',
            summary: this.translateService.instant('common.notice') || 'Notice',
            detail
        });
    }
}
