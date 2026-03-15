import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const usersRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./users-list/users-list.component').then(m => m.UsersListComponent),
        canActivate: [authGuard]
    },
    {
        path: ':id',
        loadComponent: () => import('./user-edit/user-edit.component').then(m => m.UserEditComponent),
        canActivate: [authGuard]
    }
];

