import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const machineCatalogRoutes: Routes = [
    {
        path: '',
        redirectTo: 'categories',
        pathMatch: 'full'
    },
    {
        path: 'categories',
        loadComponent: () => import('./categories/machine-categories.component').then(m => m.MachineCategoriesComponent),
        canActivate: [authGuard]
    },
    {
        path: 'families',
        loadComponent: () => import('./families/machine-families.component').then(m => m.MachineFamiliesComponent),
        canActivate: [authGuard]
    },
    {
        path: 'definitions',
        loadComponent: () => import('./definitions/machine-definitions-list.component').then(m => m.MachineDefinitionsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'definitions/new',
        loadComponent: () => import('./definitions/machine-definition-form.component').then(m => m.MachineDefinitionFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'definitions/:id/edit',
        loadComponent: () => import('./definitions/machine-definition-form.component').then(m => m.MachineDefinitionFormComponent),
        canActivate: [authGuard]
    }
];
