import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    template: `<div class="layout-footer">
        <img src="assets/img/logo.jpeg" alt="MABA Logo" style="height: 24px; width: auto; border-radius: 4px; margin-left: 0.5rem; margin-right: 0.5rem;" />
        <span class="font-bold">MABA</span> - Engineering Solutions
    </div>`
})
export class AppFooter {}
