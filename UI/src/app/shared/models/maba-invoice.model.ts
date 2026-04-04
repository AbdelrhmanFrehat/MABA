/** Structured invoice document for preview + PDF (MABA branding). */

export type MabaInvoiceStatus = 'paid' | 'unpaid' | 'partial' | 'quote' | 'proforma' | 'refunded';

export interface MabaInvoiceCompany {
    legalName: string;
    tagline?: string;
    website: string;
    email: string;
    phone: string;
    addressLines: string[];
}

export interface MabaInvoiceClient {
    fullName: string;
    companyName?: string;
    email: string;
    phone?: string;
    addressLines: string[];
}

export interface MabaInvoiceProject {
    title: string;
    reference: string;
    description?: string;
}

export interface MabaInvoiceLineItem {
    title: string;
    description?: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
}

export interface MabaInvoiceTotals {
    subtotal: number;
    discount: number;
    tax: number;
    shipping: number;
    grandTotal: number;
    currency: string;
}

export interface MabaInvoiceDocument {
    invoiceNumber: string;
    issueDate: string;
    dueDate?: string;
    status: MabaInvoiceStatus;
    statusLabel: string;
    company: MabaInvoiceCompany;
    client: MabaInvoiceClient;
    project: MabaInvoiceProject;
    lineItems: MabaInvoiceLineItem[];
    totals: MabaInvoiceTotals;
    paymentInstructions?: string;
    notes?: string;
    terms?: string;
    footerNote?: string;
}
