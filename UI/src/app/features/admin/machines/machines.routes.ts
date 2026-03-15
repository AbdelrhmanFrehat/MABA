import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const machinesRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./machines-list/machines-list.component').then(m => m.MachinesListComponent),
        canActivate: [authGuard]
    }
];

