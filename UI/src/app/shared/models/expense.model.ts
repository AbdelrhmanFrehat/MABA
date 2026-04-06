export interface ExpenseCategory {
    id: string;
    key: string;
    nameEn: string;
    nameAr: string;
    createdAt?: string;
    updatedAt?: string;
}

export interface Expense {
    id: string;
    expenseCategoryId: string;
    expenseCategoryKey: string;
    descriptionEn?: string;
    descriptionAr?: string;
    amount: number;
    currency: string;
    spentAt: string;
    receiptMediaId?: string;
    enteredByUserId: string;
    enteredByUserFullName: string;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateExpenseRequest {
    expenseCategoryId: string;
    descriptionEn?: string;
    descriptionAr?: string;
    amount: number;
    currency?: string;
    spentAt: string;
    receiptMediaId?: string;
    enteredByUserId: string;
}
