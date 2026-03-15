export interface SalesReportItem {
    invoiceDate: string;
    invoiceNumber: string;
    customerNameAr?: string;
    customerNameEn?: string;
    totalAmount: number;
    discountAmount: number;
    netAmount: number;
    paidAmount: number;
    remainingAmount: number;
}

export interface PurchaseReportItem {
    invoiceDate: string;
    invoiceNumber: string;
    supplierNameAr?: string;
    supplierNameEn?: string;
    totalAmount: number;
    discountAmount: number;
    netAmount: number;
    paidAmount: number;
    remainingAmount: number;
}

export interface ProfitReportItem {
    date: string;
    salesAmount: number;
    purchasesAmount: number;
    expensesAmount: number;
    profit: number;
}

export interface StockReportItem {
    itemId: number;
    itemCode: string;
    itemNameAr?: string;
    itemNameEn?: string;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    currentQuantity: number;
    minQuantity: number;
    isLowStock: boolean;
}

export interface LowStockReportItem {
    itemId: number;
    itemCode: string;
    itemNameAr?: string;
    itemNameEn?: string;
    warehouseId: number;
    warehouseNameAr?: string;
    warehouseNameEn?: string;
    currentQuantity: number;
    minQuantity: number;
    shortage: number;
}

export interface DashboardSummary {
    totalOrders: number;
    totalRevenue: number;
    totalCustomers: number;
    active3DJobs: number;
    lowStockItemsCount: number;
    pendingReviews: number;
    // Legacy fields (for backward compatibility)
    totalSales?: number;
    totalPurchases?: number;
    totalExpenses?: number;
    totalProfit?: number;
    totalSalesToday?: number;
    totalPurchasesToday?: number;
    pendingSalesInvoicesCount?: number;
    pendingPurchaseInvoicesCount?: number;
    pendingSalesAmount?: number;
    pendingPurchasesAmount?: number;
}
