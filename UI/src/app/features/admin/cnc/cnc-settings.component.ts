import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-cnc-settings',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="cnc-settings">
            <h1>CNC Settings</h1>
            <p>CNC settings management - Coming soon</p>
        </div>
    `,
    styles: [`
        .cnc-settings {
            padding: 2rem;
        }
    `]
})
export class CncSettingsComponent {}
