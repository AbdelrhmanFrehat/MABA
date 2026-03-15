import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const itemMachineLinksRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./item-machine-links-list/item-machine-links-list.component').then(m => m.ItemMachineLinksListComponent),
        canActivate: [authGuard]
    }
];

