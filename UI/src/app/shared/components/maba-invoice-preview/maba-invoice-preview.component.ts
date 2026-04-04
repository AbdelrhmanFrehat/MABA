import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MabaInvoiceDocument } from '../../models/maba-invoice.model';

@Component({
    selector: 'app-maba-invoice-preview',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="inv" *ngIf="invoice as doc">
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
                    <h1 class="inv-title">Invoice</h1>
                    <div class="inv-meta-row"><span class="k">Invoice #</span><span class="v">{{ doc.invoiceNumber }}</span></div>
                    <div class="inv-meta-row"><span class="k">Issue date</span><span class="v">{{ formatDate(doc.issueDate) }}</span></div>
                    <div class="inv-meta-row" *ngIf="doc.dueDate"><span class="k">Due date</span><span class="v">{{ formatDate(doc.dueDate) }}</span></div>
                    <div class="inv-badge" [attr.data-status]="doc.status">{{ doc.statusLabel }}</div>
                </div>
            </header>

            <div class="inv-rule"></div>

            <section class="inv-two">
                <div class="inv-card">
                    <h2>From</h2>
                    <p class="strong">{{ doc.company.legalName }}</p>
                    <p *ngIf="doc.company.tagline">{{ doc.company.tagline }}</p>
                    <p>{{ doc.company.website }}</p>
                    <p>{{ doc.company.email }}</p>
                    <p>{{ doc.company.phone }}</p>
                    <p *ngFor="let line of doc.company.addressLines">{{ line }}</p>
                </div>
                <div class="inv-card">
                    <h2>Bill to</h2>
                    <p class="strong">{{ doc.client.fullName }}</p>
                    <p *ngIf="doc.client.companyName">{{ doc.client.companyName }}</p>
                    <p>{{ doc.client.email }}</p>
                    <p *ngIf="doc.client.phone">{{ doc.client.phone }}</p>
                    <p *ngFor="let line of doc.client.addressLines">{{ line }}</p>
                </div>
            </section>

            <section class="inv-project">
                <h2>Project</h2>
                <div class="inv-project-inner">
                    <div class="inv-project-title">{{ doc.project.title }}</div>
                    <div class="inv-project-ref">Reference: {{ doc.project.reference }}</div>
                    <p class="inv-project-desc" *ngIf="doc.project.description">{{ doc.project.description }}</p>
                </div>
            </section>

            <section class="inv-table-wrap">
                <table class="inv-table">
                    <thead>
                        <tr>
                            <th>Item / Service</th>
                            <th>Description</th>
                            <th class="num">Qty</th>
                            <th class="num">Unit price</th>
                            <th class="num">Line total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr *ngFor="let row of doc.lineItems">
                            <td>{{ row.title }}</td>
                            <td class="muted">{{ row.description || '—' }}</td>
                            <td class="num">{{ row.quantity }}</td>
                            <td class="num">{{ formatMoney(row.unitPrice, doc.totals.currency) }}</td>
                            <td class="num">{{ formatMoney(row.lineTotal, doc.totals.currency) }}</td>
                        </tr>
                        <tr *ngIf="doc.lineItems.length === 0">
                            <td colspan="5" class="muted center">No line items</td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <section class="inv-totals">
                <div class="inv-totals-inner">
                    <div class="row"><span>Subtotal</span><span>{{ formatMoney(doc.totals.subtotal, doc.totals.currency) }}</span></div>
                    <div class="row" *ngIf="doc.totals.discount > 0">
                        <span>Discount</span><span>− {{ formatMoney(doc.totals.discount, doc.totals.currency) }}</span>
                    </div>
                    <div class="row"><span>Tax / VAT</span><span>{{ formatMoney(doc.totals.tax, doc.totals.currency) }}</span></div>
                    <div class="row"><span>Shipping</span><span>{{ formatMoney(doc.totals.shipping, doc.totals.currency) }}</span></div>
                    <div class="row grand"><span>Grand total</span><span>{{ formatMoney(doc.totals.grandTotal, doc.totals.currency) }}</span></div>
                    <div class="curr">Currency: {{ doc.totals.currency }}</div>
                </div>
            </section>

            <section class="inv-notes" *ngIf="doc.paymentInstructions || doc.notes || doc.terms">
                <h2>Payment &amp; notes</h2>
                <p *ngIf="doc.paymentInstructions">{{ doc.paymentInstructions }}</p>
                <h3 *ngIf="doc.notes">Notes</h3>
                <p *ngIf="doc.notes" class="preserve">{{ doc.notes }}</p>
                <h3 *ngIf="doc.terms">Terms</h3>
                <p *ngIf="doc.terms" class="terms">{{ doc.terms }}</p>
            </section>

            <footer class="inv-footer">
                <div class="inv-footer-rule"></div>
                <p>{{ doc.footerNote }}</p>
            </footer>
        </div>
    `,
    styles: [
        `
            .inv {
                font-family: system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                color: #1a1d26;
                background: #fff;
                max-width: 880px;
                margin: 0 auto;
                padding: 1.5rem 1.75rem 2rem;
                box-sizing: border-box;
            }
            .inv-header {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                gap: 1.5rem;
                flex-wrap: wrap;
            }
            .inv-brand {
                display: flex;
                align-items: center;
                gap: 1rem;
                min-width: 0;
            }
            .inv-logo {
                height: 56px;
                width: auto;
                max-width: min(240px, 100%);
                object-fit: contain;
                display: block;
            }
            .inv-legal {
                font-weight: 800;
                font-size: 1.15rem;
                color: #0c1445;
                letter-spacing: -0.02em;
            }
            .inv-tagline {
                font-size: 0.8rem;
                color: #5b6478;
                margin-top: 0.15rem;
            }
            .inv-meta {
                text-align: right;
                min-width: 200px;
            }
            .inv-title {
                margin: 0 0 0.75rem;
                font-size: 1.75rem;
                font-weight: 800;
                color: #0c1445;
                letter-spacing: -0.03em;
            }
            .inv-meta-row {
                display: flex;
                justify-content: flex-end;
                gap: 0.75rem;
                font-size: 0.875rem;
                margin-bottom: 0.35rem;
            }
            .inv-meta-row .k {
                color: #6b7280;
            }
            .inv-meta-row .v {
                font-weight: 600;
                color: #111827;
                min-width: 8rem;
                text-align: right;
            }
            .inv-badge {
                display: inline-block;
                margin-top: 0.65rem;
                padding: 0.25rem 0.75rem;
                border-radius: 999px;
                font-size: 0.7rem;
                font-weight: 700;
                letter-spacing: 0.06em;
                text-transform: uppercase;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: #fff;
            }
            .inv-rule {
                height: 3px;
                margin: 1.25rem 0 1.5rem;
                border-radius: 2px;
                background: linear-gradient(90deg, #667eea 0%, #764ba2 55%, rgba(102, 126, 234, 0.15) 100%);
            }
            .inv-two {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 1.25rem;
            }
            @media (max-width: 640px) {
                .inv-two {
                    grid-template-columns: 1fr;
                }
                .inv-meta {
                    text-align: left;
                }
                .inv-meta-row {
                    justify-content: flex-start;
                }
            }
            .inv-card {
                background: #f8f9fc;
                border: 1px solid #e8eaf2;
                border-radius: 12px;
                padding: 1rem 1.1rem;
                font-size: 0.875rem;
                line-height: 1.5;
            }
            .inv-card h2 {
                margin: 0 0 0.6rem;
                font-size: 0.7rem;
                font-weight: 700;
                letter-spacing: 0.12em;
                text-transform: uppercase;
                color: #667eea;
            }
            .inv-card p {
                margin: 0.15rem 0;
                color: #374151;
            }
            .inv-card .strong {
                font-weight: 700;
                color: #0c1445;
            }
            .inv-project {
                margin-top: 1.25rem;
            }
            .inv-project h2 {
                margin: 0 0 0.5rem;
                font-size: 0.7rem;
                font-weight: 700;
                letter-spacing: 0.12em;
                text-transform: uppercase;
                color: #667eea;
            }
            .inv-project-inner {
                background: #fff;
                border: 1px solid #e8eaf2;
                border-radius: 12px;
                padding: 1rem 1.1rem;
            }
            .inv-project-title {
                font-weight: 700;
                color: #0c1445;
            }
            .inv-project-ref {
                font-size: 0.85rem;
                color: #4b5563;
                margin-top: 0.25rem;
            }
            .inv-project-desc {
                margin: 0.5rem 0 0;
                font-size: 0.85rem;
                color: #6b7280;
                line-height: 1.5;
            }
            .inv-table-wrap {
                margin-top: 1.25rem;
                overflow: auto;
                border-radius: 12px;
                border: 1px solid #e8eaf2;
            }
            .inv-table {
                width: 100%;
                border-collapse: collapse;
                font-size: 0.8125rem;
            }
            .inv-table th {
                text-align: left;
                padding: 0.65rem 0.75rem;
                background: #0c1445;
                color: #fff;
                font-weight: 600;
                white-space: nowrap;
            }
            .inv-table td {
                padding: 0.6rem 0.75rem;
                border-bottom: 1px solid #eef0f6;
                vertical-align: top;
            }
            .inv-table tbody tr:nth-child(even) td {
                background: #fafbff;
            }
            .inv-table .num {
                text-align: right;
                white-space: nowrap;
            }
            .muted {
                color: #6b7280;
            }
            .center {
                text-align: center;
            }
            .inv-totals {
                display: flex;
                justify-content: flex-end;
                margin-top: 1rem;
            }
            .inv-totals-inner {
                min-width: 280px;
                max-width: 100%;
            }
            .inv-totals .row {
                display: flex;
                justify-content: space-between;
                gap: 1rem;
                padding: 0.35rem 0;
                font-size: 0.875rem;
                border-bottom: 1px solid #f0f2f8;
            }
            .inv-totals .row.grand {
                font-weight: 800;
                font-size: 1.05rem;
                color: #0c1445;
                border-bottom: none;
                margin-top: 0.25rem;
                padding-top: 0.5rem;
                border-top: 2px solid #e5e7ef;
            }
            .inv-totals .curr {
                text-align: right;
                font-size: 0.75rem;
                color: #9ca3af;
                margin-top: 0.35rem;
            }
            .inv-notes {
                margin-top: 1.5rem;
                font-size: 0.875rem;
                line-height: 1.6;
                color: #374151;
            }
            .inv-notes h2 {
                margin: 0 0 0.5rem;
                font-size: 0.85rem;
                color: #0c1445;
            }
            .inv-notes h3 {
                margin: 0.75rem 0 0.35rem;
                font-size: 0.75rem;
                text-transform: uppercase;
                letter-spacing: 0.06em;
                color: #6b7280;
            }
            .preserve {
                white-space: pre-wrap;
            }
            .terms {
                color: #6b7280;
                font-size: 0.8125rem;
            }
            .inv-footer {
                margin-top: 2rem;
                text-align: center;
                font-size: 0.75rem;
                color: #9ca3af;
            }
            .inv-footer-rule {
                height: 1px;
                background: linear-gradient(90deg, transparent, rgba(102, 126, 234, 0.5), transparent);
                margin-bottom: 0.75rem;
            }
            @media print {
                .inv {
                    max-width: none;
                    padding: 0;
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
            return new Intl.NumberFormat('en-IL', { style: 'currency', currency }).format(amount);
        } catch {
            return `${amount.toFixed(2)} ${currency}`;
        }
    }
}
