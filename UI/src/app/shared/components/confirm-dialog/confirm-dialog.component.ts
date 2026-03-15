import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-confirm-dialog',
    standalone: true,
    imports: [CommonModule, ButtonModule, TranslateModule],
    template: `
        <div class="p-4">
            <h3 class="text-xl font-semibold mb-4">{{ config.data?.title || ('common.confirm' | translate) }}</h3>
            <p class="mb-6">{{ config.data?.message || ('common.confirmMessage' | translate) }}</p>
            <div class="flex justify-end gap-2">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                ></p-button>
                <p-button 
                    [label]="'common.confirm' | translate" 
                    severity="danger"
                    (click)="ref.close(true)"
                ></p-button>
            </div>
        </div>
    `
})
export class ConfirmDialogComponent {
    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {}
}

