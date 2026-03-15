import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-cnc-materials-list',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="cnc-materials">
            <h1>CNC Materials</h1>
            <p>CNC materials management - Coming soon</p>
        </div>
    `,
    styles: [`
        .cnc-materials {
            padding: 2rem;
        }
    `]
})
export class CncMaterialsListComponent {}
