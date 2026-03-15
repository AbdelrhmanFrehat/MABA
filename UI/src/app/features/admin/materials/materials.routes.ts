import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const materialsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./materials-list/materials-list.component').then(m => m.MaterialsListComponent),
        canActivate: [authGuard]
    }
];
