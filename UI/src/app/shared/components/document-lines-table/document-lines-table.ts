import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ItemsApiService } from '../../services/items-api.service';
import { Item } from '../../models/item.model';

export interface DocLine {
    lineNumber: number;
    itemId: string;
    itemName?: string;
    itemSku?: string;
    unitOfMeasureId?: string;
    quantity: number;
    unitPrice: number;
    discountPercent?: number;
    discountAmount: number;
    taxPercent?: number;
    taxAmount: number;
    lineTotal: number;
    notes?: string;
}

/**
 * Reusable document lines editor for Sales/Purchase Quotation, Order, Invoice forms.
 * Handles item selection, quantity, price, discount, tax, line total calculation.
 */
@Component({
    selector: 'app-document-lines-table',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TableModule, ButtonModule,
        InputNumberModule, InputTextModule, SelectModule, TooltipModule, TranslateModule
    ],
    template: `
        <div class="lines-section">
            <div class="flex justify-between items-center mb-3">
                <h3 class="m-0">{{ 'common.items' | translate }}</h3>
                <p-button
                    [label]="'common.addLine' | translate"
                    icon="pi pi-plus"
                    severity="secondary"
                    [outlined]="true"
                    size="small"
                    (onClick)="addLine()"
                    [disabled]="readonly"
                ></p-button>
            </div>

            <p-table [value]="lines" [tableStyle]="{ 'min-width': '60rem' }" styleClass="p-datatable-sm">
                <ng-template #header>
                    <tr>
                        <th style="width: 3rem">#</th>
                        <th>{{ 'common.item' | translate }}</th>
                        <th style="width: 7rem">{{ 'common.quantity' | translate }}</th>
                        <th style="width: 8rem">{{ 'common.unitPrice' | translate }}</th>
                        <th style="width: 6rem">{{ 'common.discountPct' | translate }}</th>
                        <th style="width: 7rem">{{ 'common.discount' | translate }}</th>
                        <th style="width: 6rem" *ngIf="showTax">{{ 'common.taxPct' | translate }}</th>
                        <th style="width: 7rem" *ngIf="showTax">{{ 'common.tax' | translate }}</th>
                        <th style="width: 8rem">{{ 'common.total' | translate }}</th>
                        <th style="width: 4rem"></th>
                    </tr>
                </ng-template>
                <ng-template #body let-line let-ri="rowIndex">
                    <tr>
                        <td>{{ ri + 1 }}</td>
                        <td>
                            <p-select
                                [options]="items"
                                optionLabel="nameEn"
                                optionValue="id"
                                [(ngModel)]="line.itemId"
                                (ngModelChange)="onItemSelected(line, $event)"
                                [placeholder]="'common.selectItem' | translate"
                                [filter]="true"
                                filterBy="nameEn,sku"
                                styleClass="w-full"
                                [disabled]="readonly"
                            ></p-select>
                        </td>
                        <td>
                            <p-inputNumber
                                [(ngModel)]="line.quantity"
                                (ngModelChange)="recalculateLine(line)"
                                [min]="0.01"
                                [maxFractionDigits]="2"
                                styleClass="w-full"
                                [disabled]="readonly"
                            ></p-inputNumber>
                        </td>
                        <td>
                            <p-inputNumber
                                [(ngModel)]="line.unitPrice"
                                (ngModelChange)="recalculateLine(line)"
                                [min]="0"
                                [maxFractionDigits]="2"
                                styleClass="w-full"
                                [disabled]="readonly || !canEditPrice"
                            ></p-inputNumber>
                        </td>
                        <td>
                            <p-inputNumber
                                [(ngModel)]="line.discountPercent"
                                (ngModelChange)="onDiscountPercentChange(line)"
                                [min]="0"
                                [max]="100"
                                [maxFractionDigits]="2"
                                suffix="%"
                                styleClass="w-full"
                                [disabled]="readonly || !canEditDiscount"
                            ></p-inputNumber>
                        </td>
                        <td>
                            <span class="font-semibold">{{ line.discountAmount | number:'1.2-2' }}</span>
                        </td>
                        <td *ngIf="showTax">
                            <p-inputNumber
                                [(ngModel)]="line.taxPercent"
                                (ngModelChange)="recalculateLine(line)"
                                [min]="0"
                                [max]="100"
                                [maxFractionDigits]="2"
                                suffix="%"
                                styleClass="w-full"
                                [disabled]="readonly"
                            ></p-inputNumber>
                        </td>
                        <td *ngIf="showTax">
                            <span class="font-semibold">{{ line.taxAmount | number:'1.2-2' }}</span>
                        </td>
                        <td>
                            <span class="font-bold">{{ line.lineTotal | number:'1.2-2' }}</span>
                        </td>
                        <td>
                            <p-button
                                icon="pi pi-trash"
                                [rounded]="true"
                                [outlined]="true"
                                severity="danger"
                                size="small"
                                (onClick)="removeLine(ri)"
                                [disabled]="readonly"
                                [pTooltip]="'common.delete' | translate"
                            ></p-button>
                        </td>
                    </tr>
                </ng-template>
                <ng-template #emptymessage>
                    <tr>
                        <td [attr.colspan]="showTax ? 10 : 8" style="text-align: center; padding: 2rem;">
                            <span class="text-gray-500">{{ 'common.noLinesAdded' | translate }}</span>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <!-- Totals Summary -->
            <div class="totals-section mt-4">
                <div class="totals-grid">
                    <div class="total-row">
                        <span>{{ 'common.subtotal' | translate }}</span>
                        <span class="font-semibold">{{ subTotal | number:'1.2-2' }}</span>
                    </div>
                    <div class="total-row">
                        <span>{{ 'common.lineDiscounts' | translate }}</span>
                        <span class="font-semibold text-orange-500">-{{ totalLineDiscount | number:'1.2-2' }}</span>
                    </div>
                    <div class="total-row" *ngIf="showDocumentDiscount">
                        <span>{{ 'common.documentDiscount' | translate }}</span>
                        <div class="flex items-center gap-2">
                            <p-inputNumber
                                [(ngModel)]="documentDiscountPercent"
                                (ngModelChange)="onDocumentDiscountChange()"
                                [min]="0"
                                [max]="100"
                                [maxFractionDigits]="2"
                                suffix="%"
                                styleClass="w-24"
                                [disabled]="readonly || !canEditDiscount"
                            ></p-inputNumber>
                            <span class="font-semibold text-orange-500">-{{ documentDiscountAmount | number:'1.2-2' }}</span>
                        </div>
                    </div>
                    <div class="total-row" *ngIf="showTax">
                        <span>{{ 'common.taxTotal' | translate }}</span>
                        <span class="font-semibold">{{ totalTax | number:'1.2-2' }}</span>
                    </div>
                    <div class="total-row total-grand">
                        <span>{{ 'common.grandTotal' | translate }}</span>
                        <span class="font-bold text-xl">{{ grandTotal | number:'1.2-2' }}</span>
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .lines-section { width: 100%; }
        .totals-section { display: flex; justify-content: flex-end; }
        .totals-grid {
            display: flex; flex-direction: column; gap: 0.5rem;
            min-width: 320px; padding: 1rem;
            background: var(--surface-ground);
            border-radius: 8px; border: 1px solid var(--surface-border);
        }
        .total-row {
            display: flex; justify-content: space-between; align-items: center;
            padding: 0.25rem 0;
        }
        .total-grand {
            border-top: 2px solid var(--surface-border);
            padding-top: 0.75rem; margin-top: 0.25rem;
        }
    `]
})
export class DocumentLinesTableComponent implements OnInit {
    @Input() lines: DocLine[] = [];
    @Input() readonly = false;
    @Input() showTax = true;
    @Input() showDocumentDiscount = true;
    @Input() canEditPrice = true;
    @Input() canEditDiscount = true;
    @Input() defaultTaxPercent = 0;

