import { Routes } from '@angular/router';

export const softwareAdminRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./software-products-list.component').then(m => m.SoftwareProductsListComponent)
    },
    {
        path: ':id/releases',
        loadComponent: () => import('./software-releases.component').then(m => m.SoftwareReleasesComponent)
    }
];
