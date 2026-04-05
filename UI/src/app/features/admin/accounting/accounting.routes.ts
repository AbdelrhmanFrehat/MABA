import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/guards/auth.guard';

export const accountingRoutes: Routes = [
    {
        path: 'chart-of-accounts',
        loadComponent: () => import('./chart-of-accounts.component').then(m => m.ChartOfAccountsComponent),
        canActivate: [authGuard]
    },
    {
        path: 'journal-entries',
        loadComponent: () => import('./journal-entries-list.component').then(m => m.JournalEntriesListComponent),
        canActivate: [authGuard]
    },
    {
        path: 'journal-entries/new',
        loadComponent: () => import('./journal-entry-form.component').then(m => m.JournalEntryFormComponent),
        canActivate: [authGuard]
    },
    {
        path: 'ledger',
        loadComponent: () => import('./ledger-view.component').then(m => m.LedgerViewComponent),
        canActivate: [authGuard]
    },
    {
        path: 'trial-balance',
        loadComponent: () => import('./trial-balance.component').then(m => m.TrialBalanceComponent),
        canActivate: [authGuard]
    }
];
