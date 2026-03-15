import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const cmsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./cms-management.component').then(m => m.CmsManagementComponent),
        canActivate: [authGuard]
    },
    {
        path: 'pages',
        loadComponent: () => import('./pages-list/pages-list.component').then(m => m.PagesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'pages/new',
        loadComponent: () => import('./page-editor/page-editor.component').then(m => m.PageEditorComponent),
        canActivate: [authGuard]
    },
    {
        path: 'pages/:id',
        loadComponent: () => import('./page-editor/page-editor.component').then(m => m.PageEditorComponent),
        canActivate: [authGuard]
    }
];

