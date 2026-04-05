import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const expensesRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./expenses-list.component').then(m => m.ExpensesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'new',
        loadComponent: () => import('./expense-form.component').then(m => m.ExpenseFormComponent),
        canActivate: [authGuard]
    }
];
