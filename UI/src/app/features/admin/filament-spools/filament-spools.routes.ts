import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const FILAMENT_SPOOLS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./filament-spools-list.component').then((m) => m.FilamentSpoolsListComponent),
        canActivate: [authGuard]
    }
];
