import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { MabaInvoiceDocument } from '../models/maba-invoice.model';

const BRAND_NAVY: [number, number, number] = [12, 20, 69];
const BRAND_PURPLE: [number, number, number] = [102, 126, 234];
const LOGO_PATH = 'assets/images/maba-hero-logo.png';

@Injectable({ providedIn: 'root' })
export class MabaInvoicePdfService {
    private http = inject(HttpClient);

    /** Loads logo as data URL; returns null if unavailable (PDF still generates). */
    private async loadLogoDataUrl(): Promise<string | null> {
        try {
            const blob = await firstValueFrom(this.http.get(LOGO_PATH, { responseType: 'blob' }));
            return await new Promise((resolve, reject) => {
                const r = new FileReader();
                r.onloadend = () => resolve(r.result as string);
                r.onerror = reject;
                r.readAsDataURL(blob);
            });
        } catch {
            return null;
        }
    }

    private formatMoney(amount: number, currency: string): string {
        try {
            return new Intl.NumberFormat('en-IL', { style: 'currency', currency }).format(amount);
        } catch {
            return `${amount.toFixed(2)} ${currency}`;
        }
    }

    private formatDate(iso: string): string {
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

    /**
     * Generates a print-ready A4 PDF with MABA branding.
     */
    async generatePdf(invoice: MabaInvoiceDocument): Promise<Blob> {
        const logo = await this.loadLogoDataUrl();
        const doc = new jsPDF({ unit: 'mm', format: 'a4', compress: true });
        const pageW = doc.internal.pageSize.getWidth();
        const margin = 14;
        const contentW = pageW - margin * 2;
        let y = margin;

        // --- Header: logo + title block ---
        const logoW = 52;
        const logoH = (logoW * 682) / 1024;
        if (logo) {
            try {
                doc.addImage(logo, 'PNG', margin, y, logoW, logoH);
            } catch {
                /* invalid image data */
            }
        }

        doc.setFont('helvetica', 'bold');
        doc.setFontSize(18);
        doc.setTextColor(...BRAND_NAVY);
        doc.text('INVOICE', pageW - margin, y + 8, { align: 'right' });

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(60, 60, 60);
        let metaY = y + 14;
        doc.text(`No. ${invoice.invoiceNumber}`, pageW - margin, metaY, { align: 'right' });
        metaY += 5;
        doc.text(`Issue: ${this.formatDate(invoice.issueDate)}`, pageW - margin, metaY, { align: 'right' });
        metaY += 5;
        if (invoice.dueDate) {
            doc.text(`Due: ${this.formatDate(invoice.dueDate)}`, pageW - margin, metaY, { align: 'right' });
            metaY += 5;
        }

        doc.setFillColor(...BRAND_PURPLE);
        doc.roundedRect(pageW - margin - 28, metaY - 1, 28, 7, 1.5, 1.5, 'F');
        doc.setTextColor(255, 255, 255);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(8.5);
        doc.text(invoice.statusLabel.toUpperCase(), pageW - margin - 14, metaY + 4, { align: 'center' });

        y = Math.max(y + logoH, metaY + 12) + 4;

        doc.setDrawColor(...BRAND_PURPLE);
        doc.setLineWidth(0.4);
        doc.line(margin, y, pageW - margin, y);
        y += 6;

        // --- Company + Client ---
        const colGap = 10;
        const colW = (contentW - colGap) / 2;
        const leftX = margin;
        const rightX = margin + colW + colGap;

        doc.setTextColor(...BRAND_NAVY);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(10);
        doc.text('From / MABA', leftX, y);
        doc.text('Bill To', rightX, y);
        y += 5;

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(45, 45, 45);
        const companyLines = [
            invoice.company.legalName,
            invoice.company.tagline,
            invoice.company.website,
            invoice.company.email,
            invoice.company.phone,
            ...invoice.company.addressLines
        ].filter(Boolean) as string[];

        let ly = y;
        for (const line of companyLines) {
            doc.text(doc.splitTextToSize(String(line), colW - 1), leftX, ly);
            ly += line.length > 60 ? 8 : 5;
        }

        let ry = y;
        const clientLines = [
            invoice.client.fullName,
            invoice.client.companyName,
            invoice.client.email,
            invoice.client.phone,
            ...invoice.client.addressLines
        ].filter(Boolean) as string[];
        for (const line of clientLines) {
            doc.text(doc.splitTextToSize(String(line), colW - 1), rightX, ry);
            ry += line.length > 60 ? 8 : 5;
        }

        y = Math.max(ly, ry) + 8;

        // --- Project ---
        doc.setFillColor(248, 249, 252);
        doc.roundedRect(margin, y, contentW, 18, 2, 2, 'F');
        doc.setDrawColor(230, 232, 240);
        doc.roundedRect(margin, y, contentW, 18, 2, 2, 'S');
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9.5);
        doc.setTextColor(...BRAND_NAVY);
        doc.text('Project / Reference', margin + 3, y + 6);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(50, 50, 50);
        doc.text(invoice.project.title, margin + 3, y + 12);
        doc.setFont('helvetica', 'bold');
        doc.text(`Ref: ${invoice.project.reference}`, margin + 3, y + 16);
        y += 22;
        if (invoice.project.description) {
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(8);
            doc.setTextColor(90, 90, 90);
            const desc = doc.splitTextToSize(invoice.project.description, contentW);
            doc.text(desc, margin, y);
            y += desc.length * 4 + 4;
        } else {
            y += 4;
        }

        // --- Line items table ---
        const body =
            invoice.lineItems.length > 0
                ? invoice.lineItems.map((row) => [
                      row.title,
                      row.description || '—',
                      String(row.quantity),
                      this.formatMoney(row.unitPrice, invoice.totals.currency),
                      this.formatMoney(row.lineTotal, invoice.totals.currency)
                  ])
                : [
                      [
                          '—',
                          'No line items on this record',
                          '—',
                          '—',
                          '—'
                      ]
                  ];

        autoTable(doc, {
            startY: y,
            head: [['Item / Service', 'Description', 'Qty', 'Unit price', 'Line total']],
            body,
            theme: 'plain',
            styles: {
                fontSize: 8.5,
                cellPadding: 2.5,
                textColor: [40, 40, 40],
                lineColor: [220, 224, 232],
                lineWidth: 0.1
            },
            headStyles: {
                fillColor: BRAND_NAVY,
                textColor: [255, 255, 255],
                fontStyle: 'bold',
                halign: 'left'
            },
            columnStyles: {
                0: { cellWidth: 38 },
                1: { cellWidth: 58 },
                2: { halign: 'center', cellWidth: 16 },
                3: { halign: 'right', cellWidth: 28 },
                4: { halign: 'right', cellWidth: 28 }
            },
            margin: { left: margin, right: margin }
        });

        const auto = (doc as unknown as { lastAutoTable?: { finalY: number } }).lastAutoTable;
        const finalY = auto?.finalY ?? y + 40;
        y = finalY + 8;

        // --- Totals (right aligned) ---
        const totalsX = pageW - margin - 60;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(9);
        doc.setTextColor(50, 50, 50);
        const rows: [string, string][] = [
            ['Subtotal', this.formatMoney(invoice.totals.subtotal, invoice.totals.currency)]
        ];
        if (invoice.totals.discount > 0) {
            rows.push(['Discount', `− ${this.formatMoney(invoice.totals.discount, invoice.totals.currency)}`]);
        }
        rows.push(['Tax / VAT', this.formatMoney(invoice.totals.tax, invoice.totals.currency)]);
        rows.push(['Shipping', this.formatMoney(invoice.totals.shipping, invoice.totals.currency)]);
        rows.push(['Grand Total', this.formatMoney(invoice.totals.grandTotal, invoice.totals.currency)]);

        let ty = y;
        for (let i = 0; i < rows.length; i++) {
            const [k, v] = rows[i];
            const bold = i === rows.length - 1;
            doc.setFont('helvetica', bold ? 'bold' : 'normal');
            doc.text(k, totalsX, ty);
            doc.text(v, pageW - margin, ty, { align: 'right' });
            ty += bold ? 7 : 5.5;
        }
        y = ty + 6;

        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8);
        doc.setTextColor(100, 100, 100);
        doc.text(`Currency: ${invoice.totals.currency}`, pageW - margin, y, { align: 'right' });
        y += 10;

