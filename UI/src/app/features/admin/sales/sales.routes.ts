import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const salesRoutes: Routes = [
    {
        path: 'quotations',
        loadComponent: () => import('./quotations/sales-quotations-list.component').then(m => m.SalesQuotationsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'quotations/new',
        loadComponent: () => import('./quotations/sales-quotation-form.component').then(m => m.SalesQuotationFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'quotations/:id/edit',
        loadComponent: () => import('./quotations/sales-quotation-form.component').then(m => m.SalesQuotationFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'orders',
        loadComponent: () => import('./orders/sales-orders-list.component').then(m => m.SalesOrdersListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'orders/new',
        loadComponent: () => import('./orders/sales-order-form.component').then(m => m.SalesOrderFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'orders/:id/edit',
        loadComponent: () => import('./orders/sales-order-form.component').then(m => m.SalesOrderFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'invoices',
        loadComponent: () => import('./invoices/sales-invoices-list.component').then(m => m.SalesInvoicesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'invoices/new',
        loadComponent: () => import('./invoices/sales-invoice-form.component').then(m => m.SalesInvoiceFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'invoices/:id/edit',
        loadComponent: () => import('./invoices/sales-invoice-form.component').then(m => m.SalesInvoiceFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'returns',
        loadComponent: () => import('./returns/sales-returns-list.component').then(m => m.SalesReturnsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'returns/new',
        loadComponent: () => import('./returns/sales-return-form.component').then(m => m.SalesReturnFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'returns/:id/edit',
        loadComponent: () => import('./returns/sales-return-form.component').then(m => m.SalesReturnFormComponent),
        canActivate: [authGuard]
    }
];
