import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const assetsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./assets-list.component').then(m => m.AssetsListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'new',
        loadComponent: () => import('./asset-form.component').then(m => m.AssetFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'categories',
        loadComponent: () => import('./asset-categories.component').then(m => m.AssetCategoriesComponent),
        canActivate: [authGuard]
    },
    {
        path: 'settings',
        loadComponent: () => import('./asset-settings.component').then(m => m.AssetSettingsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'print/:id',
        loadComponent: () => import('./asset-label-print.component').then(m => m.AssetLabelPrintComponent),
        canActivate: [authGuard]
    },
    {
        path: ':id/edit',
        loadComponent: () => import('./asset-form.component').then(m => m.AssetFormComponent),
        canActivate: [authGuard]
    },
    {
        path: ':id',
        loadComponent: () => import('./asset-detail.component').then(m => m.AssetDetailComponent),
        canActivate: [authGuard]
    }
];
