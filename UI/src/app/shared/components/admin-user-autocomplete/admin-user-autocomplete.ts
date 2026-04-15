import { Component, Input, forwardRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { UsersService } from '../../services/users.service';

interface AdminOption {
    id: string;
    fullName: string;
    email: string;
}

@Component({
    selector: 'app-admin-user-autocomplete',
    standalone: true,
    imports: [CommonModule, FormsModule, AutoCompleteModule],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => AdminUserAutocompleteComponent),
        multi: true
    }],
    template: `
        <p-autoComplete
            [(ngModel)]="selected"
            [suggestions]="suggestions"
            (completeMethod)="search($event)"
            (onSelect)="onSelect($event)"
            (onClear)="onClear()"
            [optionLabel]="'fullName'"
            [placeholder]="placeholder"
            [forceSelection]="true"
            [disabled]="disabled"
            [dropdown]="true"
            styleClass="w-full"
            [style]="{ width: '100%' }"
            [inputStyle]="{ width: '100%' }"
        >
            <ng-template let-opt pTemplate="item">
                <div class="flex flex-col">
                    <span class="font-medium">{{ opt.fullName }}</span>
                    <small class="text-600">{{ opt.email }}</small>
                </div>
            </ng-template>
        </p-autoComplete>
    `
})
export class AdminUserAutocompleteComponent implements ControlValueAccessor, OnInit {
    @Input() placeholder = 'Select admin';
    @Input() disabled = false;

    suggestions: AdminOption[] = [];
    selected: AdminOption | null = null;
    private currentId: string | null = null;

    private usersService = inject(UsersService);
    private onChange: (v: string | null) => void = () => {};
    private onTouched: () => void = () => {};

    ngOnInit(): void {
        if (this.currentId) this.resolveInitial();
    }

    search(ev: { query: string }) {
        this.usersService.searchAdmins(ev.query).subscribe({
            next: list => (this.suggestions = list),
            error: () => (this.suggestions = [])
        });
    }

    onSelect(ev: { value: AdminOption }) {
        this.selected = ev.value;
        this.currentId = ev.value?.id ?? null;
        this.onChange(this.currentId);
    }

    onClear() {
        this.selected = null;
        this.currentId = null;
        this.onChange(null);
    }

    writeValue(v: string | null): void {
        this.currentId = v;
        if (v) this.resolveInitial();
        else this.selected = null;
    }
    registerOnChange(fn: (v: string | null) => void): void { this.onChange = fn; }
    registerOnTouched(fn: () => void): void { this.onTouched = fn; }
    setDisabledState(d: boolean): void { this.disabled = d; }

    private resolveInitial() {
        if (!this.currentId) return;
        this.usersService.searchAdmins('').subscribe({
            next: list => {
                const found = list.find(u => u.id === this.currentId);
                if (found) this.selected = found;
            }
        });
    }
}
