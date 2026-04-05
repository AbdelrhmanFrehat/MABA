import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const purchasingRoutes: Routes = [
    {
        path: 'quotations',
        loadComponent: () => import('./quotations/purchase-quotations-list.component').then(m => m.PurchaseQuotationsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'orders',
        loadComponent: () => import('./orders/purchase-orders-list.component').then(m => m.PurchaseOrdersListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'invoices',
        loadComponent: () => import('./invoices/purchase-invoices-list.component').then(m => m.PurchaseInvoicesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'returns',
        loadComponent: () => import('./returns/purchase-returns-list.component').then(m => m.PurchaseReturnsListComponent),
        canActivate: [authGuard]
    }
];
