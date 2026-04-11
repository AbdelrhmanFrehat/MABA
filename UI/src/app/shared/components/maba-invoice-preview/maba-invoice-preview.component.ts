import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MabaInvoiceDocument } from '../../models/maba-invoice.model';

@Component({
    selector: 'app-maba-invoice-preview',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="inv" *ngIf="invoice as doc">

            <!-- ── HEADER ──────────────────────────────────── -->
            <header class="inv-header">
                <div class="inv-brand">
                    <img
                        class="inv-logo"
                        src="assets/images/maba-hero-logo.png"
                        width="2048"
                        height="1364"
                        alt="MABA" />
                    <div class="inv-company-text">
                        <div class="inv-legal">{{ doc.company.legalName }}</div>
                        <div class="inv-tagline" *ngIf="doc.company.tagline">{{ doc.company.tagline }}</div>
                    </div>
                </div>
                <div class="inv-meta">
                    <h1 class="inv-title">INVOICE</h1>
                    <div class="inv-meta-row"><span class="k">Invoice #</span><span class="v">{{ doc.invoiceNumber }}</span></div>
                    <div class="inv-meta-row"><span class="k">Issue date</span><span class="v">{{ formatDate(doc.issueDate) }}</span></div>
                    <div class="inv-meta-row" *ngIf="doc.dueDate"><span class="k">Due date</span><span class="v">{{ formatDate(doc.dueDate) }}</span></div>
                    <div class="inv-badge" [attr.data-status]="doc.status">{{ doc.statusLabel }}</div>
                </div>
            </header>

            <div class="inv-rule"></div>

            <!-- ── FROM / BILL TO ──────────────────────────── -->
            <section class="inv-two">
                <div class="inv-card">
                    <h2>From</h2>
                    <p class="strong">{{ doc.company.legalName }}</p>
                    <p *ngIf="doc.company.tagline" class="sub">{{ doc.company.tagline }}</p>
                    <p *ngIf="doc.company.website">{{ doc.company.website }}</p>
                    <p>{{ doc.company.email }}</p>
                    <p *ngIf="doc.company.phone">{{ doc.company.phone }}</p>
                    <p *ngFor="let line of doc.company.addressLines">{{ line }}</p>
                </div>
                <div class="inv-card">
                    <h2>Bill to</h2>
                    <p class="strong">{{ doc.client.fullName }}</p>
                    <p *ngIf="doc.client.companyName" class="sub">{{ doc.client.companyName }}</p>
                    <p>{{ doc.client.email }}</p>
                    <p *ngIf="doc.client.phone">{{ doc.client.phone }}</p>
                    <p *ngFor="let line of doc.client.addressLines">{{ line }}</p>
                </div>
            </section>

            <!-- ── REFERENCE ───────────────────────────────── -->
            <section class="inv-ref" *ngIf="doc.project.title || doc.project.reference">
                <h2>Reference</h2>
                <div class="inv-ref-inner">
                    <span class="inv-ref-title" *ngIf="doc.project.title">{{ doc.project.title }}</span>
                    <span class="inv-ref-sep" *ngIf="doc.project.title && doc.project.reference"> &middot; </span>
                    <span class="inv-ref-num" *ngIf="doc.project.reference">{{ doc.project.reference }}</span>
                </div>
            </section>

            <!-- ── LINE ITEMS ──────────────────────────────── -->
            <section class="inv-table-wrap">
                <table class="inv-table">
                    <thead>
                        <tr>
                            <th>Item / Service</th>
                            <th>Description</th>
                            <th class="center">Qty</th>
                            <th class="right">Unit price</th>
                            <th class="right">Line total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr *ngFor="let row of doc.lineItems">
                            <td class="item-name">{{ row.title }}</td>
                            <td class="muted">{{ row.description || '—' }}</td>
                            <td class="center">{{ row.quantity }}</td>
                            <td class="right mono">{{ formatMoney(row.unitPrice, doc.totals.currency) }}</td>
                            <td class="right mono">{{ formatMoney(row.lineTotal, doc.totals.currency) }}</td>
                        </tr>
                        <tr *ngIf="doc.lineItems.length === 0">
                            <td colspan="5" class="muted center empty">No line items</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <!-- ── TOTALS ──────────────────────────────────── -->
            <section class="inv-totals">
                <div class="inv-totals-inner">
                    <div class="tot-row">
                        <span class="tot-label">Subtotal</span>
                        <span class="tot-value mono">{{ formatMoney(doc.totals.subtotal, doc.totals.currency) }}</span>
                    </div>
                    <div class="tot-row" *ngIf="doc.totals.discount > 0">
                        <span class="tot-label">Discount</span>
                        <span class="tot-value mono discount">− {{ formatMoney(doc.totals.discount, doc.totals.currency) }}</span>
                    </div>
                    <div class="tot-row">
                        <span class="tot-label">Tax / VAT</span>
                        <span class="tot-value mono">{{ formatMoney(doc.totals.tax, doc.totals.currency) }}</span>
                    </div>
                    <div class="tot-row">
                        <span class="tot-label">Shipping</span>
                        <span class="tot-value mono">{{ formatMoney(doc.totals.shipping, doc.totals.currency) }}</span>
                    </div>
                    <div class="tot-row grand">
                        <span class="tot-label">Grand total</span>
                        <span class="tot-value mono">{{ formatMoney(doc.totals.grandTotal, doc.totals.currency) }}</span>
                    </div>
                </div>
            </section>

            <!-- ── PAYMENT & TERMS ─────────────────────────── -->
            <section class="inv-notes" *ngIf="doc.paymentInstructions || doc.notes || doc.terms">
                <h2>Payment &amp; Terms</h2>
                <p *ngIf="doc.paymentInstructions">{{ doc.paymentInstructions }}</p>
                <div *ngIf="doc.notes" class="inv-notes-block">
                    <span class="notes-label">Notes</span>
                    <p class="preserve">{{ doc.notes }}</p>
                </div>
                <div *ngIf="doc.terms" class="inv-notes-block">
                    <span class="notes-label">Terms &amp; Conditions</span>
                    <p class="terms">{{ doc.terms }}</p>
                </div>
            </section>

            <!-- ── FOOTER ──────────────────────────────────── -->
            <footer class="inv-footer">
                <div class="inv-footer-rule"></div>
                <p>This is an official document issued by MABA Solutions.</p>
            </footer>

        </div>
    `,
    styles: [
        `
            /* ── Base ─────────────────────────────────────────── */
            .inv {
                font-family: 'Inter', system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                color: #1a1d26;
                background: #fff;
                max-width: 880px;
                margin: 0 auto;
                padding: 2.5rem 2.75rem 3rem;
                box-sizing: border-box;
                font-size: 0.875rem;
                line-height: 1.55;
            }

            /* ── Header ───────────────────────────────────────── */
            .inv-header {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                gap: 2rem;
                flex-wrap: wrap;
            }
            .inv-brand {
                display: flex;
                align-items: center;
                gap: 0.875rem;
                min-width: 0;
            }
            .inv-logo {
                height: 52px;
                width: auto;
                max-width: min(220px, 100%);
                object-fit: contain;
                display: block;
            }
            .inv-legal {
                font-weight: 800;
                font-size: 1.1rem;
                color: #0c1445;
                letter-spacing: -0.02em;
            }
            .inv-tagline {
                font-size: 0.775rem;
                color: #6b7280;
                margin-top: 0.15rem;
            }

            /* ── Meta (right side) ────────────────────────────── */
            .inv-meta {
                text-align: right;
                min-width: 210px;
            }
            .inv-title {
                margin: 0 0 0.875rem;
                font-size: 2rem;
                font-weight: 900;
                color: #0c1445;
                letter-spacing: 0.06em;
                line-height: 1;
            }
            .inv-meta-row {
                display: flex;
                justify-content: flex-end;
                gap: 1rem;
                font-size: 0.8125rem;
                margin-bottom: 0.3rem;
            }
            .inv-meta-row .k {
                color: #9ca3af;
                min-width: 5.5rem;
                text-align: right;
            }
            .inv-meta-row .v {
                font-weight: 600;
                color: #111827;
                min-width: 8rem;
                text-align: right;
            }

            /* ── Status badge ─────────────────────────────────── */
            .inv-badge {
                display: inline-block;
                margin-top: 0.75rem;
                padding: 0.3rem 0.9rem;
                border-radius: 4px;
                font-size: 0.6875rem;
                font-weight: 700;
                letter-spacing: 0.1em;
                text-transform: uppercase;
                background: #1e40af;
                color: #fff;
            }
            .inv-badge[data-status="paid"]         { background: #166534; color: #fff; }
            .inv-badge[data-status="Paid"]          { background: #166534; color: #fff; }
            .inv-badge[data-status="unpaid"]        { background: #1e40af; color: #fff; }
            .inv-badge[data-status="Unpaid"]        { background: #1e40af; color: #fff; }
            .inv-badge[data-status="overdue"]       { background: #991b1b; color: #fff; }
            .inv-badge[data-status="Overdue"]       { background: #991b1b; color: #fff; }
            .inv-badge[data-status="partial"]       { background: #92400e; color: #fff; }
            .inv-badge[data-status="PartiallyPaid"] { background: #92400e; color: #fff; }
            .inv-badge[data-status="draft"]         { background: #374151; color: #fff; }
            .inv-badge[data-status="Draft"]         { background: #374151; color: #fff; }
            .inv-badge[data-status="cancelled"]     { background: #6b7280; color: #fff; }
            .inv-badge[data-status="Cancelled"]     { background: #6b7280; color: #fff; }

            /* ── Divider ──────────────────────────────────────── */
            .inv-rule {
                height: 2px;
                margin: 1.5rem 0 1.75rem;
                background: #0c1445;
                border-radius: 1px;
            }

            /* ── Two-column section ───────────────────────────── */
            .inv-two {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 1rem;
            }
            @media (max-width: 640px) {
                .inv-two { grid-template-columns: 1fr; }
                .inv-meta { text-align: left; }
                .inv-meta-row { justify-content: flex-start; }
            }
            .inv-card {
                background: #f9fafb;
                border: 1px solid #e5e7eb;
                border-radius: 6px;
                padding: 1rem 1.125rem;
                font-size: 0.8125rem;
                line-height: 1.6;
            }
            .inv-card h2 {
                margin: 0 0 0.5rem;
                font-size: 0.6875rem;
                font-weight: 700;
                letter-spacing: 0.1em;
                text-transform: uppercase;
                color: #6b7280;
                border-bottom: 1px solid #e5e7eb;
                padding-bottom: 0.4rem;
            }
            .inv-card p {
                margin: 0.125rem 0;
                color: #374151;
            }
            .inv-card .strong {
                font-weight: 700;
                font-size: 0.9rem;
                color: #0c1445;
            }
            .inv-card .sub {
                color: #6b7280;
                font-size: 0.775rem;
            }

            /* ── Reference ────────────────────────────────────── */
            .inv-ref {
                margin-top: 1.25rem;
            }
            .inv-ref h2 {
                margin: 0 0 0.4rem;
                font-size: 0.6875rem;
                font-weight: 700;
                letter-spacing: 0.1em;
                text-transform: uppercase;
                color: #6b7280;
            }
            .inv-ref-inner {
                background: #f9fafb;
                border: 1px solid #e5e7eb;
                border-radius: 6px;
                padding: 0.625rem 1rem;
                font-size: 0.8125rem;
            }
            .inv-ref-title {
                font-weight: 600;
                color: #0c1445;
            }
            .inv-ref-sep {
                color: #9ca3af;
                margin: 0 0.25rem;
            }
            .inv-ref-num {
                color: #374151;
            }

            /* ── Table ────────────────────────────────────────── */
            .inv-table-wrap {
                margin-top: 1.5rem;
                overflow: auto;
                border: 1px solid #e5e7eb;
                border-radius: 6px;
            }
            .inv-table {
                width: 100%;
                border-collapse: collapse;
                font-size: 0.8125rem;
            }
            .inv-table thead tr {
                background: #0c1445;
            }
            .inv-table th {
                text-align: left;
                padding: 0.75rem 0.875rem;
                color: #e5e7eb;
                font-weight: 600;
                font-size: 0.75rem;
                letter-spacing: 0.04em;
                text-transform: uppercase;
                white-space: nowrap;
            }
            .inv-table td {
                padding: 0.65rem 0.875rem;
                border-bottom: 1px solid #f3f4f6;
                vertical-align: top;
                color: #374151;
            }
            .inv-table tbody tr:last-child td {
                border-bottom: none;
            }
            .inv-table tbody tr:nth-child(even) td {
                background: #fafafa;
            }
            .inv-table .center,
            .inv-table th.center {
                text-align: center;
            }
            .inv-table .right,
            .inv-table th.right {
                text-align: right;
            }
            .item-name {
                font-weight: 600;
                color: #0c1445;
            }
            .muted { color: #9ca3af; }
            .empty { padding: 1.5rem 0.875rem; }
            .mono  { font-variant-numeric: tabular-nums; }

            /* ── Totals ───────────────────────────────────────── */
            .inv-totals {
                display: flex;
                justify-content: flex-end;
                margin-top: 1.25rem;
            }
            .inv-totals-inner {
                min-width: 300px;
                max-width: 100%;
                border: 1px solid #e5e7eb;
                border-radius: 6px;
                padding: 0.125rem 0;
                background: #f9fafb;
            }
            .tot-row {
                display: flex;
                justify-content: space-between;
                align-items: center;
                gap: 2rem;
                padding: 0.45rem 1rem;
                font-size: 0.8125rem;
                color: #374151;
            }
            .tot-row + .tot-row {
                border-top: 1px solid #f3f4f6;
            }
            .tot-label {
                color: #6b7280;
            }
            .tot-value {
                font-weight: 500;
                color: #111827;
                text-align: right;
            }
            .tot-value.discount {
                color: #166534;
            }
            .tot-row.grand {
                background: #0c1445;
                border-top: none !important;
                border-radius: 0 0 5px 5px;
                margin-top: 0;
            }
            .tot-row.grand .tot-label {
                font-weight: 700;
                font-size: 0.875rem;
                color: #93c5fd;
                letter-spacing: 0.04em;
                text-transform: uppercase;
            }
            .tot-row.grand .tot-value {
                font-weight: 800;
                font-size: 1.2rem;
                color: #fff;
                letter-spacing: -0.01em;
            }

            /* ── Notes / Payment / Terms ──────────────────────── */
            .inv-notes {
                margin-top: 1.75rem;
                padding-top: 1.25rem;
                border-top: 1px solid #e5e7eb;
                font-size: 0.8125rem;
                line-height: 1.6;
                color: #374151;
            }
            .inv-notes h2 {
                margin: 0 0 0.75rem;
                font-size: 0.75rem;
                font-weight: 700;
                letter-spacing: 0.1em;
                text-transform: uppercase;
                color: #6b7280;
            }
            .inv-notes > p {
                margin: 0 0 0.75rem;
                color: #374151;
            }
            .inv-notes-block {
                margin-top: 0.75rem;
            }
            .notes-label {
                display: block;
                font-size: 0.75rem;
                font-weight: 600;
                color: #9ca3af;
                text-transform: uppercase;
                letter-spacing: 0.08em;
                margin-bottom: 0.25rem;
            }
            .preserve { white-space: pre-wrap; }
            .terms    { color: #6b7280; font-size: 0.8rem; }

            /* ── Footer ───────────────────────────────────────── */
            .inv-footer {
                margin-top: 2.5rem;
                text-align: center;
                font-size: 0.75rem;
                color: #9ca3af;
            }
            .inv-footer-rule {
                height: 1px;
                background: #e5e7eb;
                margin-bottom: 0.75rem;
            }
            .inv-footer p { margin: 0; }

            /* ── Print ────────────────────────────────────────── */
            @media print {
                .inv {
                    max-width: none;
                    padding: 1.25cm 1.5cm;
                }
                .inv-table-wrap,
                .inv-card,
                .inv-ref-inner,
                .inv-totals-inner {
                    break-inside: avoid;
                }
            }
        `
    ]
})
export class MabaInvoicePreviewComponent {
    @Input() invoice: MabaInvoiceDocument | null = null;

    formatDate(iso: string): string {
        try {
            return new Date(iso).toLocaleDateString('en-GB', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch {
            return iso;
        }
    }

    formatMoney(amount: number, currency: string): string {
        try {
            return new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency,
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).format(amount);
        } catch {
            return `${amount.toFixed(2)} ${currency}`;
        }
    }
}
