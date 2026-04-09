import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { SalesApiService } from '../../services/sales-api.service';
import {
    RequestCommercialLinks,
    RequestCommercialDraft,
    CreateQuotationFromRequestRequest,
    QuotationItemRequest
} from '../../models/sales.model';
import { AllRequestType } from '../../models/all-requests.model';

@Component({
    selector: 'app-request-commercial-panel',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        ButtonModule,
        CardModule,
        DialogModule,
        InputNumberModule,
        InputTextModule,
        TagModule,
        ToastModule,
        TooltipModule,
        TableModule,
        DatePickerModule,
        TextareaModule
    ],
    providers: [MessageService],
    templateUrl: './request-commercial-panel.component.html'
})
export class RequestCommercialPanelComponent implements OnInit {
    @Input() requestType!: AllRequestType;
    @Input() requestId!: string;
    @Input() linkedQuotationId?: string | null;

    private salesApi = inject(SalesApiService);
    private router = inject(Router);
    private messageService = inject(MessageService);

    loading = signal(false);
    creating = signal(false);
    showCreateDialog = signal(false);

    links = signal<RequestCommercialLinks | null>(null);
    draft = signal<RequestCommercialDraft | null>(null);

    // Create quotation form state
    validUntil: Date | null = null;
    notes = '';
    items: QuotationItemRequest[] = [];

    ngOnInit(): void {
        this.loadLinks();
    }

    loadLinks(): void {
        if (!this.requestType || !this.requestId) return;
        this.loading.set(true);
        this.salesApi.getCommercialLinks(this.requestType, this.requestId).subscribe({
            next: data => { this.links.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    openCreateDialog(): void {
        this.salesApi.getCommercialDraft(this.requestType, this.requestId).subscribe({
            next: data => {
                this.draft.set(data);
                this.items = data.defaultItems.map(i => ({
                    description: i.description,
                    quantity: i.quantity,
                    unit: i.unit,
                    unitPrice: i.unitPrice,
                    discountPercent: 0,
                    taxPercent: 17
                }));
                this.notes = data.notes ?? '';
                this.validUntil = null;
                this.showCreateDialog.set(true);
            }
        });
    }

    addItem(): void {
        this.items = [...this.items, { description: '', quantity: 1, unit: 'pcs', unitPrice: 0, discountPercent: 0, taxPercent: 17 }];
    }

    removeItem(index: number): void {
        this.items = this.items.filter((_, i) => i !== index);
    }

    submitCreate(): void {
        if (this.items.length === 0) return;
        this.creating.set(true);

        const payload: CreateQuotationFromRequestRequest = {
            requestType: this.requestType,
            requestId: this.requestId,
            validUntil: this.validUntil ? this.validUntil.toISOString() : null,
            notes: this.notes || null,
            items: this.items
        };

        this.salesApi.createQuotationFromRequest(payload).subscribe({
            next: quotation => {
                this.creating.set(false);
                this.showCreateDialog.set(false);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Quotation created successfully.' });
                this.loadLinks();
            },
            error: err => {
                this.creating.set(false);
                const msg = err?.error?.message ?? 'Failed to create quotation.';
                this.messageService.add({ severity: 'error', summary: 'Error', detail: msg });
            }
        });
    }

    navigateToQuotation(id: string): void {
        this.router.navigate(['/admin/erp/sales/quotations', id]);
    }

    navigateToOrder(id: string): void {
        this.router.navigate(['/admin/erp/sales/orders', id]);
    }

    navigateToInvoice(id: string): void {
        this.router.navigate(['/admin/erp/sales/invoices', id]);
    }

    get hasLinkedDocuments(): boolean {
        const l = this.links();
        return !!l && (l.quotations.length > 0 || l.orders.length > 0 || l.invoices.length > 0);
    }

    get canCreateQuotation(): boolean {
        const l = this.links();
        if (!l) return false;
        return l.canCreateQuotation;
    }

    get blockedReason(): string | null | undefined {
        return this.links()?.blockedReason;
    }
}
