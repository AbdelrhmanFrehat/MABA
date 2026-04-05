import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const crmRoutes: Routes = [
    {
        path: 'customers',
        loadComponent: () => import('./customers/customers-list.component').then(m => m.CustomersListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'customers/:id',
        loadComponent: () => import('./customers/customer-detail.component').then(m => m.CustomerDetailComponent),
        canActivate: [authGuard]
    },
    {
        path: 'suppliers',
        loadComponent: () => import('./suppliers/suppliers-list.component').then(m => m.SuppliersListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'suppliers/:id',
        loadComponent: () => import('./suppliers/supplier-detail.component').then(m => m.SupplierDetailComponent),
        canActivate: [authGuard]
    }
];
