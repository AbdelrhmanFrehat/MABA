import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const itemsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./items-list/items-list.component').then(m => m.ItemsListComponent),
        canActivate: [authGuard]
    }
];

