import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const ordersRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./orders-list/orders-list.component').then(m => m.OrdersListComponent),
        canActivate: [authGuard]
    },
    {
        path: ':id',
        loadComponent: () => import('./order-detail/order-detail.component').then(m => m.OrderDetailComponent),
        canActivate: [authGuard]
    }
];

