import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AssetsService } from '../../../shared/services/assets.service';

@Component({
    selector: 'app-asset-settings',
    standalone: true,
    imports: [CommonModule, RouterModule, ReactiveFormsModule, ButtonModule, CardModule, InputTextModule, InputNumberModule, ToastModule],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">Asset Numbering</h1>
                    <p class="text-600 mt-2 mb-0">Default prefix and counter used when categories don't override.</p>
                </div>
                <p-button label="Back to Assets" severity="secondary" [outlined]="true" routerLink="/admin/assets"></p-button>
            </div>

            <p-card>
                <form [formGroup]="form" (ngSubmit)="save()">
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div class="flex flex-col gap-1">
                            <label class="font-medium">Prefix</label>
                            <input pInputText formControlName="prefix" placeholder="A-" class="w-full" />
                        </div>
                        <div class="flex flex-col gap-1">
                            <label class="font-medium">Pad Width</label>
                            <p-inputNumber formControlName="padWidth" [min]="1" [max]="12" styleClass="w-full"></p-inputNumber>
                        </div>
                        <div class="flex flex-col gap-1">
                            <label class="font-medium">Next Number</label>
                            <p-inputNumber formControlName="nextNumber" [min]="1" styleClass="w-full"></p-inputNumber>
                        </div>
                    </div>
                    <p class="text-sm text-600 mt-3 mb-0" *ngIf="preview">Preview of next generated number: <strong>{{ preview }}</strong></p>
                    <div class="flex justify-end gap-2 mt-4">
                        <p-button label="Save" type="submit" [disabled]="form.invalid || saving" [loading]="saving"></p-button>
                    </div>
                </form>
            </p-card>
        </div>
    `
})
export class AssetSettingsComponent implements OnInit {
    saving = false;

    form = inject(FormBuilder).group({
        prefix: ['A-', Validators.required],
        padWidth: [4, [Validators.required, Validators.min(1)]],
        nextNumber: [1, [Validators.required, Validators.min(1)]]
    });

    private svc = inject(AssetsService);
    private msg = inject(MessageService);

    get preview(): string {
        const v = this.form.getRawValue();
        const n = String(v.nextNumber ?? 1).padStart(v.padWidth ?? 4, '0');
        return `${v.prefix ?? ''}${n}`;
    }

    ngOnInit() {
        this.svc.getNumbering().subscribe({
            next: s => this.form.patchValue(s),
            error: err => this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load.' })
        });
    }

    save() {
        if (this.form.invalid) return;
        this.saving = true;
        const v = this.form.getRawValue();
        this.svc.updateNumbering({
            prefix: v.prefix || 'A-',
            padWidth: v.padWidth ?? 4,
            nextNumber: v.nextNumber ?? 1
        }).subscribe({
            next: s => { this.saving = false; this.form.patchValue(s); this.msg.add({ severity: 'success', summary: 'Saved', detail: 'Numbering updated.' }); },
            error: err => { this.saving = false; this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to save.' }); }
        });
    }
}
