import { Routes } from '@angular/router';
import { authGuard } from '../../shared/guards/auth.guard';
import { storeOwnerGuard } from '../../shared/guards/auth.guard';

export const adminRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
        canActivate: [authGuard]
    },
    {
        path: 'lookups',
        loadChildren: () => import('./lookups/lookups.routes').then(m => m.lookupsRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'crm',
        loadChildren: () => import('./crm/crm.routes').then(m => m.crmRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'sales',
        loadChildren: () => import('./sales/sales.routes').then(m => m.salesRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'purchasing',
        loadChildren: () => import('./purchasing/purchasing.routes').then(m => m.purchasingRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'payments',
        loadChildren: () => import('./payments/payments.routes').then(m => m.paymentsRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'business-inventory',
        loadChildren: () => import('./biz-inventory/biz-inventory.routes').then(m => m.businessInventoryRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'accounting',
        loadChildren: () => import('./accounting/accounting.routes').then(m => m.accountingRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'pricing',
        loadChildren: () => import('./pricing/pricing.routes').then(m => m.pricingRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'expenses',
        loadChildren: () => import('./expenses/expenses.routes').then(m => m.expensesRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'reports',
        loadChildren: () => import('./reports/reports.routes').then(m => m.reportsRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'users',
        loadChildren: () => import('./users/users.routes').then(m => m.usersRoutes)
    },
    {
        path: 'roles',
        loadChildren: () => import('./roles/roles.routes').then(m => m.rolesRoutes)
    },
    {
        path: 'permissions',
        loadComponent: () => import('./permissions/permissions-list.component').then(m => m.PermissionsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'media',
        loadChildren: () => import('./media/media.routes').then(m => m.mediaRoutes)
    },
    {
        path: 'categories',
        loadChildren: () => import('./categories/categories.routes').then(m => m.categoriesRoutes)
    },
    {
        path: 'tags',
        loadChildren: () => import('./tags/tags.routes').then(m => m.tagsRoutes)
    },
    {
        path: 'brands',
        loadChildren: () => import('./brands/brands.routes').then(m => m.brandsRoutes)
    },
    {
        path: 'items',
        loadChildren: () => import('./items/items.routes').then(m => m.itemsRoutes)
    },
    {
        path: 'inventory',
        loadChildren: () => import('./inventory/inventory.routes').then(m => m.inventoryRoutes)
    },
    {
        path: 'machines',
        loadChildren: () => import('./machines/machines.routes').then(m => m.machinesRoutes)
    },
    {
        path: 'machine-parts',
        loadComponent: () => import('./machine-parts/machine-parts-list.component').then(m => m.MachinePartsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'item-machine-links',
        loadChildren: () => import('./item-machine-links/item-machine-links.routes').then(m => m.itemMachineLinksRoutes)
    },
    {
        path: 'orders',
        loadChildren: () => import('./orders/orders.routes').then(m => m.ordersRoutes),
        canActivate: [authGuard]
    },
    {
        path: '3d-requests',
        loadComponent: () => import('./3d-requests/3d-requests-list.component').then(m => m.ThreeDRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'design-requests',
        loadComponent: () => import('./design-requests/design-requests-list.component').then(m => m.DesignRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'cad-requests',
        loadComponent: () => import('./design-cad-requests/design-cad-requests-list.component').then(m => m.DesignCadRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'materials',
        loadChildren: () => import('./materials/materials.routes').then(m => m.materialsRoutes)
    },
    {
        path: 'laser-materials',
        loadChildren: () => import('./laser-materials/laser-materials.routes').then(m => m.laserMaterialsRoutes)
    },
    {
        path: 'laser-requests',
        loadComponent: () => import('./laser-requests/laser-requests-list.component').then(m => m.LaserRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'print-quality-profiles',
        loadChildren: () => import('./print-quality-profiles/print-quality-profiles.routes').then(m => m.printQualityProfilesRoutes)
    },
    {
        path: 'filament-spools',
        loadChildren: () => import('./filament-spools/filament-spools.routes').then(m => m.FILAMENT_SPOOLS_ROUTES)
    },
    {
        path: 'reviews',
        loadComponent: () => import('./reviews/reviews-list.component').then(m => m.ReviewsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'finance',
        loadComponent: () => import('./finance/finance-dashboard.component').then(m => m.FinanceDashboardComponent),
        canActivate: [authGuard]
    },
    {
        path: 'cms',
        loadChildren: () => import('./cms/cms.routes').then(m => m.cmsRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'home-stats',
        loadComponent: () => import('./cms/home-stats-settings.component').then(m => m.HomeStatsSettingsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'hero-ticker',
        loadChildren: () => import('./hero-ticker/hero-ticker.routes').then(m => m.heroTickerRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'faq',
        loadChildren: () => import('./faq/faq.routes').then(m => m.faqRoutes),
        canActivate: [storeOwnerGuard]
    },
    {
        path: 'support-chat',
        loadComponent: () => import('./support-chat/support-chat.component').then(m => m.SupportChatComponent),
        canActivate: [storeOwnerGuard]
    },
    {
        path: 'software',
        loadChildren: () => import('./software/software.routes').then(m => m.softwareAdminRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'requests',
        loadChildren: () => import('./unified-requests/unified-requests.routes').then(m => m.UNIFIED_REQUESTS_ROUTES),
        canActivate: [authGuard]
    },
    {
        path: 'cnc-materials',
        loadComponent: () => import('./cnc/cnc-materials-list.component').then(m => m.CncMaterialsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'cnc-requests/:id',
        loadComponent: () => import('./cnc/cnc-request-detail.component').then(m => m.CncRequestDetailComponent),
        canActivate: [authGuard]
    },
    {
        path: 'cnc-requests',
        loadComponent: () => import('./cnc/cnc-requests-list.component').then(m => m.CncRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'cnc-settings',
        loadComponent: () => import('./cnc/cnc-settings.component').then(m => m.CncSettingsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'projects',
        loadComponent: () => import('./projects/projects-list.component').then(m => m.AdminProjectsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'project-requests',
        loadComponent: () => import('./projects/project-requests-list.component').then(m => m.AdminProjectRequestsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'storage',
        loadComponent: () => import('./storage/storage-items.component').then(m => m.StorageItemsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'assets',
        loadChildren: () => import('./assets/assets.routes').then(m => m.assetsRoutes),
        canActivate: [authGuard]
    }
];

