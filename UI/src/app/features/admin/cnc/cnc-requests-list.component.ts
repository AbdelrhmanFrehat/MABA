import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-cnc-requests-list',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="cnc-requests">
            <h1>CNC Requests</h1>
            <p>CNC requests management - Coming soon</p>
        </div>
    `,
    styles: [`
        .cnc-requests {
            padding: 2rem;
        }
    `]
})
export class CncRequestsListComponent {}
