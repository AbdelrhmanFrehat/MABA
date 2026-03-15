import { Routes } from '@angular/router';

export const printQualityProfilesRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./print-quality-profiles-list/print-quality-profiles-list.component').then(m => m.PrintQualityProfilesListComponent),
    }
];
