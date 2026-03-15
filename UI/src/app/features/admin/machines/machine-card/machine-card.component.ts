import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { LanguageService } from '../../../../shared/services/language.service';
import { Machine } from '../../../../shared/models/machine.model';

@Component({
    selector: 'app-machine-card',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        TranslateModule,
        CardModule,
        ButtonModule,
        TagModule,
        TooltipModule
    ],
    template: `
        <p-card class="machine-card">
            <div class="machine-card-content">
                <div class="machine-header">
                    <h3 class="machine-name">{{ machineName }}</h3>
                    @if (getPartsCount() > 0) {
                        <p-tag 
                            [value]="getPartsCount() + ' ' + ('admin.machines.parts' | translate)" 
                            severity="info"
                            [rounded]="true">
                        </p-tag>
                    }
                </div>
                
                <div class="machine-details">
                    @if (machine.manufacturer) {
                        <div class="detail-item">
                            <i class="pi pi-building text-500"></i>
                            <span class="label">{{ 'admin.machines.manufacturer' | translate }}:</span>
                            <span class="value">{{ machine.manufacturer }}</span>
                        </div>
                    }
                    
                    @if (machine.model) {
                        <div class="detail-item">
                            <i class="pi pi-cog text-500"></i>
                            <span class="label">{{ 'admin.machines.model' | translate }}:</span>
                            <span class="value">{{ machine.model }}</span>
                        </div>
                    }
                    
                    @if (machine.yearFrom || machine.yearTo) {
                        <div class="detail-item">
                            <i class="pi pi-calendar text-500"></i>
                            <span class="label">{{ 'admin.machines.yearRange' | translate }}:</span>
                            <span class="value">{{ yearRange }}</span>
                        </div>
                    }
                    
                    @if (machine.parts && machine.parts.length > 0) {
                        <div class="detail-item">
                            <i class="pi pi-wrench text-500"></i>
                            <span class="label">{{ 'admin.machines.partsCount' | translate }}:</span>
                            <span class="value">{{ machine.parts.length }}</span>
                        </div>
                    }
                </div>
                
                <div class="machine-actions">
                    <p-button 
                        [label]="'common.view' | translate"
                        icon="pi pi-eye"
                        [outlined]="true"
                        styleClass="flex-1"
                        [routerLink]="['/admin/machines', machine.id]">
                    </p-button>
                    <p-button 
                        [icon]="'pi pi-pencil'"
                        [pTooltip]="'common.edit' | translate"
                        tooltipPosition="top"
                        [rounded]="true"
                        [outlined]="true"
                        (click)="onEdit.emit(machine)">
                    </p-button>
                    <p-button 
                        [icon]="'pi pi-trash'"
                        [pTooltip]="'common.delete' | translate"
                        tooltipPosition="top"
                        [rounded]="true"
                        [outlined]="true"
                        severity="danger"
                        (click)="onDelete.emit(machine)">
                    </p-button>
                </div>
            </div>
        </p-card>
    `,
    styles: [`
        .machine-card {
            height: 100%;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .machine-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
        }
        .machine-card-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }
        .machine-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            gap: 1rem;
        }
        .machine-name {
            margin: 0;
            font-size: 1.25rem;
            font-weight: 600;
            color: var(--text-color);
            flex: 1;
        }
        .machine-details {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }
        .detail-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 0.875rem;
        }
        .detail-item i {
            width: 1.25rem;
        }
        .detail-item .label {
            font-weight: 600;
            color: var(--text-color-secondary);
            min-width: 100px;
        }
        .detail-item .value {
            color: var(--text-color);
        }
        .machine-actions {
            display: flex;
            gap: 0.5rem;
            margin-top: auto;
            padding-top: 1rem;
            border-top: 1px solid var(--surface-border);
        }
    `]
})
export class MachineCardComponent {
    @Input() machine!: Machine;
    @Output() onEdit = new EventEmitter<Machine>();
    @Output() onDelete = new EventEmitter<Machine>();

    constructor(public languageService: LanguageService) {}

    get machineName(): string {
        return this.languageService.language === 'ar' && this.machine.nameAr 
            ? this.machine.nameAr 
            : this.machine.nameEn;
    }

    get yearRange(): string {
        if (!this.machine.yearFrom && !this.machine.yearTo) return '-';
        if (this.machine.yearFrom && this.machine.yearTo) {
            return `${this.machine.yearFrom} - ${this.machine.yearTo}`;
        }
        return this.machine.yearFrom?.toString() || this.machine.yearTo?.toString() || '-';
    }

    getPartsCount(): number {
        return this.machine.parts?.length || 0;
    }
}
