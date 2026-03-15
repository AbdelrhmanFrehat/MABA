// Order Models
export interface Order {
    id: string; // GUID
    orderNumber: string;
    userId: string;
    customerName: string;
    customerEmail: string;
    customerPhone?: string;
    status: OrderStatus;
    statusId: string;
    totalAmount: number;
    currency: string;
    subtotal: number;
    taxAmount: number;
    shippingAmount: number;
    discountAmount: number;
    paymentStatus: PaymentStatus;
    paymentMethod?: PaymentMethodType;
    shippingMethod?: ShippingMethod;
    shippingAddress: ShippingAddress;
    billingAddress: BillingAddress;
    items: OrderItem[];
    notes: OrderNote[];
    statusHistory: OrderStatusHistory[];
    createdAt: string;
    updatedAt?: string;
    shippedAt?: string;
    deliveredAt?: string;
    cancelledAt?: string;
}

export interface OrderItem {
    id: string; // GUID
    orderId: string;
    itemId: string;
    itemNameEn: string;
    itemNameAr: string;
    itemSku: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
    discountAmount: number;
    total: number;
    mediaAssetId?: string;
    mediaAssetUrl?: string;
}

export interface ShippingAddress {
    fullName: string;
    phone: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state?: string;
    postalCode?: string;
    country: string;
}

export interface BillingAddress {
    fullName: string;
    phone?: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state?: string;
    postalCode?: string;
    country: string;
}

export interface OrderNote {
    id: string; // GUID
    orderId: string;
    note: string;
    isInternal: boolean;
    createdBy: string;
    createdByName?: string;
    createdAt: string;
}

export interface OrderStatusHistory {
    id: string; // GUID
    orderId: string;
    statusId: string;
    status: OrderStatus;
    changedBy: string;
    changedByName?: string;
    notes?: string;
    createdAt: string;
}

export interface OrderStatus {
    id: string; // GUID
    key: string;
    nameEn: string;
    nameAr: string;
    color?: string;
    order: number;
}

export enum PaymentStatus {
    Pending = 'Pending',
    Paid = 'Paid',
    PartiallyPaid = 'PartiallyPaid',
    Refunded = 'Refunded',
    Failed = 'Failed'
}

export enum PaymentMethodType {
    Cash = 'Cash',
    CreditCard = 'CreditCard',
    DebitCard = 'DebitCard',
    BankTransfer = 'BankTransfer',
    Installment = 'Installment',
    Other = 'Other'
}

export enum ShippingMethod {
    Standard = 'Standard',
    Express = 'Express',
    Overnight = 'Overnight',
    Pickup = 'Pickup'
}

export interface CreateOrderRequest {
    shippingAddress: ShippingAddress;
    billingAddress: BillingAddress;
    paymentMethod: PaymentMethodType;
    shippingMethod?: ShippingMethod;
    notes?: string;
    couponCode?: string;
}

export interface UpdateOrderStatusRequest {
    statusId: string;
    notes?: string;
}

export interface AddOrderNoteRequest {
    note: string;
    isInternal: boolean;
}

export interface OrderListResponse {
    items: Order[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

