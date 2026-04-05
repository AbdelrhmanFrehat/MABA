import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const paymentsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./payment-vouchers-list.component').then(m => m.PaymentVouchersListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'new',
        loadComponent: () => import('./payment-form.component').then(m => m.PaymentFormComponent),
        canActivate: [authGuard]
    },
    {
        path: ':id/edit',
        loadComponent: () => import('./payment-form.component').then(m => m.PaymentFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'plans',
        loadComponent: () => import('./payment-plans-page.component').then(m => m.PaymentPlansPageComponent),
        canActivate: [authGuard]
    },
    {
        path: 'installments',
        loadComponent: () => import('./installments-page.component').then(m => m.InstallmentsPageComponent),
        canActivate: [authGuard]
    }
];
