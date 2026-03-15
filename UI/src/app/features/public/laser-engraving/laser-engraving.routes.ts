import { Routes } from '@angular/router';

export const laserEngravingRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./laser-engraving-landing.component').then(m => m.LaserEngravingLandingComponent)
    }
];
