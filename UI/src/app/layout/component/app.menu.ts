import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { Subscription, filter } from 'rxjs';
import { AppMenuitem } from './app.menuitem';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule, TranslateModule],
    template: `<ul class="layout-menu">
        <ng-container *ngFor="let item of model; let i = index">
            <li app-menuitem *ngIf="!item.separator" [item]="item" [index]="i" [root]="true"></li>
            <li *ngIf="item.separator" class="menu-separator"></li>
        </ng-container>
    </ul> `
})
export class AppMenu implements OnInit, OnDestroy {
    model: MenuItem[] = [];
    private langSubscription!: Subscription;
    private routerSubscription!: Subscription;

    constructor(
        private translateService: TranslateService,
        private router: Router
    ) {}

    ngOnInit() {
        this.buildMenu();
        
        // Rebuild menu when language changes
        this.langSubscription = this.translateService.onLangChange.subscribe(() => {
            this.buildMenu();
        });

        // Rebuild menu when route changes
        this.routerSubscription = this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe(() => {
                this.buildMenu();
            });
    }

    ngOnDestroy() {
        if (this.langSubscription) {
            this.langSubscription.unsubscribe();
        }
        if (this.routerSubscription) {
            this.routerSubscription.unsubscribe();
        }
    }

    private buildMenu() {
        const currentPath = this.router.url;
        const isAdminRoute = currentPath.startsWith('/admin');

        if (isAdminRoute) {
            // Admin menu
            this.model = [
                {
                    label: this.translateService.instant('menu.home'),
                    items: [
                        { label: this.translateService.instant('menu.dashboard'), icon: 'pi pi-fw pi-home', routerLink: ['/admin'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.users'),
                    items: [
                        { label: this.translateService.instant('menu.usersList'), icon: 'pi pi-fw pi-users', routerLink: ['/admin/users'] },
                        { label: this.translateService.instant('menu.roles'), icon: 'pi pi-fw pi-shield', routerLink: ['/admin/roles'] },
                        { label: this.translateService.instant('menu.permissions'), icon: 'pi pi-fw pi-key', routerLink: ['/admin/permissions'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.media'),
                    items: [
                        { label: this.translateService.instant('menu.mediaLibrary'), icon: 'pi pi-fw pi-images', routerLink: ['/admin/media'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.catalog'),
                    items: [
                        { label: this.translateService.instant('menu.categories'), icon: 'pi pi-fw pi-sitemap', routerLink: ['/admin/categories'] },
                        { label: this.translateService.instant('menu.tags'), icon: 'pi pi-fw pi-tags', routerLink: ['/admin/tags'] },
                        { label: this.translateService.instant('menu.brands'), icon: 'pi pi-fw pi-bookmark', routerLink: ['/admin/brands'] },
                        { label: this.translateService.instant('menu.items'), icon: 'pi pi-fw pi-box', routerLink: ['/admin/items'] },
                        { label: this.translateService.instant('menu.inventory'), icon: 'pi pi-fw pi-warehouse', routerLink: ['/admin/inventory'] },
                        { label: this.translateService.instant('menu.storageItems'), icon: 'pi pi-fw pi-database', routerLink: ['/admin/storage'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.erp'),
                    items: [
                        { label: this.translateService.instant('menu.lookups'), icon: 'pi pi-fw pi-list', routerLink: ['/admin/lookups'] },
                        {
                            label: this.translateService.instant('menu.sales'),
                            icon: 'pi pi-fw pi-shopping-bag',
                            items: [
                                { label: this.translateService.instant('menu.sales'), icon: 'pi pi-fw pi-receipt', routerLink: ['/admin/sales/invoices'] },
                                { label: this.translateService.instant('menu.salesOrders'), icon: 'pi pi-fw pi-shopping-cart', routerLink: ['/admin/sales/orders'] },
                                { label: this.translateService.instant('menu.customers'), icon: 'pi pi-fw pi-users', routerLink: ['/admin/crm/customers'] },
                                { label: this.translateService.instant('menu.returns'), icon: 'pi pi-fw pi-refresh', routerLink: ['/admin/sales/returns'] }
                            ]
                        },
                        {
                            label: this.translateService.instant('menu.purchasing'),
                            icon: 'pi pi-fw pi-truck',
                            items: [
                                { label: this.translateService.instant('menu.purchasing'), icon: 'pi pi-fw pi-file', routerLink: ['/admin/purchasing/invoices'] },
                                { label: this.translateService.instant('menu.purchaseOrders'), icon: 'pi pi-fw pi-shopping-cart', routerLink: ['/admin/purchasing/orders'] },
                                { label: this.translateService.instant('menu.suppliers'), icon: 'pi pi-fw pi-building', routerLink: ['/admin/crm/suppliers'] },
                                { label: this.translateService.instant('menu.returns'), icon: 'pi pi-fw pi-refresh', routerLink: ['/admin/purchasing/returns'] },
                                { label: this.translateService.instant('menu.expenses'), icon: 'pi pi-fw pi-money-bill', routerLink: ['/admin/expenses'] }
                            ]
                        },
                        {
                            label: 'Asset Management',
                            icon: 'pi pi-fw pi-box',
                            items: [
                                { label: 'Assets', icon: 'pi pi-fw pi-list', routerLink: ['/admin/assets'] },
                                { label: 'Categories', icon: 'pi pi-fw pi-tags', routerLink: ['/admin/assets/categories'] },
                                { label: 'Numbering', icon: 'pi pi-fw pi-cog', routerLink: ['/admin/assets/settings'] }
                            ]
                        },
                        { label: this.translateService.instant('menu.businessInventory'), icon: 'pi pi-fw pi-warehouse', routerLink: ['/admin/business-inventory/stock'] },
                        { label: this.translateService.instant('menu.payments'), icon: 'pi pi-fw pi-wallet', routerLink: ['/admin/payments'] },
                        { label: this.translateService.instant('menu.accounting'), icon: 'pi pi-fw pi-calculator', routerLink: ['/admin/accounting/chart-of-accounts'] },
                        { label: this.translateService.instant('menu.pricing'), icon: 'pi pi-fw pi-percentage', routerLink: ['/admin/pricing/price-lists'] },
                        { label: this.translateService.instant('menu.erpReports'), icon: 'pi pi-fw pi-chart-line', routerLink: ['/admin/reports/sales'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.projects'),
                    items: [
                        { label: this.translateService.instant('menu.projectsList'), icon: 'pi pi-fw pi-briefcase', routerLink: ['/admin/projects'] },
                        { label: this.translateService.instant('menu.projectRequests'), icon: 'pi pi-fw pi-inbox', routerLink: ['/admin/project-requests'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.machines'),
                    items: [
                        { label: this.translateService.instant('menu.machinesList'), icon: 'pi pi-fw pi-cog', routerLink: ['/admin/machines'] },
                        { label: this.translateService.instant('menu.machineParts'), icon: 'pi pi-fw pi-wrench', routerLink: ['/admin/machine-parts'] },
                        { label: this.translateService.instant('menu.itemMachineLinks'), icon: 'pi pi-fw pi-link', routerLink: ['/admin/item-machine-links'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.printing3d'),
                    items: [
                        { label: this.translateService.instant('menu.materials'), icon: 'pi pi-fw pi-palette', routerLink: ['/admin/materials'] },
                        { label: this.translateService.instant('menu.printQualityProfiles'), icon: 'pi pi-fw pi-sliders-h', routerLink: ['/admin/print-quality-profiles'] },
                        { label: this.translateService.instant('menu.spools'), icon: 'pi pi-fw pi-database', routerLink: ['/admin/filament-spools'] },
                        { label: this.translateService.instant('menu.printing3dRequests'), icon: 'pi pi-fw pi-print', routerLink: ['/admin/3d-requests'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.laser'),
                    items: [
                        { label: this.translateService.instant('menu.laserMaterials'), icon: 'pi pi-fw pi-th-large', routerLink: ['/admin/laser-materials'] },
                        { label: this.translateService.instant('menu.laserRequests'), icon: 'pi pi-fw pi-bolt', routerLink: ['/admin/laser-requests'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.cnc'),
                    items: [
                        { label: this.translateService.instant('menu.cncMaterials'), icon: 'pi pi-fw pi-th-large', routerLink: ['/admin/cnc-materials'] },
                        { label: this.translateService.instant('menu.cncRequests'), icon: 'pi pi-fw pi-cog', routerLink: ['/admin/cnc-requests'] },
                        { label: this.translateService.instant('menu.cncSettings'), icon: 'pi pi-fw pi-sliders-h', routerLink: ['/admin/cnc-settings'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.orders'),
                    items: [
                        { label: this.translateService.instant('menu.ordersList'), icon: 'pi pi-fw pi-shopping-cart', routerLink: ['/admin/orders'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.reviews'),
                    items: [
                        { label: this.translateService.instant('menu.reviewsList'), icon: 'pi pi-fw pi-star', routerLink: ['/admin/reviews'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.finance'),
                    items: [
                        { label: this.translateService.instant('menu.financeDashboard'), icon: 'pi pi-fw pi-dollar', routerLink: ['/admin/finance'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.faq'),
                    items: [
                        { label: this.translateService.instant('admin.faq.title'), icon: 'pi pi-fw pi-question-circle', routerLink: ['/admin/faq'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.cms'),
                    items: [
                        { label: this.translateService.instant('menu.cmsManagement'), icon: 'pi pi-fw pi-file', routerLink: ['/admin/cms'] },
                        { label: this.translateService.instant('menu.heroTicker'), icon: 'pi pi-fw pi-images', routerLink: ['/admin/hero-ticker'] },
                        { label: this.translateService.instant('menu.pages'), icon: 'pi pi-fw pi-file-edit', routerLink: ['/admin/cms/pages'] },
                        { label: this.translateService.instant('menu.homeStatistics'), icon: 'pi pi-fw pi-chart-bar', routerLink: ['/admin/home-stats'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.requests'),
                    items: [
                        { label: this.translateService.instant('menu.allRequests'), icon: 'pi pi-fw pi-inbox', routerLink: ['/admin/requests'] },
                        { label: this.translateService.instant('menu.projectRequests'), icon: 'pi pi-fw pi-briefcase', routerLink: ['/admin/project-requests'] },
                        { label: this.translateService.instant('menu.3dRequests'), icon: 'pi pi-fw pi-box', routerLink: ['/admin/3d-requests'] },
                        { label: this.translateService.instant('menu.designRequests'), icon: 'pi pi-fw pi-pencil', routerLink: ['/admin/design-requests'] },
                        { label: this.translateService.instant('menu.cadRequests'), icon: 'pi pi-fw pi-pencil', routerLink: ['/admin/cad-requests'] },
                        { label: this.translateService.instant('menu.laserRequests'), icon: 'pi pi-fw pi-bolt', routerLink: ['/admin/laser-requests'] },
                        { label: this.translateService.instant('menu.cncRequests'), icon: 'pi pi-fw pi-cog', routerLink: ['/admin/cnc-requests'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.supportChat'),
                    items: [
                        { label: this.translateService.instant('menu.supportChat'), icon: 'pi pi-fw pi-comments', routerLink: ['/admin/support-chat'] }
                    ]
                },
                {
                    label: this.translateService.instant('menu.software'),
                    items: [
                        { label: this.translateService.instant('menu.softwareProducts'), icon: 'pi pi-fw pi-download', routerLink: ['/admin/software'] }
                    ]
                }
            ];
        } else {
            // Regular menu
            this.model = [
                {
                    label: this.translateService.instant('menu.home'),
                    items: [
                        { label: this.translateService.instant('menu.dashboard'), icon: 'pi pi-fw pi-home', routerLink: ['/'] }
                    ]
                }
            ];
        }
    }
}
