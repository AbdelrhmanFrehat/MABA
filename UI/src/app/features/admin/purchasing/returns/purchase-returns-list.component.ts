import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-purchase-returns-list',
    standalone: true,
    imports: [CommonModule, ButtonModule, TranslateModule],
    template: `<div class="p-4"><h1 class="text-2xl font-bold">{{ 'admin.purchasing.returns.title' | translate }}</h1><div class="surface-card p-4 border-round border-1 surface-border mt-4">{{ 'admin.common.moduleScaffolded' | translate }}</div></div>`
})
export class PurchaseReturnsListComponent {}
