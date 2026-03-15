/**
 * Standard paginated response from the API
 */
export interface PagedResponse<T> {
    items: T[];
    totalCount: number;
}

/**
 * API error response format
 */
export interface ApiError {
    error: {
        message: string;
        details?: string;
    };
}

/**
 * Query parameters for paginated requests
 */
export interface PaginationParams {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    sortBy?: string;
    sortDirection?: 'ASC' | 'DESC';
}

/**
 * Dropdown option for select components
 */
export interface DropdownOption {
    label: string;
    value: any;
}

// Enum values as per API documentation

export enum InvoiceStatus {
    Draft = 1,
    Confirmed = 2,
    Cancelled = 3
}

export enum MovementType {
    Purchase = 1,
    Sale = 2,
    Adjustment = 3,
    TransferIn = 4,
    TransferOut = 5,
    Return = 6
}

export enum PartyType {
    Supplier = 1,
    Customer = 2
}

export enum PaymentDirection {
    Incoming = 1,
    Outgoing = 2
}

export enum PaymentMethod {
    Cash = 1,
    Card = 2,
    BankTransfer = 3,
    Check = 4
}

export enum ReferenceType {
    PurchaseInvoice = 1,
    SalesInvoice = 2,
    Payment = 3,
    Adjustment = 4
}
