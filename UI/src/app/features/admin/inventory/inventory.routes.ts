import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const inventoryRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./inventory-list/inventory-list.component').then(m => m.InventoryListComponent),
        canActivate: [authGuard]
    }
];

