import { Routes } from '@angular/router';

export const UNIFIED_REQUESTS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./unified-requests-list.component').then(m => m.UnifiedRequestsListComponent)
    }
];
