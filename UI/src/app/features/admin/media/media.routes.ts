import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const mediaRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./media-list/media-list.component').then(m => m.MediaListComponent),
        canActivate: [authGuard]
    }
];

