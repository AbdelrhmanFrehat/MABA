import { DocumentLine, CreateDocumentLineRequest } from './sales.model';

export interface PurchaseQuotation {
    id: string;
    quotationNumber: string;
    supplierId: string;
    supplierName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    quotationDate: string;
    validUntil?: string;
    currency: string;
    subTotal: number;
    discountAmount: number;
    discountPercent?: number;
    taxAmount: number;
    total: number;
    notes?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: PurchaseQuotationLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface PurchaseQuotationLine extends DocumentLine {
    purchaseQuotationId?: string;
}

export interface PurchaseOrder {
    id: string;
    orderNumber: string;
    supplierId: string;
    supplierName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    sourceQuotationId?: string;
    sourceQuotationNumber?: string;
    orderDate: string;
    expectedDeliveryDate?: string;
    currency: string;
    subTotal: number;
    discountAmount: number;
    discountPercent?: number;
    taxAmount: number;
    total: number;
    notes?: string;
    approvedByUserId?: string;
    approvedByName?: string;
    approvedAt?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: PurchaseOrderLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface PurchaseOrderLine extends DocumentLine {
    purchaseOrderId?: string;
    quantityReceived: number;
    quantityReturned: number;
}

export interface PurchaseInvoice {
    id: string;
    invoiceNumber: string;
    supplierId: string;
    supplierName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    purchaseOrderId?: string;
    purchaseOrderNumber?: string;
    supplierInvoiceNumber?: string;
    invoiceDate: string;
    dueDate?: string;
    currency: string;
    subTotal: number;
    discountAmount: number;
    discountPercent?: number;
    taxAmount: number;
    total: number;
    amountPaid: number;
    amountDue: number;
    isPosted: boolean;
    journalEntryId?: string;
    notes?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: PurchaseInvoiceLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface PurchaseInvoiceLine extends DocumentLine {
    purchaseInvoiceId?: string;
    purchaseOrderLineId?: string;
}

export interface PurchaseReturn {
    id: string;
    returnNumber: string;
    supplierId: string;
    supplierName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    purchaseInvoiceId: string;
    purchaseInvoiceNumber?: string;
    returnReasonLookupId?: string;
    returnReasonName?: string;
    returnDate: string;
    currency: string;
    subTotal: number;
    taxAmount: number;
    total: number;
    isPosted: boolean;
    deductFromInventory: boolean;
    notes?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: PurchaseReturnLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface PurchaseReturnLine extends DocumentLine {
    purchaseReturnId?: string;
    purchaseInvoiceLineId?: string;
}

// --- Request DTOs ---

export interface CreatePurchaseQuotationRequest {
    supplierId: string;
    quotationDate: string;
    validUntil?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    notes?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface CreatePurchaseOrderRequest {
    supplierId: string;
    sourceQuotationId?: string;
    orderDate: string;
    expectedDeliveryDate?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    notes?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface CreatePurchaseInvoiceRequest {
    supplierId: string;
    purchaseOrderId?: string;
    supplierInvoiceNumber?: string;
    invoiceDate: string;
    dueDate?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    notes?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface ConvertPurchaseOrderToInvoiceRequest {
    invoiceDate: string;
    dueDate?: string;
    supplierInvoiceNumber?: string;
    warehouseId?: string;
    notes?: string;
    lineQuantities?: { purchaseOrderLineId: string; quantity: number }[];
}

export interface CreatePurchaseReturnRequest {
    purchaseInvoiceId: string;
    returnReasonLookupId?: string;
    returnDate: string;
    deductFromInventory?: boolean;
    notes?: string;
    warehouseId?: string;
    lines: { purchaseInvoiceLineId: string; quantity: number; notes?: string }[];
}
