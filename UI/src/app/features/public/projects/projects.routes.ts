import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const projectsRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./projects-landing.component').then(m => m.ProjectsLandingComponent)
    },
    {
        path: 'request',
        loadComponent: () => import('./project-request.component').then(m => m.ProjectRequestComponent),
        canActivate: [authGuard]
    },
    {
        path: ':slug',
        loadComponent: () => import('./project-detail.component').then(m => m.ProjectDetailComponent)
    }
];
