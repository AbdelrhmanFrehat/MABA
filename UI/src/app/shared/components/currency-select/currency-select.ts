import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { CommonModule } from '@angular/common';

export interface CurrencyOption {
    code: string;
    label: string;
}

export const CURRENCIES: CurrencyOption[] = [
    { code: 'ILS', label: 'ILS – Israeli Shekel (₪)' },
    { code: 'USD', label: 'USD – US Dollar ($)' },
    { code: 'EUR', label: 'EUR – Euro (€)' },
    { code: 'JOD', label: 'JOD – Jordanian Dinar' },
    { code: 'EGP', label: 'EGP – Egyptian Pound' },
    { code: 'SAR', label: 'SAR – Saudi Riyal' },
    { code: 'AED', label: 'AED – UAE Dirham' },
    { code: 'GBP', label: 'GBP – British Pound (£)' },
];

@Component({
    selector: 'app-currency-select',
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => CurrencySelectComponent),
            multi: true
        }
    ],
    template: `
        <p-select
            [options]="currencies"
            optionLabel="label"
            optionValue="code"
            [placeholder]="placeholder"
            [filter]="true"
            [disabled]="disabled"
            [styleClass]="styleClass"
            [(ngModel)]="value"
            (ngModelChange)="onValueChange($event)"
        ></p-select>
    `
})
export class CurrencySelectComponent implements ControlValueAccessor {
    @Input() placeholder = 'Select currency';
    @Input() disabled = false;
    @Input() styleClass = 'w-full';

    currencies = CURRENCIES;
    value: string | null = null;

    private onChange: (v: string | null) => void = () => {};
    private onTouched: () => void = () => {};

    onValueChange(val: string | null) {
        this.value = val;
        this.onChange(val);
        this.onTouched();
    }

    writeValue(val: string | null): void { this.value = val; }
    registerOnChange(fn: (v: string | null) => void): void { this.onChange = fn; }
    registerOnTouched(fn: () => void): void { this.onTouched = fn; }
    setDisabledState(isDisabled: boolean): void { this.disabled = isDisabled; }
}
