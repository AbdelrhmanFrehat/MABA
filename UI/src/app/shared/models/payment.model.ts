// Payment Models (Extended)
import { PaymentStatus, PaymentMethodType } from './order.model';

// Re-export for convenience
export { PaymentMethodType } from './order.model';

export interface Payment {
    id: string; // GUID
    orderId?: string;
    userId: string;
    amount: number;
    currency: string;
    status: PaymentStatus;
    method: PaymentMethodType;
    transactionId?: string;
    paymentDate: string;
    notes?: string;
    createdAt: string;
}

export interface UserPaymentMethod {
    id: string; // GUID
    userId: string;
    type: PaymentMethodType;
    provider: string; // Stripe, PayPal, etc.
    last4?: string; // Last 4 digits of card
    expiryMonth?: number;
    expiryYear?: number;
    isDefault: boolean;
    createdAt: string;
}

export interface PaymentTransaction {
    id: string; // GUID
    paymentId: string;
    transactionId: string;
    amount: number;
    currency: string;
    status: TransactionStatus;
    providerResponse?: any;
    createdAt: string;
}

export enum TransactionStatus {
    Pending = 'Pending',
    Processing = 'Processing',
    Completed = 'Completed',
    Failed = 'Failed',
    Refunded = 'Refunded'
}

export interface InstallmentPlan {
    id: string; // GUID
    orderId: string;
    totalAmount: number;
    currency: string;
    numberOfInstallments: number;
    installmentAmount: number;
    status: InstallmentPlanStatus;
    schedule: InstallmentSchedule[];
    createdAt: string;
}

export enum InstallmentPlanStatus {
    Active = 'Active',
    Completed = 'Completed',
    Defaulted = 'Defaulted',
    Cancelled = 'Cancelled'
}

export interface InstallmentSchedule {
    id: string; // GUID
    installmentPlanId: string;
    installmentNumber: number;
    amount: number;
    dueDate: string;
    paidDate?: string;
    status: InstallmentStatus;
}

export enum InstallmentStatus {
    Pending = 'Pending',
    Paid = 'Paid',
    Overdue = 'Overdue',
    Skipped = 'Skipped'
}

export interface CreateUserPaymentMethodRequest {
    type: PaymentMethodType;
    provider: string;
    token?: string; // For card tokens
    isDefault?: boolean;
}

// Note: PaymentMethod enum exists in api-response.model.ts
// Use UserPaymentMethod interface for user payment methods

export interface ProcessPaymentRequest {
    orderId: string;
    paymentMethodId: string;
    amount: number;
}
