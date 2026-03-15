import { Routes } from '@angular/router';

export const accountRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./account-overview.component').then(m => m.AccountOverviewComponent)
    },
    {
        path: 'requests',
        loadComponent: () => import('./account-requests.component').then(m => m.AccountRequestsComponent)
    },
    {
        path: 'orders',
        loadComponent: () => import('./account-orders.component').then(m => m.AccountOrdersComponent)
    },
    {
        path: 'orders/:id',
        loadComponent: () => import('./order-detail/order-detail.component').then(m => m.OrderDetailComponent)
    },
    {
        path: 'designs',
        loadComponent: () => import('./account-designs.component').then(m => m.AccountDesignsComponent)
    },
    {
        path: 'profile',
        loadComponent: () => import('./account-profile.component').then(m => m.AccountProfileComponent)
    },
    {
        path: 'addresses',
        loadComponent: () => import('./account-addresses.component').then(m => m.AccountAddressesComponent)
    },
    {
        path: 'payment-methods',
        loadComponent: () => import('./account-payment-methods.component').then(m => m.AccountPaymentMethodsComponent)
    }
];

