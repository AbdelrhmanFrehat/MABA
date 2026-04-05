import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const lookupsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./lookups-list.component').then(m => m.LookupsListComponent),
        canActivate: [authGuard]
    },
    {
        path: ':typeKey',
        loadComponent: () => import('./lookup-values-list.component').then(m => m.LookupValuesListComponent),
        canActivate: [authGuard]
    }
];
