import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const reportsRoutes: Routes = [
    {
        path: 'sales',
        loadComponent: () => import('./sales-report.component').then(m => m.SalesReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'purchases',
        loadComponent: () => import('./purchase-report.component').then(m => m.PurchaseReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'inventory',
        loadComponent: () => import('./inventory-report.component').then(m => m.InventoryReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'customers-statement',
        loadComponent: () => import('./customer-statement-report.component').then(m => m.CustomerStatementReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'suppliers-statement',
        loadComponent: () => import('./supplier-statement-report.component').then(m => m.SupplierStatementReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'profit-overview',
        loadComponent: () => import('./profit-overview-report.component').then(m => m.ProfitOverviewReportComponent),
        canActivate: [authGuard]
    },
    {
        path: 'trial-balance',
        loadComponent: () => import('./trial-balance-report.component').then(m => m.TrialBalanceReportComponent),
        canActivate: [authGuard]
    }
];
