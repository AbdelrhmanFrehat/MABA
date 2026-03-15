import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MenuItem } from 'primeng/api';

@Component({
    selector: 'app-breadcrumbs',
    standalone: true,
    imports: [CommonModule, RouterModule, BreadcrumbModule],
    template: `
        <p-breadcrumb [model]="items" [home]="homeItem"></p-breadcrumb>
    `
})
export class BreadcrumbsComponent {
    @Input() items: MenuItem[] = [];
    
    homeItem: MenuItem = {
        icon: 'pi pi-home',
        routerLink: '/'
    };
}

