export interface ExpenseCategory {
    id?: number;
    nameAr: string;
    nameEn: string;
}

export interface Expense {
    id?: number;
    expenseCategoryId: number;
    expenseCategoryNameAr?: string;
    expenseCategoryNameEn?: string;
    expenseDate: string;
    amount: number;
    description: string;
}

export interface ExpenseCategoryListResponse {
    items: ExpenseCategory[];
    totalCount: number;
}

export interface ExpenseListResponse {
    items: Expense[];
    totalCount: number;
}
