import { InvoiceStatus } from './api-response.model';

export interface InvoiceLine {
    id?: number;
    itemId: number;
    itemNameAr?: string;
    itemNameEn?: string;
    quantity: number;
    unitPrice: number;
    lineDiscount: number;
    lineTotal?: number;
}

export interface PurchaseInvoice {
    id?: number;
    invoiceNumber: string;
    invoiceDate: string;
    supplierId: number;
    supplierNameAr?: string;
    supplierNameEn?: string;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    totalAmount?: number;
    discountAmount: number;
    netAmount?: number;
    paidAmount?: number;
    remainingAmount?: number;
    status?: InvoiceStatus;
    notes?: string;
    lines: InvoiceLine[];
}

export interface SalesInvoice {
    id?: number;
    invoiceNumber: string;
    invoiceDate: string;
    customerId: number;
    customerNameAr?: string;
    customerNameEn?: string;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    totalAmount?: number;
    discountAmount: number;
    netAmount?: number;
    paidAmount?: number;
    remainingAmount?: number;
    status?: InvoiceStatus;
    notes?: string;
    lines: InvoiceLine[];
}

export interface InvoiceListResponse<T> {
    items: T[];
    totalCount: number;
}
