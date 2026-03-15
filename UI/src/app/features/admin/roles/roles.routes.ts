import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const rolesRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./roles-list/roles-list.component').then(m => m.RolesListComponent),
        canActivate: [authGuard]
    }
];

