import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { CurrencySelectComponent } from '../../../shared/components/currency-select/currency-select';
import { ExpensesService } from '../../../shared/services/expenses.service';
import { ExpenseCategory } from '../../../shared/models/expense.model';
import { AuthService } from '../../../shared/services/auth.service';

@Component({
    selector: 'app-expense-form',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        ButtonModule,
        CardModule,
        InputNumberModule,
        InputTextModule,
        TextareaModule,
        SelectModule,
        DatePickerModule,
        MessageModule,
        ToastModule,
        TranslateModule,
        CurrencySelectComponent
    ],
    providers: [MessageService],
    styles: [`
        .expense-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1.25rem; }
        .field { display: flex; flex-direction: column; gap: 0.4rem; }
        .field-span2 { grid-column: span 2; }
        .field-full { grid-column: 1 / -1; }
        @media (max-width: 640px) { .expense-grid { grid-template-columns: 1fr; } .field-span2, .field-full { grid-column: 1; } }
    `],
    template: `
        <p-toast />

        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <div>
                    <h1 class="text-2xl font-bold m-0">{{ 'admin.expenses.new' | translate }}</h1>
                    <p class="text-600 mt-2 mb-0">Create an expense entry and attach it to one of the existing expense categories.</p>
                </div>
                <p-button [label]="'common.back' | translate" severity="secondary" [outlined]="true" routerLink="/admin/expenses"></p-button>
            </div>

            <p-card>
                @if (errorMessage) {
                    <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
                }

                <form [formGroup]="form" (ngSubmit)="save()">
                    <div class="expense-grid">
                        <div class="field">
                            <label class="font-medium">Category <span class="text-red-500">*</span></label>
                            <p-select
                                formControlName="expenseCategoryId"
                                [options]="categories"
                                optionLabel="nameEn"
                                optionValue="id"
                                [filter]="true"
                                filterBy="nameEn,nameAr,key"
                                placeholder="Select category"
                                styleClass="w-full"
                            ></p-select>
                        </div>

                        <div class="field">
                            <label class="font-medium">Date <span class="text-red-500">*</span></label>
                            <p-datepicker formControlName="spentAt" appendTo="body" inputStyleClass="w-full" dateFormat="yy-mm-dd"></p-datepicker>
                        </div>

                        <div class="field">
                            <label class="font-medium">Currency</label>
                            <app-currency-select formControlName="currency"></app-currency-select>
                        </div>

                        <div class="field">
                            <label class="font-medium">Amount <span class="text-red-500">*</span></label>
                            <p-inputNumber formControlName="amount" mode="decimal" [min]="0" [maxFractionDigits]="2" styleClass="w-full"></p-inputNumber>
                        </div>

                        <div class="field field-span2">
                            <label class="font-medium">Description (English)</label>
                            <input pInputText formControlName="descriptionEn" class="w-full" />
                        </div>

                        <div class="field field-full">
                            <label class="font-medium">Description (Arabic)</label>
                            <textarea pTextarea rows="3" class="w-full" formControlName="descriptionAr"></textarea>
                        </div>
                    </div>

                    <div class="flex justify-end gap-2 mt-4">
                        <p-button [label]="'common.cancel' | translate" severity="secondary" [outlined]="true" routerLink="/admin/expenses" [disabled]="saving"></p-button>
                        <p-button [label]="'common.save' | translate" type="submit" [loading]="saving" [disabled]="form.invalid || saving"></p-button>
                    </div>
                </form>
            </p-card>
        </div>
    `
})
export class ExpenseFormComponent implements OnInit {
    categories: ExpenseCategory[] = [];
    saving = false;
    errorMessage = '';

    form = inject(FormBuilder).group({
        expenseCategoryId: ['', Validators.required],
        spentAt: [new Date(), Validators.required],
        amount: [0, [Validators.required, Validators.min(0.01)]],
        currency: ['ILS', [Validators.required, Validators.maxLength(3)]],
        descriptionEn: [''],
        descriptionAr: ['']
    });

    private expensesService = inject(ExpensesService);
    private authService = inject(AuthService);
    private router = inject(Router);
    private messageService = inject(MessageService);

    ngOnInit(): void {
        this.expensesService.getCategories().subscribe({
            next: categories => {
                this.categories = categories ?? [];
            },
            error: err => {
                this.errorMessage = err.error?.message || 'Failed to load expense categories.';
            }
        });
    }

    save(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const userId = this.authService.user?.id;
        if (!userId) {
            this.errorMessage = 'Your session is missing the current user id. Please sign in again.';
            return;
        }

        const value = this.form.getRawValue();
        this.saving = true;
        this.errorMessage = '';

        this.expensesService.createExpense({
            expenseCategoryId: value.expenseCategoryId ?? '',
            spentAt: value.spentAt ? value.spentAt.toISOString() : new Date().toISOString(),
            amount: value.amount ?? 0,
            currency: (value.currency || 'ILS').toUpperCase(),
            descriptionEn: value.descriptionEn || undefined,
            descriptionAr: value.descriptionAr || undefined,
            enteredByUserId: userId
        }).subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Expense created successfully.'
                });
                this.router.navigate(['/admin/expenses']);
            },
            error: err => {
                this.saving = false;
                this.errorMessage = err.error?.message || 'Failed to create expense.';
            }
        });
    }
}
