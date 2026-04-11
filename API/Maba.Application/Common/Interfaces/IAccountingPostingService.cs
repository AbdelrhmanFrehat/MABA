namespace Maba.Application.Common.Interfaces;

/// <summary>
/// Creates balanced double-entry journal entries for commercial documents.
/// </summary>
public interface IAccountingPostingService
{
    /// <summary>
    /// Posts a sales invoice: DR Accounts Receivable / CR Sales Revenue.
    /// Idempotent — throws if already posted.
    /// </summary>
    Task PostInvoiceAsync(Guid invoiceId, Guid postedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a customer payment: DR Cash/Bank / CR Accounts Receivable.
    /// Updates invoice paid status when fully covered.
    /// </summary>
    Task PostPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a completed sales return reversal: DR Sales Revenue / CR Accounts Receivable.
    /// </summary>
    Task PostSalesReturnAsync(Guid salesReturnId, Guid postedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts an approved expense: DR Expense Account / CR Cash or Payable.
    /// </summary>
    Task PostExpenseAsync(Guid expenseId, Guid postedByUserId, CancellationToken cancellationToken = default);
}
