import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const appAnnouncementsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./app-announcements-list.component').then(m => m.AppAnnouncementsListComponent),
        canActivate: [authGuard]
    }
];
