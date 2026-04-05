import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-expenses-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TranslateModule],
    template: `<div class="p-4"><div class="flex justify-between items-center mb-4"><h1 class="text-2xl font-bold m-0">{{ 'admin.expenses.title' | translate }}</h1><p-button [label]="'admin.expenses.new' | translate" icon="pi pi-plus" routerLink="/admin/expenses/new"></p-button></div><div class="surface-card p-4 border-round border-1 surface-border">{{ 'admin.common.moduleScaffolded' | translate }}</div></div>`
})
export class ExpensesListComponent {}
