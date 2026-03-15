import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { LanguageService } from '../../../../shared/services/language.service';

@Component({
    selector: 'app-learn-more-dialog',
    standalone: true,
    imports: [CommonModule, TranslateModule, DialogModule, ButtonModule],
    template: `
        <p-dialog
            [(visible)]="visible"
            [modal]="true"
            [closable]="true"
            [dismissableMask]="true"
            [style]="{ width: '90vw', maxWidth: '480px' }"
            [draggable]="false"
            [resizable]="false"
            [contentStyle]="{ padding: '1.5rem' }"
            (onHide)="visibleChange.emit(false)">
            <ng-template pTemplate="header">
                <h2 class="m-0 learn-more-title">{{ 'home.learnMore.question' | translate }}</h2>
            </ng-template>
            <div class="learn-more-options">
                <p-button
                    [label]="'home.learnMore.projects' | translate"
                    icon="pi pi-folder-open"
                    styleClass="learn-more-option-btn w-full mb-2"
                    (onClick)="select('projects')">
                </p-button>
                <p-button
                    [label]="'home.learnMore.laserServices' | translate"
                    icon="pi pi-bolt"
                    styleClass="learn-more-option-btn w-full"
                    (onClick)="select('laser-services')">
                </p-button>
            </div>
        </p-dialog>
    `,
    styles: [`
        .learn-more-title {
            font-size: 1.25rem;
            font-weight: 600;
            color: var(--p-text-color);
        }
        .learn-more-options {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }
        :host ::ng-deep .learn-more-option-btn {
            justify-content: flex-start;
            padding: 1rem 1.25rem;
        }
    `]
})
export class LearnMoreDialogComponent {
    @Input() visible = false;
    @Output() visibleChange = new EventEmitter<boolean>();

    private router = inject(Router);

    select(route: 'projects' | 'laser-services') {
        this.visibleChange.emit(false);
        this.router.navigate(['/' + route]);
    }
}
