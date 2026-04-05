import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-sales-returns-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, TranslateModule],
    template: `
        <div class="p-4 page-shell">
            <div class="flex justify-between items-center mb-4">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.sales.returns.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">{{ 'admin.sales.returns.subtitle' | translate }}</p>
                </div>
                <p-button [label]="'admin.sales.returns.new' | translate" icon="pi pi-plus" routerLink="/admin/sales/returns/new"></p-button>
            </div>
            <div class="surface-card p-4 border-round border-1 surface-border">{{ 'admin.common.moduleScaffolded' | translate }}</div>
        </div>
    `
})
export class SalesReturnsListComponent {}
