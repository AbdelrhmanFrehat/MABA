export interface PaymentVoucher {
    id: string;
    voucherNumber: string;
    paymentDirectionLookupId: string;
    paymentDirectionName?: string;
    paymentMethodLookupId: string;
    paymentMethodName?: string;
    paymentStatusLookupId: string;
    paymentStatusName?: string;
    paymentStatusColor?: string;
    customerId?: string;
    customerName?: string;
    supplierId?: string;
    supplierName?: string;
    amount: number;
    currency: string;
    paymentDate: string;
    referenceNumber?: string;
    bankName?: string;
    chequeNumber?: string;
    transactionId?: string;
    notes?: string;
    isPosted: boolean;
    journalEntryId?: string;
    createdByUserId: string;
    allocations: BizPaymentAllocation[];
    createdAt: string;
    updatedAt?: string;
}

export interface BizPaymentAllocation {
    id?: string;
    paymentVoucherId?: string;
    salesInvoiceId?: string;
    salesInvoiceNumber?: string;
    purchaseInvoiceId?: string;
    purchaseInvoiceNumber?: string;
    allocatedAmount: number;
}

export interface BizPaymentPlan {
    id: string;
    salesInvoiceId?: string;
    salesInvoiceNumber?: string;
    customerId?: string;
    customerName?: string;
    totalAmount: number;
    downPayment: number;
    installmentsCount: number;
    frequencyLookupId: string;
    frequencyName?: string;
    interestRate: number;
    remainingAmount: number;
    statusLookupId: string;
    statusName?: string;
    installments: BizInstallment[];
    createdAt: string;
    updatedAt?: string;
}

export interface BizInstallment {
    id: string;
    paymentPlanId: string;
    seq: number;
    dueDate: string;
    amount: number;
    amountPaid: number;
    statusLookupId: string;
    statusName?: string;
    statusColor?: string;
    paidAt?: string;
    paymentVoucherId?: string;
}

// --- Request DTOs ---

export interface CreatePaymentVoucherRequest {
    paymentDirectionLookupId: string;
    paymentMethodLookupId: string;
    customerId?: string;
    supplierId?: string;
    amount: number;
    currency?: string;
    paymentDate: string;
    referenceNumber?: string;
    bankName?: string;
    chequeNumber?: string;
    notes?: string;
    allocations: CreateBizPaymentAllocationRequest[];
}

export interface CreateBizPaymentAllocationRequest {
    salesInvoiceId?: string;
    purchaseInvoiceId?: string;
    allocatedAmount: number;
}

export interface CreateBizPaymentPlanRequest {
    salesInvoiceId: string;
    customerId?: string;
    downPayment: number;
    installmentsCount: number;
    frequencyLookupId: string;
    interestRate?: number;
}

export interface RecordBizInstallmentPaymentRequest {
    installmentId: string;
    paymentMethodLookupId: string;
    amount: number;
    paymentDate: string;
    referenceNumber?: string;
    notes?: string;
}