        // --- Payment & notes ---
        doc.setTextColor(...BRAND_NAVY);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(9.5);
        doc.text('Payment & notes', margin, y);
        y += 5;
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(8.5);
        doc.setTextColor(55, 55, 55);
        if (invoice.paymentInstructions) {
            doc.text(doc.splitTextToSize(invoice.paymentInstructions, contentW), margin, y);
            y += 12;
        }
        if (invoice.notes) {
            doc.setFont('helvetica', 'bold');
            doc.text('Order notes', margin, y);
            y += 4;
            doc.setFont('helvetica', 'normal');
            doc.text(doc.splitTextToSize(invoice.notes, contentW), margin, y);
            y += 14;
        }
        if (invoice.terms) {
            doc.setFont('helvetica', 'bold');
            doc.text('Terms', margin, y);
            y += 4;
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(8);
            doc.setTextColor(90, 90, 90);
            doc.text(doc.splitTextToSize(invoice.terms, contentW), margin, y);
            y += 12;
        }

        // --- Footer ---
        const footY = doc.internal.pageSize.getHeight() - 16;
        doc.setDrawColor(...BRAND_PURPLE);
        doc.setLineWidth(0.3);
        doc.line(margin, footY - 4, pageW - margin, footY - 4);
        doc.setFontSize(8);
        doc.setTextColor(120, 120, 120);
        doc.text(invoice.footerNote || 'Thank you for your business.', pageW / 2, footY, { align: 'center' });

        return doc.output('blob');
    }

    /** Trigger browser download for a PDF blob. */
    downloadBlob(blob: Blob, filename: string): void {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        URL.revokeObjectURL(url);
    }

    /** Open PDF in a new tab (print-friendly). */
    openInNewTab(blob: Blob): void {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank', 'noopener,noreferrer');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
    }
}
