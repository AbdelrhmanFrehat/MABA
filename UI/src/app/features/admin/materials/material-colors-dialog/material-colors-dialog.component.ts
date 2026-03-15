import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-material-colors-dialog',
    standalone: true,
    imports: [CommonModule, ButtonModule, TranslateModule],
    template: `
        <div class="colors-dialog">
            <p>Material colors management - Coming soon</p>
            <div class="dialog-actions">
                <p-button label="Close" (click)="close()"></p-button>
            </div>
        </div>
    `,
    styles: [`
        .colors-dialog {
            padding: 1rem;
        }
        .dialog-actions {
            margin-top: 1rem;
            display: flex;
            justify-content: flex-end;
        }
    `]
})
export class MaterialColorsDialogComponent {
    private ref = inject(DynamicDialogRef);
    private config = inject(DynamicDialogConfig);

    close() {
        this.ref.close();
    }
}
