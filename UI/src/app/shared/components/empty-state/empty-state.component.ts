import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-empty-state',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TranslateModule],
    template: `
        <div class="empty-state flex flex-column align-items-center justify-content-center p-6 text-center">
            <i [class]="icon || 'pi pi-inbox'" class="text-6xl text-500 mb-4"></i>
            <h3 class="text-2xl font-bold mb-2">{{ titleKey ? (titleKey | translate) : title }}</h3>
            <p class="text-500 mb-4" *ngIf="description || descriptionKey">
                {{ descriptionKey ? (descriptionKey | translate) : description }}
            </p>
            <p-button 
                *ngIf="actionLabel || actionLabelKey"
                [label]="actionLabelKey ? (actionLabelKey | translate) : actionLabel"
                [icon]="actionIcon"
                [iconPos]="actionIconPos"
                [routerLink]="actionRoute"
                (click)="onAction.emit()"
            ></p-button>
        </div>
    `,
    styles: [`
        .empty-state {
            min-height: 300px;
        }
    `]
})
export class EmptyStateComponent {
    @Input() icon?: string;
    @Input() title?: string;
    @Input() titleKey?: string;
    @Input() description?: string;
    @Input() descriptionKey?: string;
    @Input() actionLabel?: string;
    @Input() actionLabelKey?: string;
    @Input() actionIcon?: string;
    @Input() actionIconPos: 'left' | 'right' = 'left';
    @Input() actionRoute?: any[];
    @Output() onAction = new EventEmitter<void>();
}

