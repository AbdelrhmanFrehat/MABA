import { Routes } from '@angular/router';

export const catalogRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./catalog-list.component').then(m => m.CatalogListComponent)
    },
    {
        path: 'category/:slug',
        loadComponent: () => import('./catalog-list.component').then(m => m.CatalogListComponent)
    },
    {
        path: 'brand/:slug',
        loadComponent: () => import('./catalog-list.component').then(m => m.CatalogListComponent)
    },
    {
        path: 'tag/:slug',
        loadComponent: () => import('./catalog-list.component').then(m => m.CatalogListComponent)
    }
];

