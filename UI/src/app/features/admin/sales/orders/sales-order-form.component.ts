import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DocumentLinesTableComponent, DocLine } from '../../../../shared/components/document-lines-table/document-lines-table';
import { SalesApiService } from '../../../../shared/services/sales-api.service';
import { CrmApiService } from '../../../../shared/services/crm-api.service';
import { BizInventoryApiService } from '../../../../shared/services/biz-inventory-api.service';
import { Customer } from '../../../../shared/models/crm.model';
import { BizWarehouse } from '../../../../shared/models/biz-inventory.model';
import { CreateSalesOrderRequest, SalesOrder } from '../../../../shared/models/sales.model';

@Component({
    selector: 'app-sales-order-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterModule, ButtonModule, CardModule, DatePickerModule, TextareaModule, SelectModule, MessageModule, ToastModule, TranslateModule, DocumentLinesTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ isEditMode ? ('admin.sales.orders.edit' | translate) : ('admin.sales.orders.new' | translate) }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.sales.orders.flowHint' | translate }}</p>
                </div>
                <div class="flex gap-2">
                    <p-button [label]="'common.back' | translate" severity="secondary" [outlined]="true" routerLink="/admin/sales/orders"></p-button>
                    <p-button [label]="'common.save' | translate" icon="pi pi-save" [loading]="saving" (click)="save()"></p-button>
                </div>
            </div>

            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-3"></p-message>
            }

            <form [formGroup]="form" class="grid gap-4">
                <p-card [header]="'admin.common.headerSection' | translate">
                    <div class="form-grid">
                        <div class="field">
                            <label>{{ 'admin.sales.common.customer' | translate }}</label>
                            <p-select formControlName="customerId" [options]="customers" optionLabel="nameEn" optionValue="id" [filter]="true" styleClass="w-full"></p-select>
                        </div>
                        <div class="field">
                            <label>{{ 'common.date' | translate }}</label>
                            <p-datepicker formControlName="orderDate" appendTo="body" inputStyleClass="w-full"></p-datepicker>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.sales.orders.expectedDeliveryDate' | translate }}</label>
                            <p-datepicker formControlName="expectedDeliveryDate" appendTo="body" inputStyleClass="w-full"></p-datepicker>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.inventory.warehouse' | translate }}</label>
                            <p-select formControlName="warehouseId" [options]="warehouses" optionLabel="nameEn" optionValue="id" styleClass="w-full"></p-select>
                        </div>
                        <div class="field field-full">
                            <label>{{ 'common.notes' | translate }}</label>
                            <textarea pInputTextarea rows="3" formControlName="notes"></textarea>
                        </div>
                    </div>
                </p-card>

                <p-card [header]="'admin.common.linesSection' | translate">
                    <app-document-lines-table
                        [lines]="lines"
                        [showTax]="true"
                        [showDocumentDiscount]="true"
                        (linesChange)="lines = $event"
                        (totalsChange)="totals = $event"
                    ></app-document-lines-table>
                </p-card>
            </form>
        </div>
    `,
    styles: [`
        .form-grid { display: grid; gap: 1rem; grid-template-columns: repeat(2, minmax(0, 1fr)); }
        .field { display: flex; flex-direction: column; gap: 0.5rem; }
        .field-full { grid-column: 1 / -1; }
    `]
})
export class SalesOrderFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private salesApi = inject(SalesApiService);
    private crmApi = inject(CrmApiService);
    private inventoryApi = inject(BizInventoryApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    isEditMode = false;
    saving = false;
    errorMessage = '';
    customers: Customer[] = [];
    warehouses: BizWarehouse[] = [];
    lines: DocLine[] = [];
    totals = { subTotal: 0, discountAmount: 0, taxAmount: 0, total: 0 };

    form = this.fb.group({
        customerId: ['', Validators.required],
        orderDate: [new Date(), Validators.required],
        expectedDeliveryDate: [null as Date | null],
        warehouseId: [''],
        notes: ['']
    });

    ngOnInit(): void {
        this.crmApi.getCustomers().subscribe(c => this.customers = c);
        this.inventoryApi.getWarehouses().subscribe(w => this.warehouses = w);

        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.isEditMode = true;
            this.salesApi.getSalesOrderById(id).subscribe(order => this.patchOrder(order));
        } else {
            this.lines = [{ lineNumber: 1, itemId: '', quantity: 1, unitPrice: 0, discountAmount: 0, taxAmount: 0, lineTotal: 0 }];
        }
    }

    patchOrder(order: SalesOrder): void {
        this.form.patchValue({
            customerId: order.customerId,
            orderDate: new Date(order.orderDate),
            expectedDeliveryDate: order.expectedDeliveryDate ? new Date(order.expectedDeliveryDate) : null,
            warehouseId: order.warehouseId ?? '',
            notes: order.notes ?? ''
        });
        this.lines = order.lines.map((line, index) => ({
            lineNumber: index + 1,
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
        this.totals = { subTotal: order.subTotal, discountAmount: order.discountAmount, taxAmount: order.taxAmount, total: order.total };
    }

    save(): void {
        if (this.form.invalid || this.lines.length === 0) {
            this.form.markAllAsTouched();
            return;
        }
        this.saving = true;
        this.errorMessage = '';

        const request: CreateSalesOrderRequest = {
            customerId: this.form.value.customerId!,
            orderDate: (this.form.value.orderDate as Date).toISOString(),
            expectedDeliveryDate: this.form.value.expectedDeliveryDate ? (this.form.value.expectedDeliveryDate as Date).toISOString() : undefined,
            warehouseId: this.form.value.warehouseId || undefined,
            notes: this.form.value.notes || undefined,
            discountAmount: this.totals.discountAmount,
            lines: this.lines.map(line => ({
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

        const request$ = this.isEditMode
            ? this.salesApi.updateSalesOrder(this.route.snapshot.paramMap.get('id')!, request)
            : this.salesApi.createSalesOrder(request);

        request$.subscribe({
            next: () => {
                this.saving = false;
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.saved') });
                this.router.navigate(['/admin/sales/orders']);
            },
            error: () => {
                this.saving = false;
                this.errorMessage = this.translateService.instant('messages.saveError');
            }
        });
    }
}
