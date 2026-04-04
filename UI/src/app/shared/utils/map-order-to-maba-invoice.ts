import { Order, PaymentStatus } from '../models/order.model';
import { MabaInvoiceDocument, MabaInvoiceLineItem, MabaInvoiceStatus } from '../models/maba-invoice.model';
import {
    MABA_INVOICE_COMPANY,
    MABA_INVOICE_FOOTER_NOTE,
    MABA_INVOICE_PAYMENT_INSTRUCTIONS,
    MABA_INVOICE_TERMS
} from '../constants/maba-invoice-brand';

/** Accepts storefront Order or admin-transformed order payload. */
function paymentStatusToInvoice(order: Order | Record<string, unknown>): { status: MabaInvoiceStatus; label: string } {
    const r = order as Record<string, unknown>;
    const raw = r['displayPaymentStatus'] ?? r['paymentStatus'];
    const ps = String(raw ?? '');
    const s = ps.toLowerCase();
    if (s === PaymentStatus.Paid.toLowerCase() || s === 'paid') return { status: 'paid', label: 'Paid' };
    if (s === PaymentStatus.PartiallyPaid.toLowerCase() || s === 'partiallypaid')
        return { status: 'partial', label: 'Partial' };
    if (s === PaymentStatus.Refunded.toLowerCase() || s === 'refunded')
        return { status: 'refunded', label: 'Refunded' };
    if (s === PaymentStatus.Failed.toLowerCase() || s === 'failed') return { status: 'unpaid', label: 'Unpaid' };
    return { status: 'unpaid', label: 'Unpaid' };
}

function formatAddressLines(order: Order | Record<string, unknown>): string[] {
    const a = (order as Order).shippingAddress as Order['shippingAddress'] | undefined;
    if (!a) return [];
    const lines: string[] = [];
    if (a.fullName) lines.push(a.fullName);
    const line1 = [a.addressLine1, a.addressLine2].filter(Boolean).join(', ');
    if (line1) lines.push(line1);
    const cityLine = [a.city, a.state, a.postalCode].filter(Boolean).join(', ');
    if (cityLine) lines.push(cityLine);
    if (a.country) lines.push(a.country);
    return lines;
}

/**
 * Maps a storefront Order (or admin order detail payload) into a printable MABA invoice document.
 */
export function mapOrderToMabaInvoice(order: Order | Record<string, unknown>, lang: 'en' | 'ar' = 'en'): MabaInvoiceDocument {
    const o = order as Record<string, unknown>;
    const { status, label } = paymentStatusToInvoice(order);
    const createdAt = String(o['createdAt'] ?? new Date().toISOString());
    const issue = new Date(createdAt);
    const due = new Date(issue);
    due.setDate(due.getDate() + 14);

    const rawItems = (o['displayOrderItems'] ?? o['items'] ?? o['orderItems'] ?? []) as unknown[];
    const lineItems: MabaInvoiceLineItem[] = rawItems.map((raw) => {
        const it = raw as Record<string, unknown>;
        return {
        title:
            lang === 'ar'
                ? String(it['itemNameAr'] ?? it['nameAr'] ?? it['itemNameEn'] ?? it['nameEn'] ?? '')
                : String(it['itemNameEn'] ?? it['nameEn'] ?? it['itemNameAr'] ?? it['nameAr'] ?? ''),
        description: (it['itemSku'] || it['sku']) ? `SKU: ${it['itemSku'] ?? it['sku']}` : undefined,
        quantity: Number(it['quantity']) || 0,
        unitPrice: Number(it['unitPrice']) || 0,
        lineTotal:
            Number(
                it['total'] ??
                    it['lineTotal'] ??
                    (Number(it['unitPrice']) || 0) * (Number(it['quantity']) || 0)
            ) || 0
        };
    });

    const ship = (o['displayShippingAddress'] ?? o['shippingAddress']) as Order['shippingAddress'] | undefined;
    const clientName = String(
        ship?.fullName ?? o['displayCustomerName'] ?? o['customerName'] ?? '—'
    );
    const clientEmail = String(o['customerEmail'] ?? '—');
    const clientPhone = String(o['customerPhone'] ?? ship?.phone ?? '');
    const addrLines = formatAddressLines({ ...(order as Order), shippingAddress: ship } as Order);

    const notesArr = (o['notes'] ?? []) as Order['notes'];
    const notesPublic = notesArr
        .filter((n) => !n.isInternal)
        .map((n) => n.note)
        .filter(Boolean)
        .join('\n');

    const orderNumber = String(o['displayOrderNumber'] ?? o['orderNumber'] ?? o['id'] ?? '—');
    const subtotal = Number(o['displaySubtotal'] ?? o['subtotal'] ?? o['subTotal'] ?? 0);
    const tax = Number(o['displayTaxAmount'] ?? o['taxAmount'] ?? 0);
    const shipping = Number(o['displayShippingAmount'] ?? o['shippingAmount'] ?? o['shippingCost'] ?? 0);
    const discount = Number(o['displayDiscountAmount'] ?? o['discountAmount'] ?? 0);
    const total = Number(o['displayTotal'] ?? o['totalAmount'] ?? o['total'] ?? 0);
    const currency = String(o['currency'] ?? 'ILS');

    return {
        invoiceNumber: `INV-${orderNumber}`,
        issueDate: createdAt,
        dueDate: due.toISOString(),
        status,
        statusLabel: label,
        company: { ...MABA_INVOICE_COMPANY },
        client: {
            fullName: clientName,
            companyName: undefined,
            email: clientEmail,
            phone: clientPhone || undefined,
            addressLines: addrLines.length ? addrLines : ['—']
        },
        project: {
            title: lang === 'ar' ? 'طلب المتجر' : 'Store order',
            reference: orderNumber,
            description:
                lang === 'ar'
                    ? `مرجع الطلب والتسليم حسب عنوان الشحن.`
                    : `Order reference and fulfillment per shipping address.`
        },
        lineItems,
        totals: {
            subtotal,
            discount,
            tax,
            shipping,
            grandTotal: total,
            currency
        },
        paymentInstructions: MABA_INVOICE_PAYMENT_INSTRUCTIONS,
        notes: notesPublic || undefined,
        terms: MABA_INVOICE_TERMS,
        footerNote: MABA_INVOICE_FOOTER_NOTE
    };
}
