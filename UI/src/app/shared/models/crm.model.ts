export interface Customer {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    customerTypeLookupId: string;
    customerTypeName?: string;
    email?: string;
    phone?: string;
    taxNumber?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    country?: string;
    postalCode?: string;
    creditLimit: number;
    balance: number;
    currency: string;
    defaultPriceListId?: string;
    userId?: string;
    isActive: boolean;
    notes?: string;
    contacts?: CustomerContact[];
    createdAt: string;
    updatedAt?: string;
}

export interface CustomerContact {
    id: string;
    customerId: string;
    name: string;
    jobTitle?: string;
    email?: string;
    phone?: string;
    isPrimary: boolean;
}

export interface Supplier {
    id: string;
    code: string;
    nameEn: string;
    nameAr: string;
    supplierTypeLookupId?: string;
    supplierTypeName?: string;
    email?: string;
    phone?: string;
    taxNumber?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    country?: string;
    postalCode?: string;
    creditLimit: number;
    balance: number;
    currency: string;
    paymentTermDays?: number;
    isActive: boolean;
    notes?: string;
    contacts?: SupplierContact[];
    createdAt: string;
    updatedAt?: string;
}

export interface SupplierContact {
    id: string;
    supplierId: string;
    name: string;
    jobTitle?: string;
    email?: string;
    phone?: string;
    isPrimary: boolean;
}

export interface CreateCustomerRequest {
    nameEn: string;
    nameAr: string;
    customerTypeLookupId: string;
    userId?: string;
    email?: string;
    phone?: string;
    taxNumber?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    country?: string;
    postalCode?: string;
    creditLimit?: number;
    currency?: string;
    defaultPriceListId?: string;
    notes?: string;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
    isActive: boolean;
}

export interface CreateSupplierRequest {
    nameEn: string;
    nameAr: string;
    supplierTypeLookupId?: string;
    email?: string;
    phone?: string;
    taxNumber?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    country?: string;
    postalCode?: string;
    creditLimit?: number;
    currency?: string;
    paymentTermDays?: number;
    notes?: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
    isActive: boolean;
}

export interface AccountStatement {
    entries: AccountStatementEntry[];
    openingBalance: number;
    closingBalance: number;
    totalDebit: number;
    totalCredit: number;
}

export interface AccountStatementEntry {
    date: string;
    documentNumber: string;
    documentType: string;
    description: string;
    debit: number;
    credit: number;
    balance: number;
}
