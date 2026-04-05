import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const businessInventoryRoutes: Routes = [
    {
        path: 'warehouses',
        loadComponent: () => import('./warehouses-list.component').then(m => m.WarehousesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'stock',
        loadComponent: () => import('./stock-overview.component').then(m => m.StockOverviewComponent),
        canActivate: [authGuard]
    },
    {
        path: 'movements',
        loadComponent: () => import('./inventory-movements.component').then(m => m.InventoryMovementsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'adjustments',
        loadComponent: () => import('./stock-adjustment.component').then(m => m.StockAdjustmentComponent),
        canActivate: [authGuard]
    }
];
