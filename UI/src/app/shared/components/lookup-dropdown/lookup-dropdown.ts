import { Component, Input, Output, EventEmitter, OnInit, forwardRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { TranslateModule } from '@ngx-translate/core';
import { LookupApiService } from '../../services/lookup-api.service';
import { LanguageService } from '../../services/language.service';
import { LookupValue } from '../../models/lookup.model';

/**
 * Reusable dropdown that loads options from LookupValue API.
 * Usage:
 *   <app-lookup-dropdown
 *     lookupTypeKey="sales_order_status"
 *     [(ngModel)]="selectedStatusId"
 *     [placeholder]="'Select Status' | translate"
 *   ></app-lookup-dropdown>
 */
@Component({
    selector: 'app-lookup-dropdown',
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule, TranslateModule],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => LookupDropdownComponent),
            multi: true
        }
    ],
    template: `
        <p-select
            [options]="options"
            [optionLabel]="labelField"
            optionValue="id"
            [placeholder]="placeholder"
            [showClear]="showClear"
            [filter]="filter"
            [disabled]="disabled"
            [styleClass]="styleClass"
            [(ngModel)]="value"
            (ngModelChange)="onValueChange($event)"
        ></p-select>
    `
})
export class LookupDropdownComponent implements OnInit, ControlValueAccessor {
    @Input() lookupTypeKey!: string;
    @Input() placeholder = '';
    @Input() showClear = true;
    @Input() filter = false;
    @Input() disabled = false;
    @Input() styleClass = 'w-full';
    @Input() activeOnly = true;

    @Output() valueChange = new EventEmitter<string>();
    @Output() optionsLoaded = new EventEmitter<LookupValue[]>();

    options: LookupValue[] = [];
    value: string | null = null;
    labelField = 'nameEn';

    private lookupApi = inject(LookupApiService);
    private languageService = inject(LanguageService);
    private onChange: (value: string | null) => void = () => {};
    private onTouched: () => void = () => {};

    ngOnInit() {
        this.labelField = this.languageService.getNameField();
        if (this.lookupTypeKey) {
            this.lookupApi.getValues(this.lookupTypeKey).subscribe(values => {
                this.options = this.activeOnly ? values.filter(v => v.isActive) : values;
                this.optionsLoaded.emit(this.options);
            });
        }
    }

    onValueChange(val: string | null) {
        this.value = val;
        this.onChange(val);
        this.valueChange.emit(val ?? '');
    }

    // --- ControlValueAccessor ---

    writeValue(val: string | null): void {
        this.value = val;
    }

    registerOnChange(fn: (value: string | null) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        this.disabled = isDisabled;
    }
}
