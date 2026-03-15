import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { SelectModule } from 'primeng/select';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../shared/services/api.service';
import { UserPaymentMethod, PaymentMethodType } from '../../../shared/models/payment.model';

@Component({
    selector: 'app-account-payment-methods',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        DialogModule,
        ToastModule,
        ConfirmDialogModule,
        TagModule,
        TooltipModule,
        SelectModule,
        TranslateModule
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />
        <div class="container mx-auto px-4 py-6" style="max-width: 1000px;">
            <div class="flex justify-content-between align-items-center mb-6">
                <h1 class="text-3xl font-bold">{{ 'account.paymentMethods.title' | translate }}</h1>
                <p-button 
                    [label]="'account.paymentMethods.addNew' | translate" 
                    icon="pi pi-plus"
                    (click)="openAddDialog()">
                </p-button>
            </div>

            <div *ngIf="loading" class="flex justify-content-center p-6">
                <i class="pi pi-spin pi-spinner text-4xl"></i>
            </div>

            <div *ngIf="!loading && paymentMethods.length === 0" class="text-center p-6">
                <i class="pi pi-credit-card text-6xl text-500 mb-4"></i>
                <h2 class="text-2xl font-bold mb-2">{{ 'account.paymentMethods.noMethods' | translate }}</h2>
                <p class="text-500 mb-4">{{ 'account.paymentMethods.noMethodsDescription' | translate }}</p>
                <p-button 
                    [label]="'account.paymentMethods.addNew' | translate" 
                    icon="pi pi-plus"
                    (click)="openAddDialog()">
                </p-button>
            </div>

            <div *ngIf="!loading && paymentMethods.length > 0" class="grid">
                <div *ngFor="let method of paymentMethods" class="col-12 md:col-6">
                    <p-card>
                        <div class="flex justify-content-between align-items-start mb-3">
                            <div>
                                <div class="flex align-items-center gap-2 mb-2">
                                    <i [class]="getPaymentIcon(method.type)"></i>
                                    <span class="font-bold">{{ getPaymentTypeName(method.type) }}</span>
                                    <p-tag *ngIf="method.isDefault" value="{{ 'account.paymentMethods.default' | translate }}" severity="success"></p-tag>
                                </div>
                                <div *ngIf="method.last4" class="text-500">
                                    **** **** **** {{ method.last4 }}
                                </div>
                                <div *ngIf="method.expiryMonth && method.expiryYear" class="text-500 text-sm">
                                    {{ 'account.paymentMethods.expires' | translate }}: {{ method.expiryMonth }}/{{ method.expiryYear }}
                                </div>
                            </div>
                            <div class="flex gap-2">
                                <p-button 
                                    *ngIf="!method.isDefault"
                                    icon="pi pi-check"
                                    [text]="true"
                                    [rounded]="true"
                                    (click)="setDefaultMethod(method)"
                                    [pTooltip]="'account.paymentMethods.setDefault' | translate">
                                </p-button>
                                <p-button 
                                    icon="pi pi-trash"
                                    [text]="true"
                                    [rounded]="true"
                                    severity="danger"
                                    (click)="deleteMethod(method)"
                                    [pTooltip]="'common.delete' | translate">
                                </p-button>
                            </div>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>

        <p-dialog 
            [header]="'account.paymentMethods.addNew' | translate"
            [(visible)]="showDialog"
            [modal]="true"
            [closable]="true"
            [dismissableMask]="true"
            [style]="{width: '500px'}"
            (onHide)="closeDialog()">
            <form [formGroup]="paymentForm" (ngSubmit)="savePaymentMethod()">
                <div class="grid">
                    <div class="col-12">
                        <label class="block font-bold mb-2">{{ 'account.paymentMethods.type' | translate }} *</label>
                        <p-select
                            formControlName="type"
                            [options]="paymentTypeOptions"
                            optionLabel="label"
                            optionValue="value"
                            class="w-full">
                        </p-select>
                    </div>
                    <div class="col-12">
                        <label class="block font-bold mb-2">{{ 'account.paymentMethods.cardNumber' | translate }} *</label>
                        <input type="text" pInputText formControlName="cardNumber" class="w-full" placeholder="1234 5678 9012 3456" />
                    </div>
                    <div class="col-12 md:col-6">
                        <label class="block font-bold mb-2">{{ 'account.paymentMethods.expiryMonth' | translate }} *</label>
                        <p-inputNumber formControlName="expiryMonth" [min]="1" [max]="12" class="w-full"></p-inputNumber>
                    </div>
                    <div class="col-12 md:col-6">
                        <label class="block font-bold mb-2">{{ 'account.paymentMethods.expiryYear' | translate }} *</label>
                        <p-inputNumber formControlName="expiryYear" [min]="2024" [max]="2099" class="w-full"></p-inputNumber>
                    </div>
                </div>
                <div class="flex justify-content-end gap-2 mt-4">
                    <p-button 
                        [label]="'common.cancel' | translate" 
                        [outlined]="true"
                        type="button"
                        (click)="closeDialog()">
                    </p-button>
                    <p-button 
                        [label]="'common.save' | translate" 
                        type="submit"
                        [disabled]="!paymentForm.valid || saving"
                        [loading]="saving">
                    </p-button>
                </div>
            </form>
        </p-dialog>
    `
})
export class AccountPaymentMethodsComponent implements OnInit {
    paymentMethods: UserPaymentMethod[] = [];
    loading = false;
    showDialog = false;
    saving = false;
    paymentForm: FormGroup;
    paymentTypeOptions: any[] = [
        { label: 'Credit Card', value: PaymentMethodType.CreditCard },
        { label: 'Debit Card', value: PaymentMethodType.DebitCard }
    ];

    private formBuilder = inject(FormBuilder);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);

    constructor() {
        this.paymentForm = this.formBuilder.group({
            type: [PaymentMethodType.CreditCard, Validators.required],
            cardNumber: ['', [Validators.required, Validators.pattern(/^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$/)]],
            expiryMonth: [null, [Validators.required, Validators.min(1), Validators.max(12)]],
            expiryYear: [null, [Validators.required, Validators.min(2024)]]
        });
    }

    ngOnInit() {
        this.loadPaymentMethods();
    }

    loadPaymentMethods() {
        this.loading = true;
        this.apiService.get<UserPaymentMethod[]>('/payment-methods').subscribe({
            next: (methods) => {
                this.paymentMethods = methods;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    openAddDialog() {
        this.paymentForm.reset({
            type: PaymentMethodType.CreditCard
        });
        this.showDialog = true;
    }

    closeDialog() {
        this.showDialog = false;
        this.paymentForm.reset();
    }

    savePaymentMethod() {
        if (!this.paymentForm.valid) return;

        this.saving = true;
        const formValue = this.paymentForm.value;
        const last4 = formValue.cardNumber.replace(/\s/g, '').slice(-4);
        
        const request = {
            type: formValue.type,
            cardNumber: formValue.cardNumber.replace(/\s/g, ''),
            expiryMonth: formValue.expiryMonth,
            expiryYear: formValue.expiryYear
        };

        this.apiService.post('/payment-methods', request).subscribe({
            next: () => {
                this.loadPaymentMethods();
                this.closeDialog();
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.saved')
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    deleteMethod(method: UserPaymentMethod) {
        this.confirmationService.confirm({
            message: this.translateService.instant('account.paymentMethods.confirmDelete'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.delete<void>('/payment-methods', method.id).subscribe({
                    next: () => {
                        this.loadPaymentMethods();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('messages.deleted')
                        });
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    setDefaultMethod(method: UserPaymentMethod) {
        this.apiService.post<void>(`/payment-methods/${method.id}/set-default`, {}).subscribe({
            next: () => {
                this.loadPaymentMethods();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('account.paymentMethods.defaultSet')
                });
            }
        });
    }

    getPaymentIcon(type: PaymentMethodType): string {
        return type === PaymentMethodType.CreditCard || type === PaymentMethodType.DebitCard
            ? 'pi pi-credit-card'
            : 'pi pi-wallet';
    }

    getPaymentTypeName(type: PaymentMethodType): string {
        return type === PaymentMethodType.CreditCard ? 'Credit Card' : 'Debit Card';
    }
}

