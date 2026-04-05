import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-journal-entry-form',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TranslateModule],
    template: `<div class="p-4"><div class="flex justify-between items-center mb-4"><h1 class="text-2xl font-bold m-0">{{ 'admin.accounting.newJournalEntry' | translate }}</h1><p-button [label]="'common.back' | translate" severity="secondary" [outlined]="true" routerLink="/admin/accounting/journal-entries"></p-button></div><div class="surface-card p-4 border-round border-1 surface-border">{{ 'admin.common.documentFormComingSoon' | translate }}</div></div>`
})
export class JournalEntryFormComponent {}
