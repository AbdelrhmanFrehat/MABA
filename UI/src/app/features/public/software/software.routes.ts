import { Routes } from '@angular/router';

export const softwareRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./software-library.component').then(m => m.SoftwareLibraryComponent)
    },
    {
        path: ':slug',
        loadComponent: () => import('./software-detail.component').then(m => m.SoftwareDetailComponent)
    }
];
