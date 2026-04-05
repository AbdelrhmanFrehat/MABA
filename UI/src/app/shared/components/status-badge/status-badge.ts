import { Component, Input, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { LookupApiService } from '../../services/lookup-api.service';
import { LanguageService } from '../../services/language.service';
import { LookupValue } from '../../models/lookup.model';

/**
 * Displays a colored PrimeNG Tag based on a LookupValue ID.
 * Usage:
 *   <app-status-badge
 *     [lookupTypeKey]="'sales_order_status'"
 *     [valueId]="order.statusLookupId"
 *   ></app-status-badge>
 *
 * Or with pre-resolved values:
 *   <app-status-badge
 *     [label]="order.statusName"
 *     [color]="order.statusColor"
 *   ></app-status-badge>
 */
@Component({
    selector: 'app-status-badge',
    standalone: true,
    imports: [CommonModule, TagModule],
    template: `
        <p-tag
            *ngIf="displayLabel"
            [value]="displayLabel"
            [severity]="computedSeverity"
            [style]="customColor ? { 'background-color': customColor } : {}"
        ></p-tag>
    `
})
export class StatusBadgeComponent implements OnInit, OnChanges {
    @Input() lookupTypeKey?: string;
    @Input() valueId?: string;
    @Input() label?: string;
    @Input() color?: string;
    @Input() severity?: 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

    displayLabel = '';
    customColor = '';
    computedSeverity: 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' = 'info';

    private lookupApi = inject(LookupApiService);
    private languageService = inject(LanguageService);

    ngOnInit() {
        this.resolve();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['valueId'] || changes['label'] || changes['color']) {
            this.resolve();
        }
    }

    private resolve() {
        if (this.label) {
            this.displayLabel = this.label;
            this.customColor = this.color || '';
            this.computedSeverity = this.severity || this.mapColorToSeverity(this.color);
            return;
        }

        if (this.lookupTypeKey && this.valueId) {
            this.lookupApi.getValues(this.lookupTypeKey).subscribe(values => {
                const found = values.find(v => v.id === this.valueId);
                if (found) {
                    const nameField = this.languageService.getNameField();
                    this.displayLabel = (found as any)[nameField] || found.nameEn;
                    this.customColor = found.color || '';
                    this.computedSeverity = this.severity || this.mapKeyToSeverity(found.key);
                }
            });
        }
    }

    private mapKeyToSeverity(key: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
        const k = key.toLowerCase();
        if (k.includes('paid') || k.includes('completed') || k.includes('approved') || k.includes('active')) return 'success';
        if (k.includes('pending') || k.includes('draft')) return 'secondary';
        if (k.includes('overdue') || k.includes('rejected') || k.includes('cancelled') || k.includes('defaulted')) return 'danger';
        if (k.includes('partial')) return 'warn';
        return 'info';
    }

    private mapColorToSeverity(color?: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
        if (!color) return 'info';
        if (color.includes('green') || color.includes('22c55e')) return 'success';
        if (color.includes('red') || color.includes('ef4444')) return 'danger';
        if (color.includes('yellow') || color.includes('f59e0b')) return 'warn';
        return 'info';
    }
}
