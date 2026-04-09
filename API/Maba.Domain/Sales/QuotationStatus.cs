namespace Maba.Domain.Sales;

public enum QuotationStatus
{
    Draft = 0,
    Sent = 1,
    Accepted = 2,
    Rejected = 3,
    Expired = 4,
    Converted = 5   // converted to a Sales Order
}
