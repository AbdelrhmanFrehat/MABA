import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { InputIconModule } from 'primeng/inputicon';
import { IconFieldModule } from 'primeng/iconfield';
import { CardModule } from 'primeng/card';

export interface FilterField {
    key: string;
    label: string;
    type: 'text' | 'select' | 'multiselect' | 'date' | 'daterange' | 'number';
    placeholder?: string;
    options?: { label: string; value: any }[];
    optionLabel?: string;
    optionValue?: string;
    icon?: string;
    width?: string;
}

@Component({
    selector: 'app-filter-bar',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        InputTextModule,
        SelectModule,
        ButtonModule,
        InputIconModule,
        IconFieldModule,
        CardModule
    ],
    template: `
        <p-card styleClass="mb-4">
            <div class="flex flex-wrap gap-3 items-end">
                <ng-container *ngFor="let field of fields">
                    <div class="flex flex-col gap-2" [style.width]="field.width || '200px'">
                        <label [for]="field.key" class="font-medium text-sm">{{ field.label }}</label>
                        
                        <ng-container [ngSwitch]="field.type">
                            <!-- Text Input -->
                            <p-iconfield *ngSwitchCase="'text'">
                                <p-inputicon [styleClass]="field.icon || 'pi pi-search'" />
                                <input 
                                    pInputText 
                                    [id]="field.key"
                                    [(ngModel)]="filterValues[field.key]"
                                    [placeholder]="field.placeholder || ''"
                                    (keyup.enter)="applyFilters()"
                                />
                            </p-iconfield>

                            <!-- Select Dropdown -->
                            <p-select 
                                *ngSwitchCase="'select'"
                                [id]="field.key"
                                [(ngModel)]="filterValues[field.key]"
                                [options]="field.options || []"
                                [optionLabel]="field.optionLabel || 'label'"
                                [optionValue]="field.optionValue || 'value'"
                                [placeholder]="field.placeholder || 'Select...'"
                                [showClear]="true"
                                styleClass="w-full"
                            />

                            <!-- Number Input -->
                            <input 
                                *ngSwitchCase="'number'"
                                pInputText 
                                type="number"
                                [id]="field.key"
                                [(ngModel)]="filterValues[field.key]"
                                [placeholder]="field.placeholder || ''"
                            />
                        </ng-container>
                    </div>
                </ng-container>

                <!-- Action Buttons -->
                <div class="flex gap-2 ml-auto">
                    <p-button 
                        label="Search" 
                        icon="pi pi-search" 
                        (onClick)="applyFilters()" 
                    />
                    <p-button 
                        label="Clear" 
                        icon="pi pi-times" 
                        severity="secondary" 
                        outlined
                        (onClick)="clearFilters()" 
                    />
                </div>
            </div>
        </p-card>
    `
})
export class FilterBarComponent {
    @Input() fields: FilterField[] = [];
    @Input() filterValues: { [key: string]: any } = {};

    @Output() onFilter = new EventEmitter<{ [key: string]: any }>();
    @Output() onClear = new EventEmitter<void>();

    applyFilters() {
        this.onFilter.emit({ ...this.filterValues });
    }

    clearFilters() {
        this.filterValues = {};
        this.onClear.emit();
    }

    setFilterValue(key: string, value: any) {
        this.filterValues[key] = value;
    }
}

