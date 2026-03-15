import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const heroTickerRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./hero-ticker-list.component').then(m => m.HeroTickerListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'new',
        loadComponent: () => import('./hero-ticker-new.component').then(m => m.HeroTickerNewComponent),
        canActivate: [authGuard]
    }
];
