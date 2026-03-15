import { Component, Input, Output, EventEmitter, ContentChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-form-dialog',
    standalone: true,
    imports: [CommonModule, DialogModule, ButtonModule],
    template: `
        <p-dialog 
            [(visible)]="visible" 
            [style]="{ width: dialogWidth }" 
            [header]="header" 
            [modal]="true"
            [closable]="closable"
            [draggable]="draggable"
            [resizable]="resizable"
            (onHide)="onHide.emit()"
        >
            <ng-template #content>
                <ng-content></ng-content>
            </ng-template>

            <ng-template #footer>
                <div class="flex justify-end gap-2">
                    <p-button 
                        *ngIf="showCancelButton"
                        [label]="cancelLabel" 
                        icon="pi pi-times" 
                        [text]="true"
                        (onClick)="onCancel.emit(); visible = false" 
                    />
                    <p-button 
                        *ngIf="showSaveButton"
                        [label]="saveLabel" 
                        icon="pi pi-check" 
                        [loading]="saving"
                        [disabled]="saveDisabled"
                        (onClick)="onSave.emit()" 
                    />
                </div>
            </ng-template>
        </p-dialog>
    `
})
export class FormDialogComponent {
    @Input() visible: boolean = false;
    @Input() header: string = '';
    @Input() dialogWidth: string = '500px';
    @Input() closable: boolean = true;
    @Input() draggable: boolean = true;
    @Input() resizable: boolean = false;
    @Input() showCancelButton: boolean = true;
    @Input() showSaveButton: boolean = true;
    @Input() cancelLabel: string = 'Cancel';
    @Input() saveLabel: string = 'Save';
    @Input() saving: boolean = false;
    @Input() saveDisabled: boolean = false;

    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() onSave = new EventEmitter<void>();
    @Output() onCancel = new EventEmitter<void>();
    @Output() onHide = new EventEmitter<void>();

    show() {
        this.visible = true;
        this.visibleChange.emit(true);
    }

    hide() {
        this.visible = false;
        this.visibleChange.emit(false);
    }
}

