import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-unified-requests-list',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="unified-requests">
            <h1>Service Requests</h1>
            <p>Unified service requests management - Coming soon</p>
        </div>
    `,
    styles: [`
        .unified-requests {
            padding: 2rem;
        }
    `]
})
export class UnifiedRequestsListComponent {}
