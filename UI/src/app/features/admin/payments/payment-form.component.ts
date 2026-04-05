import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LookupDropdownComponent } from '../../../shared/components/lookup-dropdown/lookup-dropdown';
import { PaymentVoucherApiService } from '../../../shared/services/payment-voucher-api.service';
import { CrmApiService } from '../../../shared/services/crm-api.service';
import { SalesApiService } from '../../../shared/services/sales-api.service';
import { PurchasingApiService } from '../../../shared/services/purchasing-api.service';
import { Customer, Supplier } from '../../../shared/models/crm.model';
import { SalesInvoice } from '../../../shared/models/sales.model';
import { PurchaseInvoice } from '../../../shared/models/purchasing.model';
import { CreatePaymentVoucherRequest, PaymentVoucher } from '../../../shared/models/biz-payment.model';

@Component({
    selector: 'app-payment-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterModule, ButtonModule, CardModule, DatePickerModule, InputNumberModule, InputTextModule, SelectModule, MessageModule, ToastModule, TableModule, TranslateModule, LookupDropdownComponent],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ isEditMode ? ('admin.payments.edit' | translate) : ('admin.payments.new' | translate) }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.payments.formHint' | translate }}</p>
                </div>
                <div class="flex gap-2">
                    <p-button [label]="'common.back' | translate" severity="secondary" [outlined]="true" routerLink="/admin/payments"></p-button>
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
                            <label>{{ 'admin.payments.direction' | translate }}</label>
                            <app-lookup-dropdown formControlName="paymentDirectionLookupId" lookupTypeKey="payment_direction"></app-lookup-dropdown>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.payments.method' | translate }}</label>
                            <app-lookup-dropdown formControlName="paymentMethodLookupId" lookupTypeKey="payment_method"></app-lookup-dropdown>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.crm.customers' | translate }}</label>
                            <p-select formControlName="customerId" [options]="customers" optionLabel="nameEn" optionValue="id" [filter]="true" [showClear]="true" styleClass="w-full"></p-select>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.crm.suppliers' | translate }}</label>
                            <p-select formControlName="supplierId" [options]="suppliers" optionLabel="nameEn" optionValue="id" [filter]="true" [showClear]="true" styleClass="w-full"></p-select>
                        </div>
                        <div class="field">
                            <label>{{ 'common.amount' | translate }}</label>
                            <p-inputNumber formControlName="amount" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber>
                        </div>
                        <div class="field">
                            <label>{{ 'common.date' | translate }}</label>
                            <p-datepicker formControlName="paymentDate" appendTo="body" inputStyleClass="w-full"></p-datepicker>
                        </div>
                        <div class="field">
                            <label>{{ 'admin.payments.referenceNumber' | translate }}</label>
                            <input pInputText formControlName="referenceNumber" />
                        </div>
                        <div class="field">
                            <label>{{ 'admin.payments.bankName' | translate }}</label>
                            <input pInputText formControlName="bankName" />
                        </div>
                    </div>
                </p-card>

                <p-card [header]="'admin.payments.allocations' | translate">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                        <div>
                            <label class="block mb-2">{{ 'admin.sales.invoices.title' | translate }}</label>
                            <p-select [options]="salesInvoices" optionLabel="invoiceNumber" optionValue="id" [filter]="true" [showClear]="true" styleClass="w-full" (onChange)="addSalesAllocation($event.value)"></p-select>
                        </div>
                        <div>
                            <label class="block mb-2">{{ 'admin.purchasing.invoices.title' | translate }}</label>
                            <p-select [options]="purchaseInvoices" optionLabel="invoiceNumber" optionValue="id" [filter]="true" [showClear]="true" styleClass="w-full" (onChange)="addPurchaseAllocation($event.value)"></p-select>
                        </div>
                    </div>

                    <p-table [value]="allocationControls.controls">
                        <ng-template #header>
                            <tr>
                                <th>{{ 'common.type' | translate }}</th>
                                <th>{{ 'admin.common.document' | translate }}</th>
                                <th>{{ 'common.amount' | translate }}</th>
                                <th></th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-control let-i="rowIndex">
                            <tr [formGroup]="control">
                                <td>{{ control.value.salesInvoiceId ? ('admin.sales.invoices.title' | translate) : ('admin.purchasing.invoices.title' | translate) }}</td>
                                <td>{{ resolveAllocationName(control.value) }}</td>
                                <td><p-inputNumber formControlName="allocatedAmount" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber></td>
                                <td><p-button icon="pi pi-trash" severity="danger" [outlined]="true" (click)="removeAllocation(i)"></p-button></td>
                            </tr>
                        </ng-template>
                    </p-table>
                </p-card>
            </form>
        </div>
    `,
    styles: [`
        .form-grid { display: grid; gap: 1rem; grid-template-columns: repeat(2, minmax(0, 1fr)); }
        .field { display: flex; flex-direction: column; gap: 0.5rem; }
    `]
})
export class PaymentFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private paymentsApi = inject(PaymentVoucherApiService);
    private crmApi = inject(CrmApiService);
    private salesApi = inject(SalesApiService);
    private purchasingApi = inject(PurchasingApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    isEditMode = false;
    saving = false;
    errorMessage = '';
    customers: Customer[] = [];
    suppliers: Supplier[] = [];
    salesInvoices: SalesInvoice[] = [];
    purchaseInvoices: PurchaseInvoice[] = [];

    form = this.fb.group({
        paymentDirectionLookupId: ['', Validators.required],
        paymentMethodLookupId: ['', Validators.required],
        customerId: [''],
        supplierId: [''],
        amount: [0, Validators.required],
        paymentDate: [new Date(), Validators.required],
        referenceNumber: [''],
        bankName: [''],
        allocations: this.fb.array([])
    });

    get allocationControls(): FormArray {
        return this.form.get('allocations') as FormArray;
    }

    ngOnInit(): void {
        this.crmApi.getCustomers().subscribe(c => this.customers = c);
        this.crmApi.getSuppliers().subscribe(s => this.suppliers = s);
        this.salesApi.getSalesInvoices().subscribe(i => this.salesInvoices = i);
        this.purchasingApi.getPurchaseInvoices().subscribe(i => this.purchaseInvoices = i);

        const invoiceId = this.route.snapshot.queryParamMap.get('salesInvoiceId');
        if (invoiceId) {
            this.addSalesAllocation(invoiceId);
        }

        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.isEditMode = true;
            this.paymentsApi.getPaymentVoucherById(id).subscribe(v => this.patchVoucher(v));
        }
    }

    patchVoucher(voucher: PaymentVoucher): void {
        this.form.patchValue({
            paymentDirectionLookupId: voucher.paymentDirectionLookupId,
            paymentMethodLookupId: voucher.paymentMethodLookupId,
            customerId: voucher.customerId ?? '',
            supplierId: voucher.supplierId ?? '',
            amount: voucher.amount,
            paymentDate: new Date(voucher.paymentDate),
            referenceNumber: voucher.referenceNumber ?? '',
            bankName: voucher.bankName ?? ''
        });
        this.allocationControls.clear();
        voucher.allocations.forEach(a => this.allocationControls.push(this.fb.group({
            salesInvoiceId: [a.salesInvoiceId ?? ''],
            purchaseInvoiceId: [a.purchaseInvoiceId ?? ''],
            allocatedAmount: [a.allocatedAmount, Validators.required]
        })));
    }

    addSalesAllocation(id: string): void {
        if (!id || this.allocationControls.value.some((x: any) => x.salesInvoiceId === id)) return;
        const invoice = this.salesInvoices.find(x => x.id === id);
        this.allocationControls.push(this.fb.group({ salesInvoiceId: [id], purchaseInvoiceId: [''], allocatedAmount: [invoice?.amountDue ?? 0, Validators.required] }));
    }

    addPurchaseAllocation(id: string): void {
        if (!id || this.allocationControls.value.some((x: any) => x.purchaseInvoiceId === id)) return;
        const invoice = this.purchaseInvoices.find(x => x.id === id);
        this.allocationControls.push(this.fb.group({ salesInvoiceId: [''], purchaseInvoiceId: [id], allocatedAmount: [invoice?.amountDue ?? 0, Validators.required] }));
    }

    removeAllocation(index: number): void {
        this.allocationControls.removeAt(index);
    }

    resolveAllocationName(value: { salesInvoiceId?: string; purchaseInvoiceId?: string }): string {
        if (value.salesInvoiceId) return this.salesInvoices.find(x => x.id === value.salesInvoiceId)?.invoiceNumber ?? value.salesInvoiceId;
        if (value.purchaseInvoiceId) return this.purchaseInvoices.find(x => x.id === value.purchaseInvoiceId)?.invoiceNumber ?? value.purchaseInvoiceId;
        return '';
    }

    save(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }
        this.saving = true;
        const request: CreatePaymentVoucherRequest = {
            paymentDirectionLookupId: this.form.value.paymentDirectionLookupId!,
            paymentMethodLookupId: this.form.value.paymentMethodLookupId!,
            customerId: this.form.value.customerId || undefined,
            supplierId: this.form.value.supplierId || undefined,
            amount: this.form.value.amount ?? 0,
            paymentDate: (this.form.value.paymentDate as Date).toISOString(),
            referenceNumber: this.form.value.referenceNumber || undefined,
            bankName: this.form.value.bankName || undefined,
            allocations: this.allocationControls.value.map((x: any) => ({ salesInvoiceId: x.salesInvoiceId || undefined, purchaseInvoiceId: x.purchaseInvoiceId || undefined, allocatedAmount: x.allocatedAmount }))
        };

        this.paymentsApi.createPaymentVoucher(request).subscribe({
            next: () => {
                this.saving = false;
                this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail: this.translateService.instant('messages.saved') });
                this.router.navigate(['/admin/payments']);
            },
            error: () => {
                this.saving = false;
                this.errorMessage = this.translateService.instant('messages.saveError');
            }
        });
    }
}
