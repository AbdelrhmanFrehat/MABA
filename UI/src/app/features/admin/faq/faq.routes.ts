import { Routes } from '@angular/router';
import { storeOwnerGuard } from '../../../shared/guards/auth.guard';

export const faqRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./faq-list.component').then(m => m.FaqListComponent),
        canActivate: [storeOwnerGuard]
    },
    {
        path: 'new',
        loadComponent: () => import('./faq-form.component').then(m => m.FaqFormComponent),
        canActivate: [storeOwnerGuard]
    },
    {
        path: ':id/edit',
        loadComponent: () => import('./faq-form.component').then(m => m.FaqFormComponent),
        canActivate: [storeOwnerGuard]
    }
];
