import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

/**
 * Reusable money/currency input with consistent decimal formatting.
 * Usage:
 *   <app-money-input
 *     [(ngModel)]="amount"
 *     [currency]="'ILS'"
 *     [disabled]="false"
 *   ></app-money-input>
 */
@Component({
    selector: 'app-money-input',
    standalone: true,
    imports: [CommonModule, FormsModule, InputNumberModule],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => MoneyInputComponent),
            multi: true
        }
    ],
    template: `
        <p-inputNumber
            [(ngModel)]="value"
            (ngModelChange)="onValueChange($event)"
            [mode]="mode"
            [currency]="currency"
            [locale]="locale"
            [min]="min"
            [maxFractionDigits]="maxFractionDigits"
            [minFractionDigits]="minFractionDigits"
            [disabled]="disabled"
            [styleClass]="styleClass"
            [placeholder]="placeholder"
        ></p-inputNumber>
    `
})
export class MoneyInputComponent implements ControlValueAccessor {
    @Input() currency = 'ILS';
    @Input() mode: 'decimal' | 'currency' = 'decimal';
    @Input() locale = 'en-US';
    @Input() min = 0;
    @Input() maxFractionDigits = 2;
    @Input() minFractionDigits = 2;
    @Input() disabled = false;
    @Input() styleClass = 'w-full';
    @Input() placeholder = '0.00';

    value: number | null = null;

    private onChange: (value: number | null) => void = () => {};
    private onTouched: () => void = () => {};

    onValueChange(val: number | null) {
        this.value = val;
        this.onChange(val);
    }

    writeValue(val: number | null): void {
        this.value = val;
    }

    registerOnChange(fn: (value: number | null) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        this.disabled = isDisabled;
    }
}
