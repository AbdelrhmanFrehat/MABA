import { Routes } from '@angular/router';

export const authRoutes: Routes = [
    { path: 'login', loadComponent: () => import('./public-login.component').then(m => m.PublicLoginComponent) },
    { path: 'register', loadComponent: () => import('./public-register.component').then(m => m.PublicRegisterComponent) },
    { path: 'check-email', loadComponent: () => import('./check-email.component').then(m => m.CheckEmailComponent) },
    { path: 'forgot-password', loadComponent: () => import('./forgot-password.component').then(m => m.ForgotPasswordComponent) },
    { path: 'reset-password', loadComponent: () => import('./reset-password.component').then(m => m.ResetPasswordComponent) },
    { path: '', pathMatch: 'full', redirectTo: 'login' }
];
