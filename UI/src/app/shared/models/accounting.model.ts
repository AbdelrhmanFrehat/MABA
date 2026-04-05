export interface Account {
    id: string;
    accountNumber: string;
    nameEn: string;
    nameAr: string;
    accountTypeLookupId: string;
    accountTypeName?: string;
    parentAccountId?: string;
    parentAccountNumber?: string;
    isActive: boolean;
    isSystem: boolean;
    isPostable: boolean;
    description?: string;
    balance: number;
    normalBalance: string;
    children?: Account[];
    createdAt: string;
    updatedAt?: string;
}

export interface FiscalPeriod {
    id: string;
    name: string;
    startDate: string;
    endDate: string;
    isClosed: boolean;
    closedAt?: string;
    closedByUserId?: string;
    createdAt: string;
}

export interface JournalEntry {
    id: string;
    entryNumber: string;
    journalEntryTypeLookupId: string;
    journalEntryTypeName?: string;
    entryDate: string;
    description?: string;
    sourceDocumentType?: string;
    sourceDocumentId?: string;
    sourceDocumentNumber?: string;
    fiscalPeriodId?: string;
    isPosted: boolean;
    postedAt?: string;
    postedByUserId?: string;
    isReversed: boolean;
    reversedByEntryId?: string;
    createdByUserId: string;
    notes?: string;
    lines: JournalEntryLine[];
    totalDebit?: number;
    totalCredit?: number;
    createdAt: string;
    updatedAt?: string;
}

export interface JournalEntryLine {
    id?: string;
    journalEntryId?: string;
    lineNumber: number;
    accountId: string;
    accountNumber?: string;
    accountName?: string;
    debitAmount: number;
    creditAmount: number;
    description?: string;
    customerId?: string;
    supplierId?: string;
}

export interface TrialBalanceRow {
    accountId: string;
    accountNumber: string;
    accountName: string;
    accountType: string;
    debit: number;
    credit: number;
    balance: number;
}

export interface AccountLedgerEntry {
    date: string;
    entryNumber: string;
    description: string;
    debit: number;
    credit: number;
    balance: number;
    sourceDocumentType?: string;
    sourceDocumentNumber?: string;
}

// --- Request DTOs ---

export interface CreateAccountRequest {
    accountNumber: string;
    nameEn: string;
    nameAr: string;
    accountTypeLookupId: string;
    parentAccountId?: string;
    isPostable?: boolean;
    description?: string;
}

export interface UpdateAccountRequest {
    nameEn: string;
    nameAr: string;
    isActive: boolean;
    isPostable: boolean;
    description?: string;
}

export interface CreateJournalEntryRequest {
    journalEntryTypeLookupId: string;
    entryDate: string;
    description?: string;
    notes?: string;
    lines: CreateJournalEntryLineRequest[];
}

export interface CreateJournalEntryLineRequest {
    accountId: string;
    debitAmount: number;
    creditAmount: number;
    description?: string;
    customerId?: string;
    supplierId?: string;
}

export interface CreateFiscalPeriodRequest {
    name: string;
    startDate: string;
    endDate: string;
}
