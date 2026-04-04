import { MabaInvoiceCompany } from '../models/maba-invoice.model';

/** Default issuer block for MABA invoices (matches public site contact). */
export const MABA_INVOICE_COMPANY: MabaInvoiceCompany = {
    legalName: 'MABA Solutions',
    tagline: 'Engineering high-performance systems',
    website: 'https://maba.ps',
    email: 'mabaengsol@gmail.com',
    phone: '+970 59 859 5969',
    addressLines: ['Al-Irsal Building, Al-Ithaa Street', 'Al-Bireh, Palestine']
};

export const MABA_INVOICE_PAYMENT_INSTRUCTIONS =
    'Payment via bank transfer, card, or agreed milestone. Reference your invoice number on all transfers.';

export const MABA_INVOICE_TERMS =
    'Services are provided per agreed scope and MABA project terms. Prices exclude duties unless stated.';

export const MABA_INVOICE_FOOTER_NOTE =
    'Thank you for choosing MABA. This document is a business record issued by MABA Solutions.';
