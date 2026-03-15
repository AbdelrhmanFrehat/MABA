import { Routes } from '@angular/router';

export const print3dRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./print3d-landing.component').then(m => m.Print3dLandingComponent)
    },
    {
        path: 'new',
        loadComponent: () => import('./print3d-new.component').then(m => m.Print3dNewComponent)
    }
];

