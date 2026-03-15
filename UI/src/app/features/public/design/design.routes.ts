import { Routes } from '@angular/router';

export const designRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./design-landing.component').then(m => m.DesignLandingComponent)
    },
    {
        path: 'new',
        loadComponent: () => import('./design-new-request.component').then(m => m.DesignNewRequestComponent)
    }
];
