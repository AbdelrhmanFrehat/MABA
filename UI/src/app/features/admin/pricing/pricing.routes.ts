import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const pricingRoutes: Routes = [
    {
        path: 'price-lists',
        loadComponent: () => import('./price-lists.component').then(m => m.PriceListsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'price-list-items',
        loadComponent: () => import('./price-list-items.component').then(m => m.PriceListItemsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'wholesale-rules',
        loadComponent: () => import('./wholesale-rules.component').then(m => m.WholesaleRulesComponent),
        canActivate: [authGuard]
    },
    {
        path: 'tax-profiles',
        loadComponent: () => import('./tax-profiles.component').then(m => m.TaxProfilesComponent),
        canActivate: [authGuard]
    },
    {
        path: 'units',
        loadComponent: () => import('./units-of-measure.component').then(m => m.UnitsOfMeasureComponent),
        canActivate: [authGuard]
    }
];
