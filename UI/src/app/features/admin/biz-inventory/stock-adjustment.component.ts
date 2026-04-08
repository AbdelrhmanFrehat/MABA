import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { BizInventoryApiService } from '../../../shared/services/biz-inventory-api.service';
import { BizWarehouse, BizWarehouseItem } from '../../../shared/models/biz-inventory.model';

@Component({
    selector: 'app-stock-adjustment',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, TextareaModule, SelectModule, ToastModule, TranslateModule],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="mb-4">
                <h1 class="text-2xl font-bold m-0">{{ 'admin.inventory.stockAdjustment' | translate }}</h1>
                <p class="text-600 mt-2 mb-0">Post quantity corrections and warehouse transfers without leaving the ERP workflow.</p>
            </div>

            <div class="grid">
                <div class="col-12 lg:col-6">
                    <div class="surface-card border-1 surface-border border-round p-4 h-full">
                        <h3 class="mt-0 mb-3">Manual Adjustment</h3>
                        <form [formGroup]="adjustForm" class="grid formgrid">
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Warehouse</label>
                                <p-select formControlName="warehouseId" [options]="warehouses" optionLabel="nameEn" optionValue="id" styleClass="w-full" (onChange)="onAdjustWarehouseChange()"></p-select>
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Item</label>
                                <p-select formControlName="itemId" [options]="adjustItems" optionLabel="itemName" optionValue="itemId" styleClass="w-full" [filter]="true" filterBy="itemName,itemSku"></p-select>
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Quantity Change</label>
                                <input pInputText type="number" class="w-full" formControlName="quantity" />
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Reason</label>
                                <input pInputText class="w-full" formControlName="reason" />
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Notes</label>
                                <textarea pTextarea rows="3" class="w-full" formControlName="notes"></textarea>
                            </div>
                            <div class="col-12 flex justify-content-end">
                                <p-button label="Post Adjustment" icon="pi pi-check" [disabled]="adjustForm.invalid || savingAdjust" [loading]="savingAdjust" (onClick)="submitAdjustment()"></p-button>
                            </div>
                        </form>
                    </div>
                </div>

                <div class="col-12 lg:col-6">
                    <div class="surface-card border-1 surface-border border-round p-4 h-full">
                        <h3 class="mt-0 mb-3">Transfer Between Warehouses</h3>
                        <form [formGroup]="transferForm" class="grid formgrid">
                            <div class="field col-12 md:col-6">
                                <label class="block font-medium mb-2">From Warehouse</label>
                                <p-select formControlName="fromWarehouseId" [options]="warehouses" optionLabel="nameEn" optionValue="id" styleClass="w-full" (onChange)="onTransferWarehouseChange()"></p-select>
                            </div>
                            <div class="field col-12 md:col-6">
                                <label class="block font-medium mb-2">To Warehouse</label>
                                <p-select formControlName="toWarehouseId" [options]="warehouses" optionLabel="nameEn" optionValue="id" styleClass="w-full"></p-select>
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Item</label>
                                <p-select formControlName="itemId" [options]="transferItems" optionLabel="itemName" optionValue="itemId" styleClass="w-full" [filter]="true" filterBy="itemName,itemSku"></p-select>
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Quantity</label>
                                <input pInputText type="number" class="w-full" formControlName="quantity" />
                            </div>
                            <div class="field col-12">
                                <label class="block font-medium mb-2">Notes</label>
                                <textarea pTextarea rows="3" class="w-full" formControlName="notes"></textarea>
                            </div>
                            <div class="col-12 flex justify-content-end">
                                <p-button label="Transfer Stock" icon="pi pi-send" [disabled]="transferForm.invalid || savingTransfer" [loading]="savingTransfer" (onClick)="submitTransfer()"></p-button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class StockAdjustmentComponent implements OnInit {
    warehouses: BizWarehouse[] = [];
    stock: BizWarehouseItem[] = [];
    adjustItems: BizWarehouseItem[] = [];
    transferItems: BizWarehouseItem[] = [];
    savingAdjust = false;
    savingTransfer = false;

    adjustForm = inject(FormBuilder).group({
        warehouseId: ['', Validators.required],
        itemId: ['', Validators.required],
        quantity: [0, Validators.required],
        reason: ['', Validators.required],
        notes: ['']
    });

    transferForm = inject(FormBuilder).group({
        fromWarehouseId: ['', Validators.required],
        toWarehouseId: ['', Validators.required],
        itemId: ['', Validators.required],
        quantity: [0, Validators.required],
        notes: ['']
    });

    private api = inject(BizInventoryApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    ngOnInit(): void {
        this.loadWarehouses();
        this.loadStock();
    }

    loadWarehouses(): void {
        this.api.getWarehouses().subscribe({
            next: rows => this.warehouses = rows ?? [],
            error: () => this.warehouses = []
        });
    }

    loadStock(): void {
        this.api.getStockOverview().subscribe({
            next: rows => {
                this.stock = rows ?? [];
                this.onAdjustWarehouseChange();
                this.onTransferWarehouseChange();
            },
            error: () => this.stock = []
        });
    }

    onAdjustWarehouseChange(): void {
        const warehouseId = this.adjustForm.get('warehouseId')?.value;
        this.adjustItems = this.stock.filter(x => x.warehouseId === warehouseId);
    }

    onTransferWarehouseChange(): void {
        const warehouseId = this.transferForm.get('fromWarehouseId')?.value;
        this.transferItems = this.stock.filter(x => x.warehouseId === warehouseId && (x.quantityAvailable || 0) > 0);
    }

    submitAdjustment(): void {
        if (this.adjustForm.invalid) {
            this.adjustForm.markAllAsTouched();
            return;
        }

        this.savingAdjust = true;
        this.api.adjustInventory(this.adjustForm.getRawValue() as any).subscribe({
            next: () => {
                this.savingAdjust = false;
                this.adjustForm.reset({ warehouseId: '', itemId: '', quantity: 0, reason: '', notes: '' });
                this.adjustItems = [];
                this.loadStock();
                this.showSuccess('Inventory adjustment posted.');
            },
            error: () => {
                this.savingAdjust = false;
                this.showError('Failed to post inventory adjustment.');
            }
        });
    }

    submitTransfer(): void {
        if (this.transferForm.invalid) {
            this.transferForm.markAllAsTouched();
            return;
        }

        this.savingTransfer = true;
        this.api.transferInventory(this.transferForm.getRawValue() as any).subscribe({
            next: () => {
                this.savingTransfer = false;
                this.transferForm.reset({ fromWarehouseId: '', toWarehouseId: '', itemId: '', quantity: 0, notes: '' });
                this.transferItems = [];
                this.loadStock();
                this.showSuccess('Inventory transfer posted.');
            },
            error: () => {
                this.savingTransfer = false;
                this.showError('Failed to transfer stock.');
            }
        });
    }

    private showSuccess(detail: string): void {
        this.messageService.add({ severity: 'success', summary: this.translateService.instant('messages.success'), detail });
    }

    private showError(detail: string): void {
        this.messageService.add({ severity: 'error', summary: this.translateService.instant('messages.error'), detail });
    }
}
