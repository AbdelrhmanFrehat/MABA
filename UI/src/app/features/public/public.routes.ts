import { Routes } from '@angular/router';
import { authGuard } from '../../shared/guards/auth.guard';

export const publicRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./home/home.component').then(m => m.HomeComponent)
    },
    {
        path: 'auth',
        loadChildren: () => import('./auth/auth.routes').then(m => m.authRoutes)
    },
    { path: 'login', redirectTo: 'auth/login', pathMatch: 'full' },
    { path: 'register', redirectTo: 'auth/register', pathMatch: 'full' },
    { path: 'forgot-password', redirectTo: 'auth/forgot-password', pathMatch: 'full' },
    { path: 'reset-password', redirectTo: 'auth/reset-password', pathMatch: 'full' },
    {
        path: 'catalog',
        loadChildren: () => import('./catalog/catalog.routes').then(m => m.catalogRoutes)
    },
    {
        path: 'item/:id',
        loadComponent: () => import('./item/item-details.component').then(m => m.ItemDetailsComponent)
    },
    {
        path: 'cart',
        loadComponent: () => import('./cart/cart.component').then(m => m.CartComponent)
    },
    {
        path: 'checkout',
        loadComponent: () => import('./checkout/checkout.component').then(m => m.CheckoutComponent),
        canActivate: [authGuard]
    },
    {
        path: '3d-print',
        loadChildren: () => import('./print3d/print3d.routes').then(m => m.print3dRoutes)
    },
    {
        path: 'account',
        loadChildren: () => import('./account/account.routes').then(m => m.accountRoutes),
        canActivate: [authGuard]
    },
    {
        path: 'chat',
        loadComponent: () => import('./chat/chat.component').then(m => m.ChatComponent),
        canActivate: [authGuard]
    },
    {
        path: 'contact',
        loadComponent: () => import('./contact/contact.component').then(m => m.ContactComponent)
    },
    {
        path: 'wishlist',
        loadComponent: () => import('./wishlist/wishlist.component').then(m => m.WishlistComponent)
    },
    {
        path: 'search',
        loadComponent: () => import('./search/search-results.component').then(m => m.SearchResultsComponent)
    },
    {
        path: 'compare',
        loadComponent: () => import('./compare/compare.component').then(m => m.CompareComponent)
    },
    {
        path: 'design',
        loadChildren: () => import('./design/design.routes').then(m => m.designRoutes)
    },
    {
        path: 'design-cad',
        loadChildren: () => import('./design-cad/design-cad.routes').then(m => m.designCadRoutes)
    },
    {
        path: 'about',
        loadComponent: () => import('./about/about-page.component').then(m => m.AboutPageComponent)
    },
    {
        path: 'faq',
        loadComponent: () => import('./faq/faq-page.component').then(m => m.FaqPageComponent)
    },
    { path: 'privacy', redirectTo: 'privacy-policy', pathMatch: 'full' },
    {
        path: 'privacy-policy',
        loadComponent: () => import('./legal/static-legal-page.component').then(m => m.StaticLegalPageComponent),
        data: { pageType: 'privacy-policy' }
    },
    { path: 'terms', redirectTo: 'terms-of-service', pathMatch: 'full' },
    {
        path: 'terms-of-service',
        loadComponent: () => import('./legal/static-legal-page.component').then(m => m.StaticLegalPageComponent),
        data: { pageType: 'terms-of-service' }
    },
    {
        path: 'project-terms',
        loadComponent: () => import('./legal/static-legal-page.component').then(m => m.StaticLegalPageComponent),
        data: { pageType: 'project-terms' }
    },
    {
        path: 'confidentiality',
        loadComponent: () => import('./legal/static-legal-page.component').then(m => m.StaticLegalPageComponent),
        data: { pageType: 'confidentiality' }
    },
    {
        path: 'service-sla',
        loadComponent: () => import('./legal/static-legal-page.component').then(m => m.StaticLegalPageComponent),
        data: { pageType: 'service-sla' }
    },
    {
        path: 'help',
        loadComponent: () => import('./help-center/help-center.component').then(m => m.HelpCenterComponent)
    },
    {
        path: 'projects',
        loadChildren: () => import('./projects/projects.routes').then(m => m.projectsRoutes)
    },
    {
        path: 'laser-engraving',
        loadChildren: () => import('./laser-engraving/laser-engraving.routes').then(m => m.laserEngravingRoutes)
    },
    {
        path: 'laser-services',
        loadComponent: () => import('./pages/page.component').then(m => m.PageComponent),
        data: { slug: 'laser-services' }
    },
    {
        path: 'software',
        loadChildren: () => import('./software/software.routes').then(m => m.softwareRoutes)
    },
    {
        path: 'cnc',
        loadChildren: () => import('./cnc/cnc.routes').then(m => m.cncRoutes)
    }
];

