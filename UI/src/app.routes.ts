import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Dashboard } from './app/pages/dashboard/dashboard';
import { Landing } from './app/pages/landing/landing';
import { Notfound } from './app/pages/notfound/notfound';
import { PublicLayoutComponent } from './app/features/public/layout/public-layout.component';
import { storeOwnerGuard } from './app/shared/guards/auth.guard';

export const appRoutes: Routes = [
    // Public routes with public layout
    {
        path: '',
        component: PublicLayoutComponent,
        loadChildren: () => import('./app/features/public/public.routes').then(m => m.publicRoutes)
    },
    // Admin routes with admin layout - only Admin and StoreOwner can access
    {
        path: 'admin',
        component: AppLayout,
        canActivate: [storeOwnerGuard],
        loadChildren: () => import('./app/features/admin/admin.routes').then(m => m.adminRoutes)
    },
    // Legacy routes (keep for backward compatibility if needed)
    { path: 'landing', component: Landing },
    { path: 'notfound', component: Notfound },
    { path: '**', redirectTo: '/notfound' }
];
