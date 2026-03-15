import { Routes } from '@angular/router';

export const designCadRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./design-cad-landing.component').then(m => m.DesignCadLandingComponent)
    },
    {
        path: 'new',
        loadComponent: () => import('./design-cad-new.component').then(m => m.DesignCadNewComponent)
    }
];
