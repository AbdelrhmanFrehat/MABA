import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const categoriesRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./categories-list/categories-list.component').then(m => m.CategoriesListComponent),
        canActivate: [authGuard]
    }
];

