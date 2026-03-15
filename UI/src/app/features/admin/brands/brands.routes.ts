import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const brandsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./brands-list/brands-list.component').then(m => m.BrandsListComponent),
        canActivate: [authGuard]
    }
];

