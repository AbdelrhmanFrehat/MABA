import { Routes } from '@angular/router';

export const cncRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./cnc-landing.component').then(m => m.CncLandingComponent)
    }
];