    @Output() linesChange = new EventEmitter<DocLine[]>();
    @Output() totalsChange = new EventEmitter<{
        subTotal: number; discountAmount: number; discountPercent?: number;
        taxAmount: number; total: number;
    }>();

    items: Item[] = [];
    documentDiscountPercent: number = 0;
    documentDiscountAmount: number = 0;

    private itemsApiService = inject(ItemsApiService);

    get subTotal(): number {
        return this.lines.reduce((sum, l) => sum + (l.quantity * l.unitPrice), 0);
    }

    get totalLineDiscount(): number {
        return this.lines.reduce((sum, l) => sum + l.discountAmount, 0);
    }

    get afterLineDiscount(): number {
        return this.subTotal - this.totalLineDiscount;
    }

    get totalTax(): number {
        return this.lines.reduce((sum, l) => sum + l.taxAmount, 0);
    }

    get grandTotal(): number {
        return this.afterLineDiscount - this.documentDiscountAmount + this.totalTax;
    }

    ngOnInit() {
        this.itemsApiService.getAllItems().subscribe(items => {
            this.items = items;
        });
    }

    addLine() {
        const newLine: DocLine = {
            lineNumber: this.lines.length + 1,
            itemId: '',
            quantity: 1,
            unitPrice: 0,
            discountPercent: 0,
            discountAmount: 0,
            taxPercent: this.defaultTaxPercent,
            taxAmount: 0,
            lineTotal: 0
        };
        this.lines.push(newLine);
        this.emitChanges();
    }

    removeLine(index: number) {
        this.lines.splice(index, 1);
        this.renumberLines();
        this.emitChanges();
    }

    onItemSelected(line: DocLine, itemId: string) {
        const item = this.items.find(i => i.id === itemId);
        if (item) {
            line.itemName = item.nameEn;
            line.itemSku = item.sku;
            line.unitPrice = item.price;
            const itemTaxRate = (item as any).taxRate;
            if (itemTaxRate !== null && itemTaxRate !== undefined) {
                line.taxPercent = itemTaxRate;
            }
            this.recalculateLine(line);
        }
    }

    onDiscountPercentChange(line: DocLine) {
        this.recalculateLine(line);
    }

    recalculateLine(line: DocLine) {
        const lineSubtotal = line.quantity * line.unitPrice;
        line.discountAmount = line.discountPercent
            ? Math.round(lineSubtotal * (line.discountPercent / 100) * 100) / 100
            : 0;
        const afterDiscount = lineSubtotal - line.discountAmount;
        line.taxAmount = line.taxPercent
            ? Math.round(afterDiscount * (line.taxPercent / 100) * 100) / 100
            : 0;
        line.lineTotal = Math.round((afterDiscount + line.taxAmount) * 100) / 100;
        this.emitChanges();
    }

    onDocumentDiscountChange() {
        this.documentDiscountAmount = this.documentDiscountPercent
            ? Math.round(this.afterLineDiscount * (this.documentDiscountPercent / 100) * 100) / 100
            : 0;
        this.emitChanges();
    }

    private renumberLines() {
        this.lines.forEach((l, i) => l.lineNumber = i + 1);
    }

    private emitChanges() {
        this.linesChange.emit(this.lines);
        this.totalsChange.emit({
            subTotal: this.subTotal,
            discountAmount: this.totalLineDiscount + this.documentDiscountAmount,
            discountPercent: this.documentDiscountPercent || undefined,
            taxAmount: this.totalTax,
            total: this.grandTotal
        });
    }
}
