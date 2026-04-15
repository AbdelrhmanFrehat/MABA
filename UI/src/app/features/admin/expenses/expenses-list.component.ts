import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { ExpensesService } from '../../../shared/services/expenses.service';
import { Expense, ExpenseCategory } from '../../../shared/models/expense.model';

interface ExpenseRow extends Expense {
    expenseCategoryName: string;
}

@Component({
    selector: 'app-expenses-list',
    standalone: true,
    imports: [CommonModule, RouterModule, ButtonModule, ToastModule, TranslateModule, DataTableComponent],
    providers: [MessageService],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.expenses.title' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Track operating expenses, spending dates, and who entered each record.</p>
                </div>
                <p-button [label]="'admin.expenses.new' | translate" icon="pi pi-plus" routerLink="/admin/expenses/new"></p-button>
            </div>

            <app-data-table
                [data]="rows"
                [columns]="columns"
                [loading]="loading"
                [showAddButton]="false"
                [showDeleteSelected]="false"
                [title]="'Expenses'"
                [globalFilterFields]="['expenseCategoryName', 'expenseCategoryKey', 'descriptionEn', 'descriptionAr', 'enteredByUserFullName']"
            ></app-data-table>
        </div>
    `
})
export class ExpensesListComponent implements OnInit {
    rows: ExpenseRow[] = [];
    loading = false;

    columns: TableColumn[] = [
        { field: 'spentAt', header: 'Date', type: 'date', sortable: true },
        { field: 'expenseCategoryName', header: 'Category', sortable: true },
        { field: 'descriptionEn', header: 'Description (English)', sortable: true },
        { field: 'amount', header: 'Amount', type: 'currency', currencyCode: 'ILS', sortable: true },
        { field: 'currency', header: 'Currency', sortable: true },
        { field: 'paidByUserFullName', header: 'Paid By', sortable: true },
        { field: 'paymentMethodNameEn', header: 'Payment Method', sortable: true },
        { field: 'enteredByUserFullName', header: 'Entered By', sortable: true }
    ];

    private expensesService = inject(ExpensesService);
    private messageService = inject(MessageService);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading = true;

        this.expensesService.getCategories().subscribe({
            next: categories => this.loadExpenses(categories),
            error: err => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.message || 'Failed to load expense categories.'
                });
            }
        });
    }

    private loadExpenses(categories: ExpenseCategory[]): void {
        const categoryMap = new Map(categories.map(category => [category.id, category.nameEn]));

        this.expensesService.getExpenses().subscribe({
            next: expenses => {
                this.rows = (expenses ?? []).map(expense => ({
                    ...expense,
                    expenseCategoryName: categoryMap.get(expense.expenseCategoryId) || expense.expenseCategoryKey
                }));
                this.loading = false;
            },
            error: err => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.message || 'Failed to load expenses.'
                });
            }
        });
    }
}
