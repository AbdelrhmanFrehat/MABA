import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const tagsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./tags-list/tags-list.component').then(m => m.TagsListComponent),
        canActivate: [authGuard]
    }
];

