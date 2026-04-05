import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
@Component({ selector: 'app-purchase-report', standalone: true, imports: [CommonModule, TranslateModule], template: `<div class="p-4"><h1 class="text-2xl font-bold">{{ 'admin.reports.purchases' | translate }}</h1><div class="surface-card p-4 border-round border-1 surface-border mt-4">{{ 'admin.common.moduleScaffolded' | translate }}</div></div>` })
export class PurchaseReportComponent {}
