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
        <div class="empty-state">
            <i [class]="'empty-icon ' + (icon || 'pi pi-inbox')"></i>
            <h3 class="empty-title">{{ titleKey ? (titleKey | translate) : title }}</h3>
            <p class="empty-desc" *ngIf="description || descriptionKey">
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
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            padding: 3.5rem 2rem;
            min-height: 300px;
            gap: 0.75rem;
        }
        .empty-icon {
            font-size: 3rem;
            color: #cbd5e1;
            margin-bottom: 0.5rem;
        }
        .empty-title {
            margin: 0;
            font-size: 1.25rem;
            font-weight: 700;
            color: #334155;
        }
        .empty-desc {
            margin: 0;
            color: #94a3b8;
            font-size: 0.9rem;
            max-width: 360px;
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

