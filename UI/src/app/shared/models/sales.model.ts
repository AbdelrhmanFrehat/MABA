export interface DocumentLine {
    id?: string;
    lineNumber: number;
    itemId: string;
    itemName?: string;
    itemSku?: string;
    unitOfMeasureId?: string;
    unitOfMeasureName?: string;
    quantity: number;
    unitPrice: number;
    discountPercent?: number;
    discountAmount: number;
    taxPercent?: number;
    taxAmount: number;
    lineTotal: number;
    notes?: string;
}

export interface SalesQuotation {
    id: string;
    quotationNumber: string;
    customerId: string;
    customerName?: string;
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
    priceListId?: string;
    notes?: string;
    termsAndConditions?: string;
    salesPersonUserId?: string;
    salesPersonName?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: SalesQuotationLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface SalesQuotationLine extends DocumentLine {
    salesQuotationId?: string;
}

export interface SalesOrder {
    id: string;
    orderNumber: string;
    customerId: string;
    customerName?: string;
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
    priceListId?: string;
    shippingAddress?: string;
    notes?: string;
    approvedByUserId?: string;
    approvedByName?: string;
    approvedAt?: string;
    salesPersonUserId?: string;
    salesPersonName?: string;
    warehouseId?: string;
    createdByUserId: string;
    isStorefrontOrder?: boolean;
    sourceLabel?: string;
    lines: SalesOrderLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface SalesOrderLine extends DocumentLine {
    salesOrderId?: string;
    quantityInvoiced: number;
    quantityReturned: number;
}

export interface SalesInvoice {
    id: string;
    invoiceNumber: string;
    customerId: string;
    customerName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    salesOrderId?: string;
    salesOrderNumber?: string;
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
    pdfUrl?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: SalesInvoiceLine[];
    paymentAllocations?: PaymentAllocationDto[];
    createdAt: string;
    updatedAt?: string;
}

export interface SalesInvoiceLine extends DocumentLine {
    salesInvoiceId?: string;
    salesOrderLineId?: string;
    costPrice: number;
}

export interface SalesReturn {
    id: string;
    returnNumber: string;
    customerId: string;
    customerName?: string;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    salesInvoiceId: string;
    salesInvoiceNumber?: string;
    returnReasonLookupId?: string;
    returnReasonName?: string;
    returnDate: string;
    currency: string;
    subTotal: number;
    taxAmount: number;
    total: number;
    isPosted: boolean;
    restockItems: boolean;
    notes?: string;
    warehouseId?: string;
    createdByUserId: string;
    lines: SalesReturnLine[];
    createdAt: string;
    updatedAt?: string;
}

export interface SalesReturnLine extends DocumentLine {
    salesReturnId?: string;
    salesInvoiceLineId?: string;
}

export interface PaymentAllocationDto {
    id: string;
    paymentVoucherId: string;
    paymentVoucherNumber?: string;
    allocatedAmount: number;
    paymentDate?: string;
}

// --- Request DTOs ---

export interface CreateSalesQuotationRequest {
    customerId: string;
    quotationDate: string;
    validUntil?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    priceListId?: string;
    notes?: string;
    termsAndConditions?: string;
    salesPersonUserId?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface CreateDocumentLineRequest {
    itemId: string;
    unitOfMeasureId?: string;
    quantity: number;
    unitPrice: number;
    discountPercent?: number;
    discountAmount?: number;
    taxPercent?: number;
    notes?: string;
}

export interface CreateSalesOrderRequest {
    customerId: string;
    sourceQuotationId?: string;
    orderDate: string;
    expectedDeliveryDate?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    priceListId?: string;
    shippingAddress?: string;
    notes?: string;
    salesPersonUserId?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface ConvertToInvoiceRequest {
    invoiceDate: string;
    dueDate?: string;
    warehouseId?: string;
    notes?: string;
    lineQuantities?: { salesOrderLineId: string; quantity: number }[];
}

export interface CreateSalesInvoiceRequest {
    customerId: string;
    salesOrderId?: string;
    invoiceDate: string;
    dueDate?: string;
    currency?: string;
    discountAmount?: number;
    discountPercent?: number;
    notes?: string;
    warehouseId?: string;
    lines: CreateDocumentLineRequest[];
}

export interface CreateSalesReturnRequest {
    salesInvoiceId: string;
    returnReasonLookupId?: string;
    returnDate: string;
    restockItems?: boolean;
    notes?: string;
    warehouseId?: string;
    lines: { salesInvoiceLineId: string; quantity: number; notes?: string }[];
}
